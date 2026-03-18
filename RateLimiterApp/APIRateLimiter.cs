using System;
using System.Collections.Generic;
using System.Threading;

// Cấu hình hệ thống — đơn vị nội bộ: millisecond (1 token = 1000ms)
public static class SystemConfig
{
    public const int MaxCapacityMs = 10_000; // 10 token tối đa

    // Chi phí mỗi loại yêu cầu (ms)
    public static readonly Dictionary<string, Dictionary<string, int>> CostTableMs = new()
    {
        ["free"]    = new() { ["GET"] = 1_000, ["POST"] = 3_000 },
        ["premium"] = new() { ["GET"] = 1_000, ["POST"] = 2_000 },
    };

    // Chuyển ms → token (chỉ dùng khi hiển thị)
    public static double MsToTokens(int ms) => ms / 1000.0;
}

// Kết quả trả về sau mỗi lần gọi API
public record ApiResult(int StatusCode, string Message, int TokensRemainingMs)
{
    public double TokensRemaining => SystemConfig.MsToTokens(TokensRemainingMs);
}

// Bucket token của một người dùng
public class TokenBucket
{
    private readonly string _tier;
    private int      _tokensMs;       // Token hiện có (ms)
    private DateTime _lastRefillTime; // Thời điểm cập nhật cuối

    public TokenBucket(string tier)
    {
        tier = tier.ToLower();
        if (!SystemConfig.CostTableMs.ContainsKey(tier))
            throw new ArgumentException(
                $"Tier không hợp lệ: '{tier}'. Chọn 'free' hoặc 'premium'.");

        _tier           = tier;
        _tokensMs       = SystemConfig.MaxCapacityMs; // Bắt đầu đầy
        _lastRefillTime = DateTime.UtcNow;
    }

    // Hồi phục token theo thời gian thực: tokens = Min(max, tokens + elapsed)
    private void Refill()
    {
        int elapsedMs   = Math.Max(0, (int)(DateTime.UtcNow - _lastRefillTime).TotalMilliseconds);
        _tokensMs       = Math.Min(SystemConfig.MaxCapacityMs, _tokensMs + elapsedMs);
        _lastRefillTime = DateTime.UtcNow;
    }

    // Xử lý yêu cầu API — trả về 200 nếu đủ token, 429 nếu không
    public ApiResult CallApi(string method)
    {
        method = method.ToUpper();
        if (method != "GET" && method != "POST")
            throw new ArgumentException(
                $"Phương thức không hợp lệ: '{method}'. Chọn 'GET' hoặc 'POST'.");

        Refill();

        int costMs = SystemConfig.CostTableMs[_tier][method];

        if (_tokensMs >= costMs)
        {
            _tokensMs -= costMs;
            return new ApiResult(200,
                $"OK — {method} được duyệt. Trừ {SystemConfig.MsToTokens(costMs):0.#} token ({costMs}ms).",
                _tokensMs);
        }
        else
        {
            // Token không đổi khi từ chối
            return new ApiResult(429,
                $"Too Many Requests — {method} bị từ chối. Cần {costMs}ms, có {_tokensMs}ms.",
                _tokensMs);
        }
    }

    // Trạng thái hiện tại (sau hồi phục)
    public (string Tier, int TokensMs, double Tokens) Status()
    {
        Refill();
        return (_tier, _tokensMs, SystemConfig.MsToTokens(_tokensMs));
    }

    // Đặt token thủ công — dùng trong kiểm thử
    public void SetTokensMs(int ms) =>
        _tokensMs = Math.Clamp(ms, 0, SystemConfig.MaxCapacityMs);

    public override string ToString() =>
        $"TokenBucket(tier='{_tier}', tokens={SystemConfig.MsToTokens(_tokensMs):0.##} [{_tokensMs}ms])";
}

class Program
{
    static void PrintResult(ApiResult result)
    {
        string icon = result.StatusCode == 200 ? "✅" : "❌";
        Console.WriteLine($"  {icon} [{result.StatusCode}] {result.Message}");
        Console.WriteLine($"     Token còn lại: {result.TokensRemaining:0.###} token ({result.TokensRemainingMs}ms)");
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine(new string('=', 65));
        Console.WriteLine("  API RATE LIMITER — Token Bucket Demo (C# | đơn vị: int/ms)");
        Console.WriteLine(new string('=', 65));

        // Tình huống 1: Free — gọi POST đến hết token
        Console.WriteLine("\n[TH1] FREE — Gọi POST liên tiếp đến hết token");
        var userFree = new TokenBucket("free");
        var (t1, ms1, tok1) = userFree.Status();
        Console.WriteLine($"  Khởi tạo: tier={t1}, tokens={tok1} [{ms1}ms]");

        for (int i = 1; i <= 4; i++)
        {
            Console.WriteLine($"\n  Lần {i}: POST (3.000ms)");
            PrintResult(userFree.CallApi("POST"));
        }

        // Tình huống 2: Premium — gọi POST đến hết token
        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("[TH2] PREMIUM — Gọi POST liên tiếp đến hết token");
        var userPrem = new TokenBucket("premium");
        var (t2, ms2, tok2) = userPrem.Status();
        Console.WriteLine($"  Khởi tạo: tier={t2}, tokens={tok2} [{ms2}ms]");

        for (int i = 1; i <= 6; i++)
        {
            Console.WriteLine($"\n  Lần {i}: POST (2.000ms)");
            PrintResult(userPrem.CallApi("POST"));
        }

        // Tình huống 3: Biên hồi phục — token=0, chờ 990/1000/1010ms rồi GET
        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("[TH3] FREE/GET — Kiểm tra biên hồi phục (token = 0)");

        foreach (int waitMs in new[] { 990, 1000, 1010 })
        {
            var u = new TokenBucket("free");
            u.SetTokensMs(0);
            Console.WriteLine($"\n  Chờ {waitMs}ms → GET (cần 1.000ms):");
            Thread.Sleep(waitMs);
            PrintResult(u.CallApi("GET"));
        }

        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("  Demo kết thúc.");
        Console.WriteLine(new string('=', 65));
    }
}

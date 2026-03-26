using System;
using Xunit;

// Ca kiểm thử C2 (Branch Coverage) cho TokenBucket — theo CFG đã xây dựng.
public class C2_BranchCoverageTests
{
    // ---- Constructor: TokenBucket(string tier) ----
    // Đường 1: 2(T) → throw
    [Theory]
    [InlineData("Basic")]
    public void Constructor_InvalidTier_ThrowsArgumentException(string invalidTier)
    {
        Assert.Throws<ArgumentException>(() => new TokenBucket(invalidTier));
    }

    // Đường 2: 2(F) → khởi tạo thành công
    [Theory]
    [InlineData("free")]
    [InlineData("premium")]
    public void Constructor_ValidTier_CreatesObjectSuccessfully(string validTier)
    {
        var bucket = new TokenBucket(validTier);
        var (tier, tokensMs, _) = bucket.Status();
        Assert.Equal(validTier.ToLower(), tier);
        Assert.Equal(SystemConfig.MaxCapacityMs, tokensMs);
    }

    // ---- CallApi(string method) ----
    // Đường 1: 2(T) → throw (method không hợp lệ)
    [Theory]
    [InlineData("PUT")]
    public void CallApi_InvalidMethod_ThrowsArgumentException(string invalidMethod)
    {
        var bucket = new TokenBucket("free");
        Assert.Throws<ArgumentException>(() => bucket.CallApi(invalidMethod));
    }

    // Đường 2: 2(F) → 5(T) → return 200 (đủ token)
    [Fact]
    public void CallApi_ValidMethod_SufficientTokens_Returns200()
    {
        var bucket = new TokenBucket("free");
        bucket.SetTokensMs(SystemConfig.MaxCapacityMs);

        var result = bucket.CallApi("GET");

        Assert.Equal(200, result.StatusCode);
    }

    // Đường 3: 2(F) → 5(F) → return 429 (thiếu token, KHÔNG trừ token)
    [Fact]
    public void CallApi_ValidMethod_InsufficientTokens_Returns429()
    {
        var bucket = new TokenBucket("free");
        bucket.SetTokensMs(0);

        var result = bucket.CallApi("POST");

        Assert.Equal(429, result.StatusCode);
        Assert.Equal(0, result.TokensRemainingMs);
    }
}

using System;
using System.Threading;
using Xunit;

public class RateLimiterTests
{
    // 1. Kiểm thử biên sức chứa tối đa
    [Theory]
    [InlineData(0, 0, 0)]    // BVT_1
    [InlineData(0, 1000, 1)] // BVT_2
    [InlineData(8, 1000, 9)] // BVT_3
    [InlineData(8, 2000, 10)]// BVT_4
    [InlineData(8, 5000, 10)]// BVT_5
    public void TestMaxCapacity_Boundary(int initialTokens, int waitMs, int expectedTokens)
    {
        var bucket = new TokenBucket("free");
        bucket.SetTokensMs(initialTokens * 1000);
        
        Thread.Sleep(waitMs);
        
        var (_, _, tok) = bucket.Status();
        Assert.Equal(expectedTokens, (int)tok);
    }

    // 2. Kiểm thử biên mạnh: Thời gian trôi ngược
    [Fact]
    public void CallApi_TimeGoesBack_ShouldNotResultInNegativeTokens()
    {
        var bucket = new TokenBucket("free");
        bucket.SetTokensMs(5000);

        var field = typeof(TokenBucket).GetField("_lastRefillTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(bucket, DateTime.UtcNow.AddHours(1));

        bucket.CallApi("GET"); 

        var (_, tokensMs, _) = bucket.Status();

        Assert.True(tokensMs >= 0, 
            $"BUG: Token bị ÂM ({tokensMs} ms) khi ngược thời gian!");
    }

    // 3. Biên hồi phục & Từ chối Không Trừ Token (R1 -> R12)
    [Theory]
    [InlineData("free", "GET", 950, 429)]   // R1
    [InlineData("free", "GET", 1000, 200)]  // R2
    [InlineData("free", "GET", 1010, 200)]  // R3
    [InlineData("free", "POST", 2950, 429)] // R4
    [InlineData("free", "POST", 3000, 200)] // R5
    [InlineData("free", "POST", 3010, 200)] // R6
    [InlineData("premium", "GET", 950, 429)]   // R7
    [InlineData("premium", "GET", 1000, 200)]  // R8
    [InlineData("premium", "GET", 1010, 200)]  // R9
    [InlineData("premium", "POST", 1950, 429)] // R10
    [InlineData("premium", "POST", 2000, 200)] // R11
    [InlineData("premium", "POST", 2010, 200)] // R12
    public void RecoveryBoundary_ShouldHandleCorrectly(string tier, string method, int waitMs, int expectedStatus)
    {
        var bucket = new TokenBucket(tier);
        bucket.SetTokensMs(0); 

        Thread.Sleep(waitMs);
        var result = bucket.CallApi(method);

        Assert.Equal(expectedStatus, result.StatusCode);

        var (_, _, currentToken) = bucket.Status();
        double waitToken = waitMs / 1000.0;

        if (expectedStatus == 429) 
        {
            Assert.True(Math.Abs(currentToken - waitToken) <= 0.05, 
                $"Lỗi: Từ chối nhưng token bị sai. Đợi {waitToken} nhưng báo {currentToken}");
        }
        else
        {
            double costToken = (tier, method) switch {
                ("free", "GET") => 1.0,
                ("free", "POST") => 3.0,
                ("premium", "GET") => 1.0,
                ("premium", "POST") => 2.0,
                _ => 0
            };
            
            double expectedTokens = waitToken - costToken;
            Assert.True(Math.Abs(currentToken - expectedTokens) <= 0.05, 
                $"Lỗi: Duyệt nhưng số dư sai. Đợi {expectedTokens} nhưng có {currentToken}");
        }
    }

    // 4. Kiểm thử đầu vào không hợp lệ
    [Theory]
    [InlineData("vip")]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_InvalidTier_ShouldThrowArgumentException(string invalidTier)
    {
        var exception = Assert.Throws<ArgumentException>(() => 
        {
            new TokenBucket(invalidTier);
        });
        
        Assert.Contains("Tier không hợp lệ", exception.Message);
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    [InlineData("")]
    [InlineData(" ")]
    public void CallApi_InvalidMethod_ShouldThrowArgumentException(string invalidMethod)
    {
        var bucket = new TokenBucket("free");

        var exception = Assert.Throws<ArgumentException>(() => 
        {
            bucket.CallApi(invalidMethod);
        });
        
        Assert.Contains("Phương thức không hợp lệ", exception.Message);
    }

    
}

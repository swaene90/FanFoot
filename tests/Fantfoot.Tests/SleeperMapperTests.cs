using Fantfoot.Infrastructure.Clients;
using Fantfoot.Infrastructure.Mapping;
using Xunit;

namespace Fantfoot.Tests;

public class SleeperMapperTests
{
    private static SleeperPlayerDto MinimalPlayer(string? height = null, string? weight = null) => new()
    {
        PlayerId = "1",
        FirstName = "Test",
        LastName = "Player",
        Height = height,
        Weight = weight
    };

    [Theory]
    [InlineData("6'2\"", 74)]
    [InlineData("5'10\"", 70)]
    [InlineData("6'0\"", 72)]
    public void ToPlayer_ParsesHeightFeetAndInchesToTotalInches(string height, int expected)
    {
        var player = SleeperMapper.ToPlayer(MinimalPlayer(height: height));
        Assert.Equal(expected, player.Height);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("72")]
    [InlineData("invalid")]
    public void ToPlayer_ReturnsNullHeight_WhenHeightUnparseable(string? height)
    {
        var player = SleeperMapper.ToPlayer(MinimalPlayer(height: height));
        Assert.Null(player.Height);
    }

    [Theory]
    [InlineData("225", 225)]
    [InlineData("180", 180)]
    public void ToPlayer_ParsesWeightAsInteger(string weight, int expected)
    {
        var player = SleeperMapper.ToPlayer(MinimalPlayer(weight: weight));
        Assert.Equal(expected, player.Weight);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("heavy")]
    public void ToPlayer_ReturnsNullWeight_WhenWeightUnparseable(string? weight)
    {
        var player = SleeperMapper.ToPlayer(MinimalPlayer(weight: weight));
        Assert.Null(player.Weight);
    }
}

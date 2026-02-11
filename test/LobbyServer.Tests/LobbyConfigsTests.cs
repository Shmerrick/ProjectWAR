namespace LobbyServer.Tests;

public class LobbyConfigsTests
{
    [Fact]
    public void LobbyConfigs_DefaultValues_ShouldBeValid()
    {
        // Arrange & Act
        var config = new LobbyConfigs();

        // Assert
        Assert.Equal(8048, config.ClientPort);
        Assert.Equal("1.4.8", config.ClientVersion);
        Assert.True(config.SeverOnFinish);
        Assert.NotNull(config.LogLevel);
    }

    [Fact]
    public void LobbyConfigs_CanSetCustomPort()
    {
        // Arrange
        var config = new LobbyConfigs();
        int expectedPort = 9000;

        // Act
        config.ClientPort = expectedPort;

        // Assert
        Assert.Equal(expectedPort, config.ClientPort);
    }

    [Fact]
    public void LobbyConfigs_CanSetCustomVersion()
    {
        // Arrange
        var config = new LobbyConfigs();
        string expectedVersion = "2.0.0";

        // Act
        config.ClientVersion = expectedVersion;

        // Assert
        Assert.Equal(expectedVersion, config.ClientVersion);
    }
}

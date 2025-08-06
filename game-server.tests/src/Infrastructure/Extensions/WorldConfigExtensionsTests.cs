using FluentAssertions;
using Villagers.GameServer.Domain;
using Villagers.GameServer.Infrastructure.Extensions;
using Xunit;

namespace Villagers.GameServer.Tests.Infrastructure.Extensions;

public class WorldConfigExtensionsTests
{
    [Fact]
    public void ToEntity_ShouldConvertDomainToEntity()
    {
        // Arrange
        var worldConfig = new WorldConfig("Test World", TimeSpan.FromSeconds(2));

        // Act
        var entity = worldConfig.ToEntity();

        // Assert
        entity.WorldName.Should().Be("Test World");
        entity.TickIntervalMs.Should().Be(2000);
    }

    [Fact]
    public void ToDomain_ShouldConvertEntityToDomain()
    {
        // Arrange
        var entity = new Villagers.GameServer.Entities.WorldConfigEntity
        {
            WorldName = "Restored World",
            TickIntervalMs = 1500
        };

        // Act
        var domain = entity.ToDomain();

        // Assert
        domain.WorldName.Should().Be("Restored World");
        domain.TickInterval.Should().Be(TimeSpan.FromMilliseconds(1500));
    }

    [Fact]
    public void ToEntity_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalConfig = new WorldConfig("Round Trip Test", TimeSpan.FromMilliseconds(750));

        // Act
        var entity = originalConfig.ToEntity();
        var restoredConfig = entity.ToDomain();

        // Assert
        restoredConfig.WorldName.Should().Be(originalConfig.WorldName);
        restoredConfig.TickInterval.Should().Be(originalConfig.TickInterval);
    }
}
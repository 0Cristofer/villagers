namespace Villagers.GameServer.Domain.Enums;

/// <summary>
/// Represents the starting direction for a player when entering a world
/// </summary>
public enum StartingDirection
{
    // Cardinal directions
    North = 1,
    South = 2,
    East = 3,
    West = 4,
    
    // Intermediate directions
    Northeast = 5,
    Northwest = 6,
    Southeast = 7,
    Southwest = 8,
    
    // Random direction option
    Random = 9
}
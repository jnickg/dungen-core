namespace DunGen.TerrainGen
{
  /// <summary>
  /// Enumeration of types of terrain that can be generated using this
  /// particular algorithm. If terrain type differs based on algorithm
  /// parameters, all possible types should be included
  /// </summary>
  public enum TerrainGenStyle
  {
    Uncategorized     = 0x0,        // Indeterminate or difficult-to-categorize
    Bldg_Halls        = 0x00000001, // Building-like hallways
    Bldg_Rooms        = 0x00000002, // Building-like rooms
    Bldg              = Bldg_Halls | Bldg_Rooms,
    Cave_Passages     = 0x00000004, // Cave-like passages
    Cave_Chambers     = 0x00000008, // Cave-like chambers
    Cave              = Cave_Passages | Cave_Passages,
    Open_Obstacles    = 0x00000010, // Obstacles in an open area
  }
}

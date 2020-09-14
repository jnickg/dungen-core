namespace DunGen.TerrainGen
{
  /// <summary>
  /// Enumeration of types of behavior this algorithm can have when run
  /// on a DungeonTiles that already contains data.
  /// </summary>
  public enum TerrainModBehavior
  {
    None,     // Doesn't change existing data
    Build,    // Unobtrusively builds walls where possible
    Carve,    // Unobtrusively carves through walls where possible
    Both,     // Unobtrusively builds and carves where possible
    Clobber   // Totally erases existing data, before running
  }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen
{
  /// <summary>
  /// Enumeration of types of behavior a ITerrainGenAlgorithm can have
  /// when run on a DungeonTiles that already contains data.
  /// </summary>
  public enum TerrainModification
  {
    None,     // Doesn't change existing data
    Build,    // Constructs walls where possible
    Carve,    // Carves paths through walls where possible
    Both,     // Constructs and carves where possible
    Clobber   // Totally wipes existing data, either by filling with walls or open tiles
  }

  public interface ITerrainGenAlgorithm
  {
    string Name { get; }
    TerrainModification Behavior { get; }
    void Run(DungeonTiles d, bool[,] mask, Random r);
  }
}

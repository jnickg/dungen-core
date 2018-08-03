using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace DunGen
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

  /// <summary>
  /// An interface satisfied by all Terrain Generating Algorithms
  /// which ensures that the algorithm can be run, and provide
  /// some basic information about itself
  /// </summary>
  public interface ITerrainGenAlgorithm : IAlgorithm
  {
    /// <summary>
    /// How this algorithm behaves, given its current parameters.
    /// </summary>
    TerrainModBehavior Behavior { get; }
    /// <summary>
    /// The style of this algorithm.
    /// </summary>
    TerrainGenStyle Style { get; }

    /// <summary>
    /// Run the algorithm with the current TerrainGenAlgorithmParams,
    /// and on the specified input parameters.
    /// </summary>
    void Run(DungeonTiles d, bool[,] mask, Random r);
  }

  /// <summary>
  /// Shared base implementation for Terrain Generating algorithms
  /// Currently not needed but added anyways in anticipation of
  /// TerrainGen specific implementations being needed later.
  /// </summary>
  public abstract class TerrainGenAlgorithmBase : AlgorithmBase, ITerrainGenAlgorithm
  {
    public abstract TerrainModBehavior Behavior { get; }
    public abstract TerrainGenStyle Style { get; }

    public abstract void Run(DungeonTiles d, bool[,] mask, Random r);
  }
}

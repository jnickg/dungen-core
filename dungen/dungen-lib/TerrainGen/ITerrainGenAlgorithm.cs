﻿using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;

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
  }

  /// <summary>
  /// Shared base implementation for Terrain Generating algorithms.
  /// Plugin developers can expand on this abstract type, or completely
  /// implement the ITerrainGenAlgorithm interface themselves.
  /// </summary>
  public abstract class TerrainGenAlgorithmBase : AlgorithmBase, ITerrainGenAlgorithm
  {
    #region Nested Types
    /// <summary>
    /// Style of wall formation used when introducing divisons
    /// between open tiles. All algorithms must support
    /// interacting with dungeons using any wall formation
    /// style, even if they don't support forming walls
    /// themselves
    /// </summary>
    public enum WallFormation
    {
      Tiles,
      Boundaries
    }

    /// <summary>
    /// Strategy for handling existing open tiles in a dungeon
    /// when this algorithm runs.
    /// </summary>
    public enum OpenTilesHandling
    {
      Avoid,
      ConnectToRooms,
      ConnectToHalls,
      Connect,
      Ignore,
      Overwrite
    }

    /// <summary>
    /// Strategy for handling any egress specified in the
    /// Dungeon, when this algorithm runs.
    /// </summary>
    public enum EgressHandling
    {
      Avoid,
      Connect,
      Ignore,
      Overwrite
    }
    #endregion

    #region Non-Parameter Properties
    public abstract TerrainModBehavior Behavior { get; }
    public abstract TerrainGenStyle Style { get; }
    #endregion

    #region Parameter Properties
    //======================================================================
    // Parameter values defined here should be `virtual` so that children
    // can overwrite them in cases where default values should be different,
    // or that property is actually NOT supported at all
    //======================================================================

#if DEBUG
    [BooleanParameter(
      Description = "Whether to group generated tiles for debug purposes, " +
      "often with severe performance hits",
      Default = false,
      Show = false)]
#endif
    public bool GroupForDebug { get; set; } = false; // Explicitly initialize for release builds

    /// <summary>
    /// See help text.
    /// </summary>
    [BooleanParameter(
      Description = "Whether to group any rooms generated by the algorithm",
      Default = false,
      Show = false)]
    public bool GroupRooms { get; set; }

    protected const string WallStrategy_Help =
      "How this algorithm should form walls when introducing divisions " +
      "between open tiles. Algorithms may ignore this parameter if they " +
      "can not support a particular wall formation style.";

    /// <summary>
    /// See help text.
    /// </summary>
    [SelectionParameter(
      Description = WallStrategy_Help,
      SelectionType = typeof(WallFormation),
      Default = WallFormation.Boundaries)]
    public virtual WallFormation WallStrategy { get; set; }

    protected const string OpenTilesStrategy_Help =
      "How this algorithm should interact with existing open tiles, if " +
      "there are any in its mask. Algorithms may ignore this parameter if " +
      "they can not support a particular strategy.";

    /// <summary>
    /// See help text.
    /// </summary>
    [SelectionParameter(
      Description = OpenTilesStrategy_Help,
      SelectionType = typeof(OpenTilesHandling),
      Default = OpenTilesHandling.Ignore)]
    public virtual OpenTilesHandling OpenTilesStrategy { get; set; }

    protected const string EgressStrategy_Help =
      "How this algorithm should interact with egress openings, if " +
      "there are any in its mask. Algorithms may ignore this parameter" +
      "if they cannot support a particular strategy";

    /// <summary>
    /// See help text.
    /// </summary>
    [SelectionParameter(
      Description = EgressStrategy_Help,
      SelectionType = typeof(EgressHandling),
      Default = EgressHandling.Ignore)]
    public virtual EgressHandling EgressStrategy { get; set; }
    #endregion

    #region Members
    /// <summary>
    /// Runs this algorithm on the specified tiles, with the specified mask.
    /// </summary>
    /// <param name="d">A collection of tiles on which this algorithm will
    /// operate.</param>
    /// <param name="mask">The masked subregion of the specified DungeonTiles,
    /// on which this algorithm will operate.</param>
    /// <param name="r">An optional Randomness provider.</param>
    // TODO update this to just use IAlgorithmContext rather than d, mask, r
    protected abstract void _runAlgorithm(IAlgorithmContext context);

    /// <see cref="AlgorithmBase._runInternal(IAlgorithmContext)"/>
    protected override void _runInternal(IAlgorithmContext context)
    {
      // TODO any terrain-gen specific context checking

      _runAlgorithm(context);
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendFormat("Algorithm \"{0}\" (behavior: {1} style: {2})\n", Name, Behavior, Style);
      foreach (var param in Parameters.List)
      {
        sb.AppendLine(param.ToString());
      }

      return sb.ToString();
    }
    #endregion
  }
}

﻿using DunGen.Algorithm;
using System.Text;

namespace DunGen.TerrainGen
{
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

    /// <summary>
    /// Directional selector, for any Parameters that require it.
    /// For example, the orientation of an algorithm, if it has
    /// oriented properties, such as "starting point," or "weight."
    /// </summary>
    public enum Direction
    {
      North,
      East,
      South,
      West
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
    /// <param name="context">The context in which this algorithm will be run.</param>
    protected abstract void _runAlgorithm(IAlgorithmContext context);

    /// <see cref="AlgorithmBase._runInternal(IAlgorithmContext)"/>
    sealed protected override void _runInternal(IAlgorithmContext context)
    {
      if (this.Behavior == TerrainModBehavior.Clobber)
      {
        context.D.Tiles.SetAllToo(Tiles.Tile.MoveType.Wall, context.Mask);
      }

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

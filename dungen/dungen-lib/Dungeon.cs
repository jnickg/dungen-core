using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen
{
  /// <summary>
  /// A dungeon, its tiles, infestations, and information about how
  /// it was created.
  /// </summary>
  public class Dungeon
  {
    #region Private Members
    private DungeonTiles PROPERTY_tiles;
    #endregion

    /// <summary>
    /// The tiles associated with this Dungeon
    /// </summary>
    public DungeonTiles Tiles
    {
      get { return PROPERTY_tiles; }
      set
      {
        this.PROPERTY_tiles = value;
        value.Parent = this;
      }
    }

    /// <summary>
    /// Algorithms that were run on the Dungeon's tiles, and the mask used when
    /// it was run.
    /// </summary>
    public IDictionary<ITerrainGenAlgorithm, bool[,]> Algorithms { get; set; }

    public Dungeon()
    {
      this.Algorithms = new Dictionary<ITerrainGenAlgorithm, bool[,]>();
    }
  }
}

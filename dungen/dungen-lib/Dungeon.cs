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

    public Dungeon()
    {
    }
  }
}

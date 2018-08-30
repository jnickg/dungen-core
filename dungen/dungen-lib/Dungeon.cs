using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace DunGen
{
  /// <summary>
  /// A dungeon, its tiles, infestations, and information about how
  /// it was created.
  /// </summary>
  [DataContract(Name = "dungeon")]
  public class Dungeon
  {
    #region Private Members
    private DungeonTiles PROPERTY_tiles;
    #endregion

    /// <summary>
    /// The tiles associated with this Dungeon
    /// </summary>
    [DataMember(IsRequired = true, Name = "layout")]
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
      this.Tiles = new DungeonTiles(0, 0);
    }
  }
}

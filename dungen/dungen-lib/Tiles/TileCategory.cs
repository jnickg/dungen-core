using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.Tiles
{
  /// <summary>
  /// Categorical purpose buckets into which Tiles can be put during terrain
  /// generation, to indicate a special purpose later in the process of
  /// generating a dungeon (such as placing a door, trap, etc.)
  /// </summary>
  public enum TileCategory
  {
    /// <summary>
    /// Only the Tile's data is important
    /// </summary>
    Normal,
    /// <summary>
    /// Marks a threshold between connected areas, groups, or otherwise
    /// </summary>
    Threshold,
    /// <summary>
    /// A small offshoot from a normal hallway that shouldn't be culled
    /// </summary>
    Niche,
    /// <summary>
    /// A room that should be connected to hallways
    /// </summary>
    Room,
    /// <summary>
    /// A hallway that should be connected to something else
    /// </summary>
    Hallway

    // TODO more categories?
  }
}

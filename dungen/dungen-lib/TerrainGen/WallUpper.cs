using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public class WallUpper : ITerrainGenAlgorithm
  {
    public string Name
    {
      get
      {
        return "Wall Upper";
      }
    }

    public TerrainModification Behavior
    {
      get
      {
        return TerrainModification.Build;
      }
    }

    public void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      d.SetAllToo(Tile.MoveType.Wall, mask);
    }
  }
}

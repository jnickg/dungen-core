using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public class WallUpper : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior { get => TerrainModBehavior.Build; }

    public override TerrainGenStyle Style { get => TerrainGenStyle.Uncategorized; }

    public WallUpper()
    {
      // no op
    }

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      d.SetAllToo(Tile.MoveType.Wall, mask);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public class OpenUpper : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior { get => TerrainModBehavior.Carve; }

    public override TerrainGenStyle Style { get => TerrainGenStyle.Uncategorized; }

    public OpenUpper()
    {
      // no op
    }

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      d.SetAllToo(Tile.MoveType.Open_HORIZ, mask);
    }
  }
}

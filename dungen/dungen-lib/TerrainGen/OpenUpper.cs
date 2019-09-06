using DunGen.Algorithm;
using DunGen.Tiles;
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

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      context.D.Tiles.SetAllToo(Tile.MoveType.Open_HORIZ, context.Mask);
    }
  }
}

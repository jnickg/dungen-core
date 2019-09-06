using DunGen.Algorithm;
using DunGen.Tiles;
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

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      context.D.Tiles.SetAllToo(Tile.MoveType.Wall, context.Mask);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.TerrainGen
{
  public class DeadEndFiller : TerrainGenAlgorithmBase
  {
    [IntegerAlgorithmParamInfo(
      "The number of passes to take removing all single dead-end tiles",
      50,
      0,
      int.MaxValue)]
    public int FillPasses { get; set; }

    public override TerrainModBehavior Behavior => TerrainModBehavior.Build;

    public override TerrainGenStyle Style => TerrainGenStyle.Uncategorized;

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      ISet<Tile> deadEnds = new HashSet<Tile>();
      for (int i = 0; i < this.FillPasses; ++i)
      {
        for (int y = 0; y < d.Height; ++y)
        {
          for (int x = 0; x < d.Width; ++x)
          {
            if (!mask[y, x] || d[y, x].Physics == Tile.MoveType.Wall) continue;
            bool physicsDeadEnd = (d[y, x].Physics.SidesOpened() == 1);
            bool adjacentsdeadEnd = (d.GetAdjacentOpensFor(x, y) == 1);
            if (!physicsDeadEnd && !adjacentsdeadEnd) continue;
            d[y, x].Physics = d[y, x].Physics.CloseOff(Tile.MoveType.Open_HORIZ);
          }
        }
        d.CreateGroup(deadEnds);
      }
    }
  }
}

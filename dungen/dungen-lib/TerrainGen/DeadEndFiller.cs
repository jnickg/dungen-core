using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.TerrainGen
{
  public class DeadEndFiller : TerrainGenAlgorithmBase
  {
    [IntegerParameter(
      Description = "The number of passes to take removing all single dead-end tiles",
      Default = 50,
      Minimum = 0,
      Maximum = int.MaxValue)]
    public int FillPasses { get; set; }

    public override TerrainModBehavior Behavior => TerrainModBehavior.Build;

    public override TerrainGenStyle Style => TerrainGenStyle.Uncategorized;

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      DungeonTiles workingDungeon = context.D.Tiles;
      ISet<Tile> deadEnds = new HashSet<Tile>();
      for (int i = 0; i < this.FillPasses; ++i)
      {
        for (int y = 0; y < workingDungeon.Height; ++y)
        {
          for (int x = 0; x < workingDungeon.Width; ++x)
          {
            if (!context.Mask[y, x] || workingDungeon[y, x].Physics == Tile.MoveType.Wall) continue;
            bool physicsDeadEnd = (workingDungeon[y, x].Physics.SidesOpened() == 1);
            bool adjacentsdeadEnd = (workingDungeon.GetAdjacentOpensFor(x, y) == 1);
            if (!physicsDeadEnd && !adjacentsdeadEnd) continue;
            workingDungeon[y, x].Physics = workingDungeon[y, x].Physics.CloseOff(Tile.MoveType.Open_HORIZ);
          }
        }
        workingDungeon.Parent.CreateGroup(deadEnds);
      }
    }
  }
}

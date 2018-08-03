using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.TerrainGen
{
  public class DeadEndFiller : TerrainGenAlgorithmBase
  {
    public enum CleanupStrategy
    {
      ShortestFirst,
      LongestFirst,
      Random
    }

    [SelectionAlgorithmParameterInfo(
      "How this dead end filler prioritizes which dead ends to clean up next.",
      typeof(CleanupStrategy),
      CleanupStrategy.Random)]
    public CleanupStrategy Strategy { get; set; }

    [DecimalAlgorithmParamInfo(
      "0.0 to 1.0 percentage, representing what proportion of dead ends to remove",
      0.7,
      0.0,
      1.0,
      5)]
    public double CleanupFactor { get; set; }

    public override TerrainModBehavior Behavior => TerrainModBehavior.Build;

    public override TerrainGenStyle Style => TerrainGenStyle.Uncategorized;

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      ISet<Tile> deadEnds = new HashSet<Tile>();
      for (int i = 0; i < 10; ++i)
      for (int y = 0; y < d.Height; ++y)
      {
        for (int x = 0; x < d.Width; ++x)
        {
          if (!mask[y, x] || d[y,x].Physics == Tile.MoveType.Wall) continue;
          bool physicsDeadEnd = (d[y, x].Physics.SidesOpened() == 1);
          bool adjacentsdeadEnd = (d.GetAdjacentOpensFor(x, y) == 1);
          if (!physicsDeadEnd && !adjacentsdeadEnd) continue;

          // TODO right now this just opens up one per pass. Instead, find all
          // dead ends and do a depth-first search for the first tile that has
          // greater than two openings, and build a weighted list of dead-end
          // halls
          d[y, x].Physics = d[y, x].Physics.CloseOff(Tile.MoveType.Open_HORIZ);
        }
      }
      d.CreateGroup(deadEnds);
    }
  }
}

using DunGen.Algorithm;
using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.TerrainGen
{
  public class DeadEndCleaner : TerrainGenAlgorithmBase
  {
    public enum CleanupStrategy
    {
      ShortestFirst,
      LongestFirst,
      Random
    }

    [SelectionParameter(
      Description = "How this dead end filler prioritizes which dead ends to clean up next.",
      SelectionType = typeof(CleanupStrategy),
      Default = CleanupStrategy.Random)]
    public CleanupStrategy Strategy { get; set; }

    [DecimalParameter(
      Description = "0.0 to 1.0 percentage, representing what proportion of dead ends to remove",
      Default = 0.7,
      Minimum = 0.0,
      Maximum = 1.0,
      Precision = 5)]
    public double CleanupFactor { get; set; }

    public override TerrainModBehavior Behavior => TerrainModBehavior.Build;

    public override TerrainGenStyle Style => TerrainGenStyle.Uncategorized;

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      // Find all dead ends and do a depth-first search for the first tile
      // that has greater than two openings, and build a weighted list of
      // dead-end halls
      throw new NotImplementedException();
    }
  }
}

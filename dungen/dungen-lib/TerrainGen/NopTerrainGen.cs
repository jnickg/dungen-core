using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.TerrainGen
{
  public class NopTerrainGen : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior => TerrainModBehavior.None;

    public override TerrainGenStyle Style => TerrainGenStyle.Uncategorized;

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      return;
    }
  }
}

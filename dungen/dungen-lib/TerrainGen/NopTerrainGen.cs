using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.TerrainGen
{
  public class NopTerrainGen : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior => TerrainModBehavior.None;

    public override TerrainGenStyle Style => TerrainGenStyle.Uncategorized;

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      return;
    }
  }
}

using System;
using DunGen;
using DunGen.Algorithm;
using DunGen.TerrainGen;
using DunGen.Tiles;

namespace DunGenPlugin
{
  public class CustomTerrainGenAlgorithm : TerrainGenAlgorithmBase
  {
    [IntegerParameter(
      Description = "A meaningless integer parameter",
      Maximum = int.MaxValue,
      Minimum = 0,
      Default = 37)]
    public int IntegerParam { get; set; }

    public override TerrainModBehavior Behavior => TerrainModBehavior.None;

    public override TerrainGenStyle Style => TerrainGenStyle.Uncategorized;

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      throw new NotImplementedException();
    }
  }
}

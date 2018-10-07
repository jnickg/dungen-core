using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.Algorithm
{
  /// <summary>
  /// TODO Not Implemented.
  /// </summary>
  public class CompositeAlgorithm : AlgorithmBase
  {
    [CompositeAlgorithmParameterInfo(
      Description = "The algorithms comprising this CompositeAlgorithm",
      AlgorithmBaseType = typeof(IAlgorithm))]
    public AlgorithmParameterAlgGroup Algorithms { get; set; }

    public override void Run(IAlgorithmContext context)
    {
      if (Algorithms.Count < 1) return;

      foreach (IAlgorithm alg in Algorithms)
      {
        alg.Run(context);
      }
    }
  }
}

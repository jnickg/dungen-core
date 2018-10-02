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
    // How do we hold a collection of algorithms here and
    // provide attributes for each one, like defaults or ranges?
    // I think we'll just have to treat it special and have it
    // use no parameters, plus make a shim object for holding
    // a list of algorithms | TODO

    public IList<IAlgorithm> Algorithms { get; set; }

    public override void Run(IAlgorithmContext context)
    {
      foreach (IAlgorithm alg in Algorithms)
      {
        alg.Run(context);
      }
    }
  }
}

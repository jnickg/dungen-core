using DunGen.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DunGen.Generator
{
  [CollectionDataContract(Name = "runList", ItemName = "algRunInfo")]
  [KnownType(nameof(GetKnownTypes))]
  public class AlgorithmRunInfoList : List<AlgorithmRunInfo>
  {
    public static IEnumerable<Type> GetKnownTypes()
    {
      return AlgorithmBase.GetKnownTypes();
    }

    public IList<AlgorithmRun> ReconstructRuns()
    {
      return new List<AlgorithmRun>(this.Select(r => r.ReconstructRun()));
    }
  }
}

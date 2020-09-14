using DunGen.Algorithm;
using DunGen.Plugins;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Generator
{
  [DataContract(Name = "algRunInfo")]
  public class AlgorithmRunInfo
  {
    private string _fullName = string.Empty;

    [DataMember(Name = "algInfo", Order = 1)]
    public AlgorithmInfo Info { get; set; } = new AlgorithmInfo();

    [DataMember(Name = "mask", Order = 3)]
    public BoolCollection Mask { get; set; } = null;

    [DataMember(Name = "r", Order = 2)]
    public int RandomSeed { get; set; } = default(int);

    public AlgorithmRunInfo()
    { }

    public AlgorithmRun ReconstructRun()
    {
      return new AlgorithmRun()
      {
        Alg = RecreateAlgorithmInstance(this),
        Context = new AlgorithmContextBase()
        {
          R = new AlgorithmRandom(RandomSeed),
          Mask = this.Mask.UnJaggedize(),
          D = null // Generator should set this
        },
      };
    }

    public static AlgorithmRunInfo CreateFrom(AlgorithmRun run)
    {
      return new AlgorithmRunInfo()
      {
        Info = run.Alg.ToInfo(),
        Mask = run.Context.Mask.Jaggedize_DC(),
        RandomSeed = run.Context.R.Seed
      };
    }

    private static IAlgorithm RecreateAlgorithmInstance(AlgorithmRunInfo runInfo)
    {
      return runInfo.Info.CreateInstance();
    }

    private static IAlgorithm FindAlgorithm(string typeName)
    {
      return AlgorithmPluginEnumerator.GetAlgorithm(typeName);
    }
  }

  public static partial class Extensions
  {
    public static AlgorithmRunInfo ToInfo(this AlgorithmRun run)
    {
      return AlgorithmRunInfo.CreateFrom(run);
    }
  }
}

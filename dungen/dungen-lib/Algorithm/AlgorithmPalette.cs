using DunGen.Plugins;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DunGen.Algorithm
{
  [CollectionDataContract(Name = "algPalette", KeyName = "name", ValueName = "algPreset")]
  public class AlgorithmPalette : Dictionary<string, AlgorithmPaletteItem>
  {
    public static AlgorithmPalette DefaultPalette(IEnumerable<IAlgorithm> algorithms)
    {
      AlgorithmPalette defaultPalette = new AlgorithmPalette();
      foreach (var alg in algorithms)
      {
        defaultPalette.Add(alg.Name + "_default", new AlgorithmPaletteItem()
        {
          TypeName = alg.GetType().FullName,
          ParamPresets = alg.GetParamsPrototype()
        });
      }
      return defaultPalette;
    }
  }

  [DataContract(Name = "algPreset")]
  public class AlgorithmPaletteItem : ICloneable
  {
    [DataMember(IsRequired = true, Name = "type", Order = 1)]
    public string TypeName { get; set; }

    [DataMember(IsRequired = false, Name = "params", Order = 2)]
    public AlgorithmParams ParamPresets { get; set; }

    public object Clone()
    {
      return new AlgorithmPaletteItem()
      {
        TypeName = this.TypeName,
        ParamPresets = (AlgorithmParams)this.ParamPresets.Clone()
      };
    }

    public IAlgorithm CreateInstance()
    {
      IAlgorithm alg = AlgorithmPluginEnumerator.GetAlgorithm(this.TypeName);

      if (alg.TakesParameters)
      {
        alg.Parameters = (AlgorithmParams)this.ParamPresets.Clone();
      }

      return alg;
    }
  }
}

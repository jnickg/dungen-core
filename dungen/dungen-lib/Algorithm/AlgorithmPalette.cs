using DunGen.Plugins;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        defaultPalette.Add(alg.Name + "_default", alg.ToPaletteItem(true));
      }
      return defaultPalette;
    }
  }

  [DataContract(Name = "algPreset")]
  public class AlgorithmPaletteItem : ICloneable
  {
    private Color _paletteColor = Color.Empty;

    [DataMember(IsRequired = true, Name = "type", Order = 1)]
    public string TypeName { get; set; }

    [DataMember(IsRequired = false, Name = "params", Order = 2)]
    public AlgorithmParams ParamPresets { get; set; }

    [DataMember(IsRequired = false, Name = "color", Order = 3)]
    public Color PaletteColor
    {
      get
      {
        if (_paletteColor == Color.Empty)
        {
          _paletteColor = CreateInstance().ToDefaultColor();
        }
        return _paletteColor;
      }
      set
      {
        _paletteColor = value;
      }
    }

    public object Clone()
    {
      return new AlgorithmPaletteItem()
      {
        TypeName = this.TypeName,
        ParamPresets = (AlgorithmParams)this.ParamPresets.Clone(),
        PaletteColor = this.PaletteColor
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

  public static partial class Extensions
  {
    public static Color ToDefaultColor(this IAlgorithm alg)
    {
      int hashColor = alg.GetHashCode();
      hashColor = (int)(hashColor | 0xFF000000); // No transparency
      return Color.FromArgb(hashColor);
    }

    public static AlgorithmPaletteItem ToPaletteItem(this IAlgorithm alg, bool defaultParams = false)
    {
      return new AlgorithmPaletteItem()
      {
        ParamPresets = defaultParams ? alg.GetParamsPrototype() : alg.Parameters,
        TypeName = alg.GetType().FullName,
        PaletteColor = alg.ToDefaultColor()
      };
    }
  }
}

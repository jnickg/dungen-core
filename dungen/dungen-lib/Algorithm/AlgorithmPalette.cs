using DunGen.Plugins;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

namespace DunGen.Algorithm
{
  /// <summary>
  /// A Dictionary of human readable names and their corresponding pre-set
  /// Algorithms. This facilitates the re-use of tuned and optimized configurations
  /// of Algorithms, to serve different purposes.
  /// </summary>
  [CollectionDataContract(Name = "algPalette", KeyName = "name", ValueName = "algPreset")]
  [KnownType("GetKnownTypes")]
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

    public static IEnumerable<Type> GetKnownTypes()
    {
      return AlgorithmBase.GetKnownTypes();
    }
  }

  /// <summary>
  /// A pre-set Algorithm, with some adornment to provide extra information needed
  /// by users of an Algorithm palette. Pre-set Algorithms can be assigned a color,
  /// or will automatically generate their own based on their current configuration.
  /// </summary>
  [DataContract(Name = "algPreset")]
  public class AlgorithmPaletteItem : ICloneable
  {
    private Color _paletteColor = Color.Empty;

    /// <summary>
    /// The actual information about the Algorithm
    /// </summary>
    [DataMember(IsRequired = true, Name = "algInfo", Order = 1)]
    public AlgorithmInfo Info { get; set; }

    /// <summary>
    /// The color associated with this PaletteItem instance.
    /// </summary>
    [DataMember(IsRequired = false, Name = "color", Order = 2)]
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

    /// <summary>
    /// Resets this PaletteItem's assigned color, and regenerates a default
    /// color based on the Algorithm's current configuration.
    /// </summary>
    public void ResetColor()
    {
      this.PaletteColor = CreateInstance().ToDefaultColor();
    }

    public object Clone()
    {
      return new AlgorithmPaletteItem()
      {
        Info = (AlgorithmInfo)this.Info.Clone(),
        PaletteColor = this.PaletteColor // Struct safely cloned
      };
    }

    public IAlgorithm CreateInstance()
    {
      //IAlgorithm alg = AlgorithmPluginEnumerator.GetAlgorithm(this.Info.Type.ConvertToType(true));
      IAlgorithm alg = this.Info.ToInstance();

      if (alg.TakesParameters)
      {
        alg.Parameters = (AlgorithmParams)this.Info.Parameters.Clone();
      }

      return alg;
    }
  }

  public static partial class Extensions
  {
    /// <summary>
    /// Generates a default color for this IAlgorithm instance, based on its
    /// current configuration.
    /// </summary>
    /// <returns>A default color for this IAlgorithm.</returns>
    public static Color ToDefaultColor(this IAlgorithm alg)
    {
      int hashColor = alg.GetHashCode();
      hashColor = (int)(hashColor | 0xFF000000); // No transparency
      return Color.FromArgb(hashColor);
    }

    /// <summary>
    /// Creates an AlgorithmPaletteItem based on this IAlgorithm's current state.
    /// </summary>
    /// <param name="defaultParams">Whether to use the Algorithm's default
    /// parameter settings, instead of the instance's current settings.</param>
    /// <returns>A new AlgorithmPaletteItem instance.</returns>
    public static AlgorithmPaletteItem ToPaletteItem(this IAlgorithm alg, bool defaultParams = false)
    {
      return new AlgorithmPaletteItem()
      {
        Info = alg.ToInfo(),
        PaletteColor = alg.ToDefaultColor()
      };
    }
  }
}

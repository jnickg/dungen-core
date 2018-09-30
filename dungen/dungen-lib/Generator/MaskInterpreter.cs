using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using DunGen.Algorithm;

namespace DunGen.Generator
{
  /// <summary>
  /// Provides static utilities for generating 2D boolean masks from other
  /// data types.
  /// </summary>
  public static class MaskInterpreter
  {
    public static bool[,] GetMask(Image maskSource, Color colorMask)
    {
      bool[,] desiredMask = new bool[maskSource.Height, maskSource.Width];

      using (Bitmap maskSource_bmp = new Bitmap(maskSource))
      {
        for (int y = 0; y < maskSource.Height; ++y)
        {
          for (int x = 0; x < maskSource.Width; ++x)
          {
            int pelArgb = maskSource_bmp.GetPixel(x, y).ToArgb();
            Color pelColor = Color.FromArgb(pelArgb);
            desiredMask[y, x] = pelColor.Equals(colorMask);
          }
        }
      }

      return desiredMask;
    }

    public static Dictionary<AlgorithmPaletteItem, bool[,]> ParseMasks(Image maskSource, AlgorithmPalette palette)
    {
      Dictionary<AlgorithmPaletteItem, bool[,]> maskDictionary = new Dictionary<AlgorithmPaletteItem, bool[,]>();

      foreach (var paletteItem in palette.Values)
      {
        maskDictionary[paletteItem] = GetMask(maskSource, paletteItem.PaletteColor);
      }

      return maskDictionary;
    }
  }
}

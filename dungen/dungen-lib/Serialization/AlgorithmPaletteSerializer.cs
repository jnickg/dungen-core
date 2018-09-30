﻿using DunGen.Algorithm;
using DunGen.TerrainGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace DunGen.Serialization
{
  public class AlgorithmPaletteSerializer
  {
    public AlgorithmPaletteSerializer()
    {
    }

    /// <summary>
    /// Exports this palette to a color palette loadable by Paint.NET
    /// </summary>
    public void ExportForPdn(AlgorithmPalette p, string path, FileMode mode)
    {
      using (StreamWriter writer = new StreamWriter(File.Open(Path.GetFullPath(path), mode)))
      {
        writer.WriteLine("; paint.net Palette file");
        writer.WriteLine("; Generated by Dungen .NET Core");
        writer.WriteLine(";");
        foreach (var paletteItem in p)
        {
          writer.WriteLine(String.Format("; Color for palette item name {0} (type {1}", paletteItem.Key, paletteItem.Value.TypeName));
          writer.WriteLine(String.Format("{0:X}", paletteItem.Value.PaletteColor.ToArgb()));
        }
      }
    }

    public void Save(AlgorithmPalette p, string path, FileMode mode)
    {
      if (mode == FileMode.Truncate || mode == FileMode.Append)
      {
        throw new ArgumentException("Invalid file mode to save a Palette.");
      }

      if (null == p)
        throw new ArgumentNullException("Must pass a palette");
      if (null == path || string.Empty == path)
        throw new ArgumentException("Must pass valid path");

      using (Stream fileStr = new FileStream(path, mode))
      using (Stream memoryStream = new MemoryStream())
      {
        // Serialize dungeon to XML
        DataContractSerializer xml = new DataContractSerializer(typeof(AlgorithmPalette));
        var settings = new XmlWriterSettings
        {
#if DEBUG
          Indent = true,
          IndentChars = "\t",
#endif
        };
        using (var w = XmlWriter.Create(memoryStream, settings))
        {
          xml.WriteObject(w, p);
        }

        // ZIP it up
        memoryStream.Seek(0, SeekOrigin.Begin);
        using (ZipArchive arch = new ZipArchive(fileStr, ZipArchiveMode.Create))
        {
          ZipArchiveEntry paletteEntry = arch.CreateEntry("palette", CompressionLevel.Fastest);
          using (Stream paletteEntryStream = paletteEntry.Open())
          {
            memoryStream.CopyTo(paletteEntryStream);
          }
        }
      }
    }

    public AlgorithmPalette Load(string path)
    {
      AlgorithmPalette loadedPalette = null;

      using (Stream fileStr = new FileStream(path, FileMode.Open))
      using (Stream memoryStream = new MemoryStream())
      {
        // Un ZIP
        using (ZipArchive arch = new ZipArchive(fileStr, ZipArchiveMode.Read))
        {
          ZipArchiveEntry paletteEntry = arch.GetEntry("palette");
          using (Stream paletteEntryStream = paletteEntry.Open())
          {
            paletteEntryStream.CopyTo(memoryStream);
          }
        }
        memoryStream.Seek(0, SeekOrigin.Begin);

        // Serialize dungeon to XML
        DataContractSerializer xml = new DataContractSerializer(typeof(AlgorithmPalette));
        var settings = new XmlReaderSettings();
        using (var w = XmlReader.Create(memoryStream, settings))
        {
          loadedPalette = xml.ReadObject(w) as AlgorithmPalette;
        }
      }

      return loadedPalette;
    }
  }
}

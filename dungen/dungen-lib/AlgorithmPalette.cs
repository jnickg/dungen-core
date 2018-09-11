using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace DunGen.TerrainGen
{
  [CollectionDataContract(Name = "algPalette", KeyName = "name", ValueName = "algPreset")]
  public class AlgorithmPalette : Dictionary<string, AlgorithmPaletteItem>
  {
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
      IAlgorithm alg = AlgorithmPluginManager.GetAlgorithm(this.TypeName);

      if (alg.TakesParameters)
      {
        alg.Parameters = (AlgorithmParams)this.ParamPresets.Clone();
      }

      return alg;
    }
  }

  public class AlgorithmPaletteSerializer
  {
    public AlgorithmPaletteSerializer()
    {
    }

    public void Save(AlgorithmPalette p, string path, FileMode mode)
    {
      if (mode == FileMode.Truncate || mode == FileMode.Append)
      {
        throw new ArgumentException("Invalid file mode to save a Dungeon.");
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

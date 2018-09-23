using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace DunGen.Serialization
{
  public class DungeonSerializer
  {
    public DungeonSerializer()
    {
    }

    public void Save(Dungeon d, string path, FileMode mode)
    {
      if (mode == FileMode.Truncate || mode == FileMode.Append)
      {
        throw new ArgumentException("Invalid file mode to save a Dungeon.");
      }

      if (null == d) throw new ArgumentNullException("Must pass a dungeon");
      if (null == path || string.Empty == path) throw new ArgumentException("Must pass valid path");

      using (Stream fileStr = new FileStream(path, mode))
      using (Stream memoryStream = new MemoryStream())
      {
        // Serialize dungeon to XML
        DataContractSerializer xml = new DataContractSerializer(typeof(Dungeon));
        var settings = new XmlWriterSettings
        {
#if DEBUG
          Indent = true,
          IndentChars = "\t",
#endif
        };
        using (var w = XmlWriter.Create(memoryStream, settings))
        {
          xml.WriteObject(w, d);
        }

        // ZIP it up
        memoryStream.Seek(0, SeekOrigin.Begin);
        using (ZipArchive arch = new ZipArchive(fileStr, ZipArchiveMode.Create))
        {
          ZipArchiveEntry dungeonEntry = arch.CreateEntry("dungeon", CompressionLevel.Fastest);
          using (Stream dungeonEntryStream = dungeonEntry.Open())
          {
            memoryStream.CopyTo(dungeonEntryStream);
          }
        }
      }
    }

    public Dungeon Load(string path)
    {
      Dungeon loadedDungeon = null;

      using (Stream fileStr = new FileStream(path, FileMode.Open))
      using (Stream memoryStream = new MemoryStream())
      {
        // Un ZIP
        using (ZipArchive arch = new ZipArchive(fileStr, ZipArchiveMode.Read))
        {
          ZipArchiveEntry dungeonEntry = arch.GetEntry("dungeon");
          using (Stream dungeonEntryStream = dungeonEntry.Open())
          {
            dungeonEntryStream.CopyTo(memoryStream);
          }
        }
        memoryStream.Seek(0, SeekOrigin.Begin);

        // Serialize dungeon to XML
        DataContractSerializer xml = new DataContractSerializer(typeof(Dungeon));
        var settings = new XmlReaderSettings();
        using (var w = XmlReader.Create(memoryStream, settings))
        {
          loadedDungeon = xml.ReadObject(w) as Dungeon;
        }
      }

      return loadedDungeon;
    }
  }
}

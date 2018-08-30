using Microsoft.VisualStudio.TestTools.UnitTesting;
using DunGen;
using DunGen.Lib;
using DunGen.TerrainGen;
using System.Collections.Generic;
using System;
using DunGen.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;

namespace DunGen.Lib.Test
{
  [TestClass]
  public class TestDungeon
  {
    [TestMethod]
    public void SaveAndLoad()
    {
      Random r = new Random(1337);

      int width = 51;
      int height = 51;

      DungeonGenerator generator = new DungeonGenerator();
      generator.Options = new DungeonGenerator.DungeonGeneratorOptions()
      {
        DoReset = true,
        EgressConnections = null,
        Width = width,
        Height = height,
        TerrainGenAlgRuns = new List<AlgorithmRun>
        {
          new AlgorithmRun()
          {
            Alg = new MonteCarloRoomCarver()
            {
              GroupForDebug = false,
              WallStyle = TerrainGenAlgorithmBase.WallFormationStyle.Boundaries,
              RoomWidthMin = 4,
              RoomWidthMax = 10,
              RoomHeightMin = 4,
              RoomHeightMax = 10,
              Attempts = 500,
              TargetRoomCount = 6
            },
            Context = new AlgorithmContextBase()
            {
              R = r
            }
          },
          new AlgorithmRun()
          {
            Alg = new RecursiveBacktracker()
            {
              TilesAsWalls = true,
              BorderPadding = 0,
              Momentum = 0.25,
              ExistingDataStrategy = RecursiveBacktracker.OpenTilesStrategy.ConnectToRooms,
              WallStyle = TerrainGenAlgorithmBase.WallFormationStyle.Boundaries
            },
            Context = new AlgorithmContextBase()
            {
              R = r
            }
          },
        },
      };

      Dungeon d = generator.Generate();

      List<Tile> testTiles = new List<Tile>();

      string tmpFilePath = System.IO.Path.GetTempFileName();

      DungeonSerializer serializer = new DungeonSerializer();
      serializer.Save(d, tmpFilePath, FileMode.Create);

      Assert.IsTrue(File.Exists(tmpFilePath));

      Dungeon d2;
      d2 = serializer.Load(tmpFilePath);

      Assert.IsNotNull(d2);

      Assert.IsTrue(d.Tiles.Width == d2.Tiles.Width &&
                    d.Tiles.Height == d2.Tiles.Height);

      foreach (Tile t in d.Tiles.Tiles_Set)
      {
        Assert.IsTrue(t.Physics == d2.Tiles[t.Location.Y, t.Location.X].Physics);
      }

      // TODO test more than just physics... when there is more to test.

      File.Delete(tmpFilePath);
    }
  }
}

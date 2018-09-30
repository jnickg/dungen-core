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
using System.Linq;
using DunGen.Generator;
using DunGen.Algorithm;
using DunGen.Serialization;

namespace DunGen.Lib.Test
{
  [TestClass]
  public class TestDungeon
  {
    private DungeonGenerator GetTestDungeonGenerator()
    {
      AlgorithmRandom r = new AlgorithmRandom(1337);

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
              WallStrategy = TerrainGenAlgorithmBase.WallFormation.Boundaries,
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
              BorderPadding = 0,
              Momentum = 0.25,
              OpenTilesStrategy = RecursiveBacktracker.OpenTilesHandling.ConnectToRooms,
              WallStrategy = TerrainGenAlgorithmBase.WallFormation.Boundaries
            },
            Context = new AlgorithmContextBase()
            {
              R = r
            }
          },
        },
      };
      return generator;
    }

    [TestMethod]
    public void SaveAndLoad()
    {
      DungeonGenerator generator = GetTestDungeonGenerator();

      Dungeon d = generator.Generate();

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

    [TestMethod]
    public void AlgorithmRunInfo_ExactReproduction()
    {
      DungeonGenerator g1 = GetTestDungeonGenerator();

      Dungeon d1 = g1.Generate();

      DungeonGenerator g2 = new DungeonGenerator()
      {
        Options = new DungeonGenerator.DungeonGeneratorOptions()
        {
          DoReset = true,
          Height = d1.Tiles.Height,
          Width = d1.Tiles.Width,
          // Here's what we're actually testing: that reconstructing the run gives exact results
          TerrainGenAlgRuns = new List<AlgorithmRun>(d1.Runs.Select(r => r.ReconstructRun())),
        }
      };

      Dungeon d2 = g2.Generate();

      // AHHH TODO this is failing!
      foreach (Tile t in d1.Tiles.Tiles_Set)
      {
        Assert.IsTrue(t.Physics == d2.Tiles[t.Location.Y, t.Location.X].Physics);
      }
    }
  }
}

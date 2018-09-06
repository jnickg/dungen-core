using DunGen;
using DunGen.Rendering;
using DunGen.TerrainGen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace dungen_test_cli
{
  class Program
  {
    static int imgcounter = 0;
    static void RenderToImage(DungeonTiles d)
    {
      System.Threading.Interlocked.Increment(ref imgcounter);
      DungeonTileRenderer renderer = new DungeonTileRenderer();
      using (Image renderedDungeon = renderer.Render(d))
      {
        renderedDungeon.Save(String.Format("dungeon_{0}.bmp", imgcounter), ImageFormat.Bmp);
      }
    }

    static void Main(string[] args)
    {
      // Generate Dungeon
      int width = 50,
          height = 50;

      // Generate a mask. But not a very good one.
      bool[,] algMask = new bool[height, width];
      Random r = new Random();
      for (int y = 0; y < algMask.GetLength(0); ++y)
      {
        for (int x = 0; x < algMask.GetLength(1); ++x)
        {
          //algMask[y, x] = r.NextDouble() < 0.60d;
          algMask[y, x] = true;
        }
      }

      bool debugSettings = false;
      bool groupDebug = debugSettings;
#if DEBUG
      debugSettings = true;
      //groupDebug = true;
#endif

      DungeonGenerator generator = new DungeonGenerator();
      generator.WorkingDungeon = new Dungeon()
      {
        Tiles = new DungeonTiles(width, height)
      };
      generator.Options = new DungeonGenerator.DungeonGeneratorOptions()
      {
        DoReset = false,
        EgressConnections = null,
        Width = width,
        Height = height,
        TerrainGenCallbacks = debugSettings ? new List<Action<DungeonTiles>>() { d => RenderToImage(d) } : null,
        TerrainGenAlgRuns = new List<AlgorithmRun>
        {
          new AlgorithmRun()
          {
            Alg = new MonteCarloRoomCarver()
            {
              GroupForDebug = debugSettings,
              WallStyle = TerrainGenAlgorithmBase.WallFormationStyle.Boundaries,
              RoomWidthMin = 4,
              RoomWidthMax = 10,
              RoomHeightMin = 4,
              RoomHeightMax = 10,
              Attempts = 500,
              TargetRoomCount = 15
            },
            Context = new AlgorithmContextBase()
            {
              Mask = algMask,
              R = r
            }
          },
          new AlgorithmRun()
          {
            Alg = new LinearRecursiveDivision()
            {
              GroupForDebug = groupDebug,
              GroupRooms = true,
              WallStyle = TerrainGenAlgorithmBase.WallFormationStyle.Boundaries,
              BuildStrategy = LinearRecursiveDivision.ExistingDataHandling.Avoid,
              RoomSize = 16,
              Variability = 0.0
            },
            Context = new AlgorithmContextBase()
            {
              Mask = algMask,
              R = r
            }
          },
          new AlgorithmRun()
          {
            Alg = new BlobRecursiveDivision()
            {
              GroupForDebug = groupDebug,
              GroupRooms = true,
              WallStyle = TerrainGenAlgorithmBase.WallFormationStyle.Boundaries,
              RoomSize = 20,
              GapCount = 10,
              MaxGapProportion = 0.01
            },
            Context = new AlgorithmContextBase()
            {
              Mask = algMask,
              R = r
            }
          },
          new AlgorithmRun()
          {
            Alg = new RecursiveBacktracker()
            {
              BorderPadding = 0,
              Momentum = 0.25,
              ExistingDataStrategy = RecursiveBacktracker.OpenTilesStrategy.ConnectToRooms,
              WallStyle = TerrainGenAlgorithmBase.WallFormationStyle.Tiles
            },
            Context = new AlgorithmContextBase()
            {
              Mask = algMask,
              R = r
            }
          },
          new AlgorithmRun()
          {
            Alg = new DeadEndFiller()
            {
              FillPasses = 1
            },
            Context = new AlgorithmContextBase()
            {
              Mask = algMask,
              R = r
            }
          },
        },
      };

      Dungeon dungeon = generator.Generate();

      RenderToImage(dungeon.Tiles);

      foreach (var run in generator.Options.TerrainGenAlgRuns)
      {
        Console.WriteLine("Algorithm {0}", run.Alg.Name);
        foreach (var param in run.Alg.Parameters.List)
        {
          Console.WriteLine("   {0,-10} used for {1,-10} param \'{2,-15}\'  ({3})", param.Value, param.Category, param.Name, param.Description);
        }
      }
      Console.ReadKey();
    }
  }
}

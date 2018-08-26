﻿using DunGen;
using DunGen.Rendering;
using DunGen.TerrainGen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace dungen_cli
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
      int width = 51,
          height = 51;

      // Generate a mask. But not a very good one.
      bool[,] algMask = new bool[height, width];
      Random maskGen = new Random();
      for (int y = 0; y < algMask.GetLength(0); ++y)
      {
        for (int x = 0; x < algMask.GetLength(1); ++x)
        {
          //algMask[y, x] = maskGen.NextDouble() < 0.60d;
          algMask[y, x] = true;
        }
      }

      bool debugSettings = false;
#if DEBUG
      debugSettings = true;
#endif

      DungeonGenerator generator = new DungeonGenerator();
      generator.Options = new DungeonGenerator.DungeonGeneratorOptions()
      {
        DoReset = false,
        EgressConnections = null,
        Width = width,
        Height = height,
        TerrainGenCallbacks = debugSettings ? new List<Action<DungeonTiles>>() { d => RenderToImage(d) } : null,
        TerrainGenAlgs = new Dictionary<ITerrainGenAlgorithm, bool[,]>()
        {
          {
            new MonteCarloRoomCarver()
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
            algMask
          },
          //{
          //  new LinearRecursiveDivision()
          //  {
          //    GroupForDebug = debugSettings,
          //    WallStyle = TerrainGenAlgorithmBase.WallFormationStyle.Boundaries,
          //    BuildStrategy = LinearRecursiveDivision.ExistingDataHandling.Avoid,
          //    RoomSize = 1,
          //    Variability = 0.5
          //  },
          //  algMask
          //},

          {
            new RecursiveBacktracker()
            {
              TilesAsWalls = true,
              BorderPadding = 0,
              Momentum = 0.25,
              ExistingDataStrategy = RecursiveBacktracker.OpenTilesStrategy.ConnectToRooms,
              WallStyle = TerrainGenAlgorithmBase.WallFormationStyle.Boundaries
            },
            algMask
          },
          {
            new DeadEndFiller()
            {
              FillPasses = 20
            },
            algMask
          }
        },
      };

      Dungeon dungeon = generator.Generate();

      RenderToImage(dungeon.Tiles);

      foreach(var alg in dungeon.Algorithms.Keys)
      {
        Console.WriteLine("Algorithm {0}", alg.Name);
        foreach(var param in alg.Parameters.Parameters)
        {
          Console.WriteLine("   {0,-10} used for {1,-10} param \'{2,-15}\'  ({3})", param.Value, param.Category, param.Name, param.Description);
        }
      }
      Console.ReadKey();
    }
  }
}

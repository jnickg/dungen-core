using DunGen;
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
    static void Main(string[] args)
    {
      // Generate Dungeon
      int width  = 75,
          height = 75;

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

      DungeonGenerator generator = new DungeonGenerator();
      generator.Options = new DungeonGenerator.DungeonGeneratorOptions()
      {
        DoReset = false,
        EgressConnections = null,
        Width = width,
        Height = height,
        TerrainGenAlgs = new Dictionary<ITerrainGenAlgorithm, bool[,]>()
        {
          {
            new MonteCarloRoomCarver()
            {
              RoomWidthMin = 7,
              RoomWidthMax = 13,
              RoomHeightMin = 5,
              RoomHeightMax = 9,
              Attempts = 1000,
              TargetRoomCount = 15
            },
            algMask
          },
          {
            new RecursiveBacktracker()
            {
              TilesAsWalls = true,
              BorderPadding = 0,
              Momentum = 0.45,
              ExistingDataStrategy = RecursiveBacktracker.OpenTilesStrategy.ConnectToRooms
            },
            algMask
          },
          {
            new DeadEndFiller()
            {
              FillPasses = 100
            },
            algMask
          }
        }
      };

      Dungeon dungeon = generator.Generate();

      DungeonTileRenderer renderer = new DungeonTileRenderer();
      Image renderedDungeon = renderer.Render(dungeon);
      renderedDungeon.Save("dungeon.bmp", ImageFormat.Bmp);

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

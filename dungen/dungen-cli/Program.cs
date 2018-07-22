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
      int width  = 151,
          height = 151;

      // Whatever algorithm needs to be tested
      ITerrainGenAlgorithm alg = new RecursiveBacktracker()
      {
        BorderPadding = 1,
      };

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
        TerrainGenAlgs = new Dictionary<ITerrainGenAlgorithm, bool[,]>() { { alg, algMask } },
        DoReset = false,
        EgressConnections = null,
        Width = width,
        Height = height
      };

      Dungeon dungeon = generator.Generate();

      DungeonTileRenderer renderer = new DungeonTileRenderer();
      Image renderedDungeon = renderer.Render(dungeon);
      renderedDungeon.Save("dungeon.bmp", ImageFormat.Bmp);
    }
  }
}

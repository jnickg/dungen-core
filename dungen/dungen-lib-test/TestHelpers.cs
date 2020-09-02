using DunGen.Algorithm;
using DunGen.Generator;
using DunGen.TerrainGen;
using DunGen.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using DunGen.Infestation;

namespace DunGen.Lib.Test
{
  internal static class TestHelpers
  {
    public const string baseConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=%SAMPLESDIR%\onions_and_flagons\onions_and_flagons.mdf;Integrated Security=True";
    public const int testLibraryId = 1;

    public static Library GetTestLibrary()
    {
      var samplesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "libraries");
      string cxnStr = baseConnectionString.Replace(@"%SAMPLESDIR%", samplesDir);
      InfestationLibrarySqlSerializer libGetter = new InfestationLibrarySqlSerializer(cxnStr);
      return libGetter.GetLibrary(testLibraryId);
    }


    public static DungeonGenerator GetTestDungeonGenerator()
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
        InfestationLibrary = TestHelpers.GetTestLibrary(),
        AlgRuns = new List<AlgorithmRun>
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
          new AlgorithmRun()
          {
            Alg = new BasicInfester(),
            Context = new AlgorithmContextBase()
            {
              R = r
            }
          }
        },
      };
      return generator;
    }

    public static Dungeon GenerateWith(IAlgorithm alg)
    {
      return DungeonGenerator.Generate(new DungeonGenerator.DungeonGeneratorOptions()
      {
        DoReset = true,
        Height = 50,
        Width = 50,
        InfestationLibrary = TestHelpers.GetTestLibrary(),
        AlgRuns = new List<AlgorithmRun>()
        {
          new AlgorithmRun()
          {
            Alg = alg
          }
        }
      });
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using DunGen.Algorithm;

namespace DunGen.TerrainGen
{
  public class MazeWithRooms : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior => TerrainModBehavior.Clobber;

    public override TerrainGenStyle Style => TerrainGenStyle.Bldg;

    [AlgorithmAlgorithmParameterInfo(
      Description = "Run before maze generator",
      AlgorithmBaseType = typeof(ITerrainGenAlgorithm),
      Default = typeof(MonteCarloRoomCarver))]
    public ITerrainGenAlgorithm RoomBuilder { get; set; }

    [AlgorithmAlgorithmParameterInfo(
      Description = "Run before dead-end cleaner",
      AlgorithmBaseType = typeof(ITerrainGenAlgorithm),
      Default = typeof(RecursiveBacktracker))]
    public ITerrainGenAlgorithm MazeCarver { get; set; }

    [AlgorithmAlgorithmParameterInfo(
      Description = "Removes dead ends from algorithm",
      AlgorithmBaseType = typeof(ITerrainGenAlgorithm),
      Default = typeof(NopTerrainGen))]
    public ITerrainGenAlgorithm DeadEndFiller { get; set; }

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      d.SetAllToo(Tile.MoveType.Wall, mask);
      RoomBuilder?.Run(d, mask, r);
      MazeCarver?.Run(d, mask, r);
      DeadEndFiller?.Run(d, mask, r);
    }
  }
}

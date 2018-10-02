using System;
using System.Collections.Generic;
using System.Text;
using DunGen.Algorithm;

namespace DunGen.TerrainGen
{
  /// <summary>
  /// An algorithm taking inter-changeable strategies for generating rooms,
  /// creating a maze, and removing egress. User may customize the algorithm's
  /// constituent strategies (i.e. algorithms) or replace them altogether.
  /// </summary>
  public class MazeWithRooms : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior => TerrainModBehavior.Clobber;
    public override TerrainGenStyle Style => TerrainGenStyle.Bldg;

    [AlgorithmParameter(
      Description = "Run before maze generator",
      AlgorithmBaseType = typeof(ITerrainGenAlgorithm),
      DefaultType = typeof(MonteCarloRoomCarver))]
    public ITerrainGenAlgorithm RoomBuilder { get; set; }

    [AlgorithmParameter(
      Description = "Run before dead-end cleaner",
      AlgorithmBaseType = typeof(ITerrainGenAlgorithm),
      DefaultType = typeof(RecursiveBacktracker))]
    public ITerrainGenAlgorithm MazeCarver { get; set; }

    [AlgorithmParameter(
      Description = "Removes dead ends from algorithm",
      AlgorithmBaseType = typeof(ITerrainGenAlgorithm),
      DefaultType = typeof(NopTerrainGen))]
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

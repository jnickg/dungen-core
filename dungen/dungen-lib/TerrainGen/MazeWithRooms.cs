using System;
using System.Collections.Generic;
using System.Text;
using DunGen.Algorithm;
using DunGen.Tiles;

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

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      context.D.Tiles.SetAllToo(Tile.MoveType.Wall, context.Mask);

      RoomBuilder?.Run(context);
      MazeCarver?.Run(context);
      DeadEndFiller?.Run(context);
    }
  }
}

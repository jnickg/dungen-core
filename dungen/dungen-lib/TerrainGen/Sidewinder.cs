using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.TerrainGen
{
  public class Sidewinder : TerrainGenAlgorithmBase
  {
    public override TerrainModBehavior Behavior => TerrainModBehavior.Carve;
    public override TerrainGenStyle Style => TerrainGenStyle.Bldg_Halls;

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      // Based on http://weblog.jamisbuck.org/2011/2/3/maze-generation-sidewinder-algorithm
      switch (this.WallStrategy)
      {
        case WallFormation.Tiles:
          this._runAlgorithm_tiles(context);
          break;
        case WallFormation.Boundaries:
          this._runAlgorithm_boundaries(context);
          break;
        default:
          throw new NotImplementedException("Unsupported WallStrategy value");
      }
    }

    private void _runAlgorithm_boundaries(IAlgorithmContext context)
    {
      Random r = context.R;
      DungeonTiles workingTiles = context.D.Tiles;
      bool[,] algMask = context.Mask;

      for (int y = 0; y < workingTiles.Height; ++y)
      {
        int runStart = 0;
        for (int x = 0; x < workingTiles.Width; ++x)
        {
          if (y > 0 && (x + 1 == workingTiles.Width || r.Next(2) == 0))
          {
            int x_carveNorth = runStart + r.Next(x - runStart + 1);
            workingTiles[y, x_carveNorth].Physics = workingTiles[y, x_carveNorth].Physics.OpenUp(Tile.MoveType.Open_NORTH);
            workingTiles[y - 1, x_carveNorth].Physics = workingTiles[y - 1, x_carveNorth].Physics.OpenUp(Tile.MoveType.Open_SOUTH);
            runStart = x + 1;
          }
          else if (x + 1 < workingTiles.Width)
          {
            workingTiles[y, x].Physics = workingTiles[y, x].Physics.OpenUp(Tile.MoveType.Open_EAST);
            workingTiles[y, x + 1].Physics = workingTiles[y, x + 1].Physics.OpenUp(Tile.MoveType.Open_WEST);
          }
          this.RunCallbacks(context);
        }
      }
    }

    private void _runAlgorithm_tiles(IAlgorithmContext context)
    {
      Random r = context.R;
      DungeonTiles workingTiles = context.D.Tiles;
      bool[,] algMask = context.Mask;

      for (int y = 0; y < workingTiles.Height; y += 2)
      {
        int runStart = 0;
        for (int x = 0; x < workingTiles.Width; x += 2)
        {
          if (y > 1 && (x + 1 == workingTiles.Width || r.Next(2) == 0))
          {
            // Force even since we're using whole tiles as walls
            int x_carveNorth = runStart + (r.Next(x - runStart + 2) & ~0x1);
            workingTiles[y, x_carveNorth].Physics = workingTiles[y, x_carveNorth].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            workingTiles[y - 1, x_carveNorth].Physics = workingTiles[y - 1, x_carveNorth].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            runStart = x + 2;
          }
          else if (x + 2 < workingTiles.Width)
          {
            workingTiles[y, x].Physics = workingTiles[y, x].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            workingTiles[y, x + 1].Physics = workingTiles[y, x + 1].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            workingTiles[y, x + 2].Physics = workingTiles[y, x + 2].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
          }
          this.RunCallbacks(context);
        }
      }
    }
  }
}

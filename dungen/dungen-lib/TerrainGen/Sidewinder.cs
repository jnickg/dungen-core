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

    protected const string Orientation_Help =
      "The side of the map that should have a completely-opened face.";

    [SelectionParameter(
      Description = Orientation_Help,
      SelectionType = typeof(Direction),
      Default = Direction.North)]
    public Direction Orientation { get; set; }

    protected const string BreakChance_Help =
      "0.0-1.0 percentange chance of breaking from a run and connecting" +
      " to the previous row/column.";

    [DecimalParameter(
      Description = BreakChance_Help,
      Maximum = 1.0,
      Minimum = 0.0,
      Default = 0.33)]
    public double BreakChance { get; set; }

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
          throw new NotImplementedException("Unsupported WallFormation value");
      }
    }

    private void _runAlgorithm_boundaries(IAlgorithmContext context)
    {
      Random r = context.R;
      DungeonTiles workingTiles = context.D.Tiles;
      bool[,] algMask = context.Mask;

      switch (this.Orientation)
      {
        case Direction.North:
          for (int y = 0; y < workingTiles.Height; ++y)
          {
            int runStart = 0;
            for (int x = 0; x < workingTiles.Width; ++x)
            {
              if (!algMask[y, x]) continue;

              if (y > 0 && (x + 1 == workingTiles.Width || r.NextDouble() < this.BreakChance))
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
          break;
        case Direction.East:
          for (int x = workingTiles.Width - 1; x >= 0; --x)
          {
            int runStart = 0;
            for (int y = 0; y < workingTiles.Height; ++y)
            {
              if (!algMask[y, x]) continue;

              if (x < workingTiles.Width - 1 && (y + 1 == workingTiles.Height || r.NextDouble() < this.BreakChance))
              {
                int y_carveWest = runStart + r.Next(y - runStart + 1);
                workingTiles[y_carveWest, x].Physics = workingTiles[y_carveWest, x].Physics.OpenUp(Tile.MoveType.Open_EAST);
                workingTiles[y_carveWest, x + 1].Physics = workingTiles[y_carveWest, x + 1].Physics.OpenUp(Tile.MoveType.Open_WEST);
                runStart = y + 1;
              }
              else if (y + 1 < workingTiles.Height)
              {
                workingTiles[y, x].Physics = workingTiles[y, x].Physics.OpenUp(Tile.MoveType.Open_SOUTH);
                workingTiles[y + 1, x].Physics = workingTiles[y + 1, x].Physics.OpenUp(Tile.MoveType.Open_NORTH);
              }
              this.RunCallbacks(context);
            }
          }
          break;
        case Direction.South:
          for (int y = workingTiles.Height - 1; y >= 0; --y)
          {
            int runStart = 0;
            for (int x = 0; x < workingTiles.Width; ++x)
            {
              if (!algMask[y, x]) continue;

              if (y < workingTiles.Height - 1 && (x + 1 == workingTiles.Width || r.NextDouble() < this.BreakChance))
              {
                int x_carveSouth = runStart + r.Next(x - runStart + 1);
                workingTiles[y, x_carveSouth].Physics = workingTiles[y, x_carveSouth].Physics.OpenUp(Tile.MoveType.Open_SOUTH);
                workingTiles[y + 1, x_carveSouth].Physics = workingTiles[y + 1, x_carveSouth].Physics.OpenUp(Tile.MoveType.Open_NORTH);
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
          break;
        case Direction.West:
          for (int x = 0; x < workingTiles.Width; ++x)
          {
            int runStart = 0;
            for (int y = 0; y < workingTiles.Height; ++y)
            {
              if (!algMask[y, x]) continue;

              if (x > 0 && (y + 1 == workingTiles.Height|| r.NextDouble() < this.BreakChance))
              {
                int y_carveWest = runStart + r.Next(y - runStart + 1);
                workingTiles[y_carveWest, x].Physics = workingTiles[y_carveWest, x].Physics.OpenUp(Tile.MoveType.Open_WEST);
                workingTiles[y_carveWest, x - 1].Physics = workingTiles[y_carveWest, x - 1].Physics.OpenUp(Tile.MoveType.Open_EAST);
                runStart = y + 1;
              }
              else if (y + 1 < workingTiles.Height)
              {
                workingTiles[y, x].Physics = workingTiles[y, x].Physics.OpenUp(Tile.MoveType.Open_SOUTH);
                workingTiles[y + 1, x].Physics = workingTiles[y + 1, x].Physics.OpenUp(Tile.MoveType.Open_NORTH);
              }
              this.RunCallbacks(context);
            }
          }
          break;
        default:
          throw new NotImplementedException("Unsupported Orientation value");
      }
    }

    private void _runAlgorithm_tiles(IAlgorithmContext context)
    {
      Random r = context.R;
      DungeonTiles workingTiles = context.D.Tiles;
      bool[,] algMask = context.Mask;

      switch (this.Orientation)
      {
        case Direction.North:
          for (int y = 0; y < workingTiles.Height; y += 2)
          {
            int runStart = 0;
            for (int x = 0; x < workingTiles.Width; x += 2)
            {
              if (y > 1 && (x + 1 == workingTiles.Width || r.NextDouble() < this.BreakChance))
              {
                if (!algMask[y, x]) continue;

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
          break;
        case Direction.East:
          // TODO
          break;
        case Direction.South:
          // TODO
          break;
        case Direction.West:
          // TODO
          break;
        default:
          throw new NotImplementedException("Unsupported Orientation value");
      }


    }
  }
}

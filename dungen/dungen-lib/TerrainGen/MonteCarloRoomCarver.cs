using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public class MonteCarloRoomCarver : TerrainGenAlgorithmBase
  {
    [IntegerParameter(
      Description = "Minimum width of the generated rooms",
      Default = 5,
      Minimum = 2,
      Maximum = int.MaxValue)]
    public int RoomWidthMin { get; set; }

    [IntegerParameter(
      Description = "Maximum width of the generated rooms",
      Default = 5,
      Minimum = 2,
      Maximum = int.MaxValue)]
    public int RoomWidthMax { get; set; }

    [IntegerParameter(
      Description = "Minimum height of the generated rooms",
      Default = 5,
      Minimum = 2,
      Maximum = int.MaxValue)]
    public int RoomHeightMin { get; set; }

    [IntegerParameter(
      Description = "Maximum height of the generated rooms",
      Default = 5,
      Minimum = 2,
      Maximum = int.MaxValue)]
    public int RoomHeightMax { get; set; }

    [BooleanParameter(
      Description = "If true, room carver will avoid already-open tiles when attempting to build rooms",
      Default = true)]
    public bool AvoidOpen { get; set; }

    [BooleanParameter(
      Description = "If true, room carver will turn all tiles in the mask to walls, before building rooms",
      Default = false)]
    public bool ClearArea { get; set; }

    [IntegerParameter(
      Description = "The number of random room-creation attempts to make. Will short-circuit if it reaches the desired number of rooms first",
      Default = 500,
      Minimum = 0,
      Maximum = int.MaxValue)]
    public int Attempts { get; set; }

    // TODO make room width height honor this too
    [IntegerParameter(
      Description = "If using tiles as walls, how many tiles to pad the outside of the algorithm's mask",
      Default = 1,
      Minimum = 0,
      Maximum = int.MaxValue)]
    public int BorderPadding { get; set; }

    [IntegerParameter(
      Description = "The target number of rooms to generate. Will short-circuit if it runs out of attempts before reaching this value",
      Default = 15,
      Minimum = 1,
      Maximum = int.MaxValue)]
    public int TargetRoomCount { get; set; }

    public override TerrainModBehavior Behavior => ClearArea ? TerrainModBehavior.Clobber : TerrainModBehavior.Carve;

    public override TerrainGenStyle Style => TerrainGenStyle.Bldg_Rooms;

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      DungeonTiles workingTiles = context.D.Tiles;
      bool[,] algMask = context.Mask;

      if (this.ClearArea)
      {
        workingTiles.SetAllToo(Tile.MoveType.Wall, algMask);
      }

      bool[,] isExplored = (bool[,])algMask.Clone();
      for (int y = 0; y < workingTiles.Height; ++y)
      {
        for (int x = 0; x < workingTiles.Width; ++x)
        {
          isExplored[y, x] = !algMask[y, x];
        }
      }

      // If appropriate, mask out already-opened tiles
      if (!this.ClearArea && this.AvoidOpen)
      {
        for (int y = 0; y < workingTiles.Height; ++y)
        {
          for (int x = 0; x < workingTiles.Width; ++x)
          {
            if (algMask[y, x] && workingTiles[y, x].Physics != Tile.MoveType.Wall)
            {
              isExplored[y, x] = true;
            }
          }
        }
      }

      // Create a pool of origins
      List<Point> unmaskedPoints = new List<Point>();
      for (int y = 0; y < isExplored.GetLength(0); ++y)
      {
        for (int x = 0; x < isExplored.GetLength(1); ++x)
        {
          // If it's not masked out, add it to the pool of potential
          // starting points
          if (!isExplored[y, x]) unmaskedPoints.Add(new Point(x, y));
        }
      }

      // Only odd coordinates are valid starts due to limitations of algorithm below
      Predicate<Point> oddPoints = p => p.X % 2 != 0 && p.Y % 2 != 0;
      Predicate<Point> paddedWithinBorder = p => p.X > BorderPadding && p.Y > BorderPadding && p.X < isExplored.GetLength(1) - BorderPadding && p.Y < isExplored.GetLength(0) - BorderPadding;
      List<Point> originPool = unmaskedPoints.FindAll(oddPoints).FindAll(paddedWithinBorder);

      if (originPool.Count == 0) return;

      int currentRooms = 0;
      int currentAttempt = 0;
      
      while (++currentAttempt <= this.Attempts && currentRooms < this.TargetRoomCount)
      {
        int originIdx = context.R.Next(originPool.Count);
        Point nextOrigin = originPool[originIdx];

        int y = nextOrigin.Y;
        int x = nextOrigin.X;
        int w = context.R.Next(this.RoomWidthMin, this.RoomWidthMax) - 1 | 0x1;
        int h = context.R.Next(this.RoomHeightMin, this.RoomHeightMax) - 1 | 0x1;

        if (!workingTiles.TileIsValid(x, y) || !workingTiles.TileIsValid(x, y + h) || !workingTiles.TileIsValid(x + w, y) || !workingTiles.TileIsValid(x + w, y + h)) continue;

        bool overlapsOrAdjacent = false;
        for (int nuY = y - 1; nuY < y + h + 1; ++nuY)
        {
          for (int nuX = x - 1; nuX < x + w + 1; ++nuX)
          {
            if (!workingTiles.TileIsValid(nuX, nuY) || isExplored[nuY, nuX])
            {
              overlapsOrAdjacent = true;
              break;
            }
          }
          if (overlapsOrAdjacent) break;
        }
        if (overlapsOrAdjacent) continue; // Failed attempt

        ISet<Tile> newRoom = new HashSet<Tile>();
        for (int nuY = y; nuY < y+h; ++nuY)
        {
          for (int nuX = x; nuX < x+w; ++nuX)
          {
            isExplored[nuY, nuX] = true;
            workingTiles[nuY, nuX].Physics = workingTiles[nuY, nuX].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            originPool.Remove(workingTiles[nuY, nuX].Location);
            newRoom.Add(workingTiles[nuY, nuX]);
          }
        }
        // Close off boundaries if appropriate
        if (this.WallStrategy == WallFormation.Boundaries)
        {
          for (int nuY = y; nuY < y + h; ++nuY)
          {
            for (int nuX = x; nuX < x + w; ++nuX)
            {
              if (nuY == y) workingTiles[nuY, nuX].Physics = workingTiles[nuY, nuX].Physics.CloseOff(Tile.MoveType.Open_NORTH);
              if (nuX == x) workingTiles[nuY, nuX].Physics = workingTiles[nuY, nuX].Physics.CloseOff(Tile.MoveType.Open_WEST);
              if (nuY == y+h-1) workingTiles[nuY, nuX].Physics = workingTiles[nuY, nuX].Physics.CloseOff(Tile.MoveType.Open_SOUTH);
              if (nuX == x+w-1) workingTiles[nuY, nuX].Physics = workingTiles[nuY, nuX].Physics.CloseOff(Tile.MoveType.Open_EAST);
            }
          }
        }

        // Rooms should not be orphaned!
        workingTiles.Parent.CreateGroup(newRoom, TileCategory.Room);

        this.RunCallbacks(context);

        ++currentRooms;
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public class MonteCarloRoomCarver : TerrainGenAlgorithmBase
  {
    [IntegerAlgorithmParamInfo(
      "Minimum width of the generated rooms",
      5,
      2,
      int.MaxValue)]
    public int RoomWidthMin { get; set; }

    [IntegerAlgorithmParamInfo(
      "Maximum width of the generated rooms",
      5,
      2,
      int.MaxValue)]
    public int RoomWidthMax { get; set; }

    [IntegerAlgorithmParamInfo(
      "Minimum height of the generated rooms",
      5,
      2,
      int.MaxValue)]
    public int RoomHeightMin { get; set; }

    [IntegerAlgorithmParamInfo(
      "Maximum height of the generated rooms",
      5,
      2,
      int.MaxValue)]
    public int RoomHeightMax { get; set; }

    [BooleanAlgorithmParameterInfo(
      "If true, room carver will avoid already-open tiles when attempting to build rooms",
      true)]
    public bool AvoidOpen { get; set; }

    [BooleanAlgorithmParameterInfo(
      "If true, room carver will turn all tiles in the mask to walls, before building rooms",
      false)]
    public bool ClearArea { get; set; }

    [IntegerAlgorithmParamInfo(
      "The number of random room-creation attempts to make. Will short-circuit if it reaches the desired number of rooms first",
      500,
      0,
      int.MaxValue)]
    public int Attempts { get; set; }

    // TODO make room width height honor this too
    [IntegerAlgorithmParamInfo(
      "If using tiles as walls, how many tiles to pad the outside of the algorithm's mask",
      1,
      0,
      int.MaxValue)]
    public int BorderPadding { get; set; }

    [IntegerAlgorithmParamInfo(
      "The target number of rooms to generate. Will short-circuit if it runs out of attempts before reaching this value",
      15,
      1,
      int.MaxValue)]
    public int TargetRoomCount { get; set; }

    public override TerrainModBehavior Behavior => ClearArea ? TerrainModBehavior.Clobber : TerrainModBehavior.Carve;

    public override TerrainGenStyle Style => TerrainGenStyle.Bldg_Rooms;

    public override void Run(DungeonTiles d, bool[,] mask, Random r)
    {
      if (this.ClearArea)
      {
        d.SetAllToo(Tile.MoveType.Wall, mask);
      }

      if (null == r) r = new Random();

      bool[,] isExplored = (bool[,])mask.Clone();
      for (int y = 0; y < d.Height; ++y)
      {
        for (int x = 0; x < d.Width; ++x)
        {
          isExplored[y, x] = !mask[y, x];
        }
      }

      // If appropriate, mask out already-opened tiles
      if (!this.ClearArea && this.AvoidOpen)
      {
        for (int y = 0; y < d.Height; ++y)
        {
          for (int x = 0; x < d.Width; ++x)
          {
            if (mask[y, x] && d[y, x].Physics != Tile.MoveType.Wall)
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
        int originIdx = r.Next(originPool.Count);
        Point nextOrigin = originPool[originIdx];

        int y = nextOrigin.Y;
        int x = nextOrigin.X;
        int w = r.Next(this.RoomWidthMin, this.RoomWidthMax) - 1 | 0x1;
        int h = r.Next(this.RoomHeightMin, this.RoomHeightMax) - 1 | 0x1;

        if (!d.TileIsValid(x, y) || !d.TileIsValid(x, y + h) || !d.TileIsValid(x + w, y) || !d.TileIsValid(x + w, y + h)) continue;

        bool overlapsOrAdjacent = false;
        for (int nuY = y - 1; nuY < y + h + 1; ++nuY)
        {
          for (int nuX = x - 1; nuX < x + w + 1; ++nuX)
          {
            if (!d.TileIsValid(nuX, nuY) || isExplored[nuY, nuX])
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
            d[nuY, nuX].Physics = d[nuY, nuX].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            originPool.Remove(d[nuY, nuX].Location);
            newRoom.Add(d[nuY, nuX]);
          }
        }
        // Close off boundaries if appropriate
        if (this.WallStyle == WallFormationStyle.Boundaries)
        {
          for (int nuY = y; nuY < y + h; ++nuY)
          {
            for (int nuX = x; nuX < x + w; ++nuX)
            {
              if (nuY == y) d[nuY, nuX].Physics = d[nuY, nuX].Physics.CloseOff(Tile.MoveType.Open_NORTH);
              if (nuX == x) d[nuY, nuX].Physics = d[nuY, nuX].Physics.CloseOff(Tile.MoveType.Open_WEST);
              if (nuY == y+h-1) d[nuY, nuX].Physics = d[nuY, nuX].Physics.CloseOff(Tile.MoveType.Open_SOUTH);
              if (nuX == x+w-1) d[nuY, nuX].Physics = d[nuY, nuX].Physics.CloseOff(Tile.MoveType.Open_EAST);
            }
          }
        }

        // Rooms should not be orphaned!
        d.Categorize(newRoom, DungeonTiles.Category.Room);
        if (this.GroupForDebug) d.CreateGroup(newRoom);

        this.RunCallbacks(d);

        ++currentRooms;
      }
    }
  }
}

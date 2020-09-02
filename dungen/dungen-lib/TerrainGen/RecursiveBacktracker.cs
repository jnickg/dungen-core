using DunGen.Algorithm;
using DunGen.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DunGen.TerrainGen
{
  public class RecursiveBacktracker : TerrainGenAlgorithmBase
  {
    public const string BorderPadding_Help = 
      "If using tiles as walls, how many tiles to pad the outside of the " +
      "algorithm's mask";

    [IntegerParameter(
      Description = BorderPadding_Help,
      Default = 1,
      Minimum = 0,
      Maximum = int.MaxValue)]
    public int BorderPadding { get; set; }

    public const string Momentum_Help =
      "A 0.0 to 1.0 percentage factor of how likely the algorithm is to " +
      "maintain its current direction";

    [DecimalParameter(
      Description = Momentum_Help,
      Default = 0.33,
      Minimum = 0.01,
      Maximum = 0.95,
      Precision = 2)]
    public double Momentum { get; set; }

    [SelectionParameter(
      Description = WallStrategy_Help,
      SelectionType = typeof(WallFormation),
      Default = WallFormation.Tiles)]
    public override WallFormation WallStrategy { get => base.WallStrategy; set => base.WallStrategy = value; }

    public override TerrainModBehavior Behavior
    {
      get => TerrainModBehavior.Carve;
    }

    public override TerrainGenStyle Style
    {
      get => TerrainGenStyle.Bldg_Halls;
    }

    private IAlgorithmContext _ctx { get; set; } = null;

    protected override void _runAlgorithm(IAlgorithmContext context)
    {
      _ctx = context;
      List<bool[,]> masks = context.Mask.SplitByAdjacency();
      foreach (var m in masks)
      {
        this.Generate_Priv(context.D.Tiles, m, context.R);
        // Add the generated tiles to a group
        context.D.CreateGroup(m);
      }
      _ctx = null;
    }

    private void Generate_Priv(DungeonTiles d, bool[,] mask, Random r)
    {
      // Below we assume mask and d are the same dimensions
      if (d.Height != mask.GetLength(0) || d.Width != mask.GetLength(1))
      {
        throw new ArgumentException("Invalid mask");
      }

      if (null == r) r = new Random();

      bool[,] isExplored = new bool[d.Height, d.Width];
      List<Point> unmaskedPoints = new List<Point>();
      for (int y = 0; y < isExplored.GetLength(0); ++y)
      {
        for (int x = 0; x < isExplored.GetLength(1); ++x)
        {
          isExplored[y, x] = false;
          if (this.OpenTilesStrategy == OpenTilesHandling.Avoid &&
              d[y, x].Physics != Tile.MoveType.Wall)
          {
            isExplored[y, x] = true;
          }
          // If it's not masked out, add it to the pool of potential
          // starting points
          if (mask[y, x]) unmaskedPoints.Add(new Point(x, y));
        }
      }

      // Only odd coordinates are valid starts due to limitations of algorithm below
      Predicate<Point> oddPoints = p => p.X % 2 != 0 && p.Y % 2 != 0;
      Predicate<Point> paddedWithinBorder = p => p.X > BorderPadding && p.Y > BorderPadding && p.X < isExplored.GetLength(1) - BorderPadding && p.Y < isExplored.GetLength(0) - BorderPadding;
      List<Point> originPool = unmaskedPoints.FindAll(oddPoints).FindAll(paddedWithinBorder);

      if (originPool.Count == 0) return; // no op -- we can't run algorithm at all

      // Find an origin point from which to start
      Point origin = originPool[r.Next() % originPool.Count];

      // Launch into recursive algorithm
      switch (base.WallStrategy)
      {
        case WallFormation.Tiles:
          this.RecursiveBacktrack_TilesAsWalls(d, origin.X, origin.Y, isExplored, mask, r);
          break;
        case WallFormation.Boundaries:
        this.RecursiveBacktrack_BoundariesAsWalls(d, origin.X, origin.Y, isExplored, mask, r);
          break;
        default:
          throw new NotSupportedException("Unsupported WallFormationStyile value selected");
      }
    }

    private enum Direction
    {
      N = 0,
      E,
      S,
      W
    }

    private Tile.MoveType GetMoveFor(Direction d)
    {
      switch (d)
      {
        case Direction.N:
          return Tile.MoveType.Open_NORTH;
        case Direction.E:
          return Tile.MoveType.Open_EAST;
        case Direction.S:
          return Tile.MoveType.Open_SOUTH;
        case Direction.W:
          return Tile.MoveType.Open_WEST;
        default:
          throw new ArgumentException();
      }
    }

    private struct PointTracking
    {
      public Point ThisPoint { get; set; }
      public List<Direction> UntriedDirections { get; set; }

      public PointTracking(Point whichPoint)
      {
        ThisPoint = whichPoint;
        UntriedDirections = new List<Direction>()
        {
          Direction.N,
          Direction.E,
          Direction.S,
          Direction.W
        };
      }

      public PointTracking(Point whichPoint, List<Direction> untriedDirs)
      {
        ThisPoint = whichPoint;
        UntriedDirections = untriedDirs;
      }
    }

    private void RecursiveBacktrack_BoundariesAsWalls(DungeonTiles d, int startX, int startY, bool[,] explored, bool[,] overallMask, Random r)
    {
      if (null == explored || null == overallMask) return;
      Stack<PointTracking> points = new Stack<PointTracking>();
      Direction lastDirection = Direction.N;

      points.Push(new PointTracking(new Point(startX, startY)));
      while (points.Count > 0)
      {
        PointTracking currentPointTracker = points.Pop();
        Point currentPoint = currentPointTracker.ThisPoint;
        List<Direction> currentUntried = currentPointTracker.UntriedDirections;

        if (currentUntried.Count == 0) continue;

        if (currentPoint.X >= explored.GetLength(1) || currentPoint.Y >= explored.GetLength(0) || currentPoint.X < 0 || currentPoint.Y < 0) continue;

        // Case 1: Current point is not within bounds
        if (!overallMask[currentPoint.Y, currentPoint.X]) continue;

        // Case 2: We are at an unexplored tile. Mark the tile as explored and carve it up
        explored[currentPoint.Y, currentPoint.X] = true;
        d[currentPoint.Y, currentPoint.X].Physics = d[currentPoint.Y, currentPoint.X].Physics.OpenUp(GetMoveFor(lastDirection).GetOpposite());

        // Pick a random direction to explore
        Direction dir = lastDirection;
        // Can use momentum
        if (currentUntried.Contains(lastDirection))
        {
          List<Direction> candidates = new List<Direction>();
          for (int i = 0; i < (int)(Momentum * 100); ++i)
          {
            candidates.Add(lastDirection);
          }
          foreach (Direction nextDir in currentUntried.Where(aDir => aDir != lastDirection))
          {
            for (int i = 0; i < (int)((1.0 - Momentum) * 33.3); ++i)
            {
              candidates.Add(nextDir);
            }
          }
          dir = candidates[r.Next(candidates.Count)];
        }
        else // Last direction already explored; pick randomly
        {
          dir = currentUntried[r.Next(currentUntried.Count)];
        }
        currentUntried.Remove(dir);

        int x2, y2; // Adjacent coordinate
        switch (dir)
        {
          case Direction.N: // up
            x2 = currentPoint.X;
            y2 = currentPoint.Y - 1;
            break;
          case Direction.E: // right
            x2 = currentPoint.X + 1;
            y2 = currentPoint.Y;
            break;
          case Direction.S: // down
            x2 = currentPoint.X;
            y2 = currentPoint.Y + 1;
            break;
          case Direction.W: // left
            x2 = currentPoint.X - 1;
            y2 = currentPoint.Y;
            break;
          default: // Should never happen -- just to make compiler happy
            x2 = currentPoint.X;
            y2 = currentPoint.Y;
            break;
        }
        if (x2 < BorderPadding || y2 < BorderPadding  // Out of bounds
          || x2 >= explored.GetLength(1) - BorderPadding || y2 >= explored.GetLength(0) - BorderPadding // Out of bounds
          || !overallMask[y2, x2] // Not in mask
          || explored[y2, x2] )// Already visited
        {
          points.Push(currentPointTracker);
          continue;
        }

        // Special case: we want to connect to the area due to our strategy, but do NOT want to recurse
        if (d[y2, x2].Physics != Tile.MoveType.Wall &&
          this.OpenTilesStrategy == OpenTilesHandling.ConnectToRooms)
        {
          if (d.Parent.GetCategoriesFor(x2, y2).Contains(TileCategory.Room))
          {
            explored[y2, x2] = true;
            d[currentPoint.Y, currentPoint.X].Physics = d[currentPoint.Y, currentPoint.X].Physics.OpenUp(GetMoveFor(dir));
            d[y2, x2].Physics = d[y2, x2].Physics.OpenUp(GetMoveFor(dir).GetOpposite());
            // That tile's group has been connected, so remove it from dependent groups
            // TODO is this what should really happen here???
            foreach (var group in d.Parent.Groups.Where(grp => grp.Category == TileCategory.Room))
            {
              if (group.Tiles.Contains(x2, y2))
              {
                group.Category = TileCategory.Normal;
              }
            }
          }
          points.Push(currentPointTracker);
          continue;
        }

        // Tiles as walls means we traverse an adjacent (x2,y2) tile,
        // before moving to the "next" tile (x3,y3). So, do that.
        explored[y2, x2] = true;
        d[currentPoint.Y, currentPoint.X].Physics = d[currentPoint.Y, currentPoint.X].Physics.OpenUp(GetMoveFor(dir));
        d[y2, x2].Physics = d[y2, x2].Physics.OpenUp(GetMoveFor(dir).GetOpposite());

        this.RunCallbacks(_ctx);

        lastDirection = dir;
        // R E C U R S E !
        points.Push(currentPointTracker);
        points.Push(new PointTracking(new Point(x2, y2)));

        // Case 3: We have explored this tile and all tried recursing
        // into al of its adjacent tiles. Time to pop back up in the
        // stack, to explore a previous tile's adjacents
        continue;
      }
    }

    private void RecursiveBacktrack_TilesAsWalls(DungeonTiles d, int startX, int startY, bool[,] explored, bool[,] overallMask, Random r)
    {
      if (null == explored || null == overallMask) return;
      Stack<PointTracking> points = new Stack<PointTracking>();
      Direction lastDirection = Direction.N;

      points.Push(new PointTracking(new Point(startX, startY)));
      while (points.Count > 0)
      {
        PointTracking currentPointTracker = points.Pop();
        Point currentPoint = currentPointTracker.ThisPoint;
        List<Direction> currentUntried = currentPointTracker.UntriedDirections;

        if (currentUntried.Count == 0) continue;

        if (currentPoint.X >= explored.GetLength(1) || currentPoint.Y >= explored.GetLength(0) || currentPoint.X < 0 || currentPoint.Y < 0) continue;

        // Case 1: Current point is not within bounds
        if (!overallMask[currentPoint.Y, currentPoint.X]) continue;

        // Case 2: We are at an unexplored tile. Mark the tile as explored and carve it up
        explored[currentPoint.Y, currentPoint.X] = true;
        d[currentPoint.Y, currentPoint.X].Physics = d[currentPoint.Y, currentPoint.X].Physics.OpenUp(Tile.MoveType.Open_HORIZ);

        // Pick a random direction to explore
        Direction dir = lastDirection;
        // Can use momentum
        if (currentUntried.Contains(lastDirection))
        {
          List<Direction> candidates = new List<Direction>();
          for (int i = 0; i < (int)(Momentum * 100); ++i)
          {
            candidates.Add(lastDirection);
          }
          foreach (Direction nextDir in currentUntried.Where(aDir => aDir != lastDirection))
          {
            for (int i = 0; i < (int)((1.0 - Momentum) * 33.3); ++i)
            {
              candidates.Add(nextDir);
            }
          }
          dir = candidates[r.Next(candidates.Count)];
        }
        else // Last direction already explored; pick randomly
        {
          dir = currentUntried[r.Next(currentUntried.Count)];
        }
        currentUntried.Remove(dir);

        // We calculate adjacent and proceeding coordinate so that
        // turns only occur on every other tile, so that entire tiles
        // are used as the walls between hallways
        int x2, y2, x3, y3; // Adjacent and next coordinate
        switch (dir)
        {
          case Direction.N: // up
            x2 = currentPoint.X;
            y2 = currentPoint.Y - 1;
            x3 = currentPoint.X;
            y3 = currentPoint.Y - 2;
            break;
          case Direction.E: // right
            x2 = currentPoint.X + 1;
            y2 = currentPoint.Y;
            x3 = currentPoint.X + 2;
            y3 = currentPoint.Y;
            break;
          case Direction.S: // down
            x2 = currentPoint.X;
            y2 = currentPoint.Y + 1;
            x3 = currentPoint.X;
            y3 = currentPoint.Y + 2;
            break;
          case Direction.W: // left
            x2 = currentPoint.X - 1;
            y2 = currentPoint.Y;
            x3 = currentPoint.X - 2;
            y3 = currentPoint.Y;
            break;
          default: // Should never happen -- just to make compiler happy
            x3 = x2 = currentPoint.X;
            y3 = y2 = currentPoint.Y;
            break;
        }
        if (x2 < BorderPadding || y2 < BorderPadding || x3 < BorderPadding || y3 < BorderPadding  // Out of bounds
          || x2 >= explored.GetLength(1) - BorderPadding || y2 >= explored.GetLength(0) - BorderPadding // Out of bounds
          || x3 >= explored.GetLength(1) - BorderPadding || y3 >= explored.GetLength(0) - BorderPadding// Out of bounds
          || !overallMask[y2, x2] || !overallMask[y3, x3]   // Not in mask
          || explored[y2, x2] || explored[y3, x3])      // Already visited
        {
          points.Push(currentPointTracker);
          continue;
        }

        // Special case: we want to connect to the area due to our strategy, but do NOT want to recurse
        if (d[y3, x3].Physics != Tile.MoveType.Wall &&
          this.OpenTilesStrategy == OpenTilesHandling.ConnectToRooms)
        {
          if (d.Parent.GetCategoriesFor(x3, y3).Contains(TileCategory.Room))
          {
            explored[y2, x2] = true;
            d[y2, x2].Physics = d[y2, x2].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            explored[y3, x3] = true;
            d[y3, x3].Physics = d[y3, x3].Physics.OpenUp(Tile.MoveType.Open_HORIZ);
            // That tile's group has been connected, so remove it from dependent groups
            // TODO is this what should really happen here???
            foreach (var group in d.Parent.Groups.Where(grp => grp.Category == TileCategory.Room))
            {
              if (group.Tiles.Contains(x3, y3))
              {
                group.Category = TileCategory.Normal;
              }
            }
          }
          points.Push(currentPointTracker);
          continue;
        }

        // Tiles as walls means we traverse an adjacent (x2,y2) tile,
        // before moving to the "next" tile (x3,y3). So, do that.
        explored[y2, x2] = true;
        d[y2, x2].Physics = d[y2, x2].Physics.OpenUp(Tile.MoveType.Open_HORIZ);

        this.RunCallbacks(_ctx);

        lastDirection = dir;
        // R E C U R S E !
        points.Push(currentPointTracker);
        points.Push(new PointTracking(new Point(x3, y3)));

        // Case 3: We have explored this tile and all tried recursing
        // into al of its adjacent tiles. Time to pop back up in the
        // stack, to explore a previous tile's adjacents
        continue;
      }
    }
  }
}

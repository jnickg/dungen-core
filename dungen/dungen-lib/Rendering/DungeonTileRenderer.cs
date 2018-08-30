using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DunGen.Rendering
{
  public class DungeonTileRenderer
  {
    public bool DrawBorders { get; set; }
    public bool ShadeGroups { get; set; }
    public int TileSize_Pixels { get; set; }
    public Color Background_Color { get; set; }
    public Pen WallBorder_Pen { get; set; }
    public Pen OpenBorder_Pen { get; set; }
    public Brush WallTile_Brush { get; set; }
    public Brush OpenTile_Brush { get; set; }
    public Brush WallBorder_Brush { get; set; }
    public Brush OpenBorder_Brush { get; set; }

    public DungeonTileRenderer()
    {
      this.DrawBorders = true;
      this.ShadeGroups = true;
      this.TileSize_Pixels = 10;
      this.Background_Color = Color.Black;
      this.WallBorder_Pen = Pens.DarkSlateGray;
      this.WallBorder_Brush = Brushes.DarkSlateGray;
      this.OpenBorder_Pen = Pens.GhostWhite;
      this.OpenBorder_Brush = Brushes.GhostWhite;
      this.WallTile_Brush = Brushes.Black;
      this.OpenTile_Brush = Brushes.White;
    }

    public Brush GetFillBrushFor(Tile.MoveType physics)
    {
      if (physics == Tile.MoveType.Wall)
      {
        return this.WallTile_Brush;
      }
      if (0 != (physics & Tile.MoveType.Open_ALL))
      {
        // Open the tile up, and assume the border drawing will handle
        // open-ness
        return this.OpenTile_Brush;
      }
      return new SolidBrush(this.Background_Color);
    }

    public void Render(DungeonTiles tiles, Graphics g)
    {
      if (tiles.IsHex)
      {
        throw new NotImplementedException("Hex tiles not supported yet.");
      }

      int tileSz_px = (int)(g.VisibleClipBounds.Height / tiles.Height);
      int totalWidth = (int)g.VisibleClipBounds.Width;
      int totalHeight = (int)g.VisibleClipBounds.Height;

      g.Clear(this.Background_Color);

      // Draw tiles based on their physics
      for (int y = 0; y < tiles.Height; ++y)
      {
        for (int x = 0; x < tiles.Height; ++x)
        {
          Rectangle tileRect = new Rectangle(x * tileSz_px, y * tileSz_px, tileSz_px, tileSz_px);
          Brush tileBrush = this.GetFillBrushFor(tiles[y, x].Physics);
          g.FillRectangle(tileBrush, tileRect);
        }
      }

      // Draw grid lines based on tile physics
      if (!this.DrawBorders) goto SkipBorders;
      for (int y = 0; y < tiles.Height; ++y)
      {
        for (int x = 0; x < tiles.Height; ++x)
        {
          int x1 = x * tileSz_px,
              x2 = x * tileSz_px + tileSz_px,
              y1 = y * tileSz_px,
              y2 = y * tileSz_px + tileSz_px;

          DrawBorderFor(tiles, x, y, Tile.MoveType.Open_HORIZ, g, x1, y1, x2, y2);
        }
      }
      for (int y = 0; y < tiles.Height; ++y)
      {
        for (int x = 0; x < tiles.Height; ++x)
        {
          int px1 = x * tileSz_px,
              px2 = x * tileSz_px + tileSz_px,
              py1 = y * tileSz_px,
              py2 = y * tileSz_px + tileSz_px;

          DrawTopLeftPointFor(tiles, x, y, px1, py1, g);
          // This is just to get the bottom-right border. Easier way? Sure. But this works too.
          DrawTopLeftPointFor(tiles, x + 1, y, px2, py1, g);
          DrawTopLeftPointFor(tiles, x, y + 1, px1, py2, g);
          DrawTopLeftPointFor(tiles, x + 1, y + 1, px2, py2, g);
        }
      }
      SkipBorders:

      if (!this.ShadeGroups) goto SkipGroupShading;
      Random r = new Random();
      for (int i = 0; i < tiles.Groups.Count; ++i)
      {
        Brush groupBrush = new SolidBrush(Color.FromArgb(32, r.Next(255), r.Next(255), r.Next(255)));
        foreach (var tile in tiles.Groups[i])
        {
          Point tileLoc = tiles.TilesById[tile.Id];
          Rectangle tileRect = new Rectangle(tileLoc.X * tileSz_px, tileLoc.Y * tileSz_px, tileSz_px, tileSz_px);
          g.FillRectangle(groupBrush, tileRect);
        }
      }
      SkipGroupShading:

      return;
    }

    /// <summary>
    /// Renders the specified tiles onto the given Graphics.
    /// </summary>
    public void Render(Dungeon d, Graphics g)
    {
      this.Render(d.Tiles, g);
    }

    /// <summary>
    /// Checks what color the top-left corner of tile x,y should be,
    /// and draws it at point px, py on the provided graphics object.
    /// </summary>
    private void DrawTopLeftPointFor(DungeonTiles tiles, int x, int y, int px, int py, Graphics g)
    {
      bool useWallColor = tiles.WallExists(x, y, Tile.MoveType.Open_NORTH) ||
                          tiles.WallExists(x, y, Tile.MoveType.Open_WEST) ||
                          tiles.WallExists(x - 1, y, Tile.MoveType.Open_NORTH) ||
                          tiles.WallExists(x, y - 1, Tile.MoveType.Open_WEST);

      Brush cornerBrush = useWallColor ? this.WallBorder_Brush : this.OpenBorder_Brush;

      g.FillRectangle(cornerBrush, px, py, 1, 1);
    }

    /// <summary>
    /// Draws the border for tile (x, y) in the specified DungeonTiles,
    /// to the specified graphics and rectangle coordinates.
    /// </summary>
    /// <param name="tiles">The DungeonTiles for which this border is being drawn</param>
    /// <param name="x">The x-location of the tile whose border is being drawn</param>
    /// <param name="y">The x-location of the tile whose border is being drawn</param>
    /// <param name="moveDir">The move direction. Currently supports a single horizontal square direction, or all horizontal square directions, but no other combinations</param>
    /// <param name="g">The graphics on which to draw</param>
    /// <param name="x1">x1 of the tile's square in graphics.</param>
    /// <param name="y1">y1 of the tile's square in graphics.</param>
    /// <param name="x2">x2 of the tile's square in graphics.</param>
    /// <param name="y2">y2 of the tile's square in graphics.</param>
    private void DrawBorderFor(DungeonTiles tiles, int x, int y, Tile.MoveType moveDir, Graphics g, int x1, int y1, int x2, int y2)
    {
      if (null == tiles || null == g) return; // no op
      if (false == tiles.TileIsValid(x, y)) return;

      int adjacentTile_x = 0,
          adjacentTile_y = 0;

      // Determine the correct adjacent tile to check physics
      switch (moveDir)
      {
        case Tile.MoveType.Wall:
          return; // no op
        case Tile.MoveType.Open_NORTH:
          adjacentTile_x = x;
          adjacentTile_y = y - 1;
          break;
        case Tile.MoveType.Open_EAST:
          adjacentTile_x = x + 1;
          adjacentTile_y = y;
          break;
        case Tile.MoveType.Open_SOUTH:
          adjacentTile_x = x;
          adjacentTile_y = y + 1;
          break;
        case Tile.MoveType.Open_WEST:
          adjacentTile_x = x - 1;
          adjacentTile_y = y;
          break;
        case Tile.MoveType.Open_HORIZ:
          DrawBorderFor(tiles, x, y, Tile.MoveType.Open_NORTH, g, x1, y1, x2, y2);
          DrawBorderFor(tiles, x, y, Tile.MoveType.Open_EAST,  g, x1, y1, x2, y2);
          DrawBorderFor(tiles, x, y, Tile.MoveType.Open_SOUTH, g, x1, y1, x2, y2);
          DrawBorderFor(tiles, x, y, Tile.MoveType.Open_WEST,  g, x1, y1, x2, y2);
          return;
        default:
          throw new NotImplementedException("Unsupported Tile.MoveType specified");
      }

      // 1. Check if THIS cell is even open
      bool useWall = (0 == (tiles[y, x].Physics & moveDir));
      // 2. Check if adjacent cell exists; if not, use a wall border
      useWall = (useWall || false == tiles.TileIsValid(adjacentTile_x, adjacentTile_y));
      // 3. If adjacent cell exists, check if its corresponding wall is opened too; if not, use a wall border
      useWall = (useWall || (0 == (tiles[adjacentTile_y, adjacentTile_x].Physics & moveDir.GetOpposite())));

      if (null == this.WallBorder_Pen || null == this.OpenBorder_Pen)
      {
        throw new Exception("DungeonTileRenderer is in an invalid state: set WallBorder_Pen and OpenBorder_Pen");
      }

      Pen borderPen = useWall ? this.WallBorder_Pen : this.OpenBorder_Pen;

      // All this work, just to draw a single line.
      switch (moveDir)
      {
        case Tile.MoveType.Open_NORTH:
          g.DrawLine(borderPen, x1, y1, x2, y1);
          break;
        case Tile.MoveType.Open_EAST:
          g.DrawLine(borderPen, x2, y1, x2, y2);
          break;
        case Tile.MoveType.Open_SOUTH:
          g.DrawLine(borderPen, x1, y2, x2, y2);
          break;
        case Tile.MoveType.Open_WEST:
          g.DrawLine(borderPen, x1, y1, x1, y2);
          break;
        default:
          throw new ArgumentException("Unsupported moveDir to draw border");
      }
      
    }

    public Image Render(Dungeon d)
    {
      return this.Render(d.Tiles);
    }

    public Image Render(DungeonTiles tiles)
    {
      int tileSz_px = this.TileSize_Pixels;
      int totalWidth = tiles.Width * tileSz_px + 1;
      int totalHeight = tiles.Height * tileSz_px + 1;

      Image img = new Bitmap(totalWidth, totalHeight);
      using (Graphics g = Graphics.FromImage(img))
      {
        this.Render(tiles, g);
      }

      return img;
    }
  }
}

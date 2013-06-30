using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagicalBlockSmackdown
{
    class GameplayModel
    {
        public const int NumberOfPanelColors = 6;
        public enum PanelColor
        {
            Red = 0,
            Blue = 1,
            Green = 2,
            Yellow = 3,
            Cyan = 4,
            Purple = 5,
        }

        public enum PanelState
        {
            None = 0,
            Alive = 1,
            Falling = 2,
            Exploding = 3,
        }

        public struct Panel
        {
            public PanelColor color;
            public PanelState state;
            public float explodingTime;
            public const float explodingDuration = 300f;

            public Panel(PanelColor color, PanelState state)
            {
                this.color = color;
                this.state = state;

                this.explodingTime = 0;
            }

            public void activate(PanelColor color)
            {
                state = PanelState.Alive;
                this.color = color;
            }

            public void explode()
            {
                state = PanelState.Exploding;
                explodingTime = 0;
            }

            public Color PanelColorValue()
            {
                switch (color)
                {
                    case PanelColor.Red:
                        return Color.Red;
                    case PanelColor.Green:
                        return Color.Green;
                    case PanelColor.Blue:
                        return Color.Blue;
                    case PanelColor.Cyan:
                        return Color.Cyan;
                    case PanelColor.Purple:
                        return Color.Purple;
                    case PanelColor.Yellow:
                        return Color.Yellow;
                    default:
                        return Color.White;
                }
            }
        }

        private const int gridWidth = 6;
        private const int gridHeight = 12;
        private Panel[,] grid;
        public Panel[,] Grid { get { return grid; } }

        public GameplayModel()
        {
            grid = new Panel[gridWidth, gridHeight];

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 4; j < grid.GetLength(1); j++)
                {
                    grid[i, j].state = PanelState.None;

                    if (Game1.GameRandom.Next() % 2 == 0)
                    {
                        grid[i, j] = new Panel((PanelColor)(Game1.GameRandom.Next() % NumberOfPanelColors), PanelState.Alive);
                    }
                }
            }
        }

        public void pushSwap(int x, int y)
        {
            if (x < 0 || y < 0 || x > gridWidth - 2 || y > gridHeight - 1)
            {
                return;
            }

            if ((grid[x, y].state != PanelState.Alive && grid[x, y].state != PanelState.None) || (grid[x + 1, y].state != PanelState.Alive && grid[x + 1, y].state != PanelState.None))
            {
                return;
            }
            
            //fix this into something nice later
            switchTwoTiles(x, y, x + 1, y);
        }

        private void switchTwoTiles(int x1, int y1, int x2, int y2)
        {
            Panel swap = grid[x1, y1];
            grid[x1, y1] = grid[x2, y2];
            grid[x2, y2] = swap;
        }

        private bool checkHorzontalMatch(int x, int y)
        {
            if (grid[x, y].state != PanelState.Alive)
            {
                return false;
            }

            if (x > 0 && x < grid.GetLength(0) - 1)
            {
                if ((grid[x, y].color == grid[x - 1, y].color && grid[x - 1, y].state == PanelState.Alive) && (grid[x, y].color == grid[x + 1, y].color && grid[x + 1, y].state == PanelState.Alive))
                {
                    return true;
                }
            }

            return false;
        }

        private bool checkVerticalMatch(int x, int y)
        {
            if (grid[x, y].state != PanelState.Alive)
            {
                return false;
            }

            if (y > 0 && y < grid.GetLength(1) - 1)
            {
                if ((grid[x, y].color == grid[x, y - 1].color && grid[x, y - 1].state == PanelState.Alive) && (grid[x, y].color == grid[x, y + 1].color && grid[x, y + 1].state == PanelState.Alive))
                {
                    return true;
                }
            }

            return false;
        }

        private void explodePanels(int x, int y, bool horizontal, bool vertical)
        {
            if (grid[x, y].state != PanelState.Alive || x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1))
            {
                return;
            }

            grid[x, y].explode();

            if (horizontal)
            {
                if (x - 1 >= 0)
                {
                    if (grid[x, y].color == grid[x - 1, y].color)
                    {
                        explodePanels(x - 1, y, true, false);
                    }
                }

                if (x + 1 < gridWidth)
                {
                    if (grid[x, y].color == grid[x + 1, y].color)
                    {
                        explodePanels(x + 1, y, true, false);
                    }
                }
            }

            if (vertical)
            {
                if (y - 1 >= 0)
                {
                    if (grid[x, y].color == grid[x, y - 1].color)
                    {
                        explodePanels(x, y - 1, false, true);
                    }
                }

                if (y + 1 < gridHeight)
                {
                    if (grid[x, y].color == grid[x, y + 1].color)
                    {
                        explodePanels(x, y + 1, false, true);
                    }
                }
            }
        }

        public void update(GameTime currentTime)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = grid.GetLength(1) - 1; j >= 0; j--)
                {
                    if (grid[i, j].state == PanelState.None)
                    {
                        continue;
                    }
                    else if (grid[i, j].state == PanelState.Alive)
                    {
                        //start falling if there's empty space below
                        if (j < grid.GetLength(1) - 1 && grid[i, j + 1].state == PanelState.None)
                        {
                            grid[i, j].state = PanelState.Falling;
                        }
                        else
                        {
                            if (checkHorzontalMatch(i, j) || checkVerticalMatch(i, j))
                            {
                                explodePanels(i, j, checkHorzontalMatch(i, j), checkVerticalMatch(i, j));
                            }
                        }
                    }
                    else if (grid[i, j].state == PanelState.Falling)
                    {
                        if (j >= grid.GetLength(1) - 1 || grid[i, j + 1].state != PanelState.None)
                        {
                            grid[i, j].state = PanelState.Alive;
                        }
                        else if (j < grid.GetLength(1) - 1 && grid[i, j + 1].state == PanelState.None)
                        {
                            switchTwoTiles(i, j, i, j + 1);
                        }
                    }
                    else if (grid[i, j].state == PanelState.Exploding)
                    {
                        grid[i, j].explodingTime += currentTime.ElapsedGameTime.Milliseconds;

                        if (grid[i, j].explodingTime > Panel.explodingDuration)
                        {
                            grid[i, j].state = PanelState.None;
                        }
                    }
                }
            }
        }
    }
}

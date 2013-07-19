using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagicalBlockSmackdown
{
    class GameplayModel
    {
        public enum GameplayModelState
        {
            Active,
        }

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
            public int chainValue;
            public float chainWaitTime;
            public const float chainWaitDuration = 100f;

            public Panel(PanelColor color, PanelState state)
            {
                this.color = color;
                this.state = state;
                this.chainValue = 1;
                this.chainWaitTime = 0;

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

        private struct Combo
        {
            private int id;
            public int Id { get { return id; } }
            private int count;
            public int Count { get { return count; } set { count = value; } }
            private PanelColor color;
            public PanelColor Color { get { return color; } }
            private int chainValue;
            public int ChainValue { get { return chainValue; } set { chainValue = value; } }

            /// <summary>
            /// Clears the counter for the combo, and creates a new ID.
            /// </summary>
            /// <returns>The new id for the combo.</returns>
            public int Reset(PanelColor color)
            {
                this.id = Game1.GameRandom.Next();
                this.count = 0;
                this.color = color;
                this.chainValue = 1;

                return this.id;
            }
        }

        private const int gridWidth = 6;
        private const int gridHeight = 12;
        public int GridWidth { get { return gridWidth; } }
        public int GridHeight { get { return gridHeight; } }
        private Panel[,] grid;
        public Panel[,] Grid { get { return grid; } }
        private GameplayModelState modelState;

        private float pushingUpTime;
        private float pushingUpDuration = 4500;
        public float PushingUpValue { get { return pushingUpTime / pushingUpDuration; } }
        private Panel[] nextLineOfPanels;
        public Panel[] NextLineOfPanels { get { return nextLineOfPanels; } }

        private int cursorX = 0;
        public int CursorX { get { return cursorX; } set { cursorX = value; } }
        private int cursorY = 0;
        public int CursorY { get { return cursorY; } set { cursorY = value; } }

        private int score;
        public int Score { get { return score; } }

        private int combosLogged;
        private Combo[] comboSet = null;

        public GameplayModel()
        {
            modelState = GameplayModelState.Active;

            score = 0;

            combosLogged = 0;
            comboSet = new Combo[8];

            grid = new Panel[gridWidth, gridHeight];
            nextLineOfPanels = new Panel[gridWidth];
            scrambleNextLineOfPanels();

            cursorX = grid.GetLength(0) / 2 - 1;
            cursorY = grid.GetLength(1) / 2;

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

            /*
            // chain combo test
            grid[3, grid.GetLength(1) - 1].state = PanelState.Alive;
            grid[3, grid.GetLength(1) - 1].color = PanelColor.Red;
            grid[3, grid.GetLength(1) - 1].chainValue = 1;
            grid[2, grid.GetLength(1) - 1].state = PanelState.Alive;
            grid[2, grid.GetLength(1) - 1].color = PanelColor.Red;
            grid[2, grid.GetLength(1) - 1].chainValue = 1;
            grid[1, grid.GetLength(1) - 1].state = PanelState.Alive;
            grid[1, grid.GetLength(1) - 1].color = PanelColor.Red;
            grid[1, grid.GetLength(1) - 1].chainValue = 1;

            grid[4, grid.GetLength(1) - 1].state = PanelState.Alive;
            grid[4, grid.GetLength(1) - 1].color = PanelColor.Yellow;
            grid[3, grid.GetLength(1) - 2].state = PanelState.Alive;
            grid[3, grid.GetLength(1) - 2].color = PanelColor.Yellow;
            grid[2, grid.GetLength(1) - 2].state = PanelState.Alive;
            grid[2, grid.GetLength(1) - 2].color = PanelColor.Yellow;

            grid[5, grid.GetLength(1) - 2].state = PanelState.Alive;
            grid[5, grid.GetLength(1) - 2].color = PanelColor.Cyan;
            grid[4, grid.GetLength(1) - 3].state = PanelState.Alive;
            grid[4, grid.GetLength(1) - 3].color = PanelColor.Cyan;
            grid[3, grid.GetLength(1) - 3].state = PanelState.Alive;
            grid[3, grid.GetLength(1) - 3].color = PanelColor.Cyan;
            */
        }

        public void pushSwap(int x, int y)
        {
            if (modelState != GameplayModelState.Active)
            {
                return;
            }

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

        private void explodePanels(int x, int y, bool horizontal, bool vertical, int comboID)
        {
            if (grid[x, y].state != PanelState.Alive || x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1))
            {
                return;
            }

            grid[x, y].explode();
            incrementCombo(comboID, grid[x, y].chainValue);

            if (horizontal)
            {
                if (x - 1 >= 0)
                {
                    if (grid[x, y].color == grid[x - 1, y].color)
                    {
                        explodePanels(x - 1, y, true, false, comboID);
                    }
                }

                if (x + 1 < gridWidth)
                {
                    if (grid[x, y].color == grid[x + 1, y].color)
                    {
                        explodePanels(x + 1, y, true, false, comboID);
                    }
                }
            }

            if (vertical)
            {
                if (y - 1 >= 0)
                {
                    if (grid[x, y].color == grid[x, y - 1].color)
                    {
                        explodePanels(x, y - 1, false, true, comboID);
                    }
                }

                if (y + 1 < gridHeight)
                {
                    if (grid[x, y].color == grid[x, y + 1].color)
                    {
                        explodePanels(x, y + 1, false, true, comboID);
                    }
                }
            }
        }

        private void scrambleNextLineOfPanels()
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                nextLineOfPanels[i] = new Panel((PanelColor)(Game1.GameRandom.Next() % 6), PanelState.Alive);
            }
        }

        private void pushUpNextRow()
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1) - 1; j++)
                {
                    grid[i, j] = grid[i, j + 1];
                }
            }

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                grid[i, grid.GetLength(1) - 1] = nextLineOfPanels[i];
            }

            if (cursorY > 1)
            {
                cursorY--;
            }

            scrambleNextLineOfPanels();
        }

        private int newCombo(PanelColor newColor)
        {
            if (combosLogged == comboSet.Length)
            {
                Array.Resize<Combo>(ref comboSet, 2 * comboSet.Length);
            }

            int id = comboSet[combosLogged].Reset(newColor);
            combosLogged++;
            return id;
        }

        private void incrementCombo(int id, int chainValue)
        {
            for (int i = 0; i < combosLogged; i++)
            {
                if (comboSet[i].Id == id)
                {
                    if (comboSet[i].ChainValue < chainValue)
                    {
                        comboSet[i].ChainValue = chainValue;
                    }

                    comboSet[i].Count++;
                    break;
                }
            }
        }

        /// <summary>
        /// Calculates the score results for a combo. This uses a quadratic formula which yields similar results to the combo scoring found in Pokemon Puzzle League (Nintendo, Intelligent Systems, 2000).
        /// </summary>
        /// <param name="comboCount">The number of Panels cleared in a combo.</param>
        /// <returns>The number of points to be rewarded. Approximately equal to (0.99f * comboCount)^2.47</returns>
        private int computeComboPoints(int comboCount)
        {
            // give nothing for a simple 3-block combo; that's dreary and dull.
            if (comboCount < 4)
            {
                return 0;
            }

            return (int)(Math.Pow(0.99 * comboCount, 2.47));
        }

        private int computeChainPoints(int chainValue)
        {
            switch (chainValue)
            {
                case 0:
                    return 0;
                case 1:
                    return 0;
                case 2:
                    return 50;
                case 3:
                    return 80;
                case 4:
                    return 150;
                case 5:
                    return 300;
                case 6:
                    return 400;
                case 7:
                    return 500;
                case 8:
                    return 700;
                case 9:
                    return 900;
                case 10:
                    return 1100;
                case 11:
                    return 1300;
                case 12:
                    return 1500;
                default:
                    return 1800;
            }
        }

        public void update(GameTime currentTime)
        {
            if (modelState == GameplayModelState.Active)
            {
                pushingUpTime += currentTime.ElapsedGameTime.Milliseconds;
                if (pushingUpTime > pushingUpDuration)
                {
                    pushingUpTime = 0;
                    pushUpNextRow();
                }

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
                                    explodePanels(i, j, checkHorzontalMatch(i, j), checkVerticalMatch(i, j), newCombo(grid[i, j].color));
                                }
                                else if (grid[i, j].chainValue != 1)
                                {
                                    grid[i, j].chainWaitTime += currentTime.ElapsedGameTime.Milliseconds;

                                    if (grid[i, j].chainWaitTime > Panel.chainWaitDuration)
                                    {
                                        grid[i, j].chainWaitTime = 0;
                                        grid[i, j].chainValue = 1;
                                    }
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
                                score += 10;

                                grid[i, j].state = PanelState.None;

                                for (int it = j - 1; it > 0; it--)
                                {
                                    if (grid[i, it].state != PanelState.Alive)
                                    {
                                        break;
                                    }

                                    grid[i, it].chainValue = (1 + grid[i, j].chainValue);
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < combosLogged; i++)
                {
                    score += computeComboPoints(comboSet[i].Count) + computeChainPoints(comboSet[i].ChainValue);
                    //Console.WriteLine("{0}-block {1} combo with chain of {2}", comboSet[i].Count, comboSet[i].Color, comboSet[i].ComboValue);
                }

                combosLogged = 0;
            }
        }
    }
}

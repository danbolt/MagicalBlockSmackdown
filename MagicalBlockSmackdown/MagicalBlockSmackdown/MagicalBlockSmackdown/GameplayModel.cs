using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagicalBlockSmackdown
{
    class GameplayModel
    {
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
            Exploding = 2,
        }

        public struct Panel
        {
            public PanelColor color;
            public PanelState state;

            public Panel(PanelColor color, PanelState state)
            {
                this.color = color;
                this.state = state;
            }

            public void activate(PanelColor color)
            {
                state = PanelState.Alive;
                this.color = color;
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
        }

        public void update(GameTime currentTime)
        {
            return;
        }
    }
}

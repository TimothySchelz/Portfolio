// Created by Gray Marchese, u0884194, and Timothy Schelz, u0851027
// Last Date Updated: 11/22/16
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SnakeModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SnakeGUI
{
    /// <summary>
    /// A panel to be used to display the snake game
    /// </summary>
    public class GamePanel : Panel
    {

        // the world that this panel should draw
        private World world;

        float SnakeScaling;
        float WorldScaling;

        // Current length of the player
        private float SnakeSize;


        /// <summary>
        /// Constructor for creating a Game panel.
        /// </summary>
        public GamePanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
        }

        /// <summary>
        /// Sets the world
        /// </summary>
        /// <param name="newWorld"></param>
        public void SetWorld(World newWorld)
        {
            world = newWorld;
        }

        /// <summary>
        /// What to do when this panel needs to be painted
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // check if the world exists.  If it doesn't we are done
            if (world == null || world.PlayerSnake == null)
                return;

            if (world.PlayerSnake != null)
            {
                // The Length of the snake
                SnakeSize = (float)world.PlayerSnake.GetLength() + 1;
            }

            // turn on antialiasing
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Game is scaled and focused if the snake is relatively small and alive.

            if ((SnakeSize < Math.Min(world.Width, world.Height) / 2) && world.PlayerSnake.GetHead().X != -1)
            {

                // What to scale the width by
                SnakeScaling = (float)this.Height / ((float)2 * (SnakeSize));

                // Shift to snakehead
                float xOff = (((float)this.Width / (2F)) - world.PlayerSnake.GetHead().X * SnakeScaling);
                float yOff = (((float)this.Height / (2F)) - world.PlayerSnake.GetHead().Y * SnakeScaling);

                e.Graphics.TranslateTransform(xOff, yOff);

                // Scale
                e.Graphics.ScaleTransform(SnakeScaling, SnakeScaling);

            }
            else
            {
                // Do the world scaling
                WorldScaling = (float)this.Height / (float)Math.Max(world.Height, world.Width);

                e.Graphics.ScaleTransform(WorldScaling, WorldScaling);

                float xOffset = this.Width / 2F - (world.Width * WorldScaling) / 2F;
                float yOffset = this.Height / 2F - (world.Height * WorldScaling) / 2F;

                e.Graphics.TranslateTransform(xOffset / WorldScaling, yOffset / WorldScaling);
            }


            // Paint all the items in the world
            //PaintBackground(e);  // It was causing flickering in the snake heads and tails and so we removed it
            PaintSnakes(e);
            PaintFood(e);
            PaintWalls(e);
        }

        /// <summary>
        /// Paints the background of the game panel. ... It's space
        /// </summary>
        /// <param name="e"></param>
        private void PaintBackground(PaintEventArgs e)
        {
            // bitmaps to store the background and then resize it
            Bitmap original;
            Bitmap resized = null;
            try
            {
                // get the image
                original = (Bitmap)Image.FromFile(@"..\..\..\Resources\Media\background.bmp", true);

                // resize the image
                resized = new Bitmap(original, new Size(world.Width, world.Height));
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("There was an error opening the bitmap." +
                    "Please check the path.");
                return;
            }

            using (TextureBrush texture = new TextureBrush(resized))
            {
                // set it to tesselate
                texture.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;

                //Draws the background
                e.Graphics.FillRectangle(texture, new Rectangle(0, 0, world.Height, world.Width));
            }
        }

        /// <summary>
        /// Paints the food in the world
        /// </summary>
        /// <param name="e"></param>
        private void PaintFood(PaintEventArgs e)
        {
            // Use a brush
            using (SolidBrush drawBrush = new SolidBrush(Color.GreenYellow))
            {
                HashSet<Food> Foods = world.GetFood();

                // go through all the food
                foreach (Food food in Foods)
                {

                    //don't draw eaten phood
                    if (food.loc.X == -1)
                        continue;

                    //draw the food
                    Rectangle dropFood = new Rectangle(food.loc.X, food.loc.Y, 1, 1);
                    e.Graphics.FillEllipse(drawBrush, dropFood);
                }
            }
        }

        /// <summary>
        /// Paints the snakes from the world
        /// </summary>
        /// <param name="e"></param>
        private void PaintSnakes(PaintEventArgs e)
        {
            // Use the brush
            using (SolidBrush drawBrush = new SolidBrush(Color.Black))
            {
                // Go through each snake
                foreach (Snake snake in world.GetSnakes())
                {
                    // Go through each point in this snake
                    HashSet<SnakeModel.Point> snakePoints = snake.GetSnakePoints();
                    LinkedList<SnakeModel.Point> vertices = snake.GetVerticies();

                    SnakeModel.Point prevPoint = null;
                    int avgX = -1;
                    int avgY = -1;

                    foreach (SnakeModel.Point point in snakePoints)
                    {
                        //Find average placement between rectangles to 'fill spaces'
                        if (prevPoint != null)
                        {
                            avgX = (prevPoint.X + point.X) / 2;
                            avgY = (prevPoint.Y + point.Y) / 2;
                        }

                        //don't draw dead sneaks
                        if (point.X == -1)
                            continue;

                        // change the color
                        drawBrush.Color = world.GetSnakeColor(snake.ID);


                        // Draw this point, round it if it is a vertex
                        Rectangle drawPoint = new Rectangle(point.X, point.Y, 1, 1);
                        e.Graphics.FillRectangle(drawBrush, drawPoint);

                        // Connect this point to the previous point
                        Rectangle connectPoint = new Rectangle(avgX, avgY, 1, 1);
                        e.Graphics.FillRectangle(drawBrush, connectPoint);

                        prevPoint = point;

                    }

                }
            }
        }

        /// <summary>
        /// Paints the walls on the panel
        /// </summary>
        /// <param name="e"></param>
        private void PaintWalls(PaintEventArgs e)
        {
            using (SolidBrush drawBrush = new SolidBrush(Color.Black))
            {
                Rectangle TopWall = new Rectangle(0, 0, world.Width, 1);
                e.Graphics.FillRectangle(drawBrush, TopWall);

                Rectangle BottomWall = new Rectangle(0, (world.Height - 1), world.Width, 1);
                e.Graphics.FillRectangle(drawBrush, BottomWall);

                Rectangle LeftWall = new Rectangle(0, 0, 1, world.Height);
                e.Graphics.FillRectangle(drawBrush, LeftWall);

                Rectangle RightWall = new Rectangle((world.Width - 1), 0, 1, world.Height);
                e.Graphics.FillRectangle(drawBrush, RightWall);
            }
        }
    }
}

// Created by Gray Marchese, u0884194, and Timothy Schelz, u0851027
// Last Date Updated: 11/22/16
using SnakeModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnakeGUI
{
    /// <summary>
    /// A Panel object to display the scores of all the snakes
    /// </summary>
    public class ScoreBoardPanel : Panel
    {
        // Some constants to be used in displaying the scores
        private const int TOPMARGIN = 10;
        private const int NAMEALIGN = 10;
        private const int SCOREALIGN = 200;
        private const int LINEHEIGHT = 24;
        private const int LINESPACE = 5;

        // A copy of the world
        private World world;

        /// <summary>
        /// Constructs the Panel.  Doubled buffered is on and the background is light gray
        /// </summary>
        public ScoreBoardPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.LightGray;
        }

        /// <summary>
        /// Sets the world that this panel uses
        /// </summary>
        /// <param name="newWorld">The world you want this panel to use</param>
        public void SetWorld(World newWorld)
        {
            world = newWorld;
        }

        /// <summary>
        /// Paints the panel
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // We start writing under the margin
            int currentLine = TOPMARGIN;

            // Make sure the world exists
            if (world == null)
            {
                return;
            }

            // Cycles through each snake and writes their name and score
            foreach(Snake currentSnake in world.GetSnakes())
            {

                // sets the size of the font based on the size of the name or the default size.  Whichever is smaller
                int fontSize = Math.Min(LINEHEIGHT, 10 * LINEHEIGHT / currentSnake.name.Length);

                using(Font font = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    // Draw the names
                    System.Drawing.Point NameStart = new System.Drawing.Point(NAMEALIGN, currentLine);
                    TextRenderer.DrawText(e.Graphics, currentSnake.name, font, NameStart, world.GetSnakeColor(currentSnake.ID));

                    // Draw the score
                    System.Drawing.Point ScoreStart = new System.Drawing.Point(SCOREALIGN, currentLine);
                    TextRenderer.DrawText(e.Graphics, ""+currentSnake.GetLength(), font, ScoreStart, world.GetSnakeColor(currentSnake.ID));
                }

                // go to thge next line
                currentLine += fontSize + LINESPACE;
            }
        }
    }
}

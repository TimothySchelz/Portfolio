using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnakeModel;

namespace SnakeModel
{
    public class FunWorld : World
    {
        /// <summary>
        /// Constructor for the funworld.  It basically does the same thing as the regular world's constructor
        /// </summary>
        /// <param name="Width">Width of the world</param>
        /// <param name="Height">Height of the World</param>
        /// <param name="foodDensity">the amount of food per snake</param>
        /// <param name="headroom">The amount of empty space in front of a new snake</param>
        /// <param name="snakeLength">The starting length of a snake</param>
        /// <param name="snakeRecycleRate">The rate the snakes turn into food</param>
        public FunWorld(int Width, int Height, int foodDensity, int headroom, int snakeLength, double snakeRecycleRate) : base(Width, Height, foodDensity, headroom, snakeLength, snakeRecycleRate)
        {

        }

        public direc
    }
}

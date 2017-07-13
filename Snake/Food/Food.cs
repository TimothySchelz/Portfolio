// Created by Gray Marchese, u0884194, and Timothy Schelz, u0851027
// Last Date Updated: 12/8/16
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SnakeModel
{
    /// <summary>
    /// Represents food.  Kinda obvious.  It has an ID, and a location
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Food
    {
        /// <summary>
        /// ID for this peice of food.  It is based on the location that this food was placed at.
        /// The ID should be 10000*x + y where x and y represent the x nd y coordinates of the food.
        /// </summary>
        [JsonProperty]
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// X and Y coordinate point for the food's location. When food is eaten,
        /// the point is (-1,-1).
        /// </summary>
        [JsonProperty]
        public Point loc
        {
            get;
            private set;
        }

        /// <summary>
        /// A constructor to create a food
        /// </summary>
        /// <param name="ID">The ID the food should have</param>
        /// <param name="loc">The location of the Food as a SnakeModel.Point</param>
        public Food( int ID, Point loc)
        {
            this.ID = ID;
            this.loc = loc;
        }

        /// <summary>
        /// Gets the ID of a food given it's location.  This will give ID's of food that don't exist yet
        /// So only use it for assigning IDs and food you already know exists
        /// </summary>
        /// <param name="X">X position of the food</param>
        /// <param name="Y">X position of the food</param>
        /// <returns>The ID of the food at this location</returns>
        public static int getID(int X, int Y)
        {
            return 100000 * X + Y;
        }

        /// <summary>
        /// Gets the ID of a food given it's location.  This will give ID's of food that don't exist yet
        /// So only use it for assigning IDs and food you already know exists
        /// </summary>
        /// <param name="p">The point where the food is located</param>
        /// <returns>The ID of the food at this location</returns>
        public static int getID(Point p)
        {
            return getID(p.X, p.Y);
        }
       
    }
}

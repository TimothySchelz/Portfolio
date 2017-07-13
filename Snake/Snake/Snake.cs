// Created by Gray Marchese, u0884194, and Timothy Schelz, u0851027
// Last Date Updated: 12/8/16
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SnakeModel
{
    /// <summary>
    /// A struct to hold the X and Y values of whatever
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Point
    {
        /// <summary>
        /// The X coordinate of the piont
        /// </summary>
        [JsonProperty]
        public int X;

        /// <summary>
        /// The Y coordinate of the point
        /// </summary>
        [JsonProperty]
        public int Y;

    }

    /// <summary>
    /// A class to represent a Snake.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Snake
    {
        // the length of the snake.  It is the number of points the snake takes up -1
        private int length;

        //Head, turning points, and tail of the snake.
        [JsonProperty]
        private List<Point> vertices;

        private int direction;
        private int prevDirection;


        /// <summary>
        /// Returns the length of the snake.  The first time being called it may take a moment.  After that it should run in constant time.
        /// </summary>
        /// <returns>The length of the snake</returns>
        public int GetLength()
        {
            // Calculate and store the length if it hasn't been done before
            if (length == 0)
            {
                length = this.GetSnakePoints().Count - 1;
            }

            // return the length
            return length;
        }

        /// <summary>
        /// Returns the head of the snake as a point
        /// </summary>
        /// <returns>A point representing the head of the snake</returns>
        public Point GetHead()
        {
            return vertices.Last();
        }


        /// <summary>
        /// ID of the snake.
        /// </summary>
        [JsonProperty]
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// The name of the snake choosen by the player
        /// </summary>
        [JsonProperty]
        public string name
        {
            get;
            private set;
        }

        /// <summary>
        /// The direction the snake is pointing
        /// 1: up
        /// 2: right
        /// 3: down
        /// 4: left
        /// </summary>
        public int Direction
        {
            get
            {
                lock (this)
                {
                    return direction;
                }
            }

            set
            {
                lock (this)
                {
                    
                    //Checks if given direction is valid and not
                    //Also checks that we don't move back on ourselves.
                    if ((value <= 4 && value >= 1 &&
                        value != ((prevDirection + 1) % 4 + 1)) || 
                        prevDirection == 0)
                        direction = value;

                }
            }
        }

        /// <summary>
        /// The direction the snake moved on the last update
        /// 1: up
        /// 2: right
        /// 3: down
        /// 4: left
        /// 
        /// Should only be changed when the world is updated
        /// </summary>
        public int PrevDirection
        {
            get
            {
                return prevDirection;
            }

            set
            {
                //Checks if given direction is valid or not
                //Also checks that we don't move back on ourselves.
                if (value <= 4 && value >= 1 && (value != (prevDirection+1)%4 + 1))
                    prevDirection = value;
            }
        }

        /// <summary>
        /// Constructor for the snake that takes a list of verticies, the id of the snake, and the name
        /// </summary>
        /// <param name="Verticies"></param>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        [JsonConstructor]
        public Snake(List<Point> Verticies, int ID, string Name)
        {
            this.vertices = Verticies;
            this.ID = ID;
            this.name = Name;
        }

        /// <summary>
        /// Moves this snakes head by 1 in the direction it wants to go
        /// </summary>
        /// <returns>Updated head location</returns>
        public Point moveHeadForward()
        {
            Point Head = GetHead();

            // Check if direction has changed
            if (direction == prevDirection)
            {

                //Increment/or decrement coordinate in the dicrection of motion
                switch (direction)
                {
                    case 1:
                        Head.Y--;
                        break;

                    case 2:
                        Head.X++;
                        break;

                    case 3:
                        Head.Y++;
                        break;

                    case 4:
                        Head.X--;
                        break;
                }

                // make sure to reset the previous direction
                prevDirection = direction;

                //coordinate changed properly;
                return Head;

            }
            else
            {
                Point newHead = new Point();

                //If direction has changed, create a new head in the correct position in relation to
                //the previous head.
                switch (direction)
                {
                    case 1:
                        newHead.Y = Head.Y - 1;
                        newHead.X = Head.X;
                        break;

                    case 2:
                        newHead.X = Head.X + 1;
                        newHead.Y = Head.Y;
                        break;

                    case 3:
                        newHead.Y = Head.Y + 1;
                        newHead.X = Head.X;
                        break;

                    case 4:
                        newHead.X = Head.X - 1;
                        newHead.Y = Head.Y;
                        break;
                }

                // make sure to set the previous direction
                prevDirection = direction;

                //Adds the new head to the end of our vertex list.
                vertices.Add(newHead);

                return newHead;
            }
        }

        /// <summary>
        /// Actually kill the snake.  i.e. change it's verticies
        /// </summary>
        public void KillMe()
        {
            List<Point> deadList = new List<Point>();
            Point h = new Point();
            Point t = new Point();
            h.X = -1;
            h.Y = -1;
            t.X = -1;
            t.Y = -1;
            deadList.Add(h);
            deadList.Add(t);

            vertices = deadList;
        }

        /// <summary>
        /// Moves this snakes tail forward by 1
        /// </summary>
        /// <returns>Previous tail location</returns>
        public Point moveTailForward()
        {
            //Gets points of the tail and next to last vertex
            Point Tail = vertices.First();
            Point NextToLast = vertices.ElementAt(1);

            //Records current tail position which will no longer contain a snake
            Point PrevTail = new Point();
            PrevTail.X = Tail.X;
            PrevTail.Y = Tail.Y;

            bool finalVerticiesAdj;

            //Checks whether or not the tail is adjacent to the next to last vertex.
            finalVerticiesAdj = ((Tail.X == NextToLast.X) && (Math.Abs(Tail.Y - NextToLast.Y) == 1))
                             || ((Tail.Y == NextToLast.Y) && (Math.Abs(Tail.X - NextToLast.X) == 1));

            //If adjacent, the tail can be removed. 
            if (finalVerticiesAdj)
            {
                vertices.RemoveAt(0);
            }

            //If not adjacent, the tail moves toward the second to last vertex
            else
            {
                
                if(Tail.X == NextToLast.X)
                {
                    //Tail differs in the Y direction. move accordingly.
                    if(Tail.Y > NextToLast.Y)
                    {
                        Tail.Y--;
                    }
                    else
                    {
                        Tail.Y++;
                    }
                }
                else
                {
                    //Tail differs in the X direction. move accordingly.
                    if (Tail.X > NextToLast.X)
                    {
                        Tail.X--;
                    }
                    else
                    {
                        Tail.X++;
                    }
                }

            }

            //Returns the old position of the tail.
            return PrevTail;

        }

        /// <summary>
        /// Returns a list of points specifying the head, turning points, and tail of the snake
        /// </summary>
        /// <returns></returns>
        public LinkedList<Point> GetVerticies()
        {
            LinkedList<Point> result = new LinkedList<Point>();

            // adds each vertex to a list
            foreach (Point vertex in vertices)
            {
                result.AddLast(vertex);
            }

            // return the list
            return result;
        }

        /// <summary>
        /// Returns a HashSet of all the points in this snake
        /// </summary>
        /// <returns>All the points in this snake</returns>
        public HashSet<Point> GetSnakePoints()
        {
            // A haset that stores all the points to be returned
            HashSet<Point> result = new HashSet<Point>();
            // The previous vertex
            Point PreviousPoint = new Point();

            // Go through each vertice in this snake
            foreach (Point nextPoint in vertices)
            {
                // If it isn't the first verticie do this junk
                if (!nextPoint.Equals(vertices.First()))
                {
                    // Look at the case where they have the same  but ifferent y values
                    if (PreviousPoint.X == nextPoint.X)
                    {
                        // if previous points Y is the larger one we decrement down to nextpoint y
                        if (PreviousPoint.Y > nextPoint.Y)
                        {
                            // loop through and add all the points between the vertices
                            for (int i = PreviousPoint.Y; i > nextPoint.Y; i--)
                            {
                                // go ahead and add the current point to the result
                                Point current = new Point();
                                current.X = nextPoint.X;
                                current.Y = i;
                                if (nextPoint.Y != current.Y)
                                    result.Add(current);
                            }
                            
                        }
                        // If previouspoint Y is smaller than the next point Y we increment up to it
                        else if (PreviousPoint.Y < nextPoint.Y)
                        {
                            // loop through and add all the points between the vertices
                            for (int i = PreviousPoint.Y; i < nextPoint.Y; i++)
                            {
                                // go ahead and add the current point to the result
                                Point current = new Point();
                                current.X = nextPoint.X;
                                current.Y = i;
                                if (nextPoint.Y != current.Y)
                                    result.Add(current);
                            }
                        }
                    }
                    else if (PreviousPoint.Y == nextPoint.Y)
                    {
                        // if previous points X is the larger one we decrement down to nextpoint x
                        if (PreviousPoint.X > nextPoint.X)
                        {
                            // loop through and add all the points between the vertices
                            for (int i = PreviousPoint.X; i > nextPoint.X; i--)
                            {
                                // go ahead and add the current point to the result
                                Point current = new Point();
                                current.Y = nextPoint.Y;
                                current.X = i;
                                if (nextPoint.X != current.X)
                                    result.Add(current);
                            }

                        }
                        // If previouspoint X is smaller than the next point X we increment up to it
                        else if (PreviousPoint.X < nextPoint.X)
                        {
                            // loop through and add all the points between the vertices
                            for (int i = PreviousPoint.X; i < nextPoint.X; i++)
                            {
                                // go ahead and add the current point to the result
                                Point current = new Point();
                                current.Y = nextPoint.Y;
                                current.X = i;
                                if (nextPoint.X != current.X)
                                    result.Add(current);
                            }
                        }
                    }
                    else
                    {
                        // If the snake is not composed of vertical and horizontal lines we throw an exception.  That is a bad snake! Bad, bad snake!
                        throw new Exception("The verticies of the snake do not make sense");
                    }
                }

                PreviousPoint = nextPoint;
            }
            // Add the last verticie into the reslut.  It would not be add in the above algorithm
            result.Add(vertices.Last());
            return result;
        }

    }
}

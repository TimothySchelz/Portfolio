// Created by Gray Marchese, u0884194, and Timothy Schelz, u0851027
// Last Date Updated: 12/8/16
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SnakeModel
{
    /// <summary>
    /// This class represents the world!  It contains all snakes and food. 
    /// As a reminder, first entry is X and second entry is Y!!!
    /// </summary>
    public class World
    {

        // A 2D array to model the worldspace.
        private Int32[,] Map;                     //first entry is X and the second entry is Y!!!!!
        // All the snakes in the world
        private Dictionary<int, Snake> Snakes;
        // All the Food in the world
        private Dictionary<int, Food> Foods;
        // Saves each snake's assigned color;
        private Dictionary<int, Color> SnakeColors;
        // The player
        int PlayerID;
        // RNG for random world values
        private Random rando;
        // Probability that a snake body point will turn to food
        private double snakeRecycleRate;
        // Food made available per snake.
        private int foodDensity;
        // A list of food IDs that should be removed on the next updateWorld
        private List<int> markedFood;
        // A variable specifying the amount of headroom a snake has upon spawning.
        private int headroom;
        // Length of snake upon spawning
        private int snakeLength;
        // A variable to count frames updated in the game
        private int frameCounter;
        // A variable representing which extra game modes active.
        // An extra game mode is active if its associated prime number divides this number.
        private int ExtraGameMode;


        // Locks for The Food and the Snakes so that we can only be adding or getting from them by one thread at a time
        Object SnakeLock = new object();
        Object FoodLock = new object();


        /// <summary>
        /// Snake being controlled by the player.
        /// </summary>
        public Snake PlayerSnake
        {
            get;
            private set;
        }

        /// <summary>
        /// Length of the game board.
        /// </summary>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// Width of the game board.
        /// </summary>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates A whole new world, 
        /// A new fantastic point of view
        /// No one to tell us no
        /// Or where to go
        /// Or say we're only dreaming
        /// (Only for client use)
        /// </summary>
        /// <param name="PlayerID">The ID of the player snake</param>
        /// <param name="Width">The width of the world</param>
        /// <param name="Height">The height of the world</param>
        public World(int PlayerID, int Width, int Height)
        {
            // initializes and sets everything
            this.Height = Height;
            this.Width = Width;
            Map = new Int32[Width, Height];

            //Iterate through every  cell in our map
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    //Leave the edges of our map to be 0
                    if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                        continue;

                    //Change everything else to -1
                    Map[x, y] = -1;
                }
            }

            ExtraGameMode = 1;

            //Sets the seed for our RNG with system time
            rando = new Random();

            Snakes = new Dictionary<int, Snake>();
            Foods = new Dictionary<int, Food>();
            SnakeColors = new Dictionary<int, Color>();
            this.PlayerID = PlayerID;
        }

        /// <summary>
        ///  Constructor used only by the server, where food density and snake recycle rate can be defined.
        /// </summary>
        /// <param name="Width">Width of the world</param>
        /// <param name="Height">Height of the World</param>
        /// <param name="foodDensity">the amount of food per snake</param>
        /// <param name="headroom">The amount of empty space in front of a new snake</param>
        /// <param name="snakeLength">The starting length of a snake</param>
        /// <param name="snakeRecycleRate">The rate the snakes turn into food</param>
        public World(int Width, int Height, int foodDensity, int headroom, int snakeLength, double snakeRecycleRate, int ExtraGameMode) : this(0, Width, Height)
        {
            this.snakeRecycleRate = snakeRecycleRate;
            this.ExtraGameMode = ExtraGameMode;
            this.foodDensity = foodDensity;
            this.snakeLength = snakeLength;
            this.headroom = headroom;
            this.markedFood = new List<int>();
            frameCounter = 0;


            //Sets the seed for our RNG with system time
            rando = new Random();

            //Adds Extra Walls Before the game begins.
            if(ExtraGameMode % 3 == 0)
            {
                AddExtraWalls();
            }
        }


        /// <summary>
        /// Adds Extra walls to the world depending on its size. Walls are represented by snakes with negative ID.
        /// </summary>
        private void AddExtraWalls()
        {
            
            //Builds central wall
            Point p1 = new Point();
            Point p2 = new Point();

            p1.X = Width / 3;
            p2.X = 2 * (Width / 3);

            p1.Y = Height / 2;
            p2.Y = Height / 2;

            List<Point> joints = new List<Point>() { p1, p2 };

            Snake middleWall = new Snake(joints, -1, "wall");

            updateSnake(middleWall);

            //Creates a set of alcoves filled with food
            for (int i = 0; i < 7; i++)
            {
                CreateAlcove(-(100+ i));
            }

            //Creates a few random straight walls.
            for (int i = 0; i < 15; i++)
            {
                createSnake(-(200 + i), "Wall-e");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateAlcove(int ID)
        {
            //Creates small squares filled with food.
            Point p3 = new Point();
            Point p4 = new Point();
            Point p5 = new Point();
            Point p6 = new Point();

            p3.X = rando.Next(Width - 7);
            p3.Y = rando.Next(Height - 7);

            p4.X = p3.X + 7;
            p4.Y = p3.Y;

            p5.X = p3.X;
            p5.Y = p3.Y + 7;

            p6.X = p3.X + 7;
            p6.Y = p3.Y + 7;

            List<Point> wallCorners = new List<Point>() { p3, p4, p6, p5 };

            int offset = rando.Next(4);

            for (int i = 0; i < offset; i++)
            {
                Point movingPoint = wallCorners[0];

                wallCorners.Remove(movingPoint);

                wallCorners.Add(movingPoint);
            }

            for(int x = p3.X + 1; x <= p4.X - 1; x++)
            {
                for(int y = p3.Y+1; y <= p5.Y-1; y++)
                {
                    generateFood(x, y);
                }
            }

            updateSnake(new Snake(wallCorners, ID, "SquareWall1"));
        }

        /// <summary>
        /// Advances the world to the next frame of the game. Responsible for snake motion and food generation.
        /// </summary>
        public void UpdateWorld()
        {

            lock (this)
            {
                // check for eaten food and remove it
                foreach (int foodID in markedFood)
                {
                    Foods.Remove(foodID);
                }

                markedFood.Clear();

                // Move snakes and checks for collisions and interactions
                MoveSnakes();

                // Generate food
                PopulateWithFood(foodDensity);


                // If survival mode is on
                if(ExtraGameMode % 5 == 0 && frameCounter > 60)
                {
                    foreach(KeyValuePair<int,Snake> pair in Snakes)
                    {
                        if ( pair.Key >= 0)
                        {
                            Point tail = pair.Value.GetVerticies().First.Value;
                            Map[tail.X, tail.Y] = -1;
                            pair.Value.moveTailForward();
                           
                            if (pair.Value.GetSnakePoints().Count== 2)
                            {

                                KillSnake(pair.Value);
                                
                            }

                        }
                    }

                    frameCounter = 0;
                }

                frameCounter++;
            }
        }

       /// <summary>
       /// Given a specified snake and a new location for its head,
       /// detects collisions of the snakes head and kills or grows
       /// the snake if needed.
       /// </summary>
       /// <param name="newHead">New possition of the snake head</param>
       /// <param name="snake">Snake to be updated</param>
        private void Interactions(Point newHead, Snake snake)
        {
           
            switch(Map[newHead.X, newHead.Y])
            {

                //Empty Space
                case -1:
                    Map[newHead.X, newHead.Y] = 2;

                    //If Tron mode is off, move tail forward.
                    if (ExtraGameMode % 2 != 0)
                    {
                        Point prevTail = snake.moveTailForward();
                        Map[prevTail.X, prevTail.Y] = -1;
                    }

                    break;
                
                //Wall
                case 0:
                    KillSnake(snake);
                    break;

                //Food
                case 1:
                    Map[newHead.X, newHead.Y] = 2;

                    // covers up a weird Key not found exception
                    if (Foods.ContainsKey(Food.getID(newHead)))
                    {
                        // Set the eaten foods location to (-1,-1)
                        Foods[Food.getID(newHead)].loc.X = -1;
                        Foods[Food.getID(newHead)].loc.Y = -1;

                        // Remind us to remove this food on the next frame
                        markedFood.Add(Food.getID(newHead));
                    }
                    
                    break;
                
                //Snake
                case 2:
                    KillSnake(snake);
                    break;

            }

        }

        /// <summary>
        /// Decides what to do with each body point upon death
        /// </summary>
        /// <param name="snake"></param>
        private void KillSnake(Snake snake)
        {
            //Iterates over all points in snake body
            foreach(Point point in snake.GetSnakePoints())
            {

                if(point.X == snake.GetHead().X && point.Y == snake.GetHead().Y)
                {
                    continue;
                }

                //With specified probability, turns the snake body to food or empty space
                // Also checks to make sure the location is not on the wall.  
                if (rando.NextDouble() < snakeRecycleRate &&  // Check rando
                    point.X<Width && point.X>0 &&  // Make sure the X values are valid
                    point.Y < Width && point.Y > 0) // Make sure the Y values are valid
                {
                    //Updates world map with the new food.
                    Map[point.X, point.Y] = 1;  //TODO: Possibly unnecessary code.  Should be done in update food

                    //Creates a new peice of food in our list.
                    updateFood(new Food(Food.getID(point), point));
                }
                else
                {
                    //Updates world map with empty space.
                    Map[point.X, point.Y] = -1;
                }
            }

            //this kills the snake
            snake.KillMe();
        }

        /// <summary>
        /// Populares the world with food until we have enough food to feed every snake.
        /// </summary>
        /// <param name="FoodDensity">The amount of food per snake</param>
        private void PopulateWithFood(int FoodDensity)
        {
            int FoodDisparity;

            // Determine how much food is missing from the world based on density 
            // Formula split into seperate locks to avoid nested locks and possible deadlocks.
            lock (SnakeLock)  // Deal with snake portion first.  Lock it so we don't hve any problems
            {
                FoodDisparity = (Snakes.Count * FoodDensity);
            }

            lock(FoodLock) // then deal with food portion.  Lock it so e don't have any problems
            {
                FoodDisparity = FoodDisparity - Foods.Count;
            }

            // Create new food
            if (FoodDisparity > 0)
            {
                for (int i = 0; i < FoodDisparity; i++)
                {
                    generateFood();
                }
            }
        }

        /// <summary>
        /// Moves the snakes in the direction they want to go by 1
        /// </summary>
        private void MoveSnakes()
        {
            lock(SnakeLock)
            {
                // List of snake ID's to be removed
                List<int> MarkedForRemoval = new List<int>();

                foreach (KeyValuePair<int, Snake> snakePair in Snakes)
                {
                    //Don't move snake if they are representing walls.
                    if (snakePair.Key < 0)
                        continue;

                    // Remove the snake from the list of snakes if the snake is dead.
                    if (snakePair.Value.GetHead().X == -1)
                    {
                        // Create a list of IDs of snakes to be removed... Can't actually remove 
                        // them here since we are iterating through the snakes
                        MarkedForRemoval.Add(snakePair.Key);
                        continue;
                    }

                    // Move head Forward
                    Point newHead = snakePair.Value.moveHeadForward();

                    // Checks for interactions of the snake with other objects.
                    // Updates world map & snake body accordingly.
                    Interactions(newHead, snakePair.Value);
                }

                // Actually remove the dead snakes
                foreach(int SID in MarkedForRemoval)
                {
                    Snakes.Remove(SID);
                }
                
            }
        }

        /// <summary>
        /// Gets all the snakes in the world
        /// </summary>
        /// <returns>Hashset of snakes in the world</returns>
        public HashSet<Snake> GetSnakes()
        {
            // No one messes with the Tunnel Snakes!
            // Tunnel Snakes Rule!
            // These snakes on the other hand can be messed with 1 thread at a time.
            lock(SnakeLock)
            {
                // The copy of the snakes to be returned
                HashSet<Snake> result = new HashSet<Snake>();

                // cycle through all the snakes
                foreach (KeyValuePair<int, Snake> Voldemort in Snakes)
                {
                    // add the current snake to set of things to be returned
                    result.Add(Voldemort.Value);
                }

                // Return the list of snakes
                return result;
            }
        }

        /// <summary>
        /// Returns all the Food in the world
        /// </summary>
        /// <returns>A hashset of food in the world</returns>
        public HashSet<Food> GetFood()
        {
            // Only 1 thread messes with food at a time.
            lock (FoodLock)
            {
                // The copy of the Food to be returned
                HashSet<Food> result = new HashSet<Food>();

                // cycle through all the Food
                foreach (KeyValuePair<int, Food> ComboMeal in Foods)
                {
                    // add the current Food to set of things to be returned
                    result.Add(ComboMeal.Value);
                }

                // Return the list of Food
                return result;
            }
        }

        /// <summary>
        /// Updates the given food item
        /// </summary>
        /// <param name="newFood"></param>
        public void updateFood(Food newFood)
        {
            // Only 1 thread messes with food at a time.
            lock (FoodLock)
            {
                // check if the food is already known about
                if (Foods.ContainsKey(newFood.ID))
                {
                    // If the food has been eaten we remove it
                    if (newFood.loc.X == -1)
                    {
                        Foods.Remove(newFood.ID);
                    }

                    // Reset the food incase they implement moving food or something
                    Foods[newFood.ID] = newFood;
                }
                else
                {
                    // Check if the food has been eaten 
                    if (newFood.loc.X != -1)
                    {
                        // if it hasn't, add it to our dictionary of food & our map
                        Foods.Add(newFood.ID, newFood);
                        Map[newFood.loc.X, newFood.loc.Y] = 1;

                        // otherwise don't do anything.  It is gone
                    }
                }
            }
        }

        /// <summary>
        /// Generates food in a random location in our world.
        /// </summary>
        public void generateFood() { 
            Point pt = new Point();

            pt.X = 0;
            pt.Y = 0;
            
            // If the random position of the food is already filled in we pick a new random spot
            while (Map[pt.X, pt.Y] != -1) 
            {
                // Get random x and y values that are inside of our world
                pt.X = (rando.Next(Width - 2)) + 1;
                pt.Y = (rando.Next(Height - 2)) + 1;
            }

            // Sets the food ID to be based on it's location
            int ID = Food.getID(pt);
            updateFood(new Food(Food.getID(pt), pt));
        }

        /// <summary>
        /// Places a peice of food at the specified (X,Y) coordinate.
        /// </summary>
        /// <param name="X">Horizontal coordinate for food placement</param>
        /// <param name="Y">Vertical coordinate for food placement</param>
        public void generateFood(int X, int Y)
        {
            Point pt = new Point();

            pt.X = X;
            pt.Y = Y;

            updateFood(new Food(Food.getID(pt), pt));

        }

        /// <summary>
        /// Updates the given snake
        /// </summary>
        /// <param name="newSnake">Snake that needs to be updated</param>
        public void updateSnake(Snake newSnake)
        {
            // No one messes with the Tunnel Snakes!
            // Tunnel Snakes Rule!
            // These snakes on the other hand can be messed with 1 thread at a time.
            lock (SnakeLock)
            {
                //Makes sure we can reference the player's snake easily.
                if(newSnake.ID == PlayerID)
                {
                    PlayerSnake = newSnake;
                }

                // Check if the new snake already existed
                if (Snakes.ContainsKey(newSnake.ID))
                {
                    foreach (Point point in Snakes[newSnake.ID].GetSnakePoints())
                    {
                        Map[point.X, point.Y] = -1;
                    }

                    // Check if the snake is dead
                    if (newSnake.GetVerticies().First.Value.X == -1)
                    {
                        Snakes.Remove(newSnake.ID);
                        return;
                    }

                    // Loop through each point in the snake set the value to 2
                    foreach (Point currentPoint in newSnake.GetSnakePoints())
                    {
                        Map[currentPoint.X, currentPoint.Y] = 2;
                    }

                    Snakes[newSnake.ID] = newSnake;
                }
                else
                {
                    
                    SnakeColors[newSnake.ID] = Color.FromArgb(rando.Next(255), rando.Next(255), rando.Next(255));

                    // Check if the snake is dead
                    if (newSnake.GetVerticies().First.Value.X == -1)
                    {
                        Snakes.Remove(newSnake.ID);
                        return;
                    }

                    // Loop through each point in the snake set the value to 2
                    foreach (Point currentPoint in newSnake.GetSnakePoints())
                    {
                        Map[currentPoint.X, currentPoint.Y] = 2;
                    }

                    Snakes.Add(newSnake.ID, newSnake);
                }
            }
        }

        /// <summary>
        /// Returns the color of the snake with the given ID
        /// </summary>
        /// <param name="ID">The ID of the snake</param>
        /// <returns>The color of the snake</returns>
        public Color GetSnakeColor(int ID)
        {
            // No one messes with the Tunnel Snakes!
            // Tunnel Snakes Rule!
            // These snakes on the other hand can be messed with 1 thread at a time.
            lock (SnakeLock)
            {
                if (!SnakeColors.ContainsKey(ID))
                {
                    return Color.Black;
                }

                return SnakeColors[ID];
            }
        }

        /// <summary>
        /// Creates a snake with the given ID number. This is the first appearance of this snake
        /// in the world.
        /// </summary>
        /// <param name="ID">Unique ID number for this new snake</param>
        public void createSnake(int ID, string Name)
        {

            Point Head = new Point();
            Point Tail = new Point();
           
            List<Point> verts = new List<Point>() {Tail, Head};
            Snake snake = new Snake(verts, ID, Name);

            do
            {
                //Randomly places the head at a position within the board
                //Places the snake head at least a snake length away from the nearest wall
                Head.X = rando.Next(snakeLength + 1, Width-(snakeLength+2));
                Head.Y = rando.Next(snakeLength + 1, Height-(snakeLength+2));

                //Chooses one of the 4 direction with equal probability
                snake.Direction = (rando.Next() % 4) + 1;
                snake.PrevDirection = snake.Direction;

                //Sets the tail a snake length 'behind' the head
                //'behind' is opposite the snake's current direction.
                switch (snake.Direction)
                {
                    case 1:
                        Tail.X = Head.X;
                        Tail.Y = Head.Y + snakeLength;
                        break;

                    case 2:
                        Tail.X = Head.X - snakeLength;
                        Tail.Y = Head.Y;
                        break;

                    case 3:
                        Tail.X = Head.X;
                        Tail.Y = Head.Y - snakeLength;
                        break;

                    case 4:
                        Tail.X = Head.X + snakeLength;
                        Tail.Y = Head.Y;
                        break;
                }

                //Retry random snake placement until space is clear.
            } while (!SpaceIsClear(snake));

            // Puts the new snake in the world
            this.updateSnake(snake);


        }

        /// <summary>
        /// Checks that the space that a given snake would take up is available in the world, as
        /// well as a decent amount of space in front of the snake's head. Snake should be comprised
        /// of 2 vertices.
        /// </summary>
        /// <param name="snake"></param>
        /// <returns></returns>
        private bool SpaceIsClear(Snake snake)
        {
            foreach( Point point in snake.GetSnakePoints())
            {
                if (Map[point.X, point.Y] != -1)
                    return false;
            }

            Point Head = snake.GetHead();

            switch (snake.Direction)
            {
                //Checks space above head.
                case 1:
                    for(int i = 0; i < headroom; i++)
                    {
                        if (Map[Head.X, Head.Y - i] != -1)
                            return false;
                    }
                    break;

                //Check space to the right of head
                case 2:
                    for(int i = 0; i < headroom; i++)
                    {
                        if (Map[Head.X + i, Head.Y] != -1)
                            return false;
                    }
                    break;
                
                //Check space below the head
                case 3:
                    for (int i = 0; i < headroom; i++)
                    {
                        if (Map[Head.X, Head.Y - i] != -1)
                            return false;
                    }
                    break;
                
                //Check space to the left of head
                case 4:
                    for (int i = 0; i < headroom; i++)
                    {
                        if (Map[Head.X - i, Head.Y] != -1)
                            return false;
                    }
                    break;

            }

            return true;

        }

        /// <summary>
        /// Changes the direction of the given snake to a valid direction based on the input and the prevDirection
        /// 1: up
        /// 2: right
        /// 3: down
        /// 4: left
        /// 
        /// </summary>
        /// <param name="ID">The ID os the snake to change direction</param>
        /// <param name="direction">The direction the snake should be changed too</param>
        public void ChangeSnakeDirection(int ID, int direction)
        {
            if(Snakes.ContainsKey(ID))
                Snakes[ID].Direction = direction;
        }
    }
}
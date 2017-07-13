// Created by Gray Marchese, u0884194, and Timothy Schelz, u0851027
// Last Date Updated: 12/8/16
using NetworkController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnakeModel;
using System.Collections;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace Server
{
    public class Server
    {
        // A dictionay to store ll of the socket states that are connected to clients
        Clients clients;
        // The world the game is taking plce in
        World world = new World(0, 150, 150);
        // Timer to go off when a new frame should be sent to the clients
        System.Timers.Timer FrameTimer;


        // Server settings to be read in from the settings file
        private const String SETTINGSFILE = @"..\..\..\Resources\Settings.XML";
        private int boardWidth;
        private int boardHeight;
        private int MSPerFrame;
        private int FoodDensity;
        private int headroom;
        private int snakeLength;
        private double SnakeRecycleRate;
        private int ExtraGameMode;

        /// <summary>
        /// Main method to run when the server is staarted
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args)
        {
            Server TheServer = new Server();
        }

        /// <summary>
        /// Constructor for the server.  Starts the world and listening for clients
        /// </summary>
        private Server()
        {
            // create the hashset of clients
            clients = new Clients();


            // Sets ExtraGameMode to the multiplicitive identity.
            // Extra game modes will be represented as primes multiplied on.
            ExtraGameMode = 1;

            // Read info from the settings file
            readSettings(SETTINGSFILE);

            //Instantiates the world
            world = new World(boardWidth, boardHeight, FoodDensity, headroom, snakeLength, SnakeRecycleRate, ExtraGameMode);


            Console.WriteLine("Server Started up.");

            FrameTimer = new System.Timers.Timer(MSPerFrame);
            FrameTimer.Elapsed += UpdateFrame;
            FrameTimer.Start();
            
            //Establishes a non-blocking loop to collect clients
            Networking.ServerAwaitingClientLoop(ClientConnected);

            // Keep console window open
            while (true)
                {
                Console.Read();
                }

        }

        /// <summary>
        /// A Method to read in the settings from a settings file
        /// </summary>
        /// <param name="v">The file settings will be read from</param>
        private void readSettings(string filename)
        {
            //All the work.  Make sure to catch any exception
            try
            {
                // create the reader from the file and make sure it is disposed of
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    //cycle through the file
                    while (reader.Read())
                    {
                        //Check what we have
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "BoardWidth":
                                    // Reads the info from the xml and converts it into an int
                                    reader.Read();

                                    String readWidth = reader.Value.Trim();

                                    // Make sure it as read properly
                                    if (!Int32.TryParse(readWidth, out boardWidth))
                                    {
                                        throw new ArgumentException("BoardWidth settings don't make sense.");
                                    }
                                    break;
                                case "BoardHeight":
                                    // Reads the info from the xml and converts it into an int
                                    reader.Read();

                                    String readHeight = reader.Value.Trim();

                                    // Make sure it as read properly
                                    if (!Int32.TryParse(readHeight, out boardHeight))
                                    {
                                        throw new ArgumentException("boadHeight settings don't make sense.");
                                    }
                                    break;
                                case "MSPerFrame":
                                    // Reads the info from the xml and converts it into an int
                                    reader.Read();

                                    String readMS = reader.Value.Trim();

                                    // Make sure it as read properly
                                    if (!Int32.TryParse(readMS, out MSPerFrame))
                                    {
                                        throw new ArgumentException("MSPerFrame settings don't make sense.");
                                    }
                                    break;

                                case "FoodDensity":
                                    // Reads the info from the xml and converts it into an int
                                    reader.Read();

                                    String readDensity = reader.Value.Trim();

                                    // Make sure it as read properly
                                    if (!Int32.TryParse(readDensity, out FoodDensity))
                                    {
                                        throw new ArgumentException("FoodDensity settings don't make sense.");
                                    }
                                    break;
                                case "SnakeRecycleRate":
                                    // Reads the info from the xml and converts it into an int
                                    reader.Read();

                                    String readRecycle = reader.Value.Trim();

                                    // Make sure it as read properly
                                    if (!Double.TryParse(readRecycle, out SnakeRecycleRate) || SnakeRecycleRate < 0 || SnakeRecycleRate > 1)
                                    {
                                        throw new ArgumentException("SnakeRecycleRate settings don't make sense.");
                                    }
                                    break;

                                case "headroom":
                                    // Reads the info from the xml and converts it into an int
                                    reader.Read();

                                    String readHeadroom = reader.Value.Trim();

                                    // Make sure it as read properly
                                    if (!Int32.TryParse(readHeadroom, out headroom))
                                    {
                                        throw new ArgumentException("headroom settings don't make sense.");
                                    }
                                    break;

                                case "snakeLength":
                                    // Reads the info from the xml and converts it into an int
                                    reader.Read();

                                    String readSnakeLength = reader.Value.Trim();

                                    // Make sure it as read properly
                                    if (!Int32.TryParse(readSnakeLength, out snakeLength))
                                    {
                                        throw new ArgumentException("snakeLength settings don't make sense.");
                                    }
                                    break;

                                
                                case "TronMode":
                                    // Reads the info from the xml and converts it into a bool
                                    reader.Read();

                                    String readGameMode = reader.Value.Trim();

                                    bool on;

                                    // Make sure it as read properly
                                    if (!bool.TryParse(readGameMode, out on))
                                    {
                                        throw new ArgumentException("ExtraGameMode settings don't make sense.");
                                    }

                                    //Mark it as part of our game mode
                                    if (on)
                                    {
                                        ExtraGameMode *= 2;
                                    }

                                    break;


                                case "ExtraWalls":
                                    // Reads the info from the xml and converts it into a bool
                                    reader.Read();

                                    String readExtraWalls = reader.Value.Trim();

                                    bool ExtraWallsOn;

                                    // Make sure it as read properly
                                    if (!bool.TryParse(readExtraWalls, out ExtraWallsOn))
                                    {
                                        throw new ArgumentException("Extrawalls settings don't make sense.");
                                    }

                                    //Mark it as part of our game mode
                                    if (ExtraWallsOn)
                                    {
                                        ExtraGameMode *= 3;
                                    }

                                    break;


                                case "SurvivalMode":
                                    // Reads the info from the xml and converts it into a bool
                                    reader.Read();

                                    String readSurvival = reader.Value.Trim();

                                    bool SurvivalOn;

                                    // Make sure it as read properly
                                    if (!bool.TryParse(readSurvival, out SurvivalOn))
                                    {
                                        throw new ArgumentException("Survival mode settings don't make sense.");
                                    }

                                    //Mark it as part of our game mode
                                    if (SurvivalOn)
                                    {
                                        ExtraGameMode *= 5;
                                    }

                                    break;



                                case "SnakeSettings":
                                    //Doesn't Do anything at the start of the XML file.
                                    break;

                                default:
                                    // makes sure to throw an exception if we find any incorrect opening tags

                                    throw new ArgumentException("The Settings file is invalid");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Catches whatever happened in the file and deals with it
                if (e is ArgumentException)
                {
                    throw e;
                } else
                {
                    throw new ArgumentException("There was a problem reading the settings file.");
                }
            }
        }

        /// <summary>
        /// Tells everything to update for the next frame and then send the info to the clients
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateFrame(object sender, ElapsedEventArgs e)
        {
            //Update the world
            world.UpdateWorld();

            //TODO: Put the following serialization into world and lock it with snake lock and food lock

            // Compile the date to be sent
            // Create a stringbuilder to hold data to be sent and append new items on
            StringBuilder data = new StringBuilder();
            // Append all the snakes onto the StringBuilder
            foreach (Snake snake in world.GetSnakes())
            {
                data.Append(JsonConvert.SerializeObject(snake) + "\n");
            }
            // Append all the food onto the StringBuider
            foreach (Food food in world.GetFood())
            {
                data.Append(JsonConvert.SerializeObject(food) + "\n");
            }

            String message = data.ToString();

            

            //Send data to Clients
            foreach(SocketState client in clients.GetAllClients())
            {
                try
                {
                    Networking.SendData(client, message);
                } catch
                {
                    Console.WriteLine("Tried to send data to client that has already disconnected.");
                }
                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Client"></param>
        public void ClientConnected(SocketState Client)
        {
            //Updates callback
            Client.CallMe = RecieveName;

            //Instructs the socket what to do when client disconnects.
            Client.DisconnectCallback = DisconnectClient;

            //Begins listening for name information from client
            Networking.GetData(Client);
        }

        /// <summary>
        /// Disconnects a client gracefully.
        /// </summary>
        /// <param name="State"></param>
        private void DisconnectClient(SocketState State)
        {
            clients.Remove(State.ID);
            State.theSocket.Close();
        }

        /// <summary>
        /// Callback used to parse initial information from the client. Sends
        /// start-up information to client, adds this client to the list of connected
        /// clients, begins listening for input.
        /// </summary>
        /// <param name="State"></param>
        private void RecieveName(SocketState State)
        {
            //Assigns a uniuqe ID that will be used for this client and its snake.
            State.ID = clients.GetNextID();

            //Generates start-up info for the newly connected client
            string startUpInfo = "" + State.ID + '\n' + world.Width + '\n' + world.Height + '\n';
            Networking.SendData(State, startUpInfo);

            //PARSE NAME INFO & CREATE SNAKE WITH ID = State.ID

            // Grab the data from he sb
            String data = State.sb.ToString();

            // split the data into the individul pieces
            String[] DataList = data.Split();

            String name;

            // Set the name
            if (DataList.Length == 0 || DataList[0].Equals(""))
            {
                name = "Boaty McBoatface";
            } else
            {
                name = DataList[0];
            }

            //Creates a snake for this client and places it in the world
            world.createSnake(State.ID, name);

            //Adds this client to list of connected clients
            clients.Add(State);

            //Changes the callback to listen for user input
            State.CallMe = GetInput;

            //Starts an event loop listening for info from client.
            Networking.GetData(State);
        }

        /// <summary>
        /// Callback used to continually deal with input from the client while the connection
        /// is still open.
        /// </summary>
        /// <param name="State"></param>
        private void GetInput(SocketState State)
        {
            //pull info from string builder and clear it
            String message = State.sb.ToString();
            State.sb.Clear();

            // Get the actual irection the layer wishes to move
            String direction = "" + message[message.Length-3];

            int directionInt = 1;

            Int32.TryParse(direction, out directionInt);

            //Changes the snake's direction
            world.ChangeSnakeDirection(State.ID, directionInt);

            Networking.GetData(State);
        }

        /// <summary>
        /// A collection of socket states that is locked when accessed or changed.
        /// </summary>
        private class Clients
        {
            //Collection of socet states
            private Dictionary<int,SocketState> clients;

            private int NextID;

            /// <summary>
            /// Zero-parameter constructor that creates an empty collection of clients.
            /// </summary>
            public Clients()
            {
                clients = new Dictionary<int, SocketState>();
                NextID = 0;
            }

            /// <summary>
            /// Adds given socket state to the current collection of clients
            /// </summary>
            /// <param name="newClient"></param>
            public void Add(SocketState newClient)
            {
                lock (this)
                {
                    clients.Add(newClient.ID, newClient);
                }
            }


            /// <summary>
            /// Yields all clients in our collection, gives an iterable result.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<SocketState> GetAllClients()
            {
                lock (this)
                {
                    foreach (KeyValuePair<int, SocketState> pair in clients)
                    {
                        yield return pair.Value;
                    }
                }
            }

            /// <summary>
            /// Returns the socket state associated with the given ID.
            /// </summary>
            /// <param name="ID">ID of desired socket state</param>
            /// <returns>SocketState corrisponding to th given ID</returns>
            public SocketState GetClient(int ID)
            {
                lock (this)
                {
                    return clients[ID];
                }
            }

            /// <summary>
            /// Returns an unused ID number unique to this client.
            /// </summary>
            /// <returns></returns>
            public int GetNextID()
            {
                lock (this)
                {
                    NextID++;
                    return NextID -1;
                }                
            }

            /// <summary>
            /// Removes the client with the specified id from this list
            /// </summary>
            /// <param name="ID"></param>
            /// <returns></returns>
            public bool Remove(int ID)
            {
                lock (this)
                {
                    return clients.Remove(ID);
                }
            }


        }

    }
}

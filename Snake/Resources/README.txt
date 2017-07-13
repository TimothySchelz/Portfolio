README File for Snake Game Client Project

Authors: Gray Marchese, u0884194
		 Timothy Schekz, u0851027

Last Updated: 12/8/16

+------+
|Client|
+------+

Design Decisions(Client):
Using a MVC design pattern.  The Model portion will consist of the projects World, Snake, and Food.  It will be in the SnakeModel Namespace.  The Control and 
View portions will be contained in the SnakeGUI Namespace.  There is also completely seperate Network coding that the Gui uses to get info from the server.  
The View is the main bulk of the program since the client basically just draws things and interacts with the server.  This client can run at 60 fps and should
be because it is on PC.  

Model:
To keep tack of the cells that each snake or food occupies there is a Point class defined in Snake that simply keeps track of the X and Y coordinates of the 
object.  We store each snake as a LinkedList and store an array of food in the world.  The world will also be represented as a 2D array with entries 
corresponding with what is in each cell.  The values are listed below.
-1 empty
0 Wall
1 Food
2 Snake

This 2D array is never actually used.  It will just be useful later on when we write the server.  Everything is designed to just be a client at the moment 
except the 2D array.  The Model will be expanded later in the next assignment.

View/Control:
The View takes care of communication with the server, dealing with inputs from the user, and drawing everything.  The SnakeGui it the main view of the client.
It consists of some buttons and boxes for connecting to a server, a ScoreboardPanel, and a GamePanel.  The ScoreboardPanel draws the actual snake game and the 
GamePanel draws the different players scores on the side.  The SnakeGUI also communicates with the server.  It gets all the JSON objects converts them to the
appropriate information and deals with them.  It also sends inputs from the player to the server.

Everything is designed to just be a client at the moment.  The model and NetworkController will be expanded later to serve as a server in the next assignment.

Extra Features(Client):
Added sound
Almost added a Background.  We had it working but had a strange flickering and so we commented it out.  Just go to GamePanel.cs and uncomment line 101 to check 
it out

+------+
|Server|
+------+

For the server we are using the same MVC model from the client.  

The View and the Control are taken care of by the client and so we do not have to worry about them.  We are running networking and server code to interact with
clients though.  In server we have a dictionary of socketStates that correspond to the clients.

The model is still composed of Food, Snakes, and the World.The world describes the state of the world and everything in it.  It has dictionaries for the
food, and snakes.  There is also a 2d array of ints used to help with collision detection.  the values for the 2d array are as follows
-1 empty
0 Wall
1 Food
2 Snake

Food is essentially just an ID and a location.  Each foods ID is being generated to depend on the location that the food is initially placed.  This way 
just by knowing the location of the food we can get it out of the list of food.  The formula is ID = 10000*X + Y so the leading digits of the ID are the food's
x value and the trailing digits are the food's y value.

Snakes consist of an ID, a name, a set of verticies, a direction, and a previous direction.  The ID is unique and given to players when they connect.  The name 
is a string given by the player.  The set of verticies is a list of points that is composed of the head, tail, and all joints of the snake.  Direction is the 
direction that the player wants the snake to move in next.  Therefore, if the player sends up (3) the direction will be set to 3.  The previous direction is the 
last direction that the snake moved. So if on the last frame the snake moved up then the previous direction == 1.  These values correspond to the protocol with 
the client.  

To detect collisions we use the the actual snakes and the 2d array.  We start by moving all the snakes in there desired direction by 1 cell.  To do this we change
or replace the head in the actual snake object.  Then we detect if the snake has collided with anything.  To do this we just look at the 2d array and see if there 
is anything in the spot where the snake's head has moved.  We then take care of each of the cases.  If the place where the head is was empty we just put the correct
value in the 2d array and move the tail forward.  If there is a wall or a snake where the snake's head is we kill the snake and set all the cells to their appropriate 
values.  If there is food at the snakes head we eat the food, set the value of the cell to 2 and we don't move the tail forward.

Extra Features(Server):
!!!Warning!!! If settings such as Width and Height are set to small values there may be problems with creating the world and populating it with objects.

Extra Walls:  If this is set to True in the settings random walls and alcoves will be generated in the world when they begin.  The alcoves are full of food but watch
out!  They are dangerous.

Survival Mode:  If this mode is activated in the settings file your snake will shorten by 1 every 60 frames.  If you on't eat quick enough you will die!

Tron Mode: I am almost sure everyone will have a tron mode.  If you set this to true in the settings file then your tail will not disappear as you move forword. 

The way that the server and the world detects that the extra game modes are being used may seem a bit unorthadox.  Each extra feature is associated with a prime number.
We then multiply these to determine what game mode we should be in.  So if we want to determine if we are using a specific extra feature we just see if our game mode is
divisible by the prime number associated with the specific exta feature.  So if we want Extra walls and tron mode to be on our game mode will be 6 which is divisible by
2 and 3 the primes associated with those exta features.

Extra Settings:
Headroom:  Determine how many cells in fron of your snak must be open and free of obstacles. 
Snake Length: Determine how long starting snakes should be.
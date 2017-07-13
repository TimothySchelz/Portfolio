/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 *
 * Example code: http://www.geeksforgeeks.org/socket-programming-in-cc-handling-multiple-clients-on-server-without-multi-threading/ 
 * The site above was used as a basis for building our Network class.
 *
 * Pieces have also been adapted from comment by user Remy Lebeau on stackOverflow
 * http://stackoverflow.com/questions/19127398/socket-programming-read-is-reading-all-of-my-writes
 */
#include <stdio.h> 
#include <string.h>  
#include <string> 
#include <iostream>
#include <stdlib.h> 
#include <errno.h> 
#include <unistd.h> 
#include <arpa/inet.h>  
#include <sys/types.h> 
#include <sys/socket.h> 
#include <netinet/in.h> 
#include <sys/time.h> 
#include "Network.h"

// The port the server will be using
#define PORT 2112

// The number of clients that can try to connect at the same time
#define QUEUE 10
    
using namespace std;

namespace TBD
{

	/*
	 *Network object constructor. Performs one-time instantiations of any of the variables
	 *needed to bind to the listener port, and then binds to said port.
	 */
	Network::Network(void *serverObject, callback &receive, callback &connect, callback &disconnect) 
		: serverpointer(serverObject), receiveCallback(receive), connectedCallback(connect), disconnectedCallback(disconnect)
	{

		//set of socket descriptors 
		fd_set readfds;
  
		//Reset all client sockets to 0
		resetAllClientSockets();
  
		//Create a socket to listen for new clients to connect (using TCP)
		if( (listenerSocket = socket(AF_INET , SOCK_STREAM , 0)) == 0)  
		{  
			cout << "Setting up listener socket failed" << endl;
			exit(EXIT_FAILURE);  
		} 
  
		
		//set master socket to allow multiple connections , 
		//this is just a good habit, it will work without this 
		//This is supposed to fix ERROR 98, where the server/network was not releasing the socket on the port is was bound to
		if( setsockopt(listenerSocket, SOL_SOCKET, SO_REUSEADDR, (char *)&opt, 
					   sizeof(opt)) < 0 )  
		{  
			perror("setsockopt");  
			exit(EXIT_FAILURE);  
		}  
		
		//Specify the type of socket created 
		address.sin_family = AF_INET;  
		address.sin_addr.s_addr = INADDR_ANY;  
		address.sin_port = htons( PORT );
  
		int bindValue =  bind(listenerSocket, (struct sockaddr *)&address, sizeof(address));
		//bind the socket to localhost port 2112 
		if (bindValue < 0)  
		{  
			cout << "Error binding to listenerSocket"<< endl;
			cout<<"Bind value is: "<< bindValue << endl;
			cout << "Errno = :"<< errno << endl;

			exit(EXIT_FAILURE);  
		}  
		
		
		cout <<"Listening on port ";
		cout<< PORT << endl;
  
		//Specify the amount of pending connections to queue up 
		if (listen(listenerSocket, QUEUE) < 0)  
		{  
			cout <<"Error listening" << endl;
			exit(EXIT_FAILURE);  
		}  
  
		//accept the incoming connection 
		addressLength = sizeof(address); 
	}
  
	/*
	 * sends msg to all the currently connected clients
	 */
	void Network::sendAll(vector<int> clientsList, string msg)
	{
		// Declare an iterator to a vector of strings
		vector<int>::iterator it;  
	
		//Iterate through the client list and (re)set all client sockets to 0;
		for (it = clientsList.begin(); it < clientsList.end(); ++it)
		{	
			
			sendToClient(serverpointer, msg, *it);
		}

	}
	/*
	 *	Sends the msg to all but one of the clients. AKA, the client making the edit
	 */
	void Network::sendAllButOne(vector<int> clientsList, string msg, int clientID)
	{
		// Declare an iterator to a vector of strings
		vector<int>::iterator it;  
	
		//Iterate through the client list and (re)set all client sockets to 0;
		for (it = clientsList.begin(); it < clientsList.end(); ++it)
		{	
			//If the client ID is not the same as the client making the edit, then send
			if(*it != clientID)
			{
				sendToClient(serverpointer, msg, *it);
			}
		}

	}
  

	/*
	 * Called when a message is received from a client.  Basically just passes it to the server
	 */
	string Network::receive(int socketDescriptor, string message)
	{
		// get the message that was sent as a string
		//string message(buffer);

		// give the message to the server to deal with
		receiveCallback(serverpointer, message, socketDescriptor);

		return message;
	}
  
	/*
	 *Reset all the client sockets by iterating through the vector of clients and resetting all of their
	 *values to 0.
	 */
	void Network :: resetAllClientSockets()
	{
    
		// Declare an iterator to a vector of strings
		vector<int>::iterator it;  
	
		mtx.lock();
		//Iterate through the client list and (re)set all client sockets to 0;
		for (it = Network::clients.begin(); it < Network::clients.end(); ++it)
		{	
			*it = 0;
		}
		mtx.unlock();

	}
	
	/*
	 *Copy constructor.
	 */
	Network :: Network (const Network &right)
	{
		mtx.lock();
		serverpointer = right.serverpointer;
		clients = right.clients;
		listenerSocket = right.listenerSocket;

		addressLength = right.addressLength;
		newClientSocket = right.newClientSocket;
		receiveFlag = right.receiveFlag;

		readfds = right.readfds;
		maxSocketDescriptor = right.maxSocketDescriptor;
		address = right.address;

		receiveCallback = right.receiveCallback;
		connectedCallback = right.connectedCallback;
		disconnectedCallback = right.disconnectedCallback;

		strcpy(buffer, right.buffer);
		//	buffer = right.buffer;
		text = right.text;
		//	mtx = right.mtx;
		mtx.unlock();
	}
	
	/*
	 *Assignment operator to set one network object to another.
	 */
	Network & Network :: operator = (const Network & right)
	{
		mtx.lock();
		serverpointer = right.serverpointer;
		clients = right.clients;
		listenerSocket = right.listenerSocket;

		addressLength = right.addressLength;
		newClientSocket = right.newClientSocket;
		receiveFlag = right.receiveFlag;

		readfds = right.readfds;
		maxSocketDescriptor = right.maxSocketDescriptor;
		address = right.address;

		receiveCallback = right.receiveCallback;
		connectedCallback = right.connectedCallback;
		disconnectedCallback = right.disconnectedCallback;

		strcpy(buffer, right.buffer);

		//	buffer = right.buffer;
		text = right.text;
		//	mtx = right.mtx;
		mtx.unlock();
		return *this;
		
	}



	/*
	 *This method is called once a connection is made to the listener port.
	 */
	void Network :: gotConnection()
	{
		if ((newClientSocket = accept(listenerSocket, 
									  (struct sockaddr *)&address, (socklen_t*)&addressLength))<0)  
		{  
			cout<<"Failed to accept"<<endl;  
			exit(EXIT_FAILURE);  
		}  
            
		//inform user of socket number - used in send and receive commands 
		printf("New connection , socket fd is %d , ip is : %s , port : %d \n" , newClientSocket , inet_ntoa(address.sin_addr) , ntohs (address.sin_port));  

		//Lock the client list
		mtx.lock();

		// check if this is the first client
		if (clients.size() == 0)
		{
			clients.push_back(newClientSocket);
		}
		else
		{
			// If the client list already exists we put it in the place of a previously occupied position
			bool added = false;
			for (int r = 0; r < clients.size(); r++)
			{
				if (clients[r] == 0)
				{
					added = true;
					clients[r] = newClientSocket;
					break;
				}
			}
			// If it hasn't been added yet then there was no open place and we just add to the back
			if (added == false)
			{
				clients.push_back(newClientSocket);
			}
			
			
		}
		//Unlock the client list
		mtx.unlock();

		// Call the server's connected method
		connectedCallback(serverpointer,"", newClientSocket);
	}



	/*
	 *Start listening on the listenerPort, for any new client connections.
	 */
	void Network :: startListening ()
	{
	
		// A socket descriptor that we will use throughout this method
		int socketDescriptor = 0;

		int valread = 0;

		//Continually  listen for incoming connections/data
		while(true)  
		{  
			//Clear the socket set
			FD_ZERO(&readfds);  
    
			//Add the listener socket to the set and then set the max socket descriptor
			FD_SET(listenerSocket, &readfds);  
			maxSocketDescriptor = listenerSocket;  
            

			// Declare an iterator to a vector of integers
			vector<int>::iterator it;  

			//Lock the client list
			mtx.lock();

			//Iterate through the client list and (re)set all client sockets to 0;
			for (it = Network::clients.begin(); it < Network::clients.end(); ++it)
			{	
				socketDescriptor = *it;

				//If the descriptor is valid, add it to the read lsit
				if (socketDescriptor > 0)
				{	
					FD_SET(socketDescriptor, &readfds);
				}

				//If the socketDescriptor is greater than the max, set the max to that descriptor
				if (socketDescriptor > maxSocketDescriptor)
				{
					maxSocketDescriptor = socketDescriptor;
				}
			} 
			
			//Unlock the client list
			mtx.unlock();
    
			//Wait for activity on any one of the sockets. The timeout is NULL, so it'll wait indefinitely
			//Select is what allows us to wait for a notification of activity, on any socket
			receiveFlag = select( maxSocketDescriptor + 1 , &readfds , NULL , NULL , NULL);  
      
			//Check for a select error
			if ((receiveFlag < 0) && (errno!=EINTR))  
			{  
				std :: cout <<"Select error detected" << std :: endl; 
			}  
            
			//If something happened on the master socket, then its an incoming connection 
			if (FD_ISSET(listenerSocket, &readfds))  
			{  
				gotConnection(); 
			}  
			
			//Lock the client list
			mtx.lock();
	           
			//Otherwise, it's some other input op on another socket
			for (vector<int>::iterator itr = clients.begin(); itr < clients.end(); ++itr)  
			{  						
				socketDescriptor = *itr;  
               
				//Check if the socket descriptor is set
				if (FD_ISSET( socketDescriptor , &readfds))  
				{  

					char readChar;
					vector<char> messageBuffer;
					do
					{
						//Check if the received value was a closing/disconnection message
						if ((valread = read(socketDescriptor, &readChar, 1)) == 0)  
						{  
							disconnected(itr, socketDescriptor);
							
							// We think removing this will break stuff
							memset(&buffer[0], 0, sizeof(buffer));
							break;
						}  
                    
						//Perform a receive since it is just a message from the client
						else
						{ 
							// Stick the char on the end of the vector
						    messageBuffer.push_back(readChar);

							// Check if it was the end of the message
							if (readChar == '\n')
							{
								//turn the message buffer into a string
								string message(messageBuffer.begin(), messageBuffer.end());

								// Clear the message buffer
								messageBuffer.clear();

								// Pass on the completed message
								receive(socketDescriptor, message);

								break;
							}
						}  
					}
					while(true);

					break;
				}  				

			}
			
			//Unlock the client list
			mtx.unlock();
		}  
	}

	/*
	 *This method is invoked if a client disconnects from the server. The client's socketDescriptor 
	 *is passed in and then used to close the socket.
	 */
	void Network::disconnected(vector<int>::iterator index, int socketDescriptor)
	{
		disconnectedCallback(serverpointer,"", socketDescriptor);

		//Close the socket and mark as 0 in list for reuse 
		cout <<"Disconnected client"<<endl;
		close(socketDescriptor);  
		
		
		*index = 0; 
	}

	/* 
	 * Sends the msg to the indicated client
	 *
	 * returns 1 if successful and 
	 * returns 0 if the client disconnected and send was unsuccessful.
	 */
	int Network::sendToClient(void* server, std::string msg, int socketDescriptor)
	{
	  
		// turn msg into a c string
		const char* text = msg.c_str();

		//Check that the send was possible. If not, then the connection was closed.
		//Imp: The MSG_NOSIGNAL flag keeps the server from shutting down and closing 
		//all connections, in the case of the client disconnecting, "non-gracefully"
		if (send(socketDescriptor , text , strlen(text) , MSG_NOSIGNAL ) <0)
		{
			
			//Lock the client list
			mtx.lock();
			//Iterate through the client list, find the client whose socket was closed, and 
			//set its value to 0
			for (vector<int>::iterator jtr = clients.begin(); jtr < clients.end(); ++jtr)
			{
				if (*jtr == socketDescriptor)
				{
					disconnected(jtr, socketDescriptor);
				}
			}
			
			//Unlock the client list
			mtx.unlock();

			return 0;
		}

		return 1;
	}

	/*
	 * closes any sockets that are still open
	 */
	Network::~Network()
	{
        
		// Declare an iterator to a vector of strings
		vector<int>::iterator it;  
	
		//Lock the client list
		mtx.lock();
		//Iterate through the client list and (re)set all client sockets to 0;
		for (it = Network::clients.begin(); it < Network::clients.end(); ++it)
		{	
			close(*it);
		}

		//Unlock the client list
		mtx.unlock();

		close(listenerSocket);
	}

}

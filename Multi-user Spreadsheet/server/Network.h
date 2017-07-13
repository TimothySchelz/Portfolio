/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */

#ifndef NETWORK_H
#define NETWORK_H

#include <string>
#include <vector>
#include <sys/types.h> 
#include <sys/socket.h> 
#include <netinet/in.h> 
#include <mutex> 
#include <thread>
#include <chrono>

namespace TBD
{
  /*
   * Code for establishing and communicating over a network.  This will handle all the connection and 
   * communicating messages between clients and the server
   */

	// This is a type of method used as a delegate for the Server and this networking code to communicate.
  typedef void (*callback) (void *server, std::string message, int SocketID);
  class Network
  {
  public:

    //Constructor.  Takes in delegates to be used all callback functions at the desired times.
    Network(void * serverObject, callback &receive, callback &connect, callback &disconnect);

    //Sends the msg to the client on that socket
    int sendToClient(void *server, std::string msg, int socket);

    //Sends the msg to all clients
    void sendAll(std::vector<int> clientsList, std::string msg);
	
	//Sends the msg to all but one of the clients. AKA, the client making the edit
    void sendAllButOne(std::vector<int> clientsList, std::string msg, int clientID);

    //Start listening to/for clients
    void startListening();

    //Destructor
    ~Network();

    //Assignment operator
    Network & operator=(const Network & right);
	
    //Copy constructor
    Network (const Network &other);

  private:

	int opt = 1;

	// This is a pointer to the server that this network is associated with
    void *serverpointer;
	
    //Got a new client connection
    void gotConnection();

    // all the clients currently connected
    std::vector<int> clients;
	
    //This method iterates through all client sockets and sets them to 0
    void resetAllClientSockets();	
	
	// The listener socketdescriptor along with the length of the address, a socket descriptor for new clients,
	// and a flag used when a message is received.
    int listenerSocket, addressLength, newClientSocket, receiveFlag;
	
    //Declare our fd_set
    fd_set readfds;
	
    //This is needed for the "select" method.	
    int maxSocketDescriptor;
	
    //Declare the socket address struct
    struct sockaddr_in address;
	
    // the functions to be called when a message is received
    callback receiveCallback;
    callback connectedCallback;
    callback disconnectedCallback;

	// a buffer we used to use but now we don't.  We think removing it might break things though
    char buffer[1025];
	
	// The message that will be sent to the client
    char const * text;
	
    //This is the lock for the clientVector
    std::mutex mtx;

    // what happens when a message is received
    std::string receive(int socketID, std::string message);

    // Disconnects a socket.  Takes the index of where the socket is in our list of sockets
    void disconnected(std::vector<int>::iterator index, int socketDescriptor);
  };

}
#endif

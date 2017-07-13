/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#ifndef SERVER_H
#define SERVER_H

#include <string>
#include <sys/types.h> 
#include <sys/socket.h> 
#include <netinet/in.h> 
#include <map>
#include <stack>
#include <mutex> 
#include <thread>
#include <chrono>
#include "Client.h"
#include "Network.h"
#include "LockedList.h"
#include "Splitter.h"
#include "spreadsheet.h"


namespace TBD
{

	/*
	 * This class represents a server.  It handles all the messages and makes sure the proper messages 
	 * get sent to the correct clients.
	 */
	class Server
	{

	public:

		//Constructor
		Server();

		//Parse the received message
		static void parseReceive(void *thisObject, std::string message, int socket);
	
		//This method is invoked when a new client connects
		static void clientConnected(void *thisObject, std::string message, int socket);

		//This method is invoked when a client disconnects
		static void clientDisconnected(void *thisObject, std::string message, int socket);
	

	private:
   
		// some callbacks to be passed into the network
		callback receive = parseReceive;
		callback disconnect = clientDisconnected;
		callback connect = clientConnected;
		
		// The network that communicates with the clients
		Network net;

		//The list of client objects
		LockedList clients;

		//The list of Open Spreadsheets
		std::map<std::string, SpreadsheetUtilities::Spreadsheet> spreadsheets;

		// A Map of each spreadsheet IDs and the clients that have that spreadsheet open
		std::map<std::string, std::vector<int>> whoHasWhatOpen;
		
		//A map which maps spreadsheet IDs to a stack of edits
		std::map<std::string, std::stack<std::string>> undoStack;
		
		//A map which maps spreadsheet IDs to a stack of undone undos
		std::map<std::string, std::stack<std::string>> redoStack;

		//Updates the spreadsheet and sends messages accordingly
		void update(bool addedToStack, bool redoUsed, std:: vector<std::string> & messagePieces, int socket);

		//An index to keep track of spreadsheet IDs.  Make sure to turn it into a string before using
		int DocIdIndex;
	
		//Close an open file
		void closeFile(std:: string ssID, int socket);

		//This is the lock for the clientVector
		std::mutex mtx;
	
	};
}
#endif

/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#include <string.h>  
#include <string> 
#include <iostream>
#include <vector>
#include <unordered_set>
#include <algorithm>
#include "Server.h"
#include "Network.h"



using namespace std;
using namespace SpreadsheetUtilities;

namespace TBD
{

	Server::Server(): net((void*)this, receive, connect, disconnect), clients(), spreadsheets(), DocIdIndex(0), whoHasWhatOpen(), undoStack(), redoStack()
	{
		//Start listening for connections
		net.startListening();
	}

	/*
	 *This method is invoked once a new client connects to the server.
	 */
	void Server :: clientConnected(void *thisObject, std::string message, int socket)
	{

		
		Server* thisServer = static_cast<Server*>(thisObject);
		string msg =Splitter:: intToString(socket);	
		
		//Lock the contents of this method so that a clash doesn't occur when seding a connect  message
		thisServer->mtx.lock();
		
		msg+="\n";
		thisServer->net.sendToClient(thisServer, msg, socket);
		
		//Create a client object
		Client newClient(socket);
		
		//Add it to the vector of clients
		thisServer->clients.add(newClient);
		
		//Unlock the contents of the method
		thisServer->mtx.unlock();
	}
	
	
	/*
	 *This method is invoked when a client disconnects
	 */
	void Server::clientDisconnected (void *thisObject, std::string message, int socket)
	{

		
		Server* thisServer = static_cast<Server*>(thisObject);
		
		//Lock the contents of this method so that a clash doesn't occur when seding a disconnect  message
		thisServer->mtx.lock();
		
		string msg =Splitter:: intToString(socket);
		
		// get a pointer to the actual client
		Client *currentClient = thisServer->clients.getClient(socket);

	    // Have this client close each of the spreadsheets they have open
	    map<string,string> clientsSpreadsheets = currentClient->getLocationMap();

		// Iterate through each spreadsheet the client had open
		for(map<string,string>::iterator itr = clientsSpreadsheets.begin(); itr != clientsSpreadsheets.end(); ++itr)
		{
			if(itr->second != "-1")
			{
				thisServer->closeFile(itr->first, socket);
			}
		}


		// Remove them from our list of clients
		thisServer->clients.remove(socket);	
		
		thisServer->mtx.unlock();
	}

	/*
	 *This method is invoked when the server receives a message from the client. That message is
	 *then parsed, and action is taken, based on the contents of that message.
	 */
	void Server::parseReceive(void *thisObject, std::string message, int socket)
	{

		vector<string> placeholder;
		// If the message is empty then don't do anything
		if (message.size() == 0)
		{
			return;
		}

		cout<<"The message is: \""<<message<< "\""<<endl;
		Server* thisServer = static_cast<Server*>(thisObject);
		
		//This will lock the method so that we're only working with a single message at a time
		thisServer->mtx.lock();
		
		string SocketID =Splitter :: intToString(socket);
		
		string s = "" + message;
		//Parse the received info
		vector<string> messagePieces = Splitter::formatter(message);
		
		string opCode = messagePieces[0];
	   
	
		//Check if this client has a username if not we set it
		Client *currentClient = thisServer->clients.getClient(socket);
		if (currentClient->needsUsername())
		{
			//Get rid of the newline character at the end of the string
			//opCode.pop_back();
			
			currentClient->setUsername(opCode);

			return;
		}

		//Decide what action to take 
		int opCode2 =	atoi(opCode.c_str()); 
		
		switch (opCode2)
		{
		case(0):
			{
				cout<<"0 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 1)
				{
					return;
				}
				thisServer->net.sendToClient(thisServer, Splitter ::listFiles(), socket);
			}		
			break;
		case(1):
			{
				cout<<"1 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 2)
				{
					return;
				}
				if (!Splitter :: fileExists(messagePieces[1]))
				{
					string ssID = Splitter::intToString(thisServer->DocIdIndex);
					// Increment the index
					thisServer->DocIdIndex++;

					// create the spreadsheet and at it to our map of spreadsheets
					Spreadsheet newsheet(messagePieces[1], ssID);
					newsheet.save();
					thisServer->spreadsheets.insert(pair<string,Spreadsheet>(ssID, newsheet));

					stack <string> undos;
					thisServer->undoStack.insert(pair<string, stack<string>>(ssID, undos));

					stack <string> redos;
					thisServer->redoStack.insert(pair<string, stack<string>>(ssID, redos));
				
					//set the location
					currentClient->setLocation(ssID, "A1");

					// Add this SS to the list of whoHasWhatOpen and add this client to it
					vector<int> newSheetClientList;
					newSheetClientList.push_back(socket);
					thisServer->whoHasWhatOpen.insert(pair<string, vector<int>>(ssID, newSheetClientList));

					// send the message to the client about the new spreadsheet
					string mes = "1\t" + ssID + "\n";
					thisServer->net.sendToClient(thisServer, mes, socket);
				} else 
				{
					// the file already exists so send back the list of spreadsheets
					thisServer->net.sendToClient(thisServer, Splitter ::listFiles(), socket);
				}
			}
			break;
		case(2):
			{
				cout<<"2 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 2)
				{
					return;
				}

				
				if (Splitter :: fileExists(messagePieces[1]))
				{
					// Checks if that spreadsheet is already open on the server.  We have to go through each 
					// element and check if we find something with that name.
					map<string,Spreadsheet>::iterator itr;
					string ssID = "";
					for (itr = thisServer->spreadsheets.begin(); itr != thisServer->spreadsheets.end(); ++itr)
					{
						if (itr->second.get_ss_name().compare(messagePieces[1]) == 0)
						{
							ssID = itr->second.get_id();
						}
					}


					//if it is open do one thing otherwise do the other... i just described what an if else statement is.. I need a break
					//Spreadsheet is not already open
					if (ssID.compare("") == 0) 
					{


						// Send the client open message and then all the data


						// Get a spreadsheet ID for this newly opened spreadsheet
						ssID = Splitter::intToString(thisServer->DocIdIndex);
						
						// Increment the index
						thisServer->DocIdIndex++;
						
						// Add this client to the list of clients that have this spreadsheet open
						thisServer->whoHasWhatOpen[ssID].push_back(socket);

						//Send a valid open message to the client
						string validOpen = "2\t" + ssID + "\n";
						thisServer->net.sendToClient(thisServer, validOpen, socket);

						
						// create the spreadsheet and at it to our map of spreadsheets
						Spreadsheet newsheet(messagePieces[1], ssID, "./spreadsheets/" + messagePieces[1]);
						newsheet.save();
						thisServer->spreadsheets.insert(std::pair<string,Spreadsheet>(ssID, newsheet));

						
						stack <string> undos;
						thisServer->undoStack.insert(pair<string, stack<string>>(ssID, undos));
						
						stack <string> redos;
						thisServer->redoStack.insert(pair<string, stack<string>>(ssID, redos));

						currentClient->setLocation(ssID, "A1");

						// Add this SS to the list of whoHasWhatOpen and add this client to it
						vector<int> newSheetClientList;
						newSheetClientList.push_back(socket);
						thisServer->whoHasWhatOpen.insert(pair<string, vector<int>>(ssID, newSheetClientList));

						// Send the client all the contents of all the cells
						unordered_set<string> cellSet = thisServer->spreadsheets[ssID].get_nonempty_cells();

						string outputMessage = "";
						// iterate through each nonempty cell put together the message and send it
						for (unordered_set<string>::iterator jtr = cellSet.begin(); jtr != cellSet.end(); ++jtr)
						{
							string contents = thisServer->spreadsheets[ssID].get_cell_contents(*jtr);
							outputMessage = "3\t" + ssID + "\t" + *jtr + "\t" + contents + "\n";
							thisServer->net.sendToClient(thisServer, outputMessage, socket);
						} 
					} 
					else
					{
						// Spreadsheet is already open
					
						//Send a valid open message to the client
						string validOpen = "2\t" + ssID + "\n";
						thisServer->net.sendToClient(thisServer, validOpen, socket);

						
						//set the client's location
						currentClient->setLocation(ssID, "A1");
					
						// Add this client to the list of clients that have this spreadsheet open
						thisServer->whoHasWhatOpen[ssID].push_back(socket);

						// Send the client all the contents of all the cells
						unordered_set<string> cellSet = thisServer->spreadsheets[ssID].get_nonempty_cells();

						string outputMessage = "";
						// iterate through each nonempty cell put together the message and send it
						for (unordered_set<string>::iterator jtr = cellSet.begin(); jtr != cellSet.end(); ++jtr)
						{
							string contents = thisServer->spreadsheets[ssID].get_cell_contents(*jtr);
							outputMessage = "3\t" + ssID + "\t" + *jtr + "\t" + contents + "\n";
							thisServer->net.sendToClient(thisServer, outputMessage, socket);
						} 

						// Send locations of all the clients
						vector<int> ssUsers = thisServer->whoHasWhatOpen[ssID];
						string location = "";
						for(vector<int>::iterator iter = ssUsers.begin(); iter != ssUsers.end(); ++iter)
						{
							// Dont send the client his own location
							if (socket == *iter)
							{
								continue;
							}

							// Getting the client whose location we want to send
							Client* anotherClient = thisServer->clients.getClient(*iter);

							// build the message to be sent
							location = "A\t";
							location += ssID + "\t" + anotherClient->getLocation(ssID) + "\t" + (Splitter::intToString( *iter)) + "\t" + anotherClient->getUsername()+"\n";
							
							// Send it to the client
							thisServer->net.sendToClient(thisServer, location, socket);
						}
					
					}
				} else
				{
					// The file doesn't exists.  Send back a list of spreadsheets it can open
					thisServer->net.sendToClient(thisServer, Splitter ::listFiles(), socket);
				}
			}
			break;
		case(3):
			{
				cout<<"3 Message Received.\n"<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 4)
				{
					return;
				}
				vector<string> &parts = messagePieces; 
				thisServer->update(true, false, parts, socket);


			}
			break;
		case(4):
			{
				cout<<"4 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 2)
				{
					return;
				}
				//Pop last change off of that docID's undo stack and then send change to all clients
				vector<string> &parts = messagePieces;
				thisServer->update(false, false, parts, socket);
			}
			break;
		case(5):
			{
				cout<<"5 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 2)
				{
					return;
				}
				vector<string> &parts = messagePieces; 
				thisServer->update(false, true, parts, socket);
			}
			break;
		case(6):
			{				
				cout<<"6 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 2)
				{
					return;
				}

				// Save the desired spreadsheet
				thisServer->spreadsheets[messagePieces[1]].save();
			}
			break;
		case(7):
			{
				cout<<"7 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 3)
				{
					return;
				}

				// Check if that name is already being used by another spreadsheet

				//If it is tell the client it is an invalid rename
				if (Splitter::fileExists(messagePieces[2]))
				{
					// create invalid message string
					string invalidRename = "9\t";
					invalidRename += messagePieces[1];
					invalidRename += "\n";

					//Send the invalid message back to client
					thisServer->net.sendToClient(thisServer, invalidRename, socket);
				} else
				{

					// pull the old name out so we can delete it later.
					string oldName = thisServer->spreadsheets[messagePieces[1]].get_ss_name();

					//If the name is free rename the file and send it out to all clients with that ss open
					thisServer->spreadsheets[messagePieces[1]].set_ss_name(messagePieces[2]);
					// Save thefile so the name gets saved onto the file
					thisServer->spreadsheets[messagePieces[1]].save();

					// Delete the old file
					Splitter::deleteFile(oldName);

					// Compose rename messages
					string rename8 = "8\t" + messagePieces[1] + "\n";
					string rename6 = "6\t" + messagePieces[1] + "\t" + messagePieces[2] + "\n";

					// send the valid rename message to this client
					thisServer->net.sendToClient(thisServer, rename8, socket);

					// send the rename to all the other clients
					vector<int> otherClients = thisServer->whoHasWhatOpen[messagePieces[1]];
					thisServer->net.sendAll(otherClients, rename6);
				}
				
			}
			break;
		case(8):
			{			
				cout<<"8 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 3)
				{
					return;
				}

				// set the client's location to the given location. 
				currentClient->setLocation(messagePieces[1], messagePieces[2]);

				//Compse the message to be sent out
				string editedLocation = "A\t";
				editedLocation += messagePieces[1]+ "\t" + messagePieces[2] + "\t" + (Splitter :: intToString(socket))+ "\t" + currentClient->getUsername() + "\n";

				// Then send it to all clients except this one
				thisServer->net.sendAllButOne(thisServer->whoHasWhatOpen[messagePieces[1]], editedLocation, socket);
			}	
			break;
		case(9):
			{
				cout<<"9 Message Received."<<endl;
				// If the message was not the correct size don't do anything
				if(messagePieces.size() != 2)
				{
					return;
				}
			
				string ssID = messagePieces[1];
			
				// close the indicated spreadsheet for the client indicated by socket
				thisServer->closeFile(ssID, socket);
			}
			break;
		default:
			cout<<"Undefined opCode received: "<< opCode <<endl;
			break;
		}
		
		//Unlock the method contents
		thisServer->mtx.unlock();
	}


	/*
	 * What needs to happen when a client closes a spreadsheet.  Also happens to every 
	 * spreadsheet a client has open if they disconnect.  The socket is the client that 
	 * this message was sent from or that disconnected
	 */
	void Server :: closeFile(string ssID, int socket)
	{
		Client* currentClient = clients.getClient(socket);

		// Remove the location from this client's location map
		clients.getClient(socket)->setLocation(ssID, "-1");

		// Remove this client from this spreadsheet's list of clients(whoHasWhatOpen)
		// Iterate through each element in this list and then remove it.
		for(vector<int>::iterator itr = whoHasWhatOpen[ssID].begin(); itr != whoHasWhatOpen[ssID].end(); ++itr)
		{
			if (*itr == socket)
			{
				whoHasWhatOpen[ssID].erase(itr);
				break;
			}
		}
			
		// check if any other clients have this open, if not close the spreadsheet
		if (whoHasWhatOpen[ssID].size() == 0)
		{
			// save the spreadsheet before closing
			spreadsheets[ssID].save();

			// remove the spreadsheet from the list of spreadsheets and the list of whoHasWhatOpen
			spreadsheets.erase(ssID);
			whoHasWhatOpen.erase(ssID);
		} else
		{
			// If others do have it open, send a location of -1 to all the other clients
			string ssClosedMessage = "A\t" + ssID + "\t-1\t" + Splitter::intToString(socket) + "\t" + currentClient->getUsername() + "\n";
			net.sendAll(whoHasWhatOpen[ssID], ssClosedMessage);
		}
	}


	/*
	 * A method to handle the case of when we receive an edit cell, undo, or redo message
	 */
	void Server :: update(bool addedToStack, bool redoUsed, vector<string> &messagePieces, int socket)
	{
		Spreadsheet & currentSheet = spreadsheets[messagePieces[1]];
		string previousContents = "";
		string editMessage = "3\t";

		if(messagePieces.size() == 4)
		{
			previousContents = "3\t" + messagePieces[1] + "\t" + messagePieces[2] + "\t" + currentSheet.get_cell_contents(messagePieces[2]) + "\n";
		}

		if(redoUsed)
		{
			stack<string> &redo = redoStack[messagePieces[1]];

			//Check if there is anything in the stack and return if it is. 
			if(redo.empty())
			{
				return;
			}
			editMessage = redo.top();
			redo.pop();
			vector<string> messageParts = Splitter::formatter(editMessage);

			//Get the content of the cell before the undo is processed.
			previousContents = "3\t" + messageParts[1] + "\t" + messageParts[2] + "\t" + 		currentSheet.get_cell_contents(messageParts[2]) + "\n";
			if (currentSheet.set_cell_contents(messageParts[2], messageParts[3]))
			{
				undoStack[messagePieces[1]].push(previousContents);	
				net.sendAll(whoHasWhatOpen[messagePieces[1]], editMessage);
				
			}
				
			return;
		}
			
		if(!addedToStack)
		{
			stack<string> &undo = undoStack[messagePieces[1]];

			//Check if there is anything in the stack and return if it is. 
			if(undo.empty())
			{
				return;
			}
			editMessage = undo.top();
			undo.pop();
			vector<string> messageParts = Splitter::formatter(editMessage);

			//Get the content of the cell before the undo is processed.
			previousContents = "3\t" + messageParts[1] + "\t" + messageParts[2] + "\t" + 		currentSheet.get_cell_contents(messageParts[2]) + "\n";
			
			if (currentSheet.set_cell_contents(messageParts[2], messageParts[3]))
			{
				redoStack[messagePieces[1]].push(previousContents);
				net.sendAll(whoHasWhatOpen[messagePieces[1]], editMessage);
			}
			
			return;

		}

		// Check if this edit causes a problem.
		if (currentSheet.set_cell_contents(messagePieces[2], messagePieces[3]))
		{

			if (addedToStack)
			{
				
				undoStack[messagePieces[1]].push(previousContents);
			
			}		
			// send valid edit message to the client that sent it
			string validEditMessage = "4\t" + messagePieces[1] + "\n";

					
			net.sendToClient(this,validEditMessage, socket);				


			// Send the new change to every client

			editMessage += messagePieces[1] + "\t" + messagePieces[2] + "\t" +  messagePieces[3] + "\n";
			
			net.sendAll(whoHasWhatOpen[messagePieces[1]], editMessage);
		}
		else
		{
			// send invalid edit message
			string invalidEditMessage = "5\t" + messagePieces[1] + "\n";

			net.sendToClient(this,invalidEditMessage, socket);
		}
	}
}

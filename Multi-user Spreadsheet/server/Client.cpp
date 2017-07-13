/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#include<stdio.h>
#include "Client.h"

using namespace std;

namespace TBD
{
	/*
	 *Construct a client object.
	 */
	Client :: Client(int ID)
	{
		this->ID = ID;
		this->username = "";
		
		
		needUsername = true;
	}
	
	int Client :: getID()
	{
		return this->ID;
	}
	
	string Client :: getUsername ()
	{
		return this->username;
	}

	void Client::setUsername(string username)
	{
		this->username = username;
		needUsername = false;
	}

	bool Client::needsUsername()
	{
		return needUsername;
	}
	
	void Client :: setLocation(string docID, string location)
	{
		locationMap[docID] = location;
	}

	// returns the location of this client on the given spreadsheet
	string Client :: getLocation(string docID)
	{
		return	locationMap[docID];
		
	}

	// returns the map of locations.
	map<std::string, std::string> Client::getLocationMap()
	{
		return locationMap;
	}
}


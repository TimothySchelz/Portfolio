/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#ifndef CLIENT_H
#define CLIENT_H

#include <string> 
#include<map>

namespace TBD
{
	
	/*
	 * This class represents clients and several properties they have including, 
	 * username, id, and their locations
	 */
	class Client
	{

	public:
	
		//Constructor, sets their ID
		Client(int ID);
	
		//Getters for the ID and username, location for a given spreadsheet, and their entire location map
		int getID();
		std :: string getUsername();
		std::string getLocation(std::string docID);
		std::map<std::string, std::string> getLocationMap();

		// Sets the username and location for th egiven spreadsheet
		void setUsername(std::string userName);
		void setLocation(std::string docID, std::string location);

		// A flag to determine if the username is still needed
		bool needsUsername();

	private:
	
		//ID, username, flag
		int ID;
		std :: string username;
		bool needUsername;
		
		// A map storing the location of this client keyed to the spreadsheet ID
		std::map<std::string, std::string> locationMap;
		
	};

}
#endif

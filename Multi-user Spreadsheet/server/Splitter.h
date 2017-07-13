/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */

#ifndef SPLITTER_H
#define SPLITTER_H

#include <iostream>
#include <string>
#include <sstream>
#include <vector>
#include <iterator>
#include <cstring>
#include <cstdlib>
#include <dirent.h>
#include <stdio.h>

namespace TBD
{

	/*
	 * A class of helper methods used throughout the Server
	 */
	class Splitter
	{
		
	public:
	
		/*
		 *Publicly available method to split a string message and put it into a vector.
		 */
		static std::vector<std::string> split(const std::string &s, char delim) ;
		
		/*
		 *Method to format input string. Splits first, by line, and then again by tab. Results are then stripped of
		 *any carriage return, newline, and tab chars. A string vector of the resulting items is returned.
		 */
		static	std::vector<std::string> formatter (std::string s);

		/*
		 *Put all the non-hidden files in the ./spreadsheets directory into a string, with a tab between each string name and a newline at the end.
		 */
		static	std::string listFiles();

		/*
		 *Convert an input int to a string and return that string
		 */
		static	std::string intToString(int i);
		
	
		/*
		 *Checks whether or not a file exists.
		 */
		static bool fileExists(std :: string filename);
   
		/*
		 * Deletes the  file with the given filename
		 */
		static void deleteFile(std::string filename);

		
	private:
		
		/*
		 * A helper method for split
		 */
		template<typename Out>	static void split(const std::string &s, char delim, Out result);
						
		
	};

}
#endif

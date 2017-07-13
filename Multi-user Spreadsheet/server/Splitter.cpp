/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 *
 *Two methods, split, were taken from user Evan Teran, from Stack Overflow, stackoverflow.com/questions/236129/split-a-string-in-c
 */
#include <iostream>
#include <string>
#include <sstream>
#include <vector>
#include <iterator>
#include <cstring>
#include <cstdlib>
#include <dirent.h>
#include <stdio.h>
#include<boost/regex.hpp>
#include "Splitter.h"

using namespace std;

namespace TBD
{	
	/**
	 *Private helper method to help split up the message from the client.
	 */
	template<typename Out>
	void Splitter :: split(const std::string &s, char delim, Out result) 
	{
		std::stringstream ss;
		ss.str(s);
		std::string item;
		while (std::getline(ss, item, delim)) 
		{
			*(result++) = item +'\n';
		}
 
	}
	
	/*
	 *Publicly available method to split a string message and put it into a vector.
	 */
	std::vector<std::string> Splitter ::split(const std::string &s, char delim) 
	{
		std::vector<std::string> elems;
		split(s, delim, std::back_inserter(elems));
		return elems;
	}



	/*
	 *Method to format input string. Splits first, by line, and then again by tab. Results are then stripped of
	 *any carriage return, newline, and tab chars. A string vector of the resulting items is returned.
	 */
	vector<string>  Splitter :: formatter (string s)
	{

		//Split by newline
		char delim = '\n';
		vector <string> v = split(s, delim);
		
		//Split by tab
		char delimTab = '\t';
		vector <string> vec;

		for (int i = 0; i < v.size(); i++)
		{
			vec = split(v[i], delimTab);		  
		}
		
		for (int x = 0; x < vec.size(); x++)
		{
			//Remove any carriage return chars
			vec[x].erase(remove(vec[x].begin(), vec[x].end(), '\r'),vec[x].end());
			
			//Remove any newline chars
			vec[x].erase(remove(vec[x].begin(), vec[x].end(), '\n'),vec[x].end());
			
			//Remove any tab chars
			vec[x].erase(remove(vec[x].begin(), vec[x].end(), '\t'),vec[x].end());
		}
		
		//Return the vector
		return vec;	
		  
	}

	/*
	 *Put all the non-hidden files in the ./spreadsheets directory into a string, with a tab between each string name and a newline at the end.
	 */
	string Splitter :: listFiles()
	{
		//Declare a directory
		DIR *directory;
		//Declare a struct pointer
		struct dirent *filename;
		//Add the opcode
		string allFiles = "0\t";
		//Set the path
		const char* filepath = "./spreadsheets";
		
		//Go through the directory given and so long as the file is not a hidden file, append its name to the allFIles string
		if ((directory = opendir (filepath)) != NULL) 
		{			
			while ((filename = readdir (directory)) != NULL)
			{	
				//Verify it's not a hidden file
				if((filename->d_name) != "." && ((filename->d_name) != "..") && (filename->d_name)[0] != '.')
				{
					allFiles+=filename->d_name;
					allFiles +="\t";
				}
			}
			//End the string with the newline delimeter
			allFiles += "\n";
			//Close the directory
			closedir (directory);
		}
		
		else
		{
			/* Could not open directory */
			cout<<"Could not find directory"<<endl;		
		}
		
		//Return the string with all the file names
		return allFiles;
	}
	
	/*
	 *Checks whether or not a file exists.
	 */
	bool Splitter :: fileExists(string name)
	{
		//Declare a directory
		DIR *directory;
		//Declare a pointer to a struct
		struct dirent *filename;
		//Set the filepath
		const char* filepath = "./spreadsheets";
		
		//Go through all the files in the directory and check if the input filename matches the filename we are looking at
		if ((directory = opendir (filepath)) != NULL) 
		{			
			while ((filename = readdir (directory)) != NULL)
			{		
				if(filename->d_name == name)
				{
					//File found, close directory and return true
					closedir (directory);
					return true;
				}
			}
			
			//File not found, close direcrtory
			closedir(directory);
			
		}
		
		//Return false
		return false;
		
	}
	

	/*
	 *Convert an input int to a string and return that string
	 */
	string Splitter ::intToString(int i)
	{
		stringstream ss;
		ss << i;
		return ss.str();
	}
	

	/*
	 * Deletes the  file with the given filename
	 */
	void Splitter :: deleteFile(string filename)
	{
		string filestring = "./spreadsheets/" + filename;
		char const * file = filestring.c_str();
		remove(file);
	}


}



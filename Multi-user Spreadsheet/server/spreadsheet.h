/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#ifndef SPREADSHEET_H
#define SPREADSHEET_H

#include <string>
#include <map>
#include <unordered_set>
#include<vector>
#include "dependency.h"

namespace SpreadsheetUtilities
{
	
	/*
	  A Spreadsheet that contains its name and #id as 
	  well as a dependency graph which contains a cell 
	  name and its content as strings.
	*/
	class Spreadsheet
	{
	public:
		/*
		  Default Constructor, creates an empty spreadsheet with no name or id.
		*/
		Spreadsheet();
		/*
		  Creates an empty spreadsheet with a given name and id
		*/
		Spreadsheet(std::string, std::string);
		/*
		  Creates an empty spreadsheet with a given name and id and filepath which fills
		  the spreadsheet with the contents of the spreadsheet file. 
		*/
		Spreadsheet(std::string, std::string, std::string);


		/*
		  Returns the id of this spreadsheet.
		*/
		std::string get_id();
		/*
		  Returns the name of the spreadsheet
		*/
		std::string get_ss_name();
		/*
			Sets the name of this spreadsheet
		*/
		void set_ss_name(std::string);
		/*
			Enumerates the names of all the non-empty cells in the spreadsheet
		*/	
		std::unordered_set<std::string> get_nonempty_cells();
		/*
			returns the cell content as a string
		*/
		std::string get_cell_contents(std::string);
		/*
			Saves the spreadsheet to disk.
		*/
		void save();
		/*
			Sets the string content of the cell. Returns true if the sheet was altered.
			Otherwise returns false, meaning that a circular dependency was caught and the sheet was not altered.
		*/
		bool set_cell_contents(std::string, std::string);
		
	
		
		
		
		
	private:
		/*
			Reads the spreadsheet from the file
		*/
		void read_ss_file(std::string);
		/*
			Pulls the variable names out of the string and puts them into a vector
		*/
		std::unordered_set<std::string> get_variables(std::string);
		/*
		  The id # by which the spreadsheet has been assigned. 
		*/
		std::string ss_id;
		/*
			The name in which the spreadsheet has been assigned. 
		*/
		std::string ss_name;
		/*
		  Populates the filled_cells map with a line.
		*/
		void fill_map_from_line(std::string);
		/*
		  Checks a cell names dependees for circular dependencies
		*/
		bool dependency_check(std::string, std::string);
		/*
		  Normalize the cell name to all uppercase.
		*/
		std::string normalize(std::string);
		/*
			The dependency graph for the spreadsheet. 
		*/
		DependencyGraph graph;
				/*
		  The map of string content keyed by a string cell name.
		*/
		std::map<std::string, std::string> filled_cells;
		/*
			indicates if the spreadsheet has been changed
		*/
		bool changed;
		
	};
	
	
}

#endif

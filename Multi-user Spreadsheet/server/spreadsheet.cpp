/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#include <stdio.h>
#include <cstring>
#include <iostream>
#include "spreadsheet.h"
#include<boost/regex.hpp>
#include<fstream>
using namespace std;

namespace SpreadsheetUtilities
{
	/*
	  Default Constructor, creates an empty spreadsheet with no name or id.
	*/
	Spreadsheet::Spreadsheet():
		graph(), filled_cells()
	{
		changed = false;
		ss_id = "0";
		ss_name = "";
	}
	/*
	  Creates an empty spreadsheet with a given name and id
	*/
	Spreadsheet::Spreadsheet(std::string theName, std::string theId):
		graph(), filled_cells()
	{
		changed = false;
		ss_id = theId;
		ss_name = theName;
	}
	/*
	  Creates an empty spreadsheet with a given name and id and filepath which fills
	  the spreadsheet with the contents of the spreadsheet file. 
	*/
	Spreadsheet::Spreadsheet(std::string theName, std::string theId, std::string filePath):
		graph(), filled_cells()
	{
		changed = false;
		ss_id = theId;
		ss_name = theName;
		read_ss_file(filePath);
	}

	/*
	  Returns the id of this spreadsheet.
	*/
	std::string Spreadsheet::get_id()
	{
		return ss_id;
	}
	/*
	  Returns the name of the spreadsheet
	*/
	std::string Spreadsheet::get_ss_name()
	{
		return ss_name;
	}
	/*
	  Sets the name of this spreadsheet
	*/
	void Spreadsheet::set_ss_name(std::string theName)
	{
		ss_name = theName;
	}
	/*
	  Enumerates the names of all the non-empty cells in the spreadsheet
	*/	
	std::unordered_set<std::string> Spreadsheet::get_nonempty_cells()
	{
		//Go through the filled_cells map and pull all of the keys that have a value.
		unordered_set<string> results = unordered_set<string>();
		for (map<string, string>::iterator it = filled_cells.begin(); it != filled_cells.end(); it++)
		{
			results.insert(it->first);
		}
		return results;			
	}
	/*
	  returns the cell content as a string
	*/
	std::string Spreadsheet::get_cell_contents(std::string name)
	{
		//Go into fill_cells and get the value of name
		string n_name;
	    n_name = normalize(name);
		map<string,string>::iterator it;
		it = filled_cells.find(n_name);
		if (it != filled_cells.end())
		{
			return it->second;
		}
		return "";
	}
	/*
	  Saves the spreadsheet to disk using the extension .ss .
	*/
	void Spreadsheet::save()
	{

		string output = "";

		output = "./spreadsheets/" + ss_name;



		//Saves the filled_cells with key and values to a file that is named ss_name.ss
		ofstream ss_out(output, ofstream::trunc);

		for (map<string,string>::iterator it = filled_cells.begin(); it != filled_cells.end(); it++)
		{
			string line = it->first + "\t" + it->second + "\n";
			ss_out << line;
		}
		ss_out.close();
	}
	/*
		Checks whether there is a circular dependency in the spreadsheet. Returns false if there is. 
	*/
	bool Spreadsheet::dependency_check(string orig, string var)
	{
		if(!graph.has_dependents(var))
		{
			return true;
		}
		unordered_set<string> dependents = graph.get_dependents(var);
		for (unordered_set<string>::iterator it = dependents.begin(); it != dependents.end(); ++it)
		{
			if (orig == *it)
			{
				return false;
			}
			if(dependency_check(orig, *it))
			{
				continue;
			}
			else
			{
				return false;
			}
		}
	  	return true; 
	}
	/*
	  Sets the string content of the cell. Returns an unorderedlist 
	  of all of cell names that have been changed by the setting 
	  of the cells contents.
	*/
	bool Spreadsheet::set_cell_contents(std::string name, std::string content)
	{
		unordered_set<std::string> results = unordered_set<std::string>();
		//If the string starts with '=' then graph needs to be updated with depees and depents
		string n_name;
	    n_name = normalize(name);
		string old_content = get_cell_contents(n_name);
		//cout << "Before the IF for =: " << endl;
		if(content[0] == '=')
		{
			//Circle Dependency check
			unordered_set<string> variables = get_variables(content);
			unordered_set<string> normalized_names = unordered_set<string>();
			string normalized_var;
			//Normalize the names
			for (unordered_set<string>::iterator it = variables.begin(); it != variables.end(); ++it)
			{
				normalized_var = normalize(*it);
				//graph.add_dependency(n_name, normalized_var);
				normalized_names.insert(normalized_var);
			}

			variables = normalized_names;
			graph.replace_dependents(n_name, variables);
			//graph.replace_dependents(n_name, variables);
			for (unordered_set<string>::iterator it = variables.begin(); it != variables.end(); ++it)
			{
				//cout << "The depees check: " << endl;
				//cout << *it << endl;
				if (n_name == *it)
				{
					set_cell_contents(name, old_content);
					return false;
				}
				//We have to go through the the dependees of the n_name dependents to make sure
				//That none of them match n_name, if so return false.    
				if(!dependency_check(n_name, *it))
				{
					set_cell_contents(name, old_content);
					return false; 
				}
				
			}
			//Case does not matter in content. It only matters when inputted as a member of graph and 		spreadsheet key
			filled_cells[n_name] = content;
			return true;		 
		}
		else
		{
			//Replace the dependees of the cell that is being updated.
			graph.replace_dependents(n_name, unordered_set<string>());
			graph.replace_dependees(n_name, unordered_set<string>());

			filled_cells[n_name] = content;

			//Pulls the empty cell from the filled_cells map.		
			if(content == "" || content == " ")
			{
				filled_cells.erase(n_name);
				return true;
			}
			return true;
				
		} 
		//Sets the contents of the string in the map filled_cells
	}

	string Spreadsheet::normalize(string name)
	{
		name[0] = toupper(name[0]);
		return name;
	}
	
	/*
	  Reads the spreadsheet from the file. Private method.
	*/
	void Spreadsheet::read_ss_file(std::string name)
	{
		string line;
		ifstream ss_file (name);
		if (ss_file.is_open())
		{
			while (getline (ss_file, line))
			{
				fill_map_from_line(line);  
			}
			ss_file.close();
		}
		else
		{
			cout<< "Could not read file" <<endl;
		}
	}

	void Spreadsheet::fill_map_from_line(string line)
	{
		string name = "";
		string content = "";
		boost::regex expr{"([^\t]+)"};
		boost::regex_token_iterator<std::string::iterator> it{line.begin(), line.end(), expr, 1};
		boost::regex_token_iterator<std::string::iterator> end;
		//Adding the variable names to the vector that will be returned by the method. 
		while (it != end)
		{
			if(name == "")
			{
				name = *it++;
			}
			else
			{
				content = *it++;
			}
		}
		//This means that the file takes in values that do not exist.
		if(name == "" && content == "")
		{
			return;
		}
		filled_cells[name] = content;
	}

	
	/*
	  Pulls the variable names out of the string and puts them into a vector
	*/
	std::unordered_set<std::string> Spreadsheet::get_variables(string formula)
	{
		std::unordered_set<std::string> variables = std::unordered_set<std::string>();
		string var = "";
		boost::regex expr{"([a-zA-Z]{1}[0-9]{1,2})"};
		boost::regex_token_iterator<std::string::iterator> it{formula.begin(), formula.end(), expr, 1};
		boost::regex_token_iterator<std::string::iterator> end;
		//Adding the variable names to the unordered_set that will be returned by the method. 
		while (it != end)
		{
			var = *it++;
			//std::cout << var << '\n';
			variables.insert(var);
		}
		return variables;
	}
}

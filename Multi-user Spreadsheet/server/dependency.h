/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#ifndef DEPENDENCYNODE_H
#define DEPENDENCYNODE_H

#include <stdio.h>
#include <string>
#include <iostream>
#include <map>
#include <unordered_set>

namespace SpreadsheetUtilities
{
	/*
	  A graph node that contains a collection of dependents and dependees.
	  The node knows its identification (name).
	*/
	class DependencyNode
	{
		friend class DependencyGraph;
	public:
		/*
		  Default Constructor
		*/
		DependencyNode();
		/*
		  Creates a new Dependency Node with the given id.
		*/
		DependencyNode(std::string);
		/*
		  Returns the name of this node.
		*/
		std::string get_id();
		/*
		  Returns the collection of dependents
		  represented as strings.
		*/
		std::unordered_set<std::string>  get_dependents();
		/*
		  Returns the dependees represented by strings.
		*/
		std::unordered_set<std::string>  get_dependees();
		/*
		  Adds a dependent of this node if it doesn't already exist.
		*/
		bool add_dependent(std::string);
		/*
		  Adds a dependee of this node, if it doesn't already exist.
		*/
		bool add_dependee(std::string);
		/*
		  Removes the dependency the other node has on this node.
		*/
		bool remove_dependent(std::string);
		/*
		  Removes the dependency this node has on the other.
		*/
	    bool remove_dependee(std::string);
		/*
		  Removes all the dependents of this Node.
		  Will update all dependents' dependees lists
		  that no longer needs this node.
		*/
		void remove_all_dependents(std::map<std::string, DependencyNode*>, std::unordered_set<std::string> &);
		/*
		  Removes all the dependencies of this Node.
		  Will update all dependees' dependents lists that
		  this node no longer needs/
		*/
		void remove_all_dependees(std::map<std::string, DependencyNode*>, std::unordered_set<std::string> &);
		
		
	private:
		/*
		  The name by which this Node will be identified.
		*/
		std::string id;
		/*
		  The unordered_set of Nodes that depend on this node,
		  mapped with the dependees Id's.
		*/
		std::unordered_set<std::string> dependents;
		/*
		  The unordered_set of Nodes that this node depends on,
		  mapped with the dependees Id's.
		*/
		std::unordered_set<std::string> dependees;
	};
	
	class DependencyGraph
	{
  	public:
		/*
		  Creates an empty Dependency graph.
		*/
		DependencyGraph();
		/*
		  Returns the number of dependency relations.
		*/
		int get_size() const;
	    /*
		  Reports whether dependents(s) is non-empty.
		*/
		bool has_dependents(std::string);
		/*
		  Reports whether dependees(s) is non-empty.
		*/
		bool has_dependees(std::string);
		/*
		  Returns a reference to a collection of dependents.
		*/
		std::unordered_set<std::string>  get_dependents(std::string);
		/*
		  Returns a reference to a collection of dependees.
		*/
		std::unordered_set<std::string>  get_dependees(std::string);
		/*
		  Adds the ordered pair (s,t), if it doesn't exist.
		  This is thought of as: t depends of s.
		*/
		void add_dependency(std::string s, std::string t);
		/*
		  Removes the ordered pair (s,t), if it exists.
		*/
		void remove_dependency(std::string s, std::string t);
		/*
		  Removes all existing ordered pairs of the form (s,r).
		  Then, for each t in newDependents, adds the ordered pair (s,t).
		*/
		void replace_dependents(std::string s, std::unordered_set<std::string> newDependents);
		/*
		  Removes all existing ordered pairs of the form (r,s).
		  Then, for each t in newDependees, adds the ordered pair (t,s).
		*/
		void replace_dependees(std::string s, std::unordered_set<std::string> newDependees);
		/*
		  Returns the size of dependees(s).
		  dg["a"] returns sthe size of dependees("a");
		*/
		int operator[](std::string);

	private:
		/*
		  The map of Nodes keyed by the nodes' Ids.
		*/
		std::map<std::string, DependencyNode*> dependencies;
		/*
		  The number of dependency relationships.
		*/
		int dependencyCount;
	};

}

#endif

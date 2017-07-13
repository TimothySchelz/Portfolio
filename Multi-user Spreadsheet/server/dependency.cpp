/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#include "dependency.h"

using namespace std;

namespace SpreadsheetUtilities
{
	DependencyNode::DependencyNode()
		:id(), dependents(), dependees()
	{
	}
	
	DependencyNode::DependencyNode(string _id)
		:id(_id), dependents(), dependees()
	{
	}

	string DependencyNode::get_id()
	{
		return id;
	}

	unordered_set<string> DependencyNode::get_dependents()
	{
		return dependents;
	}

	unordered_set<string> DependencyNode::get_dependees()
	{
		return dependees;
	}

	bool DependencyNode::add_dependent(string dependent)
	{
		bool present = dependents.count(dependent) > 0;
		if (!present)
		{		
			dependents.insert(dependent);
			return true;
		}
		return false;
	}
	
	/*
	  Adds a dependee of this node, if it doesn't already exist.
	*/
	bool DependencyNode::add_dependee(string dependee)
	{
		bool present = dependees.count(dependee) > 0;
		if (!present)
		{
			dependees.insert(dependee);
			return true;
		}
		return false;
	}
	
	/*
	  Removes the dependency the other node has on this node.
	*/
	bool DependencyNode::remove_dependent(string dependent)
	{
		bool present = dependents.count(dependent) > 0;
		if (present)
		{
			dependents.erase(dependent);
			return true;
		}
		return false;
	}
	/*
	  Removes the dependency this node has on the other.
	*/
	bool DependencyNode::remove_dependee(string dependee)
	{
		
		bool present = dependees.count(dependee) > 0;
		if (present)
		{
			dependees.erase(dependee);
			return true;
		}
		return false;
	}
	/*
	  Removes all the dependents of this Node.
	  Will update all dependents' dependees lists
	  that no longer needs this node.
	*/
	void DependencyNode::remove_all_dependents(map<string, DependencyNode*> nodes, unordered_set<string> & removed)
	{
		for ( auto it = dependents.begin(); it != dependents.end(); ++it )
		{
			string name = *it;
			if (nodes.count(name) > 0)
			{
				DependencyNode* node = nodes.at(name);
				node->remove_dependee(this->id);
			}
		}
		removed = dependents;
		dependents = unordered_set<string>();
	}
	/*
	  Removes all the dependencies of this Node.
	  Will update all dependees' dependents lists that
	  this node no longer needs/
	*/
	void DependencyNode::remove_all_dependees(map<string, DependencyNode*> nodes, unordered_set<string> & removed)
	{
		for ( auto it = dependees.begin(); it != dependees.end(); ++it )
		{
			string name = *it;
			if (nodes.count(name) > 0)
			{
				DependencyNode* node = nodes.at(name);
				node->remove_dependent(this->id);
			}
		}
		removed = dependees;
		dependees = unordered_set<string>();
	}

	
	/*################ Dependency Graph ###################*/
	
	/*
	  Creates an empty Dependency graph.
	*/
	DependencyGraph::DependencyGraph()
		:dependencies(), dependencyCount(0)
	{
	}
	/*
	  Returns the number of dependency relations.
	*/
	int DependencyGraph::get_size() const
	{
		return dependencyCount;
	}
	/*
	  Reports whether dependents(s) is non-empty.
	*/
	bool DependencyGraph::has_dependents(string s)
	{
		map<string, DependencyNode*>::iterator it;
		it = dependencies.find(s);
		if (it == dependencies.end())
		{
			return false;
		}
		return it->second->get_dependents().size() > 0;
	}
	/*
	  Reports whether dependees is non-empty.
	*/
	bool DependencyGraph::has_dependees(string s)
	{
		map<string,DependencyNode*>::iterator it;

		it = dependencies.find(s);
		if (it == dependencies.end())
		{
			return false;
		}
		return it->second->get_dependees().size() > 0;
	}
	/*
	  Returns a reference to a collection of dependents.
	*/
	std::unordered_set<string> DependencyGraph::get_dependents(string s)
	{	
		map<string,DependencyNode*>::iterator it;

		it = dependencies.find(s);
		if (it != dependencies.end())
		{
			return it->second->dependents;	
		}
		return unordered_set<string>();
	}
	/*
	  Returns a reference to a collection of dependees.
	*/
	std::unordered_set<string> DependencyGraph::get_dependees(string s)
	{
		
		map<string,DependencyNode*>::iterator it;

		it = dependencies.find(s);
		if (it != dependencies.end())
		{
			return it->second->dependees;			
		}
		return unordered_set<string>();		
	}
	/*
	  Adds the ordered pair (s,t), if it doesn't exist.
	  This is thought of as: t depends of s.
	*/
	void DependencyGraph::add_dependency(string s, string t)
	{
		DependencyNode* dependent;
		DependencyNode* dependee;
		
		map<string,DependencyNode*>::iterator it;
		//Add dependent to map
		it = dependencies.find(t);
		if (it == dependencies.end())
		{
			dependent = new DependencyNode(t);
			dependencies.insert(pair<string,DependencyNode*>(t,dependent));
		}
		else
		{
			dependent = it->second;
		}
		//Add dependee to map
		it = dependencies.find(s);
		if (it == dependencies.end())
		{
			dependee = new DependencyNode(s);
			dependencies.insert(pair<string,DependencyNode*>(s,dependee));
		}
		else
		{
			dependee = it->second;
		}
		//Create relationship and adjust size
		if (dependent->add_dependee(dependee->get_id()) && dependee->add_dependent(dependent->get_id()))
		{
			dependencyCount++;
		}
	}
	/*
	  Removes the ordered pair (s,t), if it exists.
	*/
	void DependencyGraph::remove_dependency(string s, string t)
	{
		DependencyNode* dependent;
		DependencyNode* dependee;

		map<string,DependencyNode*>::iterator it;

		it = dependencies.find(t);
		if (it == dependencies.end())
		{
			return;
		}
		else
		{
			dependent = it->second;
		}

		it = dependencies.find(s);
		if (it == dependencies.end())
		{
			return;
		}
		else
		{
			dependee = it->second;
		}

		//remove relationship and adjust size
		if (dependent->remove_dependee(dependee->get_id()) && dependee->remove_dependent(dependent->get_id()))
		{
			dependencyCount--;
		}
		
		//Update dictionary of Nodes by removing independent nodes
		if (!has_dependees(dependent->get_id()) && !has_dependents(dependent->get_id()))
		{
			dependencies.erase(dependent->get_id());
		}
		if (!has_dependees(dependee->get_id()) && !has_dependents(dependee->get_id()))
		{
			dependencies.erase(dependee->get_id());
		}
	}
	/*
	  Removes all existing ordered pairs of the form (s,r).
	  Then, for each t in newDependents, adds the ordered pair (s,t).
	*/
	void DependencyGraph::replace_dependents(string s, unordered_set<string> newDependents)
	{
		DependencyNode* dependee;
		//Get node in question
		map<string,DependencyNode*>::iterator it;
		it = dependencies.find(s);
		if (it == dependencies.end())
		{
			dependee = new DependencyNode(s);
			dependencies[s] = dependee;
		}
		else
		{
			dependee = dependencies[s];
		}

		//Remove Dependents
		unordered_set<string> removedDeps;
		dependee->remove_all_dependents(dependencies, removedDeps);
		//Adjust size
		dependencyCount -= removedDeps.size();

		//Add new Dependents
		unordered_set<string>::iterator n_it;
		for (n_it = newDependents.begin(); n_it != newDependents.end(); n_it++)
		{
			DependencyNode* newNode;
			it = dependencies.find(*n_it);
			if (it == dependencies.end())
			{
				newNode = new DependencyNode(*n_it);
				dependencies[*n_it] =  newNode;
			}
			else
			{
				newNode = dependencies[*n_it];
			}

			//Adjust size
			if (newNode->add_dependee(dependee->get_id()) && dependee->add_dependent(newNode->get_id()))
			{
				dependencyCount++;
			}
		}

		//Update dictionary of nodes
		unordered_set<string>::iterator r_it;
		for (r_it = removedDeps.begin(); r_it != removedDeps.end(); r_it++)
		{
			if (!has_dependees(*r_it) && !has_dependents(*r_it))
			{
				dependencies.erase(*r_it);
			}
		}	  
	}
	/*
	  Removes all existing ordered pairs of the form (r,s).
	  Then, for each t in newDependees, adds the ordered pair (t,s).
	*/
	void DependencyGraph::replace_dependees(string s, unordered_set<string> newDependees)
	{
		DependencyNode* dependent;
		//Get node in question
		map<string,DependencyNode*>::iterator it;
		it = dependencies.find(s);
		if (it == dependencies.end())
		{
			dependent = new DependencyNode(s);
			dependencies[s] = dependent;
		}
		else
		{
			dependent = dependencies[s];
		}

		//Remove Dependeees
		unordered_set<string> removedDees;
		dependent->remove_all_dependees(dependencies, removedDees);
		//Adjust size
		dependencyCount -= removedDees.size();

		//Add new Dependees
		unordered_set<string>::iterator n_it;
		for (n_it = newDependees.begin(); n_it != newDependees.end(); n_it++)
		{
			DependencyNode* newNode;
			it = dependencies.find(*n_it);
			if (it == dependencies.end())
			{
				newNode = new DependencyNode(*n_it);
				dependencies[*n_it] =  newNode;
			}
			else
			{
				newNode = dependencies[*n_it];
			}

			//Adjust size
			if (dependent->add_dependee(newNode->get_id()) && newNode->add_dependent(dependent->get_id()))
			{
				dependencyCount++;
			}
		}

		//Update dictionary of nodes
		unordered_set<string>::iterator r_it;
		for (r_it = removedDees.begin(); r_it != removedDees.end(); r_it++)
		{
			if (!has_dependees(*r_it) && !has_dependents(*r_it))
			{
				dependencies.erase(*r_it);
			}
		}
	}
	/*
	  Returns the size of dependees(s).
	  dg["a"] returns sthe size of dependees("a");
	*/
	int DependencyGraph::operator[](string s)
	{
		//Find the node in question
		map<string, DependencyNode*>::iterator it;
		it = dependencies.find(s);
		if (it != dependencies.end())
		{
			//Return the size of the node's dependees
			return it->second->get_dependees().size();
		}
		//Node DNE, so size is 0
		return 0;
	}
}



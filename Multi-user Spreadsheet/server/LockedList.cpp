/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#include <mutex> 
#include <thread>
#include <chrono>
#include "LockedList.h"

using namespace std;

namespace TBD
{
  LockedList::LockedList() : list()
  {
  }

  LockedList :: ~LockedList()
  {
  }

  bool LockedList::add(Client newClient)
  {
    mtx.lock();
    list.push_back(newClient);
    mtx.unlock();
  }

  bool LockedList::remove(int ClientID)
  {
    mtx.lock();

    // Declare an iterator to a vector of Clients
    vector<Client>::iterator it;  
	
    //Iterate through the client list and remove the desired client
    for (it = list.begin(); it < list.end(); ++it)
      {	
        if (it->getID() == ClientID)
	  {
	    list.erase(it);
	    break;
	  }
      }

    mtx.unlock();
  }

  /*
   * Be careful!  The client itself is not locked here.  The important parts of the 
   * client should be locked inside the client class!
   */
  Client* LockedList::getClient(int ClientID)
  {
	  mtx.lock();

    // Declare an iterator to a vector of Clients
    vector<Client>::iterator it;  
	
    //Iterate through the client list and remove the desired client
    for (it = list.begin(); it < list.end(); ++it)
	{	
		if (it->getID() == ClientID)
		{
			return &(*it);
		}
	}
	
    mtx.unlock();
  }

  int LockedList::size()
  {
    return list.size();
  }

  int LockedList::length()
  {
    return list.size();
  }
}

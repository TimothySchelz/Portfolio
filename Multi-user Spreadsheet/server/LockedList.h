/*
 * Operation Bundt Cake 
 * Naser Abu-Rmaileh, David Reeves, Timothy Schelz, Scott Steadham
 * Spring 2017
 */
#ifndef LOCKEDLIST_H
#define LOCKEDLIST_H

#include <vector>
#include "Client.h"
#include <mutex>

namespace TBD
{
	/*
	 * A list that is internally locked.  Be careful with getClient
	 */
  class LockedList
  {
  public:

	  /*
	   * constructor and destructor
	   */
    LockedList();
    ~LockedList();
    
	/*
	 * Add a client to the list.
	 */
    bool add(Client newClient);

	/*
	 * Remove a client from the list with the given ID
	 * Returns true if the list was changed
	 */
    bool remove(int ClientID);
	
	/* 
	 * returns a pointer to a client with the given ID. 
	 *
	 * Warning!!! The client that the return value points to is not locked!
	 * The client could be changed by another thread.
	 */
    Client *getClient(int ClientID);
    

	/*
	 * Returns the number of clients in the list.
	 */
	int size();
    int length();
    
  private:
	// The list backing this data struct
    std::vector<Client> list;

	// The mutex to lock it.
    std::mutex mtx;
  };
}
#endif

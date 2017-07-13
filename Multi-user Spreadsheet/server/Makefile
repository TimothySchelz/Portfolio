all: server

run: 
	./server.out

server:	Server.cpp Server.h Network.cpp Network.h Client.cpp Client.h main.cpp LockedList.h LockedList.cpp Splitter.cpp Splitter.h spreadsheet.cpp spreadsheet.h dependency.cpp dependency.h
	g++ -o server.out main.cpp -std=c++11 Network.cpp Server.cpp Client.cpp LockedList.cpp spreadsheet.cpp dependency.cpp Splitter.cpp /usr/local/lib/libboost_regex.a


clean-test:
	rm -f *.o *.out


clean: clean-test
	$(RM) $(EXAMPLES) $(OBJS)

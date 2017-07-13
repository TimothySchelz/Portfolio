using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkController
{
    /// <summary>
    /// This class holds all the necessary state to handle a client connection
    /// Note that all of its fields are public because we are using it like a "struct"
    /// It is a simple collection of fields
    /// </summary>
    public class SocketState
    {
        /// <summary>
        /// The socket of the stocket state.
        /// </summary>
        public Socket theSocket
        {
            get; set;
        }
        /// <summary>
        /// The ID number for the socket state.
        /// </summary>
        public int ID
        {
            get; set;
        }

        // This is the buffer where we will receive message data from the client
        public byte[] messageBuffer = new byte[16384];

        // This is a larger (growable) buffer, in case a single receive does not contain the full message.
        public StringBuilder sb = new StringBuilder();

        /// <summary>
        /// The action delegate for the socketState
        /// </summary>
        public Action<SocketState> CallMe
        {
            get; set;
        }
        /// <summary>
        /// Indicates if the socketState is connected to a server.
        /// </summary>
        public bool Connected
        {
            get; set;
        }
        /// <summary>
        /// Constructs a SocketState. Takes in a Stocket and an id.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="id"></param>
        public SocketState(Socket s, int id)
        {
            theSocket = s;
            ID = id;
            Connected = false;
        }
        /// <summary>
        /// Default constructor for the socketState.
        /// </summary>
        public SocketState()
        {

        }

    }
}
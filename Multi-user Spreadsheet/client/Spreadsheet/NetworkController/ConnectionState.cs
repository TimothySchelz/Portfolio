using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkController
{
    /// <summary>
    /// Represents the ConnectionState of the server.
    /// </summary>
    class ConnectionState
    {
        /// <summary>
        /// The TCP listener for the server.
        /// </summary>
        public TcpListener Listener
        {
            get; set;
        }
        /// <summary>
        /// The action delegate for the server.
        /// </summary>
        public Action<SocketState> CallMe
        {
            get; set;
        }
    }
}
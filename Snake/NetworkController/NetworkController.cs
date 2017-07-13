// Created by Gray Marchese, u0884194, and Timothy Schelz, u0851027
// Last Date Updated: 12/8/16
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        
        public Socket theSocket;
        public int ID;
        public Callback CallMe;
        public Callback SendCallback;
        public Callback DisconnectCallback;
        public delegate void Callback(SocketState State);


        /// <summary>
        /// Keeps track of the size of the buffer
        /// </summary>
        public int BufferSize
        {
            get;
            set;
        }

        // This is the buffer where we will receive message data from the client
        public byte[] messageBuffer = new byte[1024];

        // This is a larger (growable) buffer, in case a single receive does not contain the full message.
        public StringBuilder sb = new StringBuilder();

        /// <summary>
        /// Creates a SocketState from a socket and the ID of the socket
        /// </summary>
        /// <param name="s"></param>
        /// <param name="id"></param>
        public SocketState(Socket s, int id)
        {
            theSocket = s;
            ID = id;
            BufferSize = 1024;
        }
    }

    /// <summary>
    /// Saves the state of the TCP listener and the current callback action.
    /// </summary>
    public class listenerState
    {
        //The TCP listener curretnly in use by the server
        public TcpListener theListener
        {
            get;
            private set;
        }

        //Current callback action to be completed when a client connects
        public Callback CallMe
        {
            get;
            set;
        }

        //Type for our callback function
        public delegate void Callback(SocketState State);

        //Single parameter contructor for our TCP listenerState.
        public listenerState(TcpListener lstnr, Callback CallMe)
        {
            this.theListener = lstnr;
            this.CallMe = CallMe;

            //Starts the listener
            theListener.Start();
        }
    }

    /// <summary>
    /// A static class of methods to help with networking.  It uses the SocketState class
    /// </summary>
    public static class Networking
    {

        public const int DEFAULT_PORT = 11000;

        /// <summary>
        /// Connects to a server at the given address and then performs the Action
        /// </summary>
        /// <param name="Action">Action to be performed hen connection occurs</param>
        /// <param name="Address">IP address of the desired server</param>
        /// <returns>The SocketState that is created by the connection</returns>
        public static SocketState ConnectToServer(SocketState.Callback Action, String Address)
        {


            System.Diagnostics.Debug.WriteLine("connecting  to " + Address);


            SocketState resultSocket;
            // Connect to a remote device.
            try
            {

                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;
                IPAddress ipAddress = IPAddress.None;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(Address);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid address: " + Address);
                        return null;
                    }
                }
                catch (Exception e1)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(Address);
                }

                // Create a TCP/IP socket.
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // set some options
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                // creates the SocketState
                resultSocket = new SocketState(socket, -1);

                // Sets the Callback function
                resultSocket.CallMe = Action;

                // Begins event loop
                resultSocket.theSocket.BeginConnect(ipAddress, Networking.DEFAULT_PORT, ConnectedCallback, resultSocket);

            }
            // catches when stuff breaks
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                throw e;
            }

            // return the socketstate
            return resultSocket;
        }

        /// <summary>
        /// This function is "called" by the operating system when the remote site acknowledges connect request
        /// </summary>
        /// <param name="ar"></param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            // Extract the SocketState
            SocketState ss = (SocketState)ar.AsyncState;

            try
            {
                // Complete the connection.
                ss.theSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                // Connection failed
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return;
            }

            // Do whatever the socket state wants us to do when we finally connect
            ss.CallMe(ss);

            // Starting the event loop and receiving data
            ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, Networking.ReceiveCallback, ss);
        }

        /// <summary>
        /// Method to be called when data has been received
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;

            int bytesRead = 0;

            try
            {
                //A client may be disconnected
                bytesRead = ss.theSocket.EndReceive(ar);        
            }
            catch(SocketException se)
            {
                ss.DisconnectCallback(ss);
            }

            // If the socket is still open
            if (bytesRead > 0)
            {
                string theMessage = Encoding.UTF8.GetString(ss.messageBuffer, 0, bytesRead);
                // Append the received data to the growable buffer.
                // It may be an incomplete message, so we need to start building it up piece by piece
                ss.sb.Append(theMessage);

                ss.CallMe(ss);
            }
        }

        /// <summary>
        /// Start receiving more data from the socket
        /// </summary>
        /// <param name="ss">The socketstate to receive data from</param>
        public static void GetData(SocketState ss)
        {

            // Starts the event loop again to receive data
            ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, Networking.ReceiveCallback, ss);
        }

        /// <summary>
        /// Send data over the given socket.
        /// </summary>
        /// <param name="ss">The socketstate that the data should be sent to</param>
        /// <param name="data">The data to be sent</param>
        public static void SendData(SocketState ss, String data)
        {
            // Convert the string data to a Byte Array
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);

            // Send the data
            ss.theSocket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, Networking.SendCallback, ss);
        }

        /// <summary>
        /// This Callback goes off once data has been sent 
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            // pull out the socket state
            SocketState ss = (SocketState)ar.AsyncState;

            // End the send
            ss.theSocket.EndSend(ar);

            // Check if sendcallback is null
            if (ss.SendCallback != null)
            {
                // Do whatever the socketstate wants to do after finishing sending
                ss.CallMe(ss);
            }
            
        }


        /*
        BELOW METHODS ARE FOR THE SERVER'S USE ONLY
        */

        /// <summary>
        /// A method to be called as the server starts. This will assure that the server is 
        /// constently listenting fr clients and adds them when a connection is recognized by
        /// a TCP listener.
        /// </summary>
        /// <param name="CallMe"></param>
        public static void ServerAwaitingClientLoop(listenerState.Callback CallMe)
        {
            System.Diagnostics.Debug.WriteLine("Listening for connections");

            //Creates TCP listener
            TcpListener lstnr = new TcpListener(IPAddress.Any, DEFAULT_PORT);

            //Stores the state of the listener in a listener state
            listenerState ls = new listenerState(lstnr, CallMe);


            //Begins an event loop for listening for IP addresses.
            ls.theListener.BeginAcceptSocket(AcceptNewClient, ls);
        }

        /// <summary>
        /// Callback to be used when someone tries to connect to the server.  Performs the necessary junk and then 
        /// calls the ListenerState's callback
        /// </summary>
        /// <param name="ar"></param>
        private static void AcceptNewClient(IAsyncResult ar)
        {
            System.Diagnostics.Debug.WriteLine("Client connected");
            //retrievs the listener state that triggered this method
            listenerState ls = (listenerState)ar.AsyncState;

            //gets socket from the established connection
            Socket socket = ls.theListener.EndAcceptSocket(ar);

            //Grabs the IP address from the remote end of our socket
            IPAddress ip = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());

            //Creates a socket state
            SocketState ss = new SocketState(socket, ip.GetHashCode());

            //Perfrom's callback method retrieved from the newly connected socket
            ls.CallMe(ss);

            //Restarts the event loop to listen for more clients
            ls.theListener.BeginAcceptSocket(AcceptNewClient, ls);
        }
    }

}

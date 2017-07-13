using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

namespace NetworkController
{
    public static class Networking
    {
        public const int DEFAULT_PORT = 2112;

        /// <summary>
        /// Creates a new listener on the default port, sets the action delegate of the ConnectionState
        /// and begines accepting a socket.
        /// </summary>
        /// <param name="action"></param>
        public static void ServerAwaitingClientLoop(Action<SocketState> action)
        {
            Console.WriteLine("The Server is waiting for a client");
            TcpListener listener = new TcpListener(IPAddress.Any, DEFAULT_PORT);
            listener.Start();
            ConnectionState cs = new ConnectionState();
            cs.Listener = listener;
            cs.CallMe = action;
            listener.BeginAcceptSocket(AcceptNewClient, cs);
            Console.WriteLine("The Server is waiting for a client");
        }

        /// <summary>
        /// Accepts a new client onto the server. Uses the action delegate from the 
        /// connectionState and Begins the Accept Socket again for more clients.
        /// </summary>
        /// <param name="ar"></param>
        public static void AcceptNewClient(IAsyncResult ar)
        {
            Console.WriteLine("A client has started connection to the Server");
            Console.Read();
            ConnectionState cs = (ConnectionState)ar.AsyncState;
            Socket socket = cs.Listener.EndAcceptSocket(ar);
            SocketState ss = new SocketState();
            ss.theSocket = socket;
            ss.CallMe = cs.CallMe;
            ss.CallMe(ss);
            cs.Listener.BeginAcceptSocket(AcceptNewClient, cs);
        }
        
        /// <summary>
        /// hostname the
        ///name of the server to connect to
        ///callbackFunction a
        ///function inside the View to be called when a connection is made
        ///This function should attempt to connect to the server via a provided hostname.It should save the callback function (in a socket state object) for use
        ///when data arrives.
        ///It will need to open a socket and then use the BeginConnect method. Note this method takes the "state" object and "regurgitates" it back to you when a
        ///connection is made, thus allowing "communication" between this function and the ConnectedToServer function.
        /// </summary>
        /// <param name="callbackFunction"></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static Socket ConnectToServer(Action<SocketState> action, string hostName)
        {
            System.Diagnostics.Debug.WriteLine("connecting  to " + hostName);

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;
                IPAddress ipAddress = IPAddress.None;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
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
                        System.Diagnostics.Debug.WriteLine("Invalid addres: " + hostName);

                        return null;
                    }
                }
                catch (Exception)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                // Create a TCP/IP socket.
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                //Create a new SocketState with the socket
                SocketState theServer = new SocketState(socket, -1);
                //Set the CallMe delegate to the action.
                theServer.CallMe = action;

                theServer.theSocket.BeginConnect(ipAddress, Networking.DEFAULT_PORT, ConnectedToServer, theServer);

                return theServer.theSocket;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return null;
            }
        }



        /// <summary>
        /// 
        /// This function is referenced by the BeginConnect method above and 
        /// is "called" by the OS when the socket connects to the server. The
        ///"ar" object contains a field "AsyncState" which 
        ///contains the "state" object saved away in the above function.
        ///Once a connection is established the "saved away" callbackFunction 
        ///needs to be called. This function is saved in the socket state, and was 
        ///originally passed in to ConnectToServer.
        /// </summary>
        /// <param name="ar"></param>
        public static void ConnectedToServer(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;

            try
            {
                // Complete the connection.
                ss.theSocket.EndConnect(ar);
                ss.Connected = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                //Set connected to false because the socket did not connect to a server.
                ss.Connected = false;
            }

            // TODO: If we had a "EventProcessor" delagate stored in the state, we could call that,
            //       instead of hard-coding a method to call.
            ss.CallMe(ss);
            //Check if the socketState is connected.
            if (ss.Connected == true)
            {
                ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
            }
        }

        /// <summary>
        /// The ReceiveCallback method is called by the OS when new data arrives. This method should check to see how much data has arrived. If 0, the
        ///connection has been closed(presumably by the server). On greater than zero data, this method should call the callback function provided above.
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void ReceiveCallback(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;
            //Check if the socket is connected
            if (!ss.theSocket.Connected)
            {
                return;
            }
            int bytesRead = 0;
            try
            {
                bytesRead = ss.theSocket.EndReceive(ar);
            }
            catch (Exception)
            {
                //MessageBox.Show("The server disconnected.");
                //The socket is no longer connected to the server.
                ss.Connected = false;
            }

            // If the socket is still open
            if (bytesRead > 0)
            {
                string theMessage = Encoding.UTF8.GetString(ss.messageBuffer, 0, bytesRead);
                // Append the received data to the growable buffer.
                // It may be an incomplete message, so we need to start building it up piece by piece
                ss.sb.Append(theMessage);
            }
            else
            {
                ss.Connected = false;
                EndConnection(ss);
                return;
            }
            // Continue the "event loop" that was started on line 154.
            // Start listening for more parts of a message, or more new messages
            ss.CallMe(ss);

        }


        /// <summary>
        /// This is a small helper function that the client View code will call whenever it wants more data. Note: the client will probably want more data every time
        ///it gets data, and has finished processing it in its callbackFunction.
        /// </summary>
        /// <param name=""></param>
        public static void GetData(SocketState ss)
        {
            //Check if the socket is connected.
            if (!ss.theSocket.Connected)
            {
                return;
            }
            ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
        }


        /// <summary>
        /// This function (along with its helper 'SendCallback') will allow 
        /// a program to send data over a socket. This function needs to convert 
        /// the data into bytes and then send them using socket.BeginSend.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send    (SocketState ss, String data)
        {
            Console.WriteLine("Data is being sent to the client");
            Console.Read();
            //Converts string data into an array of bytes
            byte[] theData = Encoding.UTF8.GetBytes(data);
            if (ss.theSocket.Connected)
            {
                //Sends theData array to the server.
                ss.theSocket.BeginSend(theData, 0, theData.Length, SocketFlags.None, SendCallback, ss);
            }
        }

        /// <summary>
        /// This function "assists" the Send function. If all the data 
        /// has been sent, then life is good and nothing needs to be done 
        /// (note: you may, when first prototyping your program, put a 
        /// WriteLine in here to see when data goes out).
        /// </summary>
        public static void SendCallback(IAsyncResult ar)
        {

            SocketState ss = (SocketState)ar.AsyncState;
            // Nothing much to do here, just conclude the send operation so the socket is happy.
            if (ss.theSocket.Connected)
            {
                ss.theSocket.EndSend(ar);
            }
        }
        /// <summary>
        /// Ends the connection to the server and allows the socket to be reused.
        /// </summary>
        /// <param name="endConnection"></param>
        public static void EndConnection(SocketState ss)
        {
            if (ss.theSocket.Connected || !ss.Connected)
            {
                ss.theSocket.Shutdown(SocketShutdown.Both);
                ss.theSocket.Disconnect(true);
            }
        }
    }
}

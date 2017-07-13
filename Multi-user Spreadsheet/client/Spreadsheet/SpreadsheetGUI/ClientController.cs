using NetworkController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SS; 
using SpreadsheetUtilities;
using System.Threading;
using System.Drawing;
using SpreadsheetGUI;
using System.Windows.Forms.DataVisualization.Charting;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Authors: Scott Steadham and David Reeves
    /// </summary>
    public class ClientController
    {
        /// <summary>
        /// Represents the socketState that the server is on.
        /// </summary>
        private SocketState theServer;
        /// <summary>
        /// Address of the server.
        /// </summary>
        private string serverIP;
        /// <summary>
        /// The clientID of this client
        /// </summary>
        private string clientID;
        /// <summary>
        /// The controller that handles all user input including the view.
        /// This controller has a collection of documents keyed by their
        /// docID.
        /// </summary>
        private SSController ss_controller;
        
        /// <summary>
        /// Constructs a clientController. Every clienController is connected via socket connection to a
        /// server. 
        /// </summary>
        /// <param name="_view"></param>
        public ClientController()
        {
            serverIP = "";
            clientID = "";
        }

        public void SetClientController(SSController c)
        {
            ss_controller = c;
        }

        /// <summary>
        /// Begins the connection process between our client and the server.
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="_name"></param>
        public void BeginConnection(string serverAddress)
        {
            serverIP = serverAddress;
            //Connects to server and gets the socket that is connected to the server.
            theServer = new SocketState(NetworkController.Networking.
                ConnectToServer(FirstContact, serverAddress), -1);        
        }

        /// <summary>
        ///  This method indicates whether the client is connected to the server. There may be a slite delay.
        /// </summary>
        internal bool isConnected()
        {
            return theServer.Connected;
        }

        /// <summary>
        /// The first step in the handshake between the clientController
        /// and the server. Sets the action delegate to Recieve start up
        /// and sends the server the socket and the players name.
        /// </summary>
        /// <param name="ar"></param>
        public void FirstContact(SocketState ss)
        {
            if (ss.Connected == false)
            {
                MessageBox.Show("There was a problem connecting to the server. Please try again.");

                return;
            }
            ss.CallMe = ReceiveStartupClientID;
        }

        /// <summary>
        /// recieves the information needed from the server to start up the snake game
        /// and sets the action delegate to recieveworld and gets the data from the 
        /// server.
        /// </summary>
        /// <param name="ar"></param>
        /// <summary>
        /// Recieves the information needed from the server to start up the snake game
        /// and sets the action delegate to recieveWorld and gets the Data from the 
        /// server. The loop will not start until the server sends the Startup Info.
        /// </summary>
        /// <param name="ar"></param>
        public void ReceiveStartupClientID(SocketState ss)
        {
            //Checks to see if the socket is connected.
            if (ss.theSocket.Connected == false)
            {
                MessageBox.Show("There was a problem connecting to the server. Please try again.");
                return;
            }

            //Get first contact from server.
            //This is the time that the client ID is received from the server. 
            string totalData = ss.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            //RETRIEVE THE ClientID from what is sent from the server. 
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0)
                {
                    continue;
                }
                if (parts[i][parts[i].Length - 1] != '\n')
                {
                    break;
                }
                if (i == 0)
                {
                    clientID = parts[i];
                    //Sends the name of the user to the server. 
                    Networking.Send(theServer, ss_controller.GetUsername() + "\n");
                }
               ss.sb.Remove(0, parts[i].Length);
            }

            //Client is now connected
            ss_controller.NowConnected();

            ss.CallMe = ReceiveUpdates;

            ////The GetData waits for information from the server to be received. 
            Networking.GetData(ss);
        }

        /// <summary>
        /// Receives the spreadsheet information from the server and processes it for the 
        /// client. This is called everytime the server sends info to the spreadsheet
        /// </summary>
        /// <param name="ar"></param>
        public void ReceiveUpdates(SocketState ss)
        {
            //Check the sockets connection.
             if (ss.Connected == false)
            {
                MessageBox.Show("There was a problem connecting to the server. Please try again.");
                return;
            }
            string totalData = ss.sb.ToString();
            string[] messages = Regex.Split(totalData, @"(?<=[\n])");
            string[] parts;
                     
            for (int i = 0; i < messages.Length; i++)
            {
                //An empty message from the server has been sent.
                if (messages[i].Length == 0)
                {
                    break;
                }
                //This means the only a partial message has been received by the client.
                if (messages[i][messages[i].Length - 1] != '\n')
                {
                    break;
                }
                string opCode="";
                //parts = Regex.Split(messages[i], @"(?<=[\t])");
                parts = messages[i].Split('\t', '\n');
                //Process a single message
                if (parts.Length > 0)
                {
                    opCode = parts[0];
                    processMessage(parts);
                }
                //processMessage(parts);
                ss.sb.Remove(0, messages[i].Length);
            }
            Networking.GetData(ss);
        }
        /// <summary>
        /// Processes the message from the server depending upon the opCode in the parts array.
        /// </summary>
        /// <param name="parts"></param>
        private void processMessage(string[] parts)
        {
            //THE OPCODE SHOULD ALWAYS BE parts[0].
            string opCode = parts[0];
            string docID = "";
            string cellName = "";
            string newContent = "";
            string userID = "";
            string userName = "";
            switch (opCode)
                {
                    //File List Names
                    case "0":
                        HashSet<string> s_files = new HashSet<string>();
                        for (int i = 1; i < parts.Length; i++)
                        {
                            if (parts[i].Length > 0)
                            {
                                s_files.Add(parts[i]);
                            }
                        }
                        ss_controller.ShowOpenForm(s_files);
                        break;
                    //VALID NEW
                    case "1":
                        docID = parts[1];
                        ss_controller.createSpreadsheet(docID);
                        break;
                    //VALID OPEN
                    case "2":
                        docID = parts[1];
                        ss_controller.createSpreadsheet(docID);
                        break;
                    //CELL UPDATE
                    case "3":
                        docID = parts[1];
                        cellName = parts[2];
                        cellName = cellName.ToUpper();
                        newContent = parts[3];
                        //This will update the spreadsheet when an Cell update comes.    
                        ss_controller.UpdateCell(docID, cellName, newContent);
                        break;
                    //VALID EDIT
                    case "4":
                        docID = parts[1];
                        //Do nothing the cell changes are valid and are already done on this client. 
                        ss_controller.EnableSpreadsheet(docID);
                        break;
                    //INVALID EDIT
                    case "5":
                        docID = parts[1];
                        ss_controller.InvalidEdit(docID);
                        break;
                    //RENAME
                    case "6":
                        docID = parts[1];
                        string filename = parts[2];
                        ss_controller.ChangeFilename(docID, filename);
                        break;
                    //SAVE
                    case "7":
                        docID = parts[1];
                        ss_controller.MarkAsSaved(docID);
                        break;
                    //VALID RENAME
                    case "8":
                        //Do nothing! The client already changed its representation of the name.
                        break;
                    //INVALID RENAME
                    case "9":
                        //Change the name back to what it was because it is not valid
                        docID = parts[1];
                        ss_controller.ChangeFilenameBack(docID);
                        break;
                    //Edit Location (of another client on your spreadsheet).
                    case "A":
                        if (parts.Length < 5)
                        {
                            break;
                        }
                        docID = parts[1];
                        cellName = parts[2];
                        cellName = cellName.ToUpper();
                        userID = parts[3];
                        userName = parts[4];
                        //Don't edit location of current client.
                        if (userID.Equals(this.clientID))
                        {
                            break;
                        }
                        //ss_controller removes userID if cellName == -1
                        ss_controller.EditLocation(docID, userID, userName, cellName);
                        break;
                    default:
                        break;
                }
        }

        private HashSet<string> ExtractFiles(string files)
        {
            return new HashSet<string>(files.Split('\n'));
        }

        /// <summary>
        /// Sends a message to the server. Can take in cell information or filenames with the operation code that is desired from the client to the server.
        /// OpCodes: 0-File List, 1-New, 2-Open, 3-Edit (needs values for col and row)., 4-UNDO, 5- REDO, 6-Save, 7-Rename, 8-Edit Location.
        /// </summary>
        /// <param name="direction"></param>
        internal void SendssMessage(string docID, string edit, int col, int row, int opCode)
        {
           string cellName = "";
           cellName = ss_controller.GetCellName(row, col);
            
            try
            {
                if (theServer.theSocket.Connected)
                {
                    switch (opCode)
                    {
                        //File List
                        case 0:
                            Networking.Send(theServer, ""+ opCode +"\n");
                            break;
                        //NEW
                        case 1:
                            //"1\tFileName\n"
                            Networking.Send(theServer, "" + opCode + "\t" + edit + "\n");
                            //Disable the current spreadsheet when the new one is called.
                            break;
                        //Open
                        case 2:
                            Networking.Send(theServer, "" + opCode + "\t" + edit + "\n");
                            break;
                        //Edit
                        case 3:
                            Networking.Send(theServer, "" + opCode + "\t" + docID + "\t" + cellName + "\t" + edit + "\n");
                            ss_controller.DisableSpreadsheet(docID);
                            break;
                        //Undo
                        case 4:
                            Networking.Send(theServer, "" + opCode + "\t" + docID + "\n");
                            break;
                        //Redo
                        case 5:
                            Networking.Send(theServer, "" + opCode + "\t" + docID + "\n");
                            break;
                        //Save
                        case 6:
                            Networking.Send(theServer, "" + opCode + "\t" + docID + "\n");
                            break;
                        //Rename
                        case 7:
                            Networking.Send(theServer, "" + opCode + "\t" + docID + "\t" + edit + "\n");
                            break;
                        //Edit Location
                        case 8:
                            Networking.Send(theServer, "" + opCode + "\t" + docID + "\t" + cellName+"\n");
                            break;
                        //Close
                        case 9:
                            Networking.Send(theServer, "" + opCode + "\t" + docID + "\n");
                            break;
                        default:
                            return;
                    }                   
                }
                else
                {
                    //view.ShowErrorMessage("The client does not have a connection with the server*******. " + "");
                }
            }
            catch(Exception)
            {
             //   view.ShowErrorMessage("The client does not have a connection with the server. " + e.Message);
                //DO SOMETHING TO THE UNCONNECTED SPREADSHEET.
            }
        }

        //internal void setUnValid(int row, int col)
        //{
        //   /* unValidCellName = GetCellName(prevRow, prevCol);
        //    unValidContent = prevContent;*/
        //}

        /// <summary>
        ///// Sets the curCol, curRow, curName and curContent for the controller.
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="col"></param>
        ///// <param name="content"></param>
        //public void SetCurCellInfo(int row, int col)
        //{
        //    curCol = col;
        //    curRow = row;
        //   // curContent = content;
        //    curName = GetCellName(row, col);
        //}

        ///// <summary>
        ///// Saves the spreadsheet it its current state.
        ///// </summary>
        ///// <param name="filePath"></param>
        //public void save(String filePath)
        //{
        //    spreadsheet.Save(filePath);
        //}
    }
}

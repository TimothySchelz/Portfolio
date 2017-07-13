using SpreadsheetUtilities;
using SS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace SpreadsheetGUI
{

    public class SSController
    {
        private SpreadSheetForm init_form;
        /// <summary>
        /// Collection of spreadsheet objects keyed by the docID.
        /// </summary>
        private Dictionary<string, Spreadsheet> spreadsheets;

        private ClientController client_controller;
        /// <summary>
        /// Collection of documents keyed by their docID.
        /// </summary>
        private Dictionary<string, SpreadSheetForm> docs;

        private string username, address;

        private string temp_filename;

        private static readonly string FE_MESSAGE = "Error";
      

        public SSController(ClientController c)
        {
            client_controller = c;
            client_controller.SetClientController(this);

            docs = new Dictionary<string, SpreadSheetForm>();

            //Create SS model container
            spreadsheets = new Dictionary<string, Spreadsheet>();

            temp_filename = "";
        }

        public void SetInitSpreadsheetForm(SpreadSheetForm s)
        {
            init_form = s;
        }

        /// <summary>
        /// The user has just notified us that they want to open a spreadsheet.
        /// </summary>
        /// <param name="docID"></param>
        internal void SendOpenSS()
        {
            client_controller.SendssMessage("", "", 0, 0, 0);
        }

        internal void SendNewSS(string filename)
        {
            client_controller.SendssMessage("", filename, 0, 0, 1);
        }

        internal void SendSaveSS(string docID)
        {
            MarkAsSaved(docID);
            client_controller.SendssMessage(docID, "", 0, 0, 6);
        }

        internal void SendCloseSS(string docID)
        {
            client_controller.SendssMessage(docID, "", 0, 0, 9);
        }

        internal void SendUndoSS(string docID)
        {
            client_controller.SendssMessage(docID, "", -1, -1, 4);
        }
        internal void SendRedoSS(string docID)
        {
            client_controller.SendssMessage(docID, "", -1, -1, 5);
        }

        internal void SendRenameSS(string docID, string filename)
        {
            //Send the resend request to the server. 
            client_controller.SendssMessage(docID, filename, -1, -1, 7);
        }

        internal void InvalidEdit(string docID)
        {
                if (docs.ContainsKey(docID))
                {
                    //Have a pop up explain that the edit in cell x is not valid and then change it to the the content sent from the server.
                    docs[docID].ShowErrorMessage("Invalid edit in cell " + docs[docID].GetUnValidCellName() + ". The cell will go back to its valid value");
                    //This will update the spreadsheet when an Cell update comes.    
                    UpdateCell(docID, docs[docID].GetUnValidCellName(), docs[docID].GetUnValidContent());
                    docs[docID].EnableSpreadsheet();
                    //setPrev = true;
                }
        }

        internal void SendEditLocation(string docID, string cellname)
        {
            int row, col;
            GetRowAndColOfCell(cellname, out row, out col);
            client_controller.SendssMessage(docID, "", col, row, 8);
        }

        internal void ShowOpenForm(HashSet<string> s_files)
        {
            init_form.ShowOpenForm(s_files);
        }

        internal void SetOpenFileSelection(string filename)
        {
            temp_filename = filename;
            client_controller.SendssMessage("", filename, 0, 0, 2);
        }

        internal void StartConnection(string name, string addr)
        {
            username = name;
            address = addr;
            client_controller.BeginConnection(address);
        }

        internal void createSpreadsheet(string docID)
        {
                spreadsheets[docID] = new Spreadsheet(isValid, normalize, "ps6");
                docs[docID] = new SpreadSheetForm(true, temp_filename, docID, this);
                init_form.CreateSpreadsheet(docs[docID]);
                docs[docID].EnableRename();
        }

        internal void UpdateCell(string docID, string cellname, string newContent)
        {
            lock (spreadsheets)
            {
                if (spreadsheets.ContainsKey(docID))
                {
                    object oldContent = spreadsheets[docID].GetCellContents(cellname);
                    string oldContentString;

                    if (oldContent is Formula)
                    {
                        oldContentString = "=" + oldContent.ToString();
                    }
                    else
                    {
                        oldContentString = oldContent.ToString();
                    }
                    try
                    {

                        ISet<string> changed = spreadsheets[docID].SetContentsOfCell(cellname, newContent);
                        foreach (string cell_Name in changed)
                        {
                            int currentRow, currentCol;
                            GetRowAndColOfCell(cell_Name, out currentRow, out currentCol);
                            object cellValue = spreadsheets[docID].GetCellValue(cell_Name);
                            PaintCell(docID, currentRow, currentCol, cellValue);
                        }
                        //Todo: Update current clients contents text box if on this cell.
                        if (docs[docID].GetCurrName().Equals(cellname))
                        {
                            docs[docID].SetContentTextBox(newContent);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is ArgumentNullException)
                        {
                            docs[docID].ShowErrorMessage("The content entered causes an error for reason: " + e.Message);
                            UpdateCell(docID, cellname, oldContentString);
                            return;
                        }
                        else if (e is InvalidNameException)
                        {
                            docs[docID].ShowErrorMessage("The content entered causes an error for reason: " + e.Message);
                            UpdateCell(docID, cellname, oldContentString);
                            return;
                        }
                        else if (e is FormulaFormatException)
                        {
                            docs[docID].ShowErrorMessage("The content entered causes an error for reason: " + e.Message);
                            UpdateCell(docID, cellname, oldContentString);
                            return;
                        }
                        //else if (e is CircularException)
                        //{
                        //    docs[docID].ShowErrorMessage("The content entered causes an error for reason: " + e.Message);
                        //    UpdateCell(docID, cellname, oldContentString);
                        //    return;
                        //}
                    }

                }
            }
            
        }

        internal void NowConnected()
        {
            init_form.NowConnected();
        }

        internal string GetUsername()
        {
            return username;
        }

        internal void EnableSpreadsheet(string docID)
        {
            
                if (docs.ContainsKey(docID))
                {
                    docs[docID].EnableSpreadsheet();
                }
            
        }

        internal void DisableSpreadsheet(string docID)
        {
                if (docs.ContainsKey(docID))
                {
                    docs[docID].DisableSpreadsheet();
                }
            
        }

        internal void ShowErrorMessage(string docID, string message)
        {
          
                if (docs.ContainsKey(docID))
                {
                    docs[docID].ShowErrorMessage(message);
                }
            
        }
        internal void EditLocation(string docID, string userID, string userName, string cellName)
        {
         
                if (docs.ContainsKey(docID))
                {
                    if (cellName.Equals("-1"))
                    {
                        docs[docID].RemoveUser(userID);
                    }
                    else
                    {
                        int row, col;
                        GetRowAndColOfCell(cellName, out row, out col);
                        docs[docID].EditLocation(userID, userName, row, col);
                    }
                }
            
        }

        /// <summary>
        /// Updates the cell that is at the location prevCol and prevRow with the prevContent.
        /// Will not update if senderCode = 2, will always update if senderCode = 0, and will send only once if senderCode is 1.
        /// 
        /// </summary>
        public void UpdateCells(string docID, ref int senderCode, bool lockedUp)
        {
                if (docs.ContainsKey(docID))
                {
                    string prevContent = docs[docID].GetPrevContent().ToString();
                    string prevCellName = docs[docID].GetPrevName();
                    if (prevContent != null)
                    {

                        //Checks the old value of prevCell to the new value of prev and if they are the same do nothing.
                        if (!spreadsheets[docID].GetCellContents(prevCellName).Equals(prevContent))
                        {
                            string cellname = GetCellName(docs[docID].GetPrevRow(), docs[docID].GetPrevCol());
                            //Updates the spreadsheet of the changes.
                            UpdateCell(docID, cellname, docs[docID].GetPrevContent().ToString());

                            if (!lockedUp)
                            {
                                //Sends the update to the server.
                                UpdateSS(docID, cellname, prevContent);
                            }
                        }

                    }

                    //Sends the current cell that is currently being edited by this client.
                    string currName = docs[docID].GetCurrName();
                    if (!prevCellName.Equals(currName))
                    {
                        SendEditLocation(docID, currName);
                    }
                }
            
            
        }

        /// <summary>
        /// Marks the document as saved.
        /// </summary>
        /// <param name="docID"></param>
        internal void MarkAsSaved(string docID)
        {
            
                if (spreadsheets.ContainsKey(docID))
                {
                    spreadsheets[docID].SetChanged(false);
                }
            
        }

        internal void ChangeFilename(string docID, string filename)
        {
           
                if (docs.ContainsKey(docID))
                {
                    docs[docID].ChangeFilename(filename);
                }
            
            
        }

        internal void ChangeFilenameBack(string docID)
        {
            
                if (docs.ContainsKey(docID))
                {
                    docs[docID].ChangeFilenameBack(docs[docID]);
                }
            
            
        }

        private void UpdateSS(string docID, string cellname, string prevContent)
        {
            if (docs.ContainsKey(docID))
            {
                docs[docID].SetUnvalid(cellname, spreadsheets[docID].GetCellContents(cellname).ToString());
            }
            int row, col;
            GetRowAndColOfCell(cellname, out row, out col);
            
            client_controller.SendssMessage(docID, prevContent, col, row, 3);
        }

        /// <summary>
        /// Returns the content of the cell at the grid location of row and col.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public string GetContentText(string docID, int row, int col)
        {
            
                if (spreadsheets.ContainsKey(docID))
                {
                    string name = GetCellName(row, col);
                    object cellContent = spreadsheets[docID].GetCellContents(name);
                    if (cellContent is Formula)
                    {
                        return "=" + cellContent.ToString();
                    }
                    else
                    {
                        return cellContent.ToString();
                    }
                }
            
            return "";
            
        }


        /// <summary>
        /// Returns true if the spreadsheet has changed and false if not.
        /// </summary>
        /// <returns></returns>
        public bool SpreadsheetChanged(string docID)
        {
            
                if (spreadsheets.ContainsKey(docID))
                {
                    return spreadsheets[docID].Changed;
                }
            
            return false;
            
        }

        /// <summary>
        /// Gets the row and column number of the named cell.
        /// </summary>
        /// <param name="name">The name of the cell.</param>
        /// <param name="row">The row number will be in row</param>
        /// <param name="col">The column number will be in col</param>
        private void GetRowAndColOfCell(string name, out int row, out int col)
        {
            int number;
            Int32.TryParse(name.Substring(1), out number);
            int letterCode = char.ConvertToUtf32(name, 0);
            col = letterCode - 'A';
            row = number - 1;
        }
        /// <summary>
        /// Returns the cell name of the cell located at row and column.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public string GetCellName(int row, int col)
        {
            return char.ConvertFromUtf32('A' + col) + (row + 1).ToString();
        }



        /// <summary>
        /// Repaints the cell at the location currentRow and currentCol with the cellValue.
        /// </summary>
        /// <param name="currentRow"></param>
        /// <param name="currentCol"></param>
        /// <param name="cellValue"></param>
        private void PaintCell(string docID, int currentRow, int currentCol, object cellValue)
        {
           
                if (docs.ContainsKey(docID))
                {
                    if (cellValue is FormulaError)
                    {
                        FormulaError fe = (FormulaError)cellValue;
                        docs[docID].RepaintCell(currentRow, currentCol, FE_MESSAGE);
                    }
                    else
                    {
                        docs[docID].RepaintCell(currentRow, currentCol, cellValue.ToString());
                    }

                }
            
        }

        /// <summary>
        /// Returns true if name is a valid variable. A valid variable is defined as a variable that 
        /// has one letter and one or two numbers following. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool isValid(string name)
        {
            if (name.Length > 3)
            {
                return false;
            }
            // Patterns for valid individual names of cells and variable in the spreadsheet.
            String varPattern = @"^[a-zA-Z][0-9]+1$ || ^[a-zA-Z][0-9]+2$";
            Regex goodVar = new Regex(varPattern);
            return goodVar.IsMatch(name);
        }

        // Helpers //
        /// <summary>
        /// Takes in a string and returns the upperCase version of it.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>name as upperCase</returns>
        private string normalize(string name)
        {
            return name.ToUpper();
        }


        //################### For Graphing #####################//
        /// <summary>
        /// Takes in a string representing x values (ex. a1-z1 or a1-a99) and takes in a string representing 
        /// y values (ex. a1-z1 or a1-a99). The method will then take in these two arguments and place the values that correspond to each
        /// other in a DataPoint object which will be added to a List<DataPoint> which is returned.
        /// 
        /// -----The xRange and yRange must have the same number of cell representations in the ranges------
        /// 
        /// </summary>
        /// <param name="xRange"></param>
        /// <param name="yRange"></param>
        /// <returns></returns>
        internal List<DataPoint> GetDataPoints(string docID, string xRange, string yRange)
        {
            if (docs.ContainsKey(docID))
            {
                List<DataPoint> data = new List<DataPoint>();
                string[] xLimits = xRange.Split(new char[] { '-' });
                string[] yLimits = yRange.Split(new char[] { '-' });
                if (xLimits.Length != 2 || yLimits.Length != 2)
                {
                    docs[docID].ShowErrorMessage("The Ranges must have an upper limit and lower limit. Must have exactly two named cells.");
                    return null;
                }
                int xDiff, yDiff;
                bool xDown, xRight, xLeft, xUp;
                FindDirection(xLimits, out xDown, out xRight, out xLeft, out xUp, out xDiff);
                if (!checkRangeBooleans(docID, xDown, xRight, xLeft, xUp, "X"))
                {
                    return null;
                }

                bool yDown, yRight, yLeft, yUp;
                FindDirection(yLimits, out yDown, out yRight, out yLeft, out yUp, out yDiff);
                if (!checkRangeBooleans(docID, yDown, yRight, yLeft, yUp, "Y"))
                {
                    return null;
                }

                if (xDiff != yDiff)
                {
                    docs[docID].ShowErrorMessage("Their must be the same number of cells in the X range as in the Y range.");
                    return null;
                }

                string xStart = xLimits[0];
                string yStart = yLimits[0];
                //Loops through each of the cell names in the range and puts their values in a DataPoint which is then 
                //Added to a DataPoint List
                for (int i = 0; i < xDiff + 1; i++)
                {
                    string yName = "";
                    string xName = "";
                    if (xDown)
                    {
                        xName = xStart[0] + char.ConvertFromUtf32(xStart[1] + i);
                    }
                    if (xRight)
                    {
                        xName = char.ConvertFromUtf32(xStart[0] + i) + xStart[1];
                    }
                    if (xUp)
                    {
                        xName = xStart[0] + char.ConvertFromUtf32(xStart[1] - i);
                    }
                    if (xLeft)
                    {
                        xName = char.ConvertFromUtf32(xStart[0] - i) + xStart[1];
                    }
                    if (yDown)
                    {
                        yName = yStart[0] + Char.ConvertFromUtf32(yStart[1] + i);
                    }
                    if (yRight)
                    {
                        yName = char.ConvertFromUtf32(yStart[0] + i) + yStart[1];
                    }
                    if (yUp)
                    {
                        yName = yStart[0] + Char.ConvertFromUtf32(yStart[1] - i);
                    }
                    if (yLeft)
                    {
                        yName = char.ConvertFromUtf32(yStart[0] - i) + yStart[1];
                    }
                    object x, y;
                  
                         x = spreadsheets[docID].GetCellValue(xName);
                         y = spreadsheets[docID].GetCellValue(yName);
                    
                    double xdouble, ydouble;
                    if (!Double.TryParse(x.ToString(), out xdouble))
                    {
                        docs[docID].ShowErrorMessage("All values in the range need to be numbers");
                        return null;
                    }
                    if (!Double.TryParse(y.ToString(), out ydouble))
                    {
                        docs[docID].ShowErrorMessage("All values in the range need to be numbers");
                        return null;
                    }
                    data.Add(new DataPoint(xdouble, ydouble));
                }
                return data;
            }
            return null;
        }

        /// <summary>
        /// Checks to make sure there is at least one direction true and that the combination
        /// of the true directions is graphable and continuous in a straight line (no diagnals). 
        /// </summary>
        /// <param name="Down"></param>
        /// <param name="Right"></param>
        /// <param name="Left"></param>
        /// <param name="Up"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private bool checkRangeBooleans(string docID, bool Down, bool Right, bool Left, bool Up, String axis)
        {
            if (docs.ContainsKey(docID))
            {
                if (Down && Right || Left && Up && Down && Left || Right && Up)
                {
                    docs[docID].ShowErrorMessage("The direction of the: " + axis + " range must continuous up, down, left or right in a line.");
                    return false;
                }
                if (!Down && !Right && !Up && !Left)
                {
                    docs[docID].ShowErrorMessage("There must be at least two values in the X and Y ranges.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Outs the bool to true that the direction of this axis should take when traversing. Also gives the number of 
        /// cells in the range between the beginning of the limit and end and outs it in diff.
        /// </summary>
        /// <param name="limits"></param>
        /// <param name="down"></param>
        /// <param name="right"></param>
        /// <param name="left"></param>
        /// <param name="up"></param>
        /// <param name="diff"></param>
        private void FindDirection(string[] limits, out bool down, out bool right, out bool left, out bool up, out int diff)
        {
            int row1, col1, row2, col2;
            GetRowAndColOfCell(limits[0], out row1, out col1);
            GetRowAndColOfCell(limits[1], out row2, out col2);
            down = false;
            right = false;
            left = false;
            up = false;
            diff = 0;
            if (row1 == row2)
            {
                diff = col2 - col1;
                if (diff < 0)
                {
                    left = true;
                    diff = Math.Abs(diff);
                }
                if (diff > 0 && !left)
                {
                    right = true;
                }

            }
            if (col1 == col2)
            {
                diff = row2 - row1;
                if (diff < 0)
                {
                    up = true;
                    diff = Math.Abs(diff);
                }
                if (diff > 0 && !up)
                {
                    down = true;
                }
            }
        }

    }
}

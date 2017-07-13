using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.VisualBasic;
using System.Threading;

namespace SpreadsheetGUI
{

    /// <summary>
    /// Creates a Form that has the components needed for a spreadsheet.
    /// </summary>
    public partial class SpreadSheetForm : Form
    {
        /// <summary>
        /// The Row that will be selected when spreadsheet loads.
        /// </summary>
        private static readonly int START_ROW = 0;
        /// <summary>
        /// The Column that will be selected when the spreadsheet loads.
        /// </summary>
        private static readonly int START_COL = 0;
        /// <summary>
        /// The Number of rows in the spreadsheet.
        /// </summary>
        private static readonly int ROW_COUNT = 99;

        /// <summary>
        /// The number of columns in the spreadsheet.
        /// </summary>
        private static readonly int COL_COUNT = 26;
        /// <summary>
        /// The ralative path to the Help messages.
        /// </summary>
        private string relativeHelpPath;
        /// <summary>
        /// The controller for this view.
        /// </summary>
        private SSController controller;
        /// <summary>
        ///  An integer that signifies if something is sent. Default=0, Sent=1, Resent=2
        /// </summary>
        private int sent = 1;

        /// <summary>
        /// Represents the column and row of the previously selected cell in the grid.
        /// </summary>
        private int prevCol, prevRow;
        /// <summary>
        /// The content of the previously selected cell in the grid.
        /// </summary>
        private string prevContent, prevName, unValidCellName, unValidContent;

        //Set to true when the SSpanel is disabled
        public bool Locked { get; set; }
        public bool IsOpen { get; set; }

        /// <summary>
        /// This is what the name of the spreadsheet will be called.
        /// </summary>
        private string filename;

        private string theLock = "";
        /// <summary>
        /// The doc Id that the controller knows this ss by.
        /// </summary>
        private string docID;
        private string prevFilename;

        delegate void voidDelegate();
        delegate void argDelegate(HashSet<string> files);
        delegate void ssDelegate(SpreadSheetForm f);
        delegate void argsDelegate(string userID, string userName, int row, int col);

        /// <summary>
        /// the spreadsheet GUI that takes in a string file path and fills 
        /// the spreadsheet GUI with the contents of the .sprd file.
        /// </summary>
        /// <param name="fileName">Pathname is null if no specified file.</param>
        public SpreadSheetForm(bool showSSPanel, string _filename, string _docID, SSController controller)
        {
            //The default spreadsheet name. 
            InitializeComponent();

            this.Text = filename;
            prevCol = -1;
            prevRow = -1;
            prevName = "A1";
            unValidCellName = "A1";
            unValidContent = "";

            this.controller = controller;
            filename = _filename;
            docID = _docID;

            if (showSSPanel)
            {
                showSpreadsheet();
            }
            else
            {
                this.newToolStripMenuItem.Enabled = false;
                this.openToolStripMenuItem.Enabled = false;
                controller.SetInitSpreadsheetForm(this);
            }

            relativeHelpPath = Application.StartupPath + "..\\..\\..\\..\\Resources\\HelpMessages\\";
        }

        internal void ShowOpenForm(HashSet<string> s_files)
        {
            if (this.InvokeRequired)
            {
                argDelegate n = new argDelegate(ShowOpenForm);
                this.Invoke(n, s_files);
                return;
            }
            OpenForm o_form = new OpenForm(controller);
            o_form.AddToFilesListBox(s_files);
        }

        internal void CreateSpreadsheet(SpreadSheetForm spreadSheetForm)
        {
            if (this.InvokeRequired)
            {
                ssDelegate n = new ssDelegate(CreateSpreadsheet);
                this.Invoke(n, spreadSheetForm);
                return;
            }
            SSApplicationContext.getAppContext().RunForm(spreadSheetForm);
        }

        internal void EnableRename()
        {
            this.renameToolStripMenuItem.Enabled = true;
        }

        internal void StartConnection(string username, string address)
        {
            controller.StartConnection(username, address);
        }

        internal string GetUnValidContent()
        {
            return unValidContent;
        }

        internal string GetUnValidCellName()
        {
            return unValidCellName;
        }

        public string AskForFileName()
        {
            return Interaction.InputBox("What would you like to name your spreadsheet?", "Spreadsheet Name");
        }

        public void showSpreadsheet()
        {
            //Initialize components
            this.valueLabel = new System.Windows.Forms.Label();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.contentTextBox = new System.Windows.Forms.TextBox();
            this.contentLabel = new System.Windows.Forms.Label();
            this.updateCellsButton = new System.Windows.Forms.Button();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.spreadsheetPanel1 = new SS.SpreadsheetPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();

            //Set up components
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 0);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.valueLabel, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.valueTextBox, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(240, 2);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(342, 34);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // valueLabel
            // 
            this.valueLabel.AutoSize = true;
            this.valueLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valueLabel.Location = new System.Drawing.Point(3, 0);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(71, 34);
            this.valueLabel.TabIndex = 0;
            this.valueLabel.Text = "Cell Value";
            // 
            // valueTextBox
            // 
            this.valueTextBox.Location = new System.Drawing.Point(80, 2);
            this.valueTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.ReadOnly = true;
            this.valueTextBox.Size = new System.Drawing.Size(212, 22);
            this.valueTextBox.TabIndex = 1;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 3;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.tableLayoutPanel5.Controls.Add(this.contentTextBox, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.contentLabel, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.updateCellsButton, 2, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(588, 2);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.Size = new System.Drawing.Size(798, 34);
            this.tableLayoutPanel5.TabIndex = 2;
            // 
            // contentTextBox
            // 
            this.contentTextBox.Location = new System.Drawing.Point(93, 2);
            this.contentTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.contentTextBox.Name = "contentTextBox";
            this.contentTextBox.Size = new System.Drawing.Size(456, 22);
            this.contentTextBox.TabIndex = 0;
            this.contentTextBox.TextChanged += new System.EventHandler(this.contentTextBox_TextChanged);
            this.contentTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.contentTextBox_KeyPress);
            // 
            // contentLabel
            // 
            this.contentLabel.AutoSize = true;
            this.contentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentLabel.Location = new System.Drawing.Point(3, 0);
            this.contentLabel.Name = "contentLabel";
            this.contentLabel.Size = new System.Drawing.Size(84, 34);
            this.contentLabel.TabIndex = 1;
            this.contentLabel.Text = "Cell Content";
            // 
            // updateCellsButton
            // 
            this.updateCellsButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.updateCellsButton.Location = new System.Drawing.Point(555, 2);
            this.updateCellsButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.updateCellsButton.Name = "updateCellsButton";
            this.updateCellsButton.Size = new System.Drawing.Size(75, 30);
            this.updateCellsButton.TabIndex = 2;
            this.updateCellsButton.Text = "Enter";
            this.updateCellsButton.UseVisualStyleBackColor = true;
            this.updateCellsButton.Click += new System.EventHandler(this.updateCellsButton_Click);
            this.updateCellsButton.Enter += new System.EventHandler(this.updateCellsButton_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.nameLabel, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.nameTextBox, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 2);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(200, 30);
            this.tableLayoutPanel3.TabIndex = 3;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nameLabel.Location = new System.Drawing.Point(3, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(72, 30);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Cell Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(81, 2);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.ReadOnly = true;
            this.nameTextBox.Size = new System.Drawing.Size(100, 22);
            this.nameTextBox.TabIndex = 1;
            // 
            // spreadsheetPanel1
            // 
            this.spreadsheetPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spreadsheetPanel1.Location = new System.Drawing.Point(5, 75);
            this.spreadsheetPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.spreadsheetPanel1.Name = "spreadsheetPanel1";
            this.spreadsheetPanel1.Size = new System.Drawing.Size(1353, 363);
            this.spreadsheetPanel1.TabIndex = 0;
            this.spreadsheetPanel1.SelectionChanged += new SS.SelectionChangedHandler(this.displaySelection);
            this.spreadsheetPanel1.Load += new System.EventHandler(this.spreadsheetPanel1_Load);
            this.tableLayoutPanel1.Controls.Add(this.spreadsheetPanel1, 0, 2);

            //Final layout setup
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "SpreadSheetForm";
            this.Text = "Spreadsheet";
            this.controlPanel.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

            //Set up SS events
            spreadsheetPanel1.SelectionChanged += FocusOnContentTextBox;
            spreadsheetPanel1.SetSelection(START_COL, START_ROW);
            displaySelection(spreadsheetPanel1);
            this.saveToolStripMenuItem.Enabled = true;
            this.graphToolStripMenuItem.Enabled = true;

            this.Locked = false;
        }

        internal void SetUnvalid(string cellname, string prevContent)
        {
            unValidCellName = cellname;
            unValidContent = prevContent;
        }

        internal void SetContentTextBox(string newContent)
        {
            prevContent = newContent;
            //displaySelection(spreadsheetPanel1);
        }

        internal void RemoveUser(string userID)
        {
            if (spreadsheetPanel1 != null)
            {
                spreadsheetPanel1.EditLocation(userID, -1, -1);
            }
        }

        internal void ChangeFilenameBack(SpreadSheetForm spreadSheetForm)
        {
            filename = prevFilename;
            this.Text = filename;
            MessageBox.Show("The file name " + prevFilename + " was invalid. The change was not made.");
        }

        internal void ChangeFilename(string filename)
        {
            this.filename = filename;
            MessageBox.Show("File name successfully changed to " + filename + ".");
        }

        internal void NowConnected()
        {
            if (this.InvokeRequired)
            {
                voidDelegate n = new voidDelegate(NowConnected);
                this.Invoke(n);
                return;
            }
            this.connectToolStripMenuItem.Enabled = false;
            this.newToolStripMenuItem.Enabled = true;
            this.openToolStripMenuItem.Enabled = true;
        }

        internal string GetCurrName()
        {
            string name = "";
            if (spreadsheetPanel1 != null)
            {
                int row, col;
                spreadsheetPanel1.GetSelection(out col, out row);
                name = controller.GetCellName(row, col);
            }
            return name;
        }

        internal int GetPrevCol()
        {
            return prevCol;
        }

        internal int GetPrevRow()
        {
            return prevRow;
        }

        internal string GetPrevName()
        {
            return prevName;
        }

        internal object GetPrevContent()
        {
            return prevContent;
        }

        /// <summary>
        /// Takes in the SpreadsheetPanel and displays the Selection on the input spreadsheetPanel.
        /// </summary>
        /// <param name="ss"></param>
        private void displaySelection(SpreadsheetPanel ss)
        {
            int row, col;
            ss.GetSelection(out col, out row);
            lock (theLock)
            {
                //Will not allow the spreadsheet to be changed while it is locked. 
                if (!Locked)
                {
                    //updates the values of the previous selection
                    controller.UpdateCells(docID, ref sent, false);
                }
                else
                {
                    //updates the values of the previous selection
                    controller.UpdateCells(docID, ref sent, true);
                }
            }
            //set nameTextBox
            nameTextBox.Text = controller.GetCellName(row, col);

            //set valueTextBox
            string value;
            ss.GetValue(col, row, out value);
            valueTextBox.Text = value;

            //set contentTextBox
            string content = controller.GetContentText(docID, row, col);
            contentTextBox.Text = content;

            SetPreviousCellInfo(row, col, content);
        }

        /// <summary>
        /// Sets the prevCol, prevRow, preName and prevContent for the controller.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="content"></param>
        public void SetPreviousCellInfo(int row, int col, string content)
        {
            prevCol = col;
            prevRow = row;
            prevContent = content;
            prevName = controller.GetCellName(row, col);
        }

        /// <summary>
        /// Sets the focus to the contentTextBox as well as putting the cursing at the end of the contents in the
        /// contentTextBox.
        /// </summary>
        /// <param name="ss"></param>
        private void FocusOnContentTextBox(SpreadsheetPanel ss)
        {
            contentTextBox.Focus();
            contentTextBox.SelectionStart = contentTextBox.Text.Length;
        }

        /// <summary>
        /// Opens a new SpreadsheetForm in a new window that is not linked to the old one.
        /// </summary>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fname = AskForFileName();
            controller.SendNewSS(fname);
        }


        /// <summary>
        /// Close button will close the GUI and if the spreadsheet has changed will have a pop up window ensuring
        /// the user wants to leave without saving.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Updates the content in the controller everytime the text changes in the contentTextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contentTextBox_TextChanged(object sender, EventArgs e)
        {
            SetCellContent();
        }

        /// <summary>
        /// Updates the content in the controller and on the GUI.
        /// </summary>
        private void SetCellContent()
        {
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);
            //No new values will be set when the spreadsheet is locked. 
            if (!Locked)
            {
                spreadsheetPanel1.SetValue(col, row, contentTextBox.Text);
            }
            else
            {
                this.contentTextBox.Enabled = false;
            }
            SetPreviousCellInfo(row, col, contentTextBox.Text);
        }

        /// <summary>
        /// Updates the cell values in the controller and on the GUI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateCellsButton_Click(object sender, EventArgs e)
        {
            SetCellContent();
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);
            string value;
            spreadsheetPanel1.GetValue(col, row, out value);

            lock(theLock)
            {
            controller.UpdateCell(docID, controller.GetCellName(row, col), value);

            }
            //THESE METHODS SHOULD BE USED IN THE ReceiveUpdates(SocketState ss). 
            displaySelection(spreadsheetPanel1);
        }

        /// <summary>
        /// Shows an error message with the input string message.
        /// </summary>
        /// <param name="message"></param>
        internal void ShowErrorMessage(string message)
        {
            MessageBox.Show(message);
        }

        /// <summary>
        /// Repaints the GUI.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value"></param>
        public void RepaintCell(int row, int col, string value)
        {
            spreadsheetPanel1.SetValue(col, row, value);
        }

        /// <summary>
        /// When the open button is clicked in the menu then the open browser will open.
        /// cancel or X will close the browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SendOpenSS();
        }

        /// <summary>
        /// When the save button in the menu is chosen it will open the save browser. Cancel or X will close the 
        /// save browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sendy = 0;
            SetCellContent();
            lock(theLock)
            {
                controller.UpdateCells(docID, ref sendy, false);

            }

            controller.SendSaveSS(docID);
        }

        internal void EnableSpreadsheet()
        {
            if (this.contentTextBox.InvokeRequired || spreadsheetPanel1.InvokeRequired)
            {
                voidDelegate n = new voidDelegate(EnableSpreadsheet);
                this.Invoke(n);
                return;
            }
            spreadsheetPanel1.UseWaitCursor = false;
            this.contentTextBox.UseWaitCursor = false;
            this.contentTextBox.Enabled = true;
            this.Locked = false;
            SetCellContent();
            FocusOnContentTextBox(spreadsheetPanel1);
        }

        internal void DisableSpreadsheet()
        {
            if (this.contentTextBox.InvokeRequired || spreadsheetPanel1.InvokeRequired)
            {
                voidDelegate n = new voidDelegate(EnableSpreadsheet);
                this.Invoke(n);
                return;
            }
            spreadsheetPanel1.UseWaitCursor = true;
            this.contentTextBox.UseWaitCursor = true;
            this.Locked = true;
        }
        /// <summary>
        /// This is what happens when the form closes. If the spreadsheet has changed then there will be a pop up box
        /// asking if the user would like to save. If the yes option is chosen the save browser will appear and allow them to save.
        /// If the no button is chosen then the GUI will close without saving. If cancel is chosen then the pop up window
        /// will close, the spreadsheet GUI will stay open and not save.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //UpdateControllerOfContent();
            int sendy = 0;
            if (controller == null)
            {
                return;
            }
            bool changed = false;
            lock (theLock)
            {
            controller.UpdateCells(docID, ref sendy, false);
            changed = controller.SpreadsheetChanged(docID);

            }
            if (changed)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to close?", "Confirmation", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    controller.SendCloseSS(docID);
                }
                else if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// The location of userID has changed.
        /// Displays the change.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        internal void EditLocation(string userID, string userName, int row, int col)
        {
            if (this.InvokeRequired)
            {
                argsDelegate n = new argsDelegate(EditLocation);
                this.Invoke(n, new object[] { userID, userName, row, col});
                return;
            }
            if (spreadsheetPanel1 == null)
            {
                return;
            }
            spreadsheetPanel1.EditLocation(userID, row, col);
        }



        /// <summary>
        /// When the enter key is pressed the selection is moved down. Will not allow enter functionality when waiting for an edit to be validated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contentTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                MoveSelectionDown();
            }
        }

        /// <summary>
        /// Overrides the Cmd Key. This will allow Shift + Enter to move the selection down.
        /// Also, the arrow keys will move the direction of the selection in accordance with what 
        /// direction key is pressed. All other Cmd keys keep their original functions.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (spreadsheetPanel1 != null)
            {

                if (keyData == (Keys.Enter | Keys.Shift))
                {
                    // Shift+Enter pressed
                    MoveSelectionUp();
                    return true;
                }
                //Alt + z
                if (keyData == (Keys.Z | Keys.Alt))
                {
                    //Sends the undo request to the server.
                    controller.SendUndoSS(docID);
                    return true;
                }
                //Alt + Y
                if (keyData == (Keys.Y | Keys.Alt))
                {
                    //Sends the Redo request to the server. 
                    controller.SendRedoSS(docID);
                    return true;
                }

                switch (keyData)
                {
                    case Keys.Up:
                        MoveSelectionUp();
                        return true;

                    case Keys.Down:
                        MoveSelectionDown();
                        return true;

                    case Keys.Left:
                        MoveSelectionLeft();
                        return true;

                    case Keys.Right:
                        MoveSelectionRight();
                        return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Override of the TabKey press. If the tab key is pressed then the selection will move accross the spreadsheet
        /// to the right. If the shift + tab is pressed then the selection will move to the left accross the spreadsheet.
        /// </summary>
        /// <param name="forward"></param>
        /// <returns></returns>
        protected override bool ProcessTabKey(bool forward)
        {
            bool tabKey = true;
            if (tabKey)
            {
                if (forward)
                {
                    MoveSelectionRight();
                }
                else
                {
                    MoveSelectionLeft();
                }
                return true;
            }
            return base.ProcessTabKey(forward);
        }

        /// <summary>
        /// Moves the selection from its current location on the spreadsheet GUI. Does noting if the
        /// selector is at the left most column of the spreadsheet.
        /// </summary>
        private void MoveSelectionLeft()
        {
            if (spreadsheetPanel1 == null)
            {
                return;
            }
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);
            if (col > 0)
            {
                spreadsheetPanel1.SetSelection(--col, row);
            }
            displaySelection(spreadsheetPanel1);
            FocusOnContentTextBox(spreadsheetPanel1);
        }

        /// <summary>
        /// Moves the selection from its current location on the spreadsheet GUI. Does nothing if the selector
        /// is at the right most column of the spreadsheet.
        /// </summary>
        private void MoveSelectionRight()
        {
            if (spreadsheetPanel1 == null)
            {
                return;
            }
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);
            if (col < COL_COUNT - 1)
            {
                spreadsheetPanel1.SetSelection(++col, row);
            }
            displaySelection(spreadsheetPanel1);
            FocusOnContentTextBox(spreadsheetPanel1);
        }

        /// <summary>
        /// Moves the selection down from its current location on the spreadsheet GUI. Does nothing if the 
        /// selector is at the bottom of the spreadsheet.
        /// </summary>
        private void MoveSelectionDown()
        {
            if (spreadsheetPanel1 == null)
            {
                return;
            }
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);
            if (row < ROW_COUNT - 1)
            {
                spreadsheetPanel1.SetSelection(col, ++row);
            }
            displaySelection(spreadsheetPanel1);
            FocusOnContentTextBox(spreadsheetPanel1);
        }

        /// <summary>
        /// Moves the selection up from its current location on the spreadsheet GUI. Does nothing if the selector is at the
        /// top of the spreadsheet.
        /// </summary>
        private void MoveSelectionUp()
        {
            if (spreadsheetPanel1 == null)
            {
                return;
            }
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);
            if (row > 0)
            {
                spreadsheetPanel1.SetSelection(col, --row);
            }
            displaySelection(spreadsheetPanel1);
            FocusOnContentTextBox(spreadsheetPanel1);
        }

        /// <summary>
        /// Sets the content Text box active on the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetPanel1_Load(object sender, EventArgs e)
        {
            this.ActiveControl = contentTextBox;
        }

        /// <summary>
        /// Displays a help message for selecting cells as event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = GetTextFromFile(relativeHelpPath + "CellSelection.txt");
            MessageBox.Show(message);
        }

        /// <summary>
        /// When the openSaveErrors is clicked on in the Menu Strip it will open a pop up box with the text from
        /// the SpreadsheetErrors.txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = GetTextFromFile(relativeHelpPath + "SpreadsheetErrors.txt");
            MessageBox.Show(message);
        }

        /// <summary>
        /// When the Formula is clicked on in the Menu Strip it will open a pop up box with the text from
        /// the FormulaErrors.txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void formulaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = GetTextFromFile(relativeHelpPath + "Formula.txt");
            MessageBox.Show(message);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SplashForm sf = new SplashForm(this);
            sf.Show();
        }

        /// <summary>
        /// When the openSave is clicked on in the Menu Strip it will open a pop up box with the text from
        /// the openSave.txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = GetTextFromFile(relativeHelpPath + "openSave.txt");
            MessageBox.Show(message);
        }

        /// <summary>
        /// When the graphInstructions is clicked on in the Menu Strip it will open a pop up box with the text from
        /// the graphInfo.txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void graphInstructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = GetTextFromFile(relativeHelpPath + "GraphInstructions.txt");
            MessageBox.Show(message);
        }

        /// <summary>
        /// Clicking the spreadsheet button in the menu will open a pop up box with the text from the SpreadsheetData.txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = GetTextFromFile(relativeHelpPath + "SpreadsheetData.txt");
            MessageBox.Show(message);
        }

        private void undoRedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = GetTextFromFile(relativeHelpPath + "UndoRedo.txt");
            MessageBox.Show(message);
        }

        /// <summary>
        /// Pulls the text from a text file and returns a string.
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private string GetTextFromFile(string pathname)
        {
            return File.ReadAllText(pathname);
        }

        /// <summary>
        /// Creates a line graph with the inputs that pop up on the screen. Will cancel if the X button or cancel is
        /// clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displaySelection(spreadsheetPanel1);
            ChartForm chartForm;
            List<DataPoint> data;
            do
            {
                string xRange = SelectRange("X");
                if (xRange.Equals(""))
                {
                    return;
                }
                string yRange = SelectRange("Y");
                if (yRange.Equals(""))
                {
                    return;
                }

                chartForm = new ChartForm();
                data = controller.GetDataPoints(docID, xRange, yRange);
            }
            while (data == null);
            chartForm.SetChartSeries(data);
            chartForm.Show();
        }

        /// <summary>
        /// Returns a input pop up box for the X cell range with an explanation and example of how to enter the range.
        /// </summary>
        /// <returns></returns>
        private string SelectRange(String axis)
        {
            return Interaction.InputBox("Enter a cell range for " + axis + ".\nSeperate the cells with \"-\"\nFor Example: \nA1-A99\nor\nA1-Z1", "Select the Range for " + axis, "");
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            prevFilename = filename;
            string fName = "";
            fName = Interaction.InputBox("What would you like to rename your spreadsheet?", "Spreadsheet Name");
            //Assume it is a valid rename
            this.Text = fName;
            controller.SendRenameSS(docID, fName);
        }
    }
}

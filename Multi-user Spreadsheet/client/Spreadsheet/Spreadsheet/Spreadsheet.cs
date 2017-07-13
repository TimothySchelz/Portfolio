using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;
using System.Xml;


namespace SS
{
    /// <summary>
    /// A spreadsheet consists of an infinite number of named cells. 
    /// 
    /// A string is a cell name if and only if it consists of one or more letters,
    /// followed by one or more digits AND it satisfies the predicate IsValid.
    /// For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
    /// satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
    /// regardless of IsValid.
    /// 
    /// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
    /// must be normalized with the Normalize method before it is used by or saved in 
    /// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
    /// the Formula "x3+a5" should be converted to "X3+A5" before use.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        /// <summary>
        /// The graph of dependency relations of cells.
        /// </summary>
        private DependencyGraph graph;
        /// <summary>
        /// The Dictionary of cells in the spreadsheet that are filled.
        /// The cells are mapped with their name.
        /// </summary>
        private Dictionary<string, Cell> filledCells;

// ############################################# Constructors ############################################## //

        /// <summary>
        /// Creates an empty spreadsheet. Allows the user to provide a validity delegate (first parameter), 
        /// a normalization delegate (second parameter), and a version (third parameter)
        /// </summary>
        /// <param name="isValid">A validity delegate</param>
        /// <param name="normalize">A normalization delegate</param>
        /// <param name="version">A version</param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            graph = new DependencyGraph();
            filledCells = new Dictionary<string, Cell>();
            Changed = false;
        }

        /// <summary>
        /// Reads a saved spreadsheet from a file (see the Save method) 
        /// and uses it to construct a new spreadsheet. The new spreadsheet 
        /// uses the provided validity delegate, normalization delegate, and version.
        /// 
        /// If anything goes wrong when reading the file, the constructor throws
        /// a SpreadsheetReadWriteException
        /// </summary>
        /// <param name="pathname">A path to a file</param>
        /// <param name="isValid">A validity delegate</param>
        /// <param name="normalize">A normalization delegate</param>
        /// <param name="version">A version</param>
        public Spreadsheet(string pathname, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : this(isValid, normalize, version)
        {
            ReadSpreadsheetFromFile(pathname);
        }

        /// <summary>
        /// Creates an empty spreadsheet that imposes no extra validity conditions, 
        /// normalizes every cell name to itself, and has version "default".
        /// </summary>
        public Spreadsheet() : this(s => true, s => s, "default") { }


// ############################################# Properties ############################################## //

        /// <summary>
        /// Reflects if the spreadsheet has been modified after construction. 
        /// Additionally, if the spreadsheet is saved, it is no longer 
        /// "changed" (until another change is made).
        /// 
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }


// ############################################# Public Methods ############################################## //

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return new HashSet<string>(filledCells.Keys);
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        /// <param name="name">The name of the cell. Will be normalized.</param>
        /// <returns>The named cells contents.</returns>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            ValidateCellName(name);
            Cell cell;
            // Get the cell being set, or create a new one
            if (filledCells.TryGetValue(name, out cell))
            {
                return cell.Content;
            }
            // Cell is empty
            return "";
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        /// <param name="name">The name of the cell. Will be normalized.</param>
        /// <returns>The named cells value.</returns>
        public override object GetCellValue(string name)
        {
            name = Normalize(name);
            ValidateCellName(name);
            Cell cell;
            if (filledCells.TryGetValue(name, out cell))
            {
                return cell.Value;
            }
            //Cell is empty
            return "";
        }

        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename">The Location of the xml file.</param>
        /// <returns>The version of the spreadsheet that xml file represents.</returns>
        public override string GetSavedVersion(string filename)
        {
            if (filename == null)
            {
                throw new SpreadsheetReadWriteException("The filename can not be null.");
            }
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            if (reader.Name.Equals("spreadsheet"))
                            {
                                return reader.GetAttribute("version");
                            }
                        }
                    }
                    throw new SpreadsheetReadWriteException("The file does not start with an element 'spreadsheet'.");
                }
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// Sets the Changed value of this spreadsheet.
        /// </summary>
        /// <param name="v"></param>
        public void SetChanged(bool v)
        {
            Changed = v;
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename">The location where the file will be saved.</param>
        public override void Save(string filename)
        {
            if (filename == null)
            {
                throw new SpreadsheetReadWriteException("The file name can not be null.");
            }
            try
            {
                using (XmlWriter writer = XmlWriter.Create(filename))
                {
                    writer.WriteStartDocument();
                    //write spreadsheet version
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);
                    //write all cells
                    IEnumerable<string> cellNames = GetNamesOfAllNonemptyCells();
                    foreach (string name in cellNames)
                    {
                        writer.WriteStartElement("cell");
                        //write name
                        writer.WriteElementString("name", name);
                        //write content
                        Cell cell;
                        if (filledCells.TryGetValue(name, out cell))
                        {
                            if (cell.Content is Formula)
                            {
                                writer.WriteElementString("contents", "=" + cell.Content.ToString());
                            }
                            else //string or double
                            {
                                writer.WriteElementString("contents", cell.Content.ToString());
                            }
                        }
                        writer.WriteEndElement(); //End of cell
                    }
                    writer.WriteEndElement(); //End of spreadsheet
                    writer.WriteEndDocument();
                }
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException(e.Message);
            }
            Changed = false;
        }

        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">Name of the cell whose contents are being set. 
        /// Function will normalize the name.</param>
        /// <param name="content">The cell will be set to this content.
        /// A '=' in front of the content means the content is a formula.</param>
        /// <returns>The set of cells that have been changed.</returns>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("Content of a Cell can not be null.");
            }
            //Normilize name
            name = Normalize(name);
            string oldContent = GetCellContents(name).ToString();
            //Cell name is validated in respective SetCellContents methods.
            ISet<string> changedCells;
            double d;
            if (double.TryParse(content, out d)) //content is a double
            {
                changedCells = SetCellContents(name, d);
            }
            else if (content.Length > 0 && content[0] == '=') //content is a formula
            {
                changedCells = SetCellContents(name, new Formula(content.Substring(1), Normalize, IsValid));
            }
            else //contents is a string
            {
                changedCells = SetCellContents(name, content);
            }

            if (!oldContent.Equals(content))
            {
                Changed = true;
            }
            return changedCells;
        }


// ########################################### Protected Methods ######################################### //

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// 
        /// Also updates the named cell's value.
        /// </summary>
        /// <param name="name">Name should be normalized version of name.</param>
        /// <param name="number">The contents of the cell set to number.</param>
        /// <returns>The Set of cells that have been changed.</returns>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            ValidateCellName(name);
            Cell cell;
            // Get the cell being set, or create a new one
            if (!filledCells.TryGetValue(name, out cell))
            {
                cell = new Cell();
                filledCells.Add(name, cell);
            }
            cell.Content = number;
            //Update graph of dependees
            graph.ReplaceDependees(name, new HashSet<string>());
            IEnumerable<string> changed = GetCellsToRecalculate(name);
            RecalculateCells(changed);
            return new HashSet<string>(changed);
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// 
        /// Also updated the named cell's value.
        /// </summary>
        /// <param name="name">Name should be normalized version of name.</param>
        /// <param name="text">The text that will become cell contents</param>
        /// <returns>The set of cells that have been changed.</returns>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("The text cannot be null.");
            }
            ValidateCellName(name);
            Cell cell;
            // Get the cell being set, or create a new one
            if (!filledCells.TryGetValue(name, out cell))
            {
                cell = new Cell();
                //Only add new cell to filled cell if text is not empty string.
                if (text.Length > 0)
                {
                    filledCells.Add(name, cell);
                }
            }
            else if (text.Length == 0) //Remove the cell if empty
            {
                filledCells.Remove(name);
            }
            //Set the cell contents             
            cell.Content = text;
            //Update graph of dependents
            graph.ReplaceDependees(name, new HashSet<string>());
            IEnumerable<string> changed = GetCellsToRecalculate(name);
            RecalculateCells(changed);
            return new HashSet<string>(changed);
        }

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// 
        /// Also sets the value of the cell to the evaluation of the formula.
        /// </summary> 
        /// <param name="name">Should be normalized version of the name.</param>
        /// <param name="formula">The formula to become named cells contents.</param>
        /// <returns>The set of cells that have been changed.</returns>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            if (formula == null)
            {
                throw new ArgumentNullException("The formula cannot be null.");
            }
            ValidateCellName(name);
            //Save old content for CircularException
            object oldContent = GetCellContents(name);
            //Get cells refered to in formula
            HashSet<string> namesInFormula = new HashSet<string>(formula.GetVariables());
            //Validate and normilize names
            HashSet<string> normalizedNames = new HashSet<string>();
            foreach (string n in namesInFormula)
            {
                string normName = Normalize(n);
                ValidateCellName(normName);
                normalizedNames.Add(normName);
            }
            namesInFormula = normalizedNames; //Names are now normalized and valid.
            //Update dependency graph
            graph.ReplaceDependees(name, namesInFormula);
            //Get list of cells to change and change cell contents.
            IEnumerable<string> changed;
            try
            {
                changed = GetCellsToRecalculate(name);
            }
            // Revert to old version of graph if cycle occured
            catch (CircularException)
            {
                if (oldContent is double)
                {
                    SetCellContents(name, (double)oldContent);
                }
                else if (oldContent is Formula)
                {
                    SetCellContents(name, (Formula)oldContent);
                }
                else
                {
                    SetCellContents(name, (string)oldContent);
                }
                throw new CircularException();
            }
            //No circular exception so change spreadsheet
            //Get Cell associated with name
            Cell cell;
            if (!filledCells.TryGetValue(name, out cell))
            {
                cell = new Cell();
                filledCells.Add(name, cell);
            }
            cell.Content = formula;
            RecalculateCells(changed);
            return new HashSet<string>(changed);
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        /// <param name="name">The name of the cell. The name will be normalized.</param>
        /// <return>The collection of direct dependents of the named cell.</return>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            name = Normalize(name);
            ValidateCellName(name);
            return new HashSet<string>(graph.GetDependents(name));
        }


// ###################################### Private Methods and Classes ######################################### //

        /// <summary>
        /// Recalculates the cells by order of what needs to be recalculated first.
        /// If a cell is empty then its value will also be empty.
        /// </summary>
        /// <param name="changedCells">Ordered collection of cells to recalculate</param>
        private void RecalculateCells(IEnumerable<string> changedCells)
        {
            foreach (string name in changedCells)
            {
                Cell cell;
                //Only recalculate filled cells
                //If contents are filled then should be in Dictionary of filledCells
                //Empty cells are not added to the Dictionary of filledCells
                if (filledCells.TryGetValue(name, out cell))
                {
                    object content = cell.Content;
                    if (content is Formula)
                    {
                        try
                        {
                        cell.Value = ((Formula)content).Evaluate(LookUp);

                        }
                        catch
                        {
                            cell.Value = "FORMULA ERROR";
                        }
                    }
                    else
                    {
                        cell.Value = content;
                    }
                }
            }
        }

        /// <summary>
        /// Throws an InvalidNameException if the normilized version of the cell name is null or invalid, 
        /// as defined by the spreadsheet and the user.
        /// </summary>
        /// <param name="name">Cell name to validate. Should be normalized version of name.</param>
        private void ValidateCellName(string name)
        {
            if (name == null || name.Length < 2)
            {
                throw new InvalidNameException();
            }
            char firstChar = name[0];
            //The first character must be underscore or letter
            if (!char.IsLetter(firstChar))
            {
                throw new InvalidNameException();
            }
            bool reachedDigit = false;
            //check rest of name
            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (reachedDigit && !char.IsDigit(c))
                {
                    throw new InvalidNameException();
                }
                else if (!reachedDigit && char.IsDigit(c))
                {
                    reachedDigit = true;
                }
                else if (!reachedDigit && !char.IsLetter(c)) //Digits already checked so must be a letter
                {
                    throw new InvalidNameException();
                }
            }
            //Check if name meets user constraints
            if (!IsValid(name))
            {
                throw new InvalidNameException();
            }
            //Name is valid by spreadsheet and user. No exception thrown.
            return;
        }

        /// <summary>
        /// The spreadsheets lookup delegate.
        /// Looks up the named cells value.
        /// Throws an ArgumentException if the value of name is anything but a double.
        /// </summary>
        /// <param name="name">The name of the cell whose value is being looked up.</param>
        /// <returns>Value of named cell if a double, or throws an ArgumentException.</returns>
        private double LookUp(string name)
        {
            name = Normalize(name);
            Cell cell;
            if (filledCells.TryGetValue(name, out cell))
            {
                object result = cell.Value;
                if (result is double)
                {
                    return (double)result;
                }
                else if (result is FormulaError)
                {
                    throw new ArgumentException("Value of " + name + " is a Formula Error.");
                }
                else
                {
                    throw new ArgumentException("Value of " + name + " is a string.");
                }
            }
            throw new ArgumentException("The value of " + name + " is undefined.");

        }

        /// <summary>
        /// Creates a spreadsheet from file.
        /// 
        /// Throws a SpreasheetReadWriteException if any problems occured 
        /// while reading from the file.
        /// </summary>
        /// <param name="pathname">The location of the xml file.</param>
        private void ReadSpreadsheetFromFile(string pathname)
        {
            if (pathname == null)
            {
                throw new SpreadsheetReadWriteException("The pathname can not be null.");
            }
            try
            {
                using (XmlReader reader = XmlReader.Create(pathname))
                {
                    bool readSpread = false;
                    bool inCell = false;
                    bool reachedName = false;
                    string name = null;
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet": //Check version
                                    if (!reader.GetAttribute("version").Equals(Version))
                                    {
                                        throw new SpreadsheetReadWriteException("The version of this spreadsheet does not match the version saved.");
                                    }
                                    readSpread = true;
                                    break;
                                case "cell": //Flag that in cell
                                    if (readSpread == false)
                                    {
                                        throw new SpreadsheetReadWriteException("The saved spreadsheet did not start with <spreadsheet>.");
                                    }
                                    inCell = true;
                                    break;
                                case "name":
                                    if (inCell == false)
                                    {
                                        throw new SpreadsheetReadWriteException("The name of cell is not within a cell.");
                                    }
                                    reader.Read();
                                    name = reader.Value;
                                    reachedName = true;
                                    break;
                                case "contents":
                                    if (inCell == false)
                                    {
                                        throw new SpreadsheetReadWriteException("The contents is not in a cell.");
                                    }
                                    if (reachedName == false)
                                    {
                                        throw new SpreadsheetReadWriteException("The contents came before the name.");
                                    }
                                    reader.Read();
                                    SetContentsOfCell(name, reader.Value);
                                    inCell = false;
                                    reachedName = false;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// Cells are what make up a spreadsheet. A Cell has content and a value.
        /// </summary>
        private class Cell
        {
            /// <summary>
            /// Creates an empty cell, whose content is empty and has no value.
            /// </summary>
            internal Cell()
            {
                Content = "";
                Value = "";
            }

            /// <summary>
            /// The content of the cell. 
            /// The content of this cell can either be a string, double or Formula.
            /// A Formula can depend on the value of variables. A variable is a name of a cell, 
            /// and its value can either be a double, or undefined.
            /// An empty string means the cell is empty.
            /// </summary>
            internal object Content { get; set; }

            /// <summary>
            /// The Value of the cell.
            /// The value can either be a string, double or SpreadsheetUtilities.FormulaError.
            /// </summary>
            internal object Value { get; set; }
        }
    }
}

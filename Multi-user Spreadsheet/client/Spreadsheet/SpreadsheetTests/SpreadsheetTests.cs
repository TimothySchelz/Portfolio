using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;
using System.Linq;

namespace SpreadsheetTests
{
    /// <summary>
    /// Tests the PS5 version of Spreadsheet.
    /// </summary>
    [TestClass]
    public class SpreadsheetTests
    {
        /// <summary>
        /// An invalid name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableLooseValidator()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("a", "10");
        }

        /// <summary>
        /// An invalid name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableLooseValidator2()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("_", "");
        }

        /// <summary>
        /// An invalid name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableLooseValidator3()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("9A", "=a1");
        }

        /// <summary>
        /// An invalid name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableLooseValidator4()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=a");
        }

        /// <summary>
        /// An invalid name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetUtilities.FormulaFormatException))]
        public void TestVariableLooseValidator5()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=a&1");
        }

        /// <summary>
        /// An invalid name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableLooseValidator6()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("a1a", "10");
        }

        /// <summary>
        /// An invalid name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableLooseValidator7()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("a&1", "10");
        }

        /// <summary>
        /// An invalid name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestUserIsValid()
        {
            Spreadsheet s = new Spreadsheet(x => false, x => x, "default");
            s.SetContentsOfCell("a1", "10");
        }

        /// <summary>
        /// An invalid name should throw an exception after normilize.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestUserIsValidAfterNorm()
        {
            Spreadsheet s = new Spreadsheet(x => !x.Equals("A1"), x => x.ToUpper(), "default");
            s.SetContentsOfCell("a1", "10");
        }

        /// <summary>
        /// Value of Formula is Formula error if cell is empty
        /// </summary>
        [TestMethod]
        public void TestCellValueFormulaToEmpty()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1");
            Assert.IsTrue(s.GetCellValue("A1") is FormulaError);
        }

        /// <summary>
        /// Read file should be same as original.
        /// </summary>
        [TestMethod]
        public void TestFileSaveAndRead()
        {
            Spreadsheet ss = new Spreadsheet();
            int testSize = 1000;

            for (int i = 0; i < testSize; i++)
            {
                ss.SetContentsOfCell("A" + i, i.ToString());
            }
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());

            //Save Test
            ss.Save("..\\..\\TestResources\\Test.xml");
            //Open saved spreadsheet
            ss = new Spreadsheet("..\\..\\TestResources\\Test.xml", s => true, s => s, "default");
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());

            for (int i = 0; i < testSize; i++)
            {
                Assert.AreEqual((double)i, ss.GetCellContents("A" + i));
                ss.SetContentsOfCell("A" + i, "");
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
            //Save Test
            Assert.IsTrue(ss.Changed);
            ss.Save("..\\..\\TestResources\\Test.xml");
            Assert.IsFalse(ss.Changed);
            //Open saved spreadsheet
            ss = new Spreadsheet("..\\..\\TestResources\\Test.xml", s => true, s => s, "default");
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
        }

        /// <summary>
        /// File name can not be null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadWriteNullPathname()
        {
            try
            {
                new Spreadsheet(null, s => true, s => s, "default");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("The pathname can not be null.", e.Message);
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// The versions must match
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestVersionNotMatch()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.Save("..\\..\\TestResources\\Test.xml");
            try
            {
                new Spreadsheet("..\\..\\TestResources\\Test.xml", s => true, s => s, "not default");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("The version of this spreadsheet does not match the version saved.", e.Message);
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// The contents must be within cell element
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestContentsNotInCell()
        {
            try
            {
                new Spreadsheet("..\\..\\TestResources\\contents.xml", DefaultValid, DefaultNorm, "default");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("The contents is not in a cell.", e.Message);
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// The name element must be within cell element.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestNameNotInCell()
        {
            try
            {
                new Spreadsheet("..\\..\\TestResources\\name.xml", DefaultValid, DefaultNorm, "default");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("The name of cell is not within a cell.", e.Message);
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// The name element must come before contents element
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestContentsBeforeName()
        {
            try
            {
                new Spreadsheet("..\\..\\TestResources\\contentsBeforeName.xml", DefaultValid, DefaultNorm, "default");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("The contents came before the name.", e.Message);
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// File does not start with spreadsheet element.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestNoSpread()
        {
            try
            {
                new Spreadsheet("..\\..\\TestResources\\noSpread.xml", DefaultValid, DefaultNorm, "default");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("The saved spreadsheet did not start with <spreadsheet>.", e.Message);
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// The file name can not be null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestNullSave()
        {
            new Spreadsheet().Save(null);
        }

        /// <summary>
        /// The file name can not be empty.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestOtherSaveIssues()
        {
            new Spreadsheet().Save("");
        }

        /// <summary>
        /// The file name can not be null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestGetVersionNull()
        {
            new Spreadsheet().GetSavedVersion(null);
        }

        /// <summary>
        /// The file name can not be an empty string.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestGetVersionEmpty()
        {
            new Spreadsheet().GetSavedVersion("");
        }

        /// <summary>
        /// The versions must match.
        /// </summary>
        [TestMethod]
        public void TestGetVersionDefault()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.Save("..\\..\\TestResources\\Test.xml");
            Assert.AreEqual("default", ss.GetSavedVersion("..\\..\\TestResources\\Test.xml"));
        }

        /// <summary>
        /// The file must start with spreadsheet element.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestNoSpreadSavedVerison()
        {
            try
            {
                new Spreadsheet().GetSavedVersion("..\\..\\TestResources\\noSpread.xml");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("The file does not start with an element 'spreadsheet'.", e.Message);
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }

        /// <summary>
        /// The value of empty cell should be empty.
        /// </summary>
        [TestMethod]
        public void TestGetValueEmptyCell()
        {
            Assert.AreEqual("", new Spreadsheet().GetCellValue("ABCVFFRE1111111"));
        }

        /// <summary>
        /// The value of formula should be evaluated.
        /// </summary>
        [TestMethod]
        public void TestGetValueFormulaCell()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "5");
            ss.SetContentsOfCell("A1", "=B1");
            Assert.AreEqual(5.0, ss.GetCellValue("A1"));
        }

        /// <summary>
        /// Loading and saving should work with formulas.
        /// </summary>
        [TestMethod]
        public void TestSaveFormulas()
        {
            Spreadsheet ss = new Spreadsheet();
            int testSize = 200;
            HashSet<string> changed = new HashSet<string>();

            for (int i = 0; i < testSize; i++)
            {
                changed.Add("A" + i);
                Assert.IsTrue(changed.SetEquals(new HashSet<string>
                    (ss.SetContentsOfCell("A" + i, "=A" + (i + 1).ToString() + "+ 1"))));
                Assert.AreEqual(new Formula("A" + (i + 1).ToString() + "+1"), ss.GetCellContents("A" + i));
            }
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());

            //Save and load
            ss.Save("..\\..\\TestResources\\Test.xml");
            ss = new Spreadsheet("..\\..\\TestResources\\Test.xml", DefaultValid, DefaultNorm, "default");
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());

            for (int i = testSize - 1; i >= 0; i--)
            {
                Assert.IsTrue(changed.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, ""))));
                changed.Remove("A" + i);
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());

            //Save and load
            ss.Save("..\\..\\TestResources\\Test.xml");
            ss = new Spreadsheet("..\\..\\TestResources\\Test.xml", DefaultValid, DefaultNorm, "default");
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());

            for (int i = testSize - 1; i >= 0; i--)
            {
                Assert.IsTrue(new HashSet<string>() { "A" + i }.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, "=A" + (i + 1)))));
                Assert.AreEqual(new Formula("A" + (i + 1).ToString()), ss.GetCellContents("A" + i));
            }
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());

            //Save and load
            ss.Save("..\\..\\TestResources\\Test.xml");
            ss = new Spreadsheet("..\\..\\TestResources\\Test.xml", DefaultValid, DefaultNorm, "default");
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());

            Assert.AreEqual(0, changed.Count());
            for (int i = 0; i < testSize; i++)
            {
                Assert.IsTrue(new HashSet<string>() { "A" + i }.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, ""))));
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
            //Save and load
            ss.Save("..\\..\\TestResources\\Test.xml");
            ss = new Spreadsheet("..\\..\\TestResources\\Test.xml", DefaultValid, DefaultNorm, "default");
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());

            for (int i = testSize - 1; i >= 0; i--)
            {
                changed.Add("A" + i);
                Assert.IsTrue(new HashSet<string>() { "A" + i }.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, "=A" + (i + 1)))));
                Assert.AreEqual(new Formula("A" + (i + 1).ToString()), ss.GetCellContents("A" + i));
            }
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());
            //Save and load
            ss.Save("..\\..\\TestResources\\Test.xml");
            ss = new Spreadsheet("..\\..\\TestResources\\Test.xml", DefaultValid, DefaultNorm, "default");
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());

            for (int i = testSize - 1; i >= 0; i--)
            {
                Assert.IsTrue(changed.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, ""))));
                changed.Remove("A" + i);
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
            //Save and load
            ss.Save("..\\..\\TestResources\\Test.xml");
            ss = new Spreadsheet("..\\..\\TestResources\\Test.xml", DefaultValid, DefaultNorm, "default");
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
        }

        /// <summary>
        /// Everything is valid.
        /// </summary>
        /// <param name="s">String to Validate</param>
        /// <returns>True always.</returns>
        public bool DefaultValid(string s)
        {
            return true;
        }

        /// <summary>
        /// Everything is already normalized.
        /// </summary>
        /// <param name="s">String to normalize.</param>
        /// <returns>The input "s"</returns>
        public string DefaultNorm(string s)
        {
            return s;
        }
    }
}

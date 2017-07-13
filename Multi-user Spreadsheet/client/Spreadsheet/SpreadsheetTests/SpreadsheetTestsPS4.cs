using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Testing suite for Spreadsheet.
/// 
/// Author: Scott Steadham
/// </summary>
namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTestsPS4
    {
        /// <summary>
        /// If there are no filled cells, and a formula contains a reference to the
        /// cell who contains it, then it should throw a circular exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestSetContentsFormulaEmptyCircular()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "=a1+a2");
        }

        /// <summary>
        /// If spreadsheet is empty, all cells should be empty.
        /// </summary>
        [TestMethod]
        public void testGetNamesOfEmptySpreadsheet()
        {
            Spreadsheet ss = new Spreadsheet();
            Assert.AreEqual(0, new HashSet<string>(ss.GetNamesOfAllNonemptyCells()).Count);
        }

        /// <summary>
        /// If contents of cell are "" it should be considered empty.
        /// </summary>
        [TestMethod]
        public void TestGetNamesAfterEmptyStringAdd()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "");
            Assert.AreEqual(0, new HashSet<string>(ss.GetNamesOfAllNonemptyCells()).Count);
        }

        /// <summary>
        /// Setting the contents to "" should make the cell considered empty.
        /// </summary>
        [TestMethod]
        public void TestGetNamesAfterSettingContentstoEmtpy()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "a1");
            Assert.AreEqual(1, new HashSet<string>(ss.GetNamesOfAllNonemptyCells()).Count);
            Assert.AreEqual("a1", ss.GetCellContents("a1"));
            ss.SetContentsOfCell("a1", "");
            Assert.AreEqual(0, new HashSet<string>(ss.GetNamesOfAllNonemptyCells()).Count);
            Assert.AreEqual("", ss.GetCellContents("a1"));
        }

        /// <summary>
        /// Setting contents of cell to formula should consider cell filled.
        /// </summary>
        [TestMethod]
        public void TestGetNamesFormulaAdd()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "=b1");
            Assert.IsTrue(new HashSet<string>() { "a1" }.SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
            Assert.AreEqual(new Formula("b1"), ss.GetCellContents("a1"));
        }

        /// <summary>
        /// Setting cell contents to a double should fill the cell with a double.
        /// </summary>
        [TestMethod]
        public void TestGetNamesAddDouble()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "10");
            Assert.IsTrue(new HashSet<string>() { "a1" }.SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
            Assert.AreEqual(10.0, ss.GetCellContents("a1"));
        }

        /// <summary>
        /// If naem is null, should throw and InvalidNameException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetCellContentsNullName()
        {
            new Spreadsheet().GetCellContents(null);
        }

        /// <summary>
        /// If name is invalid should throw an InvalidNameException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetCellContentsInvalidName()
        {
            new Spreadsheet().GetCellContents("9__");
        }

        /// <summary>
        /// If name is null, should throw an InvalidNameException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsDoubleNullName()
        {
            new Spreadsheet().SetContentsOfCell(null, "10");
        }

        /// <summary>
        /// If name is invalid shoudl throw an InvalidNameException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsOfCellDoubleInvalidName()
        {
            new Spreadsheet().SetContentsOfCell("9", "1000");
        }

        /// <summary>
        /// If name is null, should throw an InvalidNameException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsTextNullName()
        {
            new Spreadsheet().SetContentsOfCell(null, "Text");
        }

        /// <summary>
        /// If name is invalid, should throw an InvalidNameException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsOfCellTextInvalidName()
        {
            new Spreadsheet().SetContentsOfCell("____&", "");
        }

        /// <summary>
        /// If text is null, should throw an ArgumentNullExeption.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetContentsOfCellTextNull()
        {
            new Spreadsheet().SetContentsOfCell("____&", (string)null);
        }

        /// <summary>
        /// If name is null, should throw an InvalidNameException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsFormulaNullName()
        {
            new Spreadsheet().SetContentsOfCell(null, "=a+b");
        }

        /// <summary>
        /// If Name is invalid should throw an InvalidNameException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsOfCellFormulaInvalidName()
        {
            new Spreadsheet().SetContentsOfCell("____&", "=a+b");
        }

        /// <summary>
        /// If formula is null should throw an ArgumentNullException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetContentsOfCellNullFormulaName()
        {
            new Spreadsheet().SetContentsOfCell("____&", null);
        }

        /// <summary>
        /// If Name is invalid should throw an InvalidNameException.
        /// "" is an invalid name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsOfCellEmptyName()
        {
            new Spreadsheet().SetContentsOfCell("", "10");
        }

        /// <summary>
        /// If name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        [TestMethod]
        public void TestSetContentsOfCellDouble()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "=A1*2");
            ss.SetContentsOfCell("C1", "=B1+A1");
            Assert.AreEqual(2, new List<string>(ss.GetNamesOfAllNonemptyCells()).Count);
            Assert.IsTrue(new HashSet<string>() { "B1", "C1" }.SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
            List<string> result = new List<string>(ss.SetContentsOfCell("A1", "10"));
            Assert.IsTrue(new List<string>() { "A1", "B1", "C1" }.SequenceEqual(result));
            Assert.AreEqual(3, new List<string>(ss.GetNamesOfAllNonemptyCells()).Count);
            Assert.IsTrue(new HashSet<string>() { "A1", "B1", "C1" }.SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
        }

        /// <summary>
        /// Cell names should be case sensitive
        /// </summary>
        [TestMethod]
        public void TestCaseSesitive()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "10");
            ss.SetContentsOfCell("b1", "11");
            ss.SetContentsOfCell("c1", "=C1");
            ss.SetContentsOfCell("C1", "12");
            ss.SetContentsOfCell("A1", "=a1+b1+c1");
            Assert.AreEqual(5, new List<string>(ss.GetNamesOfAllNonemptyCells()).Count);
            Assert.AreEqual(12.0, ss.GetCellContents("C1"));
            Assert.AreEqual("C1", ss.GetCellContents("c1").ToString());
        }

        /// <summary>
        /// A circular dependency exists when a cell depends on itself.
        /// A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
        /// A1 depends on B1, which depends on C1, which depends on A1.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircular()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "=B1*2");
            ss.SetContentsOfCell("B1", "=C1*2");
            ss.SetContentsOfCell("C1", "=A1*2");
        }

        /// <summary>
        /// if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        [TestMethod]
        public void TestSetContentsText()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "=A1*2");
            ss.SetContentsOfCell("C1", "=B1+A1");
            List<string> result = new List<string>(ss.SetContentsOfCell("A1", "Text"));
            Assert.IsTrue(new List<string>() { "A1", "B1", "C1" }.SequenceEqual(result));
        }

        /// <summary>
        /// if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        [TestMethod]
        public void TestSetContentsTextEvenWithEmpty()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "=A1*2");
            ss.SetContentsOfCell("C1", "=B1+A1");
            List<string> result = new List<string>(ss.SetContentsOfCell("A1", ""));
            Assert.IsTrue(new List<string>() { "A1", "B1", "C1" }.SequenceEqual(result));
        }

        /// <summary>
        /// if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        [TestMethod]
        public void TestSetContentsFormula()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("B1", "=A1*2");
            ss.SetContentsOfCell("C1", "=B1+A1");
            List<string> result = new List<string>(ss.SetContentsOfCell("A1", "=A2"));
            Assert.IsTrue(new List<string>() { "A1", "B1", "C1" }.SequenceEqual(result));
        }

        /// <summary>
        /// If name is null, should throw an ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetDirectDependentsNullName()
        {
            Spreadsheet ss = new Spreadsheet();
            PrivateObject ss_accessor = new PrivateObject(ss);
            ss_accessor.Invoke("GetDirectDependents", new object[] { null });
        }

        /// <summary>
        /// If name is invalid should throw InvalidNameException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetDirectDependentsInvalidName()
        {
            Spreadsheet ss = new Spreadsheet();
            PrivateObject ss_accessor = new PrivateObject(ss);
            ss_accessor.Invoke("GetDirectDependents", new object[] { "9" });
        }

        /// <summary>
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        [TestMethod]
        public void TestGetDirectDependents()
        {
            Spreadsheet ss = new Spreadsheet();
            PrivateObject ss_accessor = new PrivateObject(ss);
            ss.SetContentsOfCell("A1", "3");
            ss.SetContentsOfCell("B1", "=A1*A1");
            ss.SetContentsOfCell("C1", "=B1 + A1");
            ss.SetContentsOfCell("D1", "=B1 - C1");
            List<string> result = new List<string>((IEnumerable<string>)ss_accessor.Invoke("GetDirectDependents", new object[] { "A1" }));
            Assert.IsTrue(new List<string>() { "B1", "C1" }.SequenceEqual(result));
        }

        /// <summary>
        /// Cycle detection should not change spreadsheet.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularGraphNotChangedDouble()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "10");
            try
            {
                ss.SetContentsOfCell("a1", "=a1");
            }
            catch (CircularException)
            {
                Assert.AreEqual(10.0, ss.GetCellContents("a1"));
                ss.SetContentsOfCell("b1", "=a1");
                Assert.AreEqual(new Formula("a1"), ss.GetCellContents("b1"));
                try
                {
                    ss.SetContentsOfCell("a1", "=b1");
                }
                catch (CircularException)
                {
                    Assert.AreEqual(10.0, ss.GetCellContents("a1"));
                    Assert.AreEqual(new Formula("a1"), ss.GetCellContents("b1"));
                    Assert.IsTrue(new HashSet<string>() { "a1", "b1" }.SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
                    throw new CircularException();
                }
            }
        }

        /// <summary>
        /// Cycle detection should not change spreadsheet.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularGraphNotChangedText()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "text");
            try
            {
                ss.SetContentsOfCell("a1", "=a1");
            }
            catch (CircularException)
            {
                Assert.AreEqual("text", ss.GetCellContents("a1"));
                ss.SetContentsOfCell("b1", "=a1");
                Assert.AreEqual(new Formula("a1"), ss.GetCellContents("b1"));
                try
                {
                    ss.SetContentsOfCell("a1", "=b1");
                }
                catch (CircularException)
                {
                    Assert.AreEqual("text", ss.GetCellContents("a1"));
                    Assert.AreEqual(new Formula("a1"), ss.GetCellContents("b1"));
                    Assert.IsTrue(new HashSet<string>() { "a1", "b1" }.SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
                    throw new CircularException();
                }
            }
        }

        /// <summary>
        /// Cycle detection should not change spreadsheet.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularGraphNotChangedFormula()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("a1", "=b1");
            try
            {
                ss.SetContentsOfCell("a1", "=a1");
            }
            catch (CircularException)
            {
                Assert.AreEqual(new Formula("b1"), ss.GetCellContents("a1"));
                try
                {
                    ss.SetContentsOfCell("b1", "=a1");
                }
                catch (CircularException)
                {
                    Assert.AreEqual(new Formula("b1"), ss.GetCellContents("a1"));
                    Assert.AreEqual("", ss.GetCellContents("b1"));
                    Assert.IsTrue(new HashSet<string>() { "a1" }.SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
                    throw new CircularException();
                }
            }
        }

        /// <summary>
        /// Cycle detection should not change spreadsheet.
        /// Even with empty Graph.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularGraphNotChangedEmpty()
        {
            Spreadsheet ss = new Spreadsheet();
            try
            {
                ss.SetContentsOfCell("a1", "=a1");
            }
            catch (CircularException)
            {
                Assert.AreEqual("", ss.GetCellContents("a1"));
                Assert.IsTrue(new HashSet<string>().SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
                ss.SetContentsOfCell("b1", "=a1");
                try
                {
                    ss.SetContentsOfCell("a1", "=b1");
                }
                catch (CircularException)
                {
                    Assert.AreEqual("", ss.GetCellContents("a1"));
                    Assert.AreEqual(new Formula("a1"), ss.GetCellContents("b1"));
                    Assert.IsTrue(new HashSet<string>() { "b1" }.SetEquals(new HashSet<string>(ss.GetNamesOfAllNonemptyCells())));
                    throw new CircularException();
                }
            }

        }

        /// <summary>
        /// Creating bunch of cells, and Removing them should 
        /// end graph as empty.
        /// </summary>
        [TestMethod]
        public void StressTest1()
        {
            Spreadsheet ss = new Spreadsheet();
            int testSize = 1000;

            for (int i = 0; i < testSize; i++)
            {
                ss.SetContentsOfCell("A" + i, i.ToString());
            }
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());
            for (int i = 0; i < testSize; i++)
            {
                Assert.AreEqual((double)i, ss.GetCellContents("A" + i));
                ss.SetContentsOfCell("A" + i, "");
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
        }

        /// <summary>
        /// Setting bunch of cells to string and then setting the cells to empty
        /// should end the graph as empty.
        /// </summary>
        [TestMethod]
        public void StressTest2()
        {
            Spreadsheet ss = new Spreadsheet();
            int testSize = 1000;

            for (int i = 0; i < testSize; i++)
            {
                ss.SetContentsOfCell("A" + i, i.ToString());
            }
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());
            for (int i = 0; i < testSize; i++)
            {
                Assert.AreEqual((double)i, ss.GetCellContents("A" + i));
                ss.SetContentsOfCell("A" + i, "");
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
        }

        /// <summary>
        /// With formula, adds a much and removes a bunch in every order possible.
        /// Also checks that returns correct cells that need to be recalculated.
        /// </summary>
        [TestMethod]
        public void StressTest3()
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
            for (int i = testSize - 1; i >= 0; i--)
            {
                Assert.IsTrue(changed.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, ""))));
                changed.Remove("A" + i);
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
            for (int i = testSize - 1; i >= 0; i--)
            {
                Assert.IsTrue(new HashSet<string>() { "A" + i }.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, "=A" + (i + 1)))));
                Assert.AreEqual(new Formula("A" + (i + 1).ToString()), ss.GetCellContents("A" + i));
            }
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());
            Assert.AreEqual(0, changed.Count());
            for (int i = 0; i < testSize; i++)
            {
                Assert.IsTrue(new HashSet<string>() { "A" + i }.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, ""))));
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
            for (int i = testSize - 1; i >= 0; i--)
            {
                changed.Add("A" + i);
                Assert.IsTrue(new HashSet<string>() { "A" + i }.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, "=A" + (i + 1)))));
                Assert.AreEqual(new Formula("A" + (i + 1).ToString()), ss.GetCellContents("A" + i));
            }
            Assert.AreEqual(testSize, ss.GetNamesOfAllNonemptyCells().Count());
            for (int i = testSize - 1; i >= 0; i--)
            {
                Assert.IsTrue(changed.SetEquals(new HashSet<string>(ss.SetContentsOfCell("A" + i, ""))));
                changed.Remove("A" + i);
            }
            Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count());
        }
    }
}

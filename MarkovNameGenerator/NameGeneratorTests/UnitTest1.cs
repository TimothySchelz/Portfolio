using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MarkovNameGenerator;

namespace NameGeneratorTests
{
    [TestClass]
    public class UnitTest1
    {
        List<String> Single3;
        List<String> SingleVaried;
        List<String> None0;
        List<String> MixedCaseSingle3;
        List<String> ABC3;
        List<String> FullVaried;
        List<String> EmptyList;
        List<String> ABC;

        NameGenerator nameGen;

        [TestInitialize]
        public void TestInitialize()
        {
            // These are named using the format <Types of Letters><Length>
            Single3 = new List<string>(){"AAA", "AAA", "AAA", "AAA", "AAA", "AAA",
                "AAA", "AAA", "AAA", "AAA" };
            SingleVaried = new List<string>() { "AAAAAA", "A", "", "AAA",
                "AAAAAAAAA", "AAA", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "AAAAA",
                "A", "AA" };
            None0 = new List<string>() {"", "", "", "", "", "", "", ""};
            MixedCaseSingle3 = new List<string>() { "AAA", "AAa", "AaA", "Aaa",
                "aAA", "aAa", "aaA", "aaa" };
            ABC3 = new List<string>() {"AAA", "AAB", "AAC", "ABA", "ABB", "ABC",
                "ACA", "ACB", "ACC", "BAA", "BAB", "BAC", "BBA", "BBB", "BBC", "BCA", "BCB",
                "BCC", "CAA", "CAB", "CAC", "CBA", "CBB", "CBC", "CCA", "CCB", "CCC"};
            FullVaried = new List<string>() { "Algernon", "Alice", "Albert",
                "Alfred", "Angela", "AAron", "Arnold", "Bernard", "Barry", "Bartholomew",
                "Brandon", "Barry", "Bertram", "Bobby", "Bob", "Billy", "Billiam", "Boris" };

            // Other lists where convention doesn't apply
            EmptyList = new List<string>();
            ABC = new List<string>(){"ABC", "ABC", "ABC", "ABC", "ABC", "ABC",
                "ABC", "ABC", "ABC", "ABC", "ABC","ABC","ABC","ABC","ABC","ABC",};

            // The name generator that can be used in all the tests
            nameGen = new NameGenerator();
        }

        [TestMethod]
        public void GenerateName_AllAs()
        {
            nameGen.LoadNames(Single3);
            String result = nameGen.GenerateName();
            Console.WriteLine(result);
            Assert.IsTrue(result.Length > 0);
        }


        [TestMethod]
        public void checkFullVaried()
        {
            nameGen.LoadNames(FullVaried);
            String result = nameGen.GenerateName();
            Console.WriteLine(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void checkEmptyList()
        {
            nameGen.LoadNames(EmptyList);
            String result = nameGen.GenerateName();
            Console.WriteLine(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void checkABC()
        {
            nameGen.LoadNames(ABC);
            String result = nameGen.GenerateName();
            Console.WriteLine(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void checkABC3()
        {
            nameGen.LoadNames(ABC3);
            String result = nameGen.GenerateName();
            Console.WriteLine(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void checkMixedCaseSingle3()
        {
            nameGen.LoadNames(MixedCaseSingle3);
            String result = nameGen.GenerateName();
            Console.WriteLine(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void checkNone0()
        {
            nameGen.LoadNames(None0);
            String result = nameGen.GenerateName();
            Console.WriteLine(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void checkSingleVaried()
        {
            nameGen.LoadNames(SingleVaried);
            String result = nameGen.GenerateName();
            Console.WriteLine(result);
            Assert.IsTrue(result.Length > 0);
        }
    }
}

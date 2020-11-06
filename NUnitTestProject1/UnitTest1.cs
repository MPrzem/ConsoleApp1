using ConsoleApp1;
using NUnit.Framework;
using System;

namespace NUnitTestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var path = new Uri(
System.IO.Path.GetDirectoryName(
System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
).LocalPath;
            string file = path + "\\data_test.csv";
            EmulatorDataProviderCSV dataProviderCSV = new EmulatorDataProviderCSV(2);
            dataProviderCSV.LoadData(file);
            Assert.AreEqual(2, dataProviderCSV.nOfSections);
        }
    }
}
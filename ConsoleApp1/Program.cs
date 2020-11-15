using ConsoleApp1;
using System;

namespace CreateSimpleCsvFile
{
    class Program
    {
        static void Main()
        {
            var path = new Uri(
    System.IO.Path.GetDirectoryName(
        System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
    ).LocalPath;
            string file = path + "\\data1.csv";
            EmulatorDataProviderCSV dataProviderCSV =new EmulatorDataProviderCSV(35,14);
            dataProviderCSV.LoadData(file);
            var p = new Emulator(dataProviderCSV.nOfInputs,35,8, dataProviderCSV.outIdx);
            var n = new NeuroReg(3,6, 3,dataProviderCSV.outIdx, dataProviderCSV.soilMoisIdx, dataProviderCSV.stepsback);
            p.ReadWMatrix(path + "\\net1.bin");
            //p.Learn(dataProviderCSV, 0.2, 30, 1000000, path + "\\net1.bin"); ;
            n.Learn(dataProviderCSV, p, 0.15, 0.1, 1000000);
            Console.ReadLine();
        }
    }
}

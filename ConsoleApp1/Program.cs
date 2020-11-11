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
            string file = path + "\\data.csv";
            EmulatorDataProviderCSV dataProviderCSV =new EmulatorDataProviderCSV(21,14);
            dataProviderCSV.LoadData(file);
            var p = new Emulator(dataProviderCSV.nOfInputs,20,8,dataProviderCSV.soilMoisIdx);
            var n = new NeuroReg(4,20, 8,dataProviderCSV.outIdx);
            n.Learn(dataProviderCSV, p, 0.3, 0.1, 10000);
            p.Learn(dataProviderCSV,0.2, 0.1 ,1000000, path + "\\net1.bin");
            p.ReadWMatrix(path + "\\net1.bin");
            p.Learn(dataProviderCSV, 0.35, 0.1, 1000000, path + "\\net1.bin");
            Console.ReadLine();
        }
    }
}

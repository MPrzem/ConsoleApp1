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
            EmulatorDataProviderCSV dataProviderCSV =new EmulatorDataProviderCSV(21);
            dataProviderCSV.LoadData(file);
            int[] net_def = new int[] { 6, 15, 9, 1 };
            var p = new Emulator(6,10,8);
            p.Learn(dataProviderCSV,0.35, 0.58 ,1000000, path + "\\net1.bin");
            p.ReadWMatrix(path + "\\net1.bin");
            p.Learn(dataProviderCSV, 0.35, 0.58, 1000000, path + "\\net1.bin");
            Console.ReadLine();
        }
    }
}

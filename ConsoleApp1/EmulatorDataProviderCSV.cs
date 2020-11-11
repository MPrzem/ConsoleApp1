using Amazon.Util.Internal;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class EmulatorDataProviderCSV : IEmulatorDataProvider
    {
        private int stepsback;
        Random rnd = new Random();
        List<InputRecord> inputData_;
        List<section> sections_=new List<section>();
        public int nOfInputs { get; set; }
        public int nOfSections { get { return sections_.Count; } private set { } }

        public int outIdx { get; set; }
        public int soilMoisIdx { get; set; }

        private int minimum_section_long;
        public EmulatorDataProviderCSV(int minimum_section_long_,int stepsback_)
        {
            this.stepsback = stepsback_;
            minimum_section_long = minimum_section_long_;
            soilMoisIdx = 3;
            outIdx = 4 + stepsback;
            nOfInputs = 2 * stepsback + 5;
        }
        public struct section
        {
            public int start;
            public int end;
            public section(int _start,int _end)
            {
                start = _start;
                end = _end;
            }
        }

        private void find_sections()
        {
            int i = 0;
            int start = 0;
            while(i<(inputData_.Count-1))
            {
                long time_distance = inputData_[i + 1].ts - inputData_[i].ts;
                if (time_distance > 20000)
                {
                    if(i-start>= minimum_section_long-1)
                        sections_.Add(new section(start, i));
                    start = i+1;
                }
                Console.WriteLine("nb of sections{0}", sections_.Count);
                    i++;
            }
            sections_.Add(new section(start, i));

        }
        public void LoadData(string csvPath)
        {

            using (var streamReader = File.OpenText(csvPath))
            {
                var reader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
                inputData_ = reader.GetRecords<InputRecord>().ToList();
                inputData_.Sort((x, y) => x.ts.CompareTo(y.ts));

            }
            find_sections();
            foreach (var inputvec in inputData_)
            {
                Console.WriteLine($"{inputvec.ts}");
            }

        }
        static double scaleToOne(double val, double min, double max)
        {
            return (val - min) / (max - min);
        }
        public void GetRandInputVector(ref double[] inputs, ref double outVal)
        {
            int Idx, sectionIdx;
            sectionIdx = rnd.Next(nOfSections - 1);
            Idx = rnd.Next(sections_[sectionIdx].start+20, sections_[sectionIdx].end - 1);//one record is required for evaluate predition
            GetInputVector(ref inputs, ref outVal, Idx);
        }
        public int GetRandInputVector(ref double[] inputs, ref double outVal,int nToEnd)
        {
            int Idx, sectionIdx;
            sectionIdx = rnd.Next(nOfSections - 1);
            Idx = rnd.Next(sections_[sectionIdx].start, sections_[sectionIdx].end - nToEnd);//one record is required for evaluate predition
            GetInputVector(ref inputs, ref outVal, Idx);
            return Idx;
        }
        public void GetInputVector(ref double[] inputs, ref double outVal, int Idx)
        {
            inputs[0] = scaleToOne(inputData_[Idx].AirMoiscure, 0, 100);
            inputs[1] = scaleToOne(inputData_[Idx].AirTemp, -20, 100);
            inputs[2] = scaleToOne(inputData_[Idx].RainSens, 0, 4095);
            for(int i=0;i<=stepsback;i++)
                inputs[i+3] = scaleToOne(inputData_[Idx - i].SoilSens, 0, 4095);
            for (int i = 0; i <= stepsback; i++)
                inputs[i+4+stepsback] = scaleToOne(inputData_[Idx-i].OutVal, 0, 100);
            outVal = (double)scaleToOne(inputData_[Idx + 1].SoilSens, 0, 4095);
        }
    }
}

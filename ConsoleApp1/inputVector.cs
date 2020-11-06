using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class inputVector
    {
        public Int64 ts { get; set; }
        public int SoilSens { get; set; }
        public int RainSens { get; set; }
        public float AirMoiscure { get; set; }
        public float AirTemp { get; set; }
        public int IsOutside { get; set; }
        public float OutVal { get; set; }
        public int SoilSens_1bf { get; set; }
        public int SoilSens_20bf { get; set; }
        //byc moze w przyszlosci 0-100 szansa opadow za godzine czy tam za ileś
    }
}

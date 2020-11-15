using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public interface IEmulatorDataProvider
    {
        int nOfInputs { get; set; }
        int outIdx { get; set; }
        int soilMoisIdx { get; set; }
        int stepsback { get; set; }
        int GetRandInputVector(ref double[] inputs, ref double outVal, int nToEnd);
        void GetRandInputVector(ref double[] inputs,ref double outVal);
        bool GetInputVectorOneByOne(ref double[] inputs, ref double outVal);
        void GetInputVector(ref double[] inputs, ref double outVal, int Idx);
    }
}

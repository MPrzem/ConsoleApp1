using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public interface IEmulatorDataProvider
    {
        void GetRandInputVector(ref double[] inputs,ref double outVal);

    }
}

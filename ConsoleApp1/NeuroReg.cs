using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class NeuroReg
    {

        List<Layer> layers;
        double lastOutput;
        int outIdx = 6;
        int moiscureIdx;
        int stepsBack;
        int nOfInputs;

        public NeuroReg(int nOfInputs_,int neuronsInHiddenLay1, int neuronsInHiddenLay2, int outIdx_, int moiscureIdx_, int stepsBack_)
        {
            stepsBack = stepsBack_;
            outIdx = outIdx_;
            moiscureIdx = moiscureIdx_;
            nOfInputs = nOfInputs_;
            layers = new List<Layer>();
            layers.Add(new Layer(nOfInputs_, nOfInputs_, Neuron.ActFun, Neuron.ActFunDeriv));
            layers.Add(new Layer(neuronsInHiddenLay1, nOfInputs_, Neuron.ActFun, Neuron.ActFunDeriv));
            layers.Add(new Layer(neuronsInHiddenLay2, neuronsInHiddenLay1, Neuron.ActFun, Neuron.ActFunDeriv));
            layers.Add(new Layer(1, neuronsInHiddenLay2, Neuron.ActLinearFun, Neuron.ActDerivLinearFun));
        }
        public double Act(double[] inputs)
        {
            double[] outputs = new double[0];
            for (int i = 0; i < layers.Count; i++)
            {
                outputs = layers[i].Act(inputs);
                inputs = outputs;
            }
            lastOutput = outputs[0];
            return outputs[0];
        }

        private double[][] GetLearnArray(IEmulatorDataProvider dataProvider, int K)
        {
            double[][] ret = new double[K][];
            double[] tmpInput = new double[dataProvider.nOfInputs];
            double outVal = 0;
            ret[0] = new double[dataProvider.nOfInputs];
            int idx=dataProvider.GetRandInputVector(ref ret[0], ref outVal, 10);
            idx++;
            for (int i = 1; i < K; i++)
            {
                ret[i] = new double[dataProvider.nOfInputs];
                dataProvider.GetInputVector(ref ret[i], ref outVal, idx + i);
            }
            return ret;
        }
        public bool Learn(IEmulatorDataProvider dataProvider, Emulator emulator, double alpha, double maxError, int maxIterations, String net_path = null, int iter_save = 1)
        {
            int it = maxIterations;
            double err=0;
            while (true)
            {
                double desiremois = 0.58;
                double[][] inputs;
                do
                {
                    inputs  = GetLearnArray(dataProvider, 14);//wybranie tablicy kolejnych K próbek
                } while (inputs[0][0] < desiremois + 0.05);
                err +=ApplyBackPropagation(inputs, emulator, 0.5, desiremois, 12);
                Console.WriteLine(err + " iterations: " + (it - maxIterations));
                err = 0;
                maxIterations--;
                if (maxIterations <= 0)
                {
                    Console.WriteLine("End of iterations");
                    return false;
                }

            }
        }
        double ComputeSigmas(double EmulatorErr)
        {
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < layers[i].nNeurons; j++)
                {
                    if (i == layers.Count - 1)
                    {
                        layers[i].neurons[j].SaveSigma(EmulatorErr);
                    }
                    else
                    {
                        double sum = 0;
                        for (int k = 0; k < layers[i + 1].nNeurons; k++)
                        {
                            sum += layers[i + 1].neurons[k].weights[j] * layers[i + 1].neurons[k].sigma;
                        }
                        layers[i].neurons[j].SaveSigma(sum);
                    }
                }
            }
            double ret_val=0;
            for (int k = 0; k < layers[0].nNeurons; k++)
            {
                ret_val += layers[0].neurons[k].weights[0] * layers[0].neurons[k].sigma;///Wartość błędu pochodząca z poprzedniego wyjscia emulatora
            }
            return ret_val;
        }
        void ComputeNewWeights(double alpha, double[] inputs)///wiekoszc tego mozna wykonac w neuronach
        {
            for (int i = 0; i < layers.Count; i++)
                for (int j = 0; j < layers[i].nNeurons; j++)
                    for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                    {
                        if (i == 0)
                            layers[i].neurons[j].deltas[k] += alpha * layers[i].neurons[j].sigma * layers[i].neurons[j].actderiv_(layers[i].neurons[j].LastOut) * inputs[k];
                        else
                            layers[i].neurons[j].deltas[k] += alpha * layers[i].neurons[j].sigma * layers[i].neurons[j].actderiv_(layers[i].neurons[j].LastOut) * layers[i - 1].neurons[k].LastOut;
                    }

        }
        void ApplyDeltas()
        {
            for (int i = 0; i < layers.Count; i++)
                for (int j = 0; j < layers[i].nNeurons; j++)
                    for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                        layers[i].neurons[j].weights[k] -= layers[i].neurons[j].deltas[k];
        }
        void ClearDeltas()
        {
            for (int i = 0; i < layers.Count; i++)
                for (int j = 0; j < layers[i].nNeurons; j++)
                    for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                        layers[i].neurons[j].deltas[k] = 0;
        }
        void UpdateBias(double alpha, double[] inputs)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].nNeurons; j++)
                {
                    if (i == 0)
                        layers[i].neurons[j].bias -= alpha * layers[i].neurons[j].sigma * layers[i].neurons[j].actderiv_(layers[i].neurons[j].LastOut);
                }
            }
        }
        double ApplyBackPropagation(double[][] inputs,Emulator emulator,double alpha,double desireMoiscure,int K)
        {
            double[][] regInputs = new double[K][];
            for (int i = 0; i <K ; i++)
            {
                for (int j = 1; j <= stepsBack&&i>0; j++)
                {
                    inputs[i][outIdx + j] = inputs[i-1 ][outIdx + j - 1];
                    inputs[i][moiscureIdx + j] = inputs[i-1][moiscureIdx + j - 1];
                }//Przepisanie danych z poprzedniej próbki do nowej próbki(siec uwzględnia dane z poprzednich stanów)
                regInputs[i] = new double[nOfInputs];
                for (int j = 0; j < nOfInputs; j++)
                {
                    regInputs[i][j] = inputs[i][moiscureIdx + j];///Wybranie danych wejsciowych dla regulatora W tym momencie tylko wilgotnosc ale w przyszłosci dojdą inne dane
                }
                inputs[i][outIdx] = Act(regInputs[i]);///Zadziałaj regulatorem
                inputs[i + 1][moiscureIdx] = emulator.Act(inputs[i]);//Westymuj stan obiektu z
            }
            ///Przejscie w tył
                double err = inputs[K][moiscureIdx] - desireMoiscure;
            double enderr = err;

            for (int i = 0; i < K; i++)
            {
                ClearDeltas();
                double errorsActs = emulator.ComputeSigmas(1, K);
                ComputeSigmas(err);
                Act(regInputs[K-i-1]);//Do uzyskania LastOutput kazdego neuronu odpowiedniego dla danego K kroku(bardzo nie optymalne, wiem xD)
                ComputeNewWeights(alpha, regInputs[K-i-1]);
                ApplyDeltas();
            }
            return enderr;
        }
        public void SaveWMatrix(String neuralNetworkPath)
        {
            FileStream fs = new FileStream(neuralNetworkPath, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, this.layers);
            fs.Close();
        }

        public void ReadWMatrix(String neuralNetworkPath)
        {
            FileStream fs = new FileStream(neuralNetworkPath, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            this.layers = (List<Layer>)formatter.Deserialize(fs);
            fs.Close();
        }




    }
}

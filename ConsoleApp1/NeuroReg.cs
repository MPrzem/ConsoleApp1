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
        double AverageError(IEmulatorDataProvider dataProvider)///to do zmiany
        {
            double err = 0;
            for (int i = 0; i < 100; i++)
            {
                double outActual = new double();
                double[] inputs = new double[6];
                dataProvider.GetRandInputVector(ref inputs, ref outActual);
                err += Math.Abs(Act(inputs) - outActual);
            }
            return err;
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
                double desiremois = 0.55;

                double[][] inputs;
                
                do
                {
                    inputs  = GetLearnArray(dataProvider, 14);
                } while (inputs[0][0] < desiremois + 0.05);

                err +=ApplyBackPropagation(inputs, emulator, 0.05, desiremois, 13);
                //double err = AverageError(dataProvider);
                //if (err < 0.01)
                ///   alpha = 0.15;
                if ((it - maxIterations) % 1000 == 0)
                {
                    Console.WriteLine(err + " iterations: " + (it - maxIterations));
                    err = 0;
                }
              /*  if (err < maxError)
                {
                    SaveWMatrix(net_path);
                    Console.WriteLine("Save net to " + net_path);
                    return true;
                }*/
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
                ret_val += layers[0].neurons[k].weights[moiscureIdx] * layers[0].neurons[k].sigma;
            }
            return ret_val;
        }
        void ComputeNewWeights(double alpha, double[] inputs)
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
        /*        void ComputeNewWeights(double alpha)
                {
                    for (int i = 1; i < layers.Count; i++)
                        for (int j = 0; j < layers[i].nNeurons; j++)
                            for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                                layers[i].neurons[j].weights[k] -= alpha * layers[i].neurons[j].sigma * layers[i - 1].neurons[k].LastOut;

                }*/
        double ApplyBackPropagation(double[][] inputs,Emulator emulator,double alpha,double desireMoiscure,int K)
        {
            double[][] regInputs = new double[K][];
            for (int i = 0; i <K ; i++)
            {
                for (int j = 1; j <= stepsBack&&i>0; j++)
                {
                    inputs[i][outIdx + j] = inputs[i-1 ][outIdx + j - 1];
                    inputs[i][moiscureIdx + j] = inputs[i-1][moiscureIdx + j - 1];
                }
                regInputs[i] = new double[nOfInputs];
                regInputs[i][0] = inputs[i][outIdx+1];
                for (int j = 1; j < nOfInputs; j++)
                {
                    regInputs[i][j] = inputs[i][moiscureIdx - 1 + j];
                }
                inputs[i][outIdx] = Act(regInputs[i]);///Zadziałaj regulatorem
                inputs[i + 1][moiscureIdx] = emulator.Act(inputs[i]);//Westymuj stan obiektu z emulatora
            }
            
                double err = inputs[K][moiscureIdx] - desireMoiscure;
                double[] errorsActs=emulator.ComputeSigmas(err,K);
            for (int i = 0; i < K-1; i++)
            {
                Act(regInputs[K - 1 - i]);
                ComputeSigmas(errorsActs[i]);
                ClearDeltas();
                ComputeNewWeights(alpha, regInputs[K-1-i]);
                ApplyDeltas();

            }
            return err;
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

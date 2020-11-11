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
        public NeuroReg(int nOfInputs,int neuronsInHiddenLay1, int neuronsInHiddenLay2, int outIdx_)
        {
            outIdx = outIdx_;
            layers = new List<Layer>();
            layers.Add(new Layer(nOfInputs, nOfInputs));
            layers.Add(new Layer(neuronsInHiddenLay1, nOfInputs));
            layers.Add(new Layer(neuronsInHiddenLay2, neuronsInHiddenLay1));
            layers.Add(new Layer(1, neuronsInHiddenLay2));
        }
        public double Act(double[] inputs)
        {
            double[] outputs = new double[0];
            for (int i = 1; i < layers.Count; i++)
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
            int idx=dataProvider.GetRandInputVector(ref ret[0], ref outVal, K);
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
            while (true)
            {
                
                var inputs = GetLearnArray(dataProvider, 10);
                ApplyBackPropagation(inputs,emulator,alpha,10,0,inputs[0][emulator.moiscureIdx]-0.04);
                double err = AverageError(dataProvider);
                if (err < 0.01)
                    alpha = 0.15;
                if ((it - maxIterations) % 1000 == 0)
                {
                    Console.WriteLine(err + " iterations: " + (it - maxIterations));
                }
                if (err < maxError)
                {
                    SaveWMatrix(net_path);
                    Console.WriteLine("Save net to " + net_path);
                    return true;
                }
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
                        layers[i].neurons[j].CalculateSigma(EmulatorErr);
                    }
                    else
                    {
                        double sum = 0;
                        for (int k = 0; k < layers[i + 1].nNeurons; k++)
                        {
                            sum += layers[i + 1].neurons[k].weights[j] * layers[i + 1].neurons[k].sigma;
                        }
                        layers[i].neurons[j].CalculateSigma(sum);
                    }
                }
            }
            double ret_val=0;
            for (int k = 0; k < layers[0].nNeurons; k++)
            {
                ret_val += layers[0].neurons[k].weights[outIdx] * layers[0].neurons[k].sigma;
            }
            return ret_val;
        }
        void ComputeNewWeights(double alpha)
        {
            for (int i = 1; i < layers.Count; i++)
                for (int j = 0; j < layers[i].nNeurons; j++)
                    for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                        layers[i].neurons[j].weights[k] -= alpha * layers[i].neurons[j].sigma * Neuron.ActFun(layers[i - 1].neurons[k].ActivationSum);

        }
        double ApplyBackPropagation(double[][] inputs,Emulator emulator,double alpha,int K,int Idx,double desireMoiscure)
        {


            inputs[Idx][outIdx] = Act(inputs[Idx]);///Zadziałaj regulatorem
            inputs[++Idx][emulator.moiscureIdx]=emulator.Act(inputs[Idx]);//Westymuj stan obiektu z emulatora
            K--;
            // input[]emulator.Act(input);
            //ApplyBackPropagation(estymowany stan obiektu,
            if (K <= 0)
            {
                var err = ComputeSigmas(emulator.ComputeSigmas(desireMoiscure));
                ComputeNewWeights(alpha);
                return err;

            }
            else
            {
                var err = ComputeSigmas(emulator.ComputeSigmas(ApplyBackPropagation(inputs,emulator,alpha,K,Idx,desireMoiscure)));
                ComputeNewWeights(alpha);
                return err;

            }
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

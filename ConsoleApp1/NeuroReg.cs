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
        const int moiscureIdx = 3;
        public NeuroReg(int neuronsInHiddenLay1, int neuronsInHiddenLay2)
        {
            layers = new List<Layer>();
            layers.Add(new Layer(1, 1));
            layers.Add(new Layer(neuronsInHiddenLay1, 1));
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
        double AverageError(IEmulatorDataProvider dataProvider)
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
        public bool Learn(IEmulatorDataProvider dataProvider, double alpha, double maxError, int maxIterations, String net_path = null, int iter_save = 1)
        {
            int it = maxIterations;
            while (true)
            {
                double[] inputs = new double[6];
                double output = 0;
                dataProvider.GetRandInputVector(ref inputs, ref output);
                ApplyBackPropagation(inputs, output, alpha);
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
        double ComputeSigmas(double desiredOutput)
        {
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < layers[i].nNeurons; j++)
                {
                    if (i == layers.Count - 1)
                    {
                        layers[i].neurons[j].CalculateSigma(lastOutput - desiredOutput);
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
            double ret_val = 0;
            for (int k = 0; k < layers[0].nNeurons; k++)
            {
                ret_val += layers[0].neurons[k].weights[0] * layers[0].neurons[k].sigma;
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
        void ApplyBackPropagation(double[] inputs, double outActual, double alpha)
        {
            Act(inputs);
            ComputeSigmas(outActual);
            ComputeNewWeights(alpha);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConsoleApp1
{
    class Emulator
    {
        List<Layer> layers;
        double lastOutput;
        public Emulator(int inputs, int neuronsInHiddenLay1, int neuronsInHiddenLay2)
        {
            layers = new List<Layer>();
            layers.Add(new Layer(inputs, inputs));
            layers.Add(new Layer(neuronsInHiddenLay1, inputs));
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
                double[] inputs=new double[6];
                double output=0;
                dataProvider.GetRandInputVector(ref inputs, ref output);
                ApplyBackPropagation(inputs,output, alpha);
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
        void ComputeSigmas(double desiredOutput)
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
        }
        void ComputeNewWeights(double alpha)
        {
            for (int i = 1; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].nNeurons; j++)
                {
                    for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                    {
                        layers[i].neurons[j].weights[k] -= alpha*layers[i].neurons[j].sigma * Neuron.ActFun(layers[i - 1].neurons[k].ActivationSum);
                    }
                }
            }
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

    [Serializable]
    class Layer
    {
        public List<Neuron> neurons;
        public int nNeurons;
        public double[] output;

        public Layer(int n, int numberOfInputs)
        {
            nNeurons = n;
            neurons = new List<Neuron>();
            for (int i = 0; i < nNeurons; i++)
            {
                neurons.Add(new Neuron(numberOfInputs));
            }
        }

        public double[] Act(double[] inputs)
        {
            List<double> outputs = new List<double>();
            for (int i = 0; i < nNeurons; i++)
            {
                outputs.Add(neurons[i].Act(inputs));
            }
            output = outputs.ToArray();
            return outputs.ToArray();
        }

    }

    [Serializable]
    class Neuron
    {
        public double[] weights;
        public double ActivationSum;
        public double sigma=0;
        public Neuron(int numberOfInputs)
        {
            Random rand = new Random();
            weights = new double[numberOfInputs];
            for (int i = 0; i < numberOfInputs; i++)
            {
                weights[i] = 10 * rand.NextDouble() - 5;// bias;
            }
        }
        public void CalculateSigma(double error)        {
            sigma = (error) * Neuron.ActFunDeriv(ActivationSum);
        }
        public double Act(double[] inputs)
        {
            double activation = 0; // bias;

            for (int i = 0; i < weights.Length; i++)
            {
                activation += weights[i] * inputs[i];
            }
            ActivationSum = activation;
            activation = ActFun(activation);
            return activation;
        }
        public static double ActFun(double input)
        {
            return 1 / (1 + Math.Exp(-input));
        }
        public static double ActFunDeriv(double input)
        {
            double y = ActFun(input);
            return y * (1 - y);
        }

    }
}

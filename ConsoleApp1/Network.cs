using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
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
        public double sigma = 0;
        public Neuron(int numberOfInputs)
        {
            Random rand = new Random();
            weights = new double[numberOfInputs];
            for (int i = 0; i < numberOfInputs; i++)
            {
                weights[i] = 10 * rand.NextDouble() - 5;// bias;
            }
        }
        public void CalculateSigma(double error)
        {
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

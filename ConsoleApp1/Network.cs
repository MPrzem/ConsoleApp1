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

        public Layer(int n, int numberOfInputs, Func<double, double> activate, Func<double, double> actderiv)
        {
            nNeurons = n;
            neurons = new List<Neuron>();
            for (int i = 0; i < nNeurons; i++)
            {
                neurons.Add(new Neuron(numberOfInputs,activate,actderiv));
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
        public double[] deltas;
        public double LastOut;
        public double LastIn;
        public double sigma = 0;
        public double bias;
        public Func<double, double> activate_;
        public Func<double, double> actderiv_;

        public Neuron(int numberOfInputs,Func<double,double> activate,Func<double,double> actderiv)
        {
            activate_ = activate;
            actderiv_ = actderiv;
            Random rand = new Random();
            bias = 2 * rand.NextDouble() -1 ;
            weights = new double[numberOfInputs];
            deltas = new double[numberOfInputs];
            for (int i = 0; i < numberOfInputs; i++)
            {
                weights[i] = 2 * rand.NextDouble() - 1;// bias;
            }
        }
        public void SaveSigma(double error)
        {
            sigma = error;
        }
        public double Act(double[] inputs)
        {
            double activation = bias;

            for (int i = 0; i < weights.Length; i++)
            {
                activation += weights[i] * inputs[i];
            }
            activation = activate_(activation);
            LastOut = activation;
            return activation;
        }
        public static double ActFun(double input)
        {
            return 1 / (1 + Math.Exp(-0.5*input));
        }
        public static double ActFunDeriv(double y)
        {
            return 0.5*y * (1 - y);
        }
        public static double ActLinearFun(double input)
        {
            return 1 / (1 + Math.Exp(-0.5*input));
        }
        public static double ActDerivLinearFun(double y)
        {
            return 0.5*y * (1 - y);
        }

    }
}

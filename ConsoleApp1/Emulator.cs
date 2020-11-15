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
    public class Emulator
    {
        List<Layer> layers;
        double lastOutput;
        public int outIdx;
        public Emulator(int inputs, int neuronsInHiddenLay1, int neuronsInHiddenLay2, int outIdx_)
        {
            outIdx = outIdx_;
            layers = new List<Layer>();
            layers.Add(new Layer(inputs, inputs,Neuron.ActFun,Neuron.ActFunDeriv));
            layers.Add(new Layer(neuronsInHiddenLay1, inputs, Neuron.ActFun, Neuron.ActFunDeriv));
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
        double AverageError(IEmulatorDataProvider dataProvider)
        {
            double outActual = new double();
            double[] inputs = new double[dataProvider.nOfInputs];
            double err = 0;
            while (dataProvider.GetInputVectorOneByOne(ref inputs, ref outActual))
            {
                err += Math.Abs(Act(inputs) - outActual);
            }
            return err;
        }
        public bool Learn(IEmulatorDataProvider dataProvider, double alpha, double maxError, int maxIterations, String net_path = null, int iter_save = 1)
        {
            int it = maxIterations;
            while (true)
            {
                double[] inputs=new double[dataProvider.nOfInputs];
                double output=0;
                //  dataProvider.GetRandInputVector(ref inputs, ref output);
                while (dataProvider.GetInputVectorOneByOne(ref inputs, ref output)) {
                   ClearDeltas();
                    ApplyBackPropagation(inputs, output, alpha);
                    ApplyDeltas();
                }
                 double err = AverageError(dataProvider);
                Console.WriteLine("Err: "+err + " iterations: " + (it - maxIterations));
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
        public double ComputeSigmas(double ErrOfLastLayer,int K)
        {
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < layers[i].nNeurons; j++)
                {
                    if (i == layers.Count - 1)
                    {
                        layers[i].neurons[j].SaveSigma(ErrOfLastLayer);
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
            double errorForControler=0;
                for (int k = 0; k < layers[0].nNeurons; k++)
                {
                    errorForControler= layers[0].neurons[k].weights[outIdx] * layers[0].neurons[k].sigma;
                }
            
            return errorForControler;
        }
        public void ComputeSigmas(double ErrOfLastLayer)
        {
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < layers[i].nNeurons; j++)
                {
                    if (i == layers.Count - 1)
                    {
                        layers[i].neurons[j].SaveSigma(ErrOfLastLayer);
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
        }
        void ComputeNewWeights(double alpha,double[] inputs)
        {
            for (int i = 0; i < layers.Count; i++)
                for (int j = 0; j < layers[i].nNeurons; j++)
                    for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                    {
                        if(i==0)
                            layers[i].neurons[j].deltas[k] += alpha * layers[i].neurons[j].sigma* layers[i].neurons[j].actderiv_(layers[i].neurons[j].LastOut) * inputs[k];
                        else 
                            layers[i].neurons[j].deltas[k] += alpha * layers[i].neurons[j].sigma * layers[i].neurons[j].actderiv_(layers[i].neurons[j].LastOut)* layers[i - 1].neurons[k].LastOut;
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
        void ApplyBackPropagation(double[] inputs, double outActual, double alpha)
        { 
                Act(inputs);
                ComputeSigmas(lastOutput-outActual);
                UpdateBias(alpha,inputs);
                ComputeNewWeights(alpha,inputs);
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

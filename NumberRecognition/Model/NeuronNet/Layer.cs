using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;

namespace NumberRecognition.Model.NeuronNet
{
    class Layer
    {
        [JsonProperty("neurons")] public List<Neuron> Neurons;

        [JsonProperty("count")] public int Count { get; set; }

        public Neuron this[int index]
        {
            get => Neurons[index];
            set => Neurons[index] = value;
        }

        public Layer()
        {
            Neurons=new List<Neuron>();
        }

        public Layer(int count)
        {
            this.Count = count;
            Neurons=new List<Neuron>();
            for (int i = 0; i < count; i++)
            {
                Neurons.Add(new Neuron());
            }
        }

        public Matrix<double> ToMatrix()
        {
            return Matrix<double>.Build.Dense(Count, 1, ((i, j) => Neurons[i].Active));
        }

        public void Load(Matrix<double> matrix)
        {
            for (int i = 0; i < matrix.RowCount; i++)
            {
                Neurons[i]=new Neuron(matrix[i,0]);
            }
        }
    }
}

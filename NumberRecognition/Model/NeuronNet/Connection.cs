using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;

namespace NumberRecognition.Model.NeuronNet
{
    class Connection
    {
        [JsonProperty("weightMatrix")]
        public double[][] JsonWeightMatrix
        {
            get => WeightMatrix.ToRowArrays();
            set=> WeightMatrix=Matrix<double>.Build.Dense(value.Length, value[0].Length, (i, j) => value[i][j]);
        }

        [JsonProperty("biasesMatrix")]
        public double[] JsonBiasesMatrix
        {
            get => BiasesMatrix.ToRowMajorArray();
            set => BiasesMatrix = Matrix<double>.Build.Dense(value.Length,1,(i,j)=>value[i]);
        }

        public Matrix<double> WeightMatrix
        {
            get => weightMatrix;
            set => weightMatrix = value;
        }

        public Matrix<double> BiasesMatrix
        {
            get => biasesMatrix;
            set => biasesMatrix = value;
        }

        private Matrix<double> weightMatrix;
        private Matrix<double> biasesMatrix;

        public Connection()
        {
            WeightMatrix = Matrix<double>.Build.Random(1, 1);
            BiasesMatrix = Matrix<double>.Build.Random(1,1);
        }

        public Connection(Matrix<double> weightMatrix,Matrix<double> biasesMatrix)
        {
            this.WeightMatrix = weightMatrix;
            this.BiasesMatrix = biasesMatrix;
        }

        public Connection(int rows, int columns)
        {
            WeightMatrix = Matrix<double>.Build.Random(rows, columns);
            BiasesMatrix = Matrix<double>.Build.Random(rows,1);
        }

        public double this[int i,int j]
        {
            get=> WeightMatrix[i, j];
            set => WeightMatrix[i, j] = value;
        }

        public double this[int index]
        {
            get => BiasesMatrix[index,1];
            set => BiasesMatrix[index,1] = value;
        }

    }
}

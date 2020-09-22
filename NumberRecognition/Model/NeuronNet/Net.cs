using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MathNet.Numerics.LinearAlgebra;
using NumberRecognition.Model.Data;

namespace NumberRecognition.Model.NeuronNet
{
    class Net
    {
        [JsonProperty("layers")] public List<Layer> Layers;
        [JsonProperty("lCount")]public int LCount { get; set; }
        [JsonProperty("connection")]public List<Connection> Connections { get; set; }
        [JsonProperty("nCount")]public int NCount { get; set; }
        [JsonProperty("sourceNum")]public int SourceNum { get; set; }
        [JsonProperty("resultNum")]public int ResultNum { get; set; }
        private int ans;
        private List<Matrix<double>[]> weightMatrixList;
        private List<Matrix<double>[]> biasesMatrixList;

        public Net()
        {
            weightMatrixList = new List<Matrix<double>[]>();
            biasesMatrixList = new List<Matrix<double>[]>();
            Layers =new List<Layer>();
        }
        
        /// <summary>
        /// 创建一片神经网络
        /// </summary>
        /// <param name="lCount">神经节点层数量</param>
        /// <param name="nCount">每层神经节点的数量</param>
        /// <param name="sourceNum">源神经节点数量</param>
        /// <param name="resultNum">目标神经节点数量</param>
        public Net(int lCount,int nCount,int sourceNum,int resultNum)
        {
            weightMatrixList=new List<Matrix<double>[]>();
            biasesMatrixList=new List<Matrix<double>[]>();
            Layers = new List<Layer>();
            Connections=new List<Connection>();
            LCount = lCount;
            NCount = nCount;
            SourceNum = sourceNum;
            ResultNum = resultNum;
            BuildLayer();
            BuildConnection();
        }

        private void BuildLayer()
        {
            Layers.Add(new Layer(SourceNum));
            for (int i = 0; i < LCount; i++)
            {
                Layers.Add(new Layer(NCount));
            }
            Layers.Add(new Layer(ResultNum));
        }

        private void BuildConnection()
        {
            Connections.Add(new Connection(NCount,SourceNum));
            for (int i = 0; i < LCount-1; i++)
            {
                Connections.Add(new Connection(NCount,NCount));
            }
            Connections.Add(new Connection(ResultNum,NCount));
        }

        /// <summary>
        /// 载入首层资源
        /// </summary>
        /// <param name="pImage"></param>
        public void LoadSource(PixelImage pImage,int ans)
        {
            Layers[0].Load(Matrix<double>.Build.Dense(SourceNum,1,(i, j) => Convert.ToDouble(pImage.Data[i]) / 255));
            this.ans = ans;
        }

        /// <summary>
        /// 开始正推网络，在此之前必须先载入首层资源
        /// </summary>
        public bool BeginReason()
        {
            for (int i = 0; i < LCount + 1; i++)
            {
                Layers[i+1].Load(PushOneLayer(Layers[i],Connections[i]));
            }

            return CheckAnswer();
        }

        private bool CheckAnswer()
        {
            double max = Layers[Layers.Count - 1][0].Active;
            int maxIndex = 0;
            for (int i = 1; i < Layers[Layers.Count - 1].Count; i++)
            {
                if (max < Layers[Layers.Count - 1][i].Active)
                {
                    max = Layers[Layers.Count - 1][i].Active;
                    maxIndex = i;
                }
            }

            return maxIndex == ans;
        }

        public double Evaluation()
        {
            double res = 0;
            for (int i = 0; i < ResultNum; i++)
            {
                if (i == ans)
                {
                    res+=Math.Pow(Layers[LCount + 1][i].Active, 2);
                    continue;
                }
                res+=Math.Pow(1 - Layers[LCount + 1][i].Active, 2);
            }

            return res;
        }

        private Matrix<double> PushOneLayer(Layer layer, Connection conn)
        {
            var res = conn.WeightMatrix.Multiply(layer.ToMatrix()) + conn.BiasesMatrix;
            return SigmoidMatrix(res);
        }

        private static Matrix<double> SigmoidMatrix(Matrix<double> matrix)
        {
            for (int i = 0; i < matrix.RowCount; i++)
            {
                matrix[i, 0] = Sigmoid(matrix[i, 0]);
            }

            return matrix;
        }

        private static double Sigmoid(double num)
        {
            return 1.0 / (1.0 + Math.Pow(Math.E, -num));
        }

        private static double DerivativeOfSigmoid(double num)
        {
            return Math.Pow(Math.E, -num) / Math.Pow(1 + Math.Pow(Math.E, -num), 2);
        }

        private List<double> CalculateResultLoss()
        {
            List<double> lossList=new List<double>();
            for (int i = 0; i < ResultNum; i++)
            {
                if (i == ans)
                {
                    lossList.Add(2*Layers[LCount + 1][i].Active);
                    continue;
                }
                lossList.Add(2*(Layers[LCount + 1][i].Active - 1 ));
            }

            return lossList;
        }

        private List<double> GetMidLoss(int pointer)
        {
            List<double> lossList=new List<double>();
            for (int i = 0; i < Layers[pointer].Count; i++)
            {
                lossList.Add(Layers[pointer][i].Active);
            }

            return lossList;
        }

        private double CalculateZ(int pointer,int targetIndex)
        {
            double count = 0;
            for (int i = 0; i < Layers[pointer - 1].Count; i++)
            {
                count += Layers[pointer - 1][i].Active * Connections[pointer - 1].WeightMatrix[targetIndex, i];
            }

            return count + Connections[pointer - 1].BiasesMatrix[targetIndex, 0];
        }

        private void CalculateNeuron(int pointer,List<double> lossList)
        {
            for (int i = 0; i < Layers[pointer-1].Count; i++)
            {
                double count = 0;
                for (int j = 0; j < Layers[pointer].Count; j++)
                {
                    count+=Connections[pointer - 1].WeightMatrix[j, i] * DerivativeOfSigmoid(CalculateZ(pointer, j)) * lossList[j];
                }

                Layers[pointer - 1][i].Active = count;
            }
        }

        private Matrix<double> CalculateWeight(int pointer,List<double> lossList)
        {
            var weightMatrix=Matrix<double>.Build.DenseOfMatrix(Connections[pointer - 1].WeightMatrix);
            for (int i = 0; i < Layers[pointer - 1].Count; i++)
            {
                for (int j = 0; j < Layers[pointer].Count; j++)
                {
                    weightMatrix[j,i]=Layers[pointer-1][i].Active * DerivativeOfSigmoid(CalculateZ(pointer, j)) * lossList[j];
                }
            }

            return weightMatrix;
        }

        private Matrix<double> CalculateBiases(int pointer, List<double> lossList)
        {
            var biasesMatrix = Matrix<double>.Build.DenseOfMatrix(Connections[pointer - 1].BiasesMatrix);
            for (int i = 0; i < Layers[pointer].Count; i++)
            {
                biasesMatrix[i,0]=DerivativeOfSigmoid(CalculateZ(pointer, i)) * lossList[i];
            }

            return biasesMatrix; 
        }

        public void Recall()
        {
            Matrix<double>[] weightMatrices = new Matrix<double>[LCount + 1];
            Matrix<double>[] biasesMatrices = new Matrix<double>[LCount + 1];
            CalculateNeuron(LCount + 1, CalculateResultLoss());
            weightMatrices[LCount]=CalculateWeight(LCount + 1, CalculateResultLoss());
            biasesMatrices[LCount]=CalculateBiases(LCount + 1, CalculateResultLoss());
            for (int i = LCount; i > 0; i--)
            {
                CalculateNeuron(i,GetMidLoss(i));
                weightMatrices[i-1]=CalculateWeight(i,GetMidLoss(i));
                biasesMatrices[i-1]=CalculateBiases(i,GetMidLoss(i));
            }
            weightMatrixList.Add(weightMatrices);
            biasesMatrixList.Add(biasesMatrices);
        }

        public void Update()
        {
            var wm = AverageMatrix(weightMatrixList);
            var bm = AverageMatrix(biasesMatrixList);
            for (int i = 0; i < wm.Count; i++)
            {
                Connections[i].WeightMatrix = Connections[i].WeightMatrix - wm[i];
                Connections[i].BiasesMatrix = Connections[i].BiasesMatrix - bm[i];
            }
        }

        private List<Matrix<double>> AverageMatrix(List<Matrix<double>[]> matrixList)
        {
            List<Matrix<double>> resList=new List<Matrix<double>>();
            for (int i = 0; i < LCount + 1; i++)
            {
                for (int j = 1; j < matrixList.Count; j++)
                {
                    matrixList[0][i] += matrixList[j][i];
                }

                resList.Add(matrixList[0][i] / matrixList.Count);
            }

            return resList;
        }
    }
}

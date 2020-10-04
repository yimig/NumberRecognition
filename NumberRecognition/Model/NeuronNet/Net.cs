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

        /// <summary>
        /// 为json数据载入预留，勿调用
        /// </summary>
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
        public bool BeginReason(out int say)
        {
            for (int i = 0; i < LCount + 1; i++)
            {
                Layers[i+1].Load(PushOneLayer(Layers[i],Connections[i]));
            }
            return CheckAnswer(out say);
        }

        /// <summary>
        /// 返回神经网络末端输出层的最高激发值节点索引
        /// </summary>
        /// <returns></returns>
        private bool CheckAnswer(out int say)
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

            say = maxIndex;
            return maxIndex == ans;
        }

        /// <summary>
        /// 得到目前网络输出层的cost
        /// </summary>
        /// <returns></returns>
        public double Evaluation()
        {
            double res = 0;
            for (int i = 0; i < ResultNum; i++)
            {
                if (i == ans)
                {
                    res+=Math.Pow(1 - Layers[LCount + 1][i].Active, 2);
                    continue;
                }
                res+=Math.Pow(Layers[LCount + 1][i].Active, 2);
            }

            return res;
        }

        /// <summary>
        /// 向某层往后推理一次
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        private Matrix<double> PushOneLayer(Layer layer, Connection conn)
        {
            var res = conn.WeightMatrix.Multiply(layer.ToMatrix()) + conn.BiasesMatrix;
            return SigmoidMatrix(res);
        }

        /// <summary>
        /// 对一个矩阵进行sigmoid映射
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        private static Matrix<double> SigmoidMatrix(Matrix<double> matrix)
        {
            for (int i = 0; i < matrix.RowCount; i++)
            {
                matrix[i, 0] = Sigmoid(matrix[i, 0]);
            }

            return matrix;
        }

        /// <summary>
        /// 使用sigmoid函数将某个值映射到0-1范围内
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static double Sigmoid(double num)
        {
            return 1.0 / (1.0 + Math.Pow(Math.E, -num));
        }

        /// <summary>
        /// sigmoid函数的导函数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static double DerivativeOfSigmoid(double num)
        {
            return Math.Pow(Math.E, -num) / Math.Pow(1 + Math.Pow(Math.E, -num), 2);
        }

        /// <summary>
        /// 求得最后一层的期望差
        /// </summary>
        /// <returns></returns>
        private List<double> CalculateResultLoss()
        {
            List<double> lossList=new List<double>();
            for (int i = 0; i < ResultNum; i++)
            {
                if (i == ans)
                {
                    lossList.Add(2*(Layers[LCount + 1][i].Active-1));
                    continue;
                }
                lossList.Add(2*(Layers[LCount + 1][i].Active));
            }

            return lossList;
        }

        /// <summary>
        /// 求得除第一层和最后一层的期望差
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        private List<double> GetMidLoss(int pointer)
        {
            List<double> lossList=new List<double>();
            for (int i = 0; i < Layers[pointer].Count; i++)
            {
                lossList.Add(Layers[pointer][i].Active);
            }

            return lossList;
        }

        /// <summary>
        /// 计算反射算法中的通式z,(z=aw+b)
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        private double CalculateZ(int pointer,int targetIndex)
        {
            double count = 0;
            for (int i = 0; i < Layers[pointer - 1].Count; i++)
            {
                count += Layers[pointer - 1][i].Active * Connections[pointer - 1].WeightMatrix[targetIndex, i];
            }

            return count + Connections[pointer - 1].BiasesMatrix[targetIndex, 0];
        }

        /// <summary>
        /// 计算并填入某层经过反射算法后应得到的激发值
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="lossList"></param>
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

        /// <summary>
        /// 计算某层与下一层之间所有修正过的连接的权重
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="lossList"></param>
        /// <returns></returns>
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

        /// <summary>
        ///  计算某层与下一层之间所有修正过的连接的偏移量
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="lossList"></param>
        /// <returns></returns>
        private Matrix<double> CalculateBiases(int pointer, List<double> lossList)
        {
            var biasesMatrix = Matrix<double>.Build.DenseOfMatrix(Connections[pointer - 1].BiasesMatrix);
            for (int i = 0; i < Layers[pointer].Count; i++)
            {
                biasesMatrix[i,0]=DerivativeOfSigmoid(CalculateZ(pointer, i)) * lossList[i];
            }

            return biasesMatrix; 
        }

        /// <summary>
        /// 对整个神经网络进行一次回溯（反射算法）
        /// </summary>
        /// <returns></returns>
        public List<double> Recall()
        {
            Matrix<double>[] weightMatrices = new Matrix<double>[LCount + 1];
            Matrix<double>[] biasesMatrices = new Matrix<double>[LCount + 1];
            List<double> loss = CalculateResultLoss();
            CalculateNeuron(LCount + 1, loss);
            weightMatrices[LCount]=CalculateWeight(LCount + 1, loss);
            biasesMatrices[LCount]=CalculateBiases(LCount + 1, loss);
            for (int i = LCount; i > 0; i--)
            {
                if(i!=1)CalculateNeuron(i,GetMidLoss(i));
                weightMatrices[i-1]=CalculateWeight(i,GetMidLoss(i));
                biasesMatrices[i-1]=CalculateBiases(i,GetMidLoss(i));
            }
            weightMatrixList.Add(weightMatrices);
            biasesMatrixList.Add(biasesMatrices);
            return loss;
        }

        /// <summary>
        /// 对上次更新值之后所有的回溯结果进行整合并更新
        /// </summary>
        /// <param name="multiple">更新倍率</param>
        public void Update(double multiple)
        {
            var wm = AverageMatrix(weightMatrixList);
            var bm = AverageMatrix(biasesMatrixList);
            for (int i = 0; i < wm.Count; i++)
            {
                Connections[i].WeightMatrix = Connections[i].WeightMatrix - multiple*wm[i];
                Connections[i].BiasesMatrix = Connections[i].BiasesMatrix - multiple*bm[i];
            }
            weightMatrixList=new List<Matrix<double>[]>();
            biasesMatrixList=new List<Matrix<double>[]>();
        }

        /// <summary>
        /// 求多个矩阵的平均矩阵
        /// </summary>
        /// <param name="matrixList"></param>
        /// <returns></returns>
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using NumberRecognition.Model;
using NumberRecognition.Model.Data;
using NumberRecognition.Model.NeuronNet;
using NumberRecognition.Util.IO;

namespace NumberRecognition
{
    class Program
    {

        private const int MINIBATCH_SIZE = 1;
        private static FormChanger formChanger;
        static void Main(string[] args)
        {
            BuildNewNet();
            var form = new ImageForm();
            formChanger = new FormChanger(form);
            (new Thread(formChanger.RunForm)).Start();
            BeginLearning(0);
            //RunCmd2("shutdown", "-p");
            Console.ReadLine();
        }

        private static void BeginLearning(int startPos)
        {
            var imageBatch = new ImageBatch(DataReader.ReadTrainImage());
            var labelBatch = new LabelBatch(DataReader.ReadTrainLabel());
            var net = ResultWriter.ReadResult();
            //net.InitMomentumLists();
            for (int x = startPos/MINIBATCH_SIZE; x < (60000/MINIBATCH_SIZE); x++)
            {
                ResultWriter.WriteLog("start:" + x * MINIBATCH_SIZE + " to " + (x * MINIBATCH_SIZE + MINIBATCH_SIZE) + "\n");
                for (; ; )
                {
                    var averageCost=Learn(imageBatch, labelBatch, x, net);//学习minibatch的一份
                    if (averageCost < 0.01)
                    {
                        ResultWriter.WriteLog("cost:" + averageCost + "\n");
                        break;
                    }
                }
            }
        }

        private static double Learn(ImageBatch imb,LabelBatch lab,int x,Net net)
        {
            int correctNum = 0;
            double costSum = 0;
            for (int i = x * MINIBATCH_SIZE; i < x * MINIBATCH_SIZE + MINIBATCH_SIZE; i++)
            {
                formChanger.ChangeImage(imb[i].GetBitmap(),lab[i]+"     ("+(i+1)+"/"+imb.Count()+")"); 
                net.LoadSource(imb[i], lab[i]);//载入样本
                var isCorrect=net.BeginReason(out var say);//正推
                var cost=net.Evaluation();//计算cost
                costSum += cost;
                formChanger.AddNeuronNote(net, lab[i], say, cost);
                net.Recall();//反向传播
                if (isCorrect) correctNum++;
                PrintInForm(isCorrect,say,cost);
            }

            var averageCost = costSum / MINIBATCH_SIZE;
            formChanger.AddCost(averageCost, Convert.ToDouble(correctNum) / MINIBATCH_SIZE);
            net.Update(0.1, Net.UpdateOptimizer.SGD);
            ResultWriter.WriteResult(net);
            PrintInConsole(averageCost,correctNum);
            return averageCost;
        }

        private static void PrintInForm(bool isCorrect,int say,double cost)
        {
            formChanger.SetInfo("Ans:" + (isCorrect ? "Correct" : "Wrong") + "!    Say: " + say);
            formChanger.SetInfo("Cost:" + cost);
        }

        private static void PrintInConsole(double averageCost,double correctNum)
        {
            Console.WriteLine("Average Cost:" + averageCost);
            Console.WriteLine("Accuracy:" + Convert.ToDouble(correctNum) / MINIBATCH_SIZE);
            Console.WriteLine("=================================");
        }

        private static void BuildNewNet()
        {
            Net net =new Net(2,16,28*28,10);
            ResultWriter.WriteResult(net);
        }

        /// <summary>
        /// 运行cmd命令
        /// 不显示命令窗口
        /// </summary>
        /// <param name="cmdExe">指定应用程序的完整路径</param>
        /// <param name="cmdStr">执行命令行参数</param>
        static bool RunCmd2(string cmdExe, string cmdStr)
        {
            bool result = false;
            try
            {
                using (Process myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;
                    myPro.StartInfo.RedirectStandardInput = true;
                    myPro.StartInfo.RedirectStandardOutput = true;
                    myPro.StartInfo.RedirectStandardError = true;
                    myPro.StartInfo.CreateNoWindow = true;
                    myPro.Start();
                    //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                    string str = string.Format(@"""{0}"" {1} {2}", cmdExe, cmdStr, "&exit");

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;
                    myPro.WaitForExit();

                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }


        class FormChanger
        {
            private ImageForm form;
            public FormChanger(ImageForm form)
            {
                this.form = form;
            }


            public void RunForm()
            {
                Application.Run(form);
            }

            public void ChangeImage(Bitmap pic, string title)
            {
                form.SetNumber(pic, title);
            }

            public void SetInfo(string info)
            {
                form.SetInfo(info);
            }

            public void AddCost(double cost,double accuracy)
            {
                form.AddCost(cost, accuracy);
            }

            public void AddNeuronNote(Net net, int testNum, int sayNum, double cost)
            {
                form.AddNeuronNote(net,testNum,sayNum,cost);
            }
        }
    }
}

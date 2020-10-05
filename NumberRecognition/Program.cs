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

        private const int STEP = 2;
        private static FormChanger formChanger;
        static void Main(string[] args)
        {
            BuildNewNet();
            var form = new ImageForm();
            formChanger = new FormChanger(form);
            (new Thread(formChanger.RunForm)).Start();
            //LearnOnce(0);
            BeginLearning(0);
            //RunCmd2("shutdown", "-p");
            Console.ReadLine();
        }

        private static void LearnOnce(int pos)
        {
            var net = ResultWriter.ReadResult();
            var imb = new ImageBatch(DataReader.ReadTrainImage());
            var lab = new LabelBatch(DataReader.ReadTrainLabel());
            net.LoadSource(imb[pos], lab[pos]);
            formChanger.ChangeImage(imb[pos].GetBitmap(),lab[pos]+"");
            for (;;)
            {
                int say;
                var res = net.BeginReason(out say);
                Console.WriteLine(net.Evaluation());
                var loss=net.Recall();
                Console.WriteLine("5-Active:"+net.Layers[3][5].Active);
                // Console.WriteLine("1-loss:" + loss[1]);
                // Console.WriteLine("5-Active:" + net.Layers[3][5].Active);
                // Console.WriteLine("5-loss:" + loss[5]);
                net.Update(1);
            }
        }

        private static void BeginLearning(int startPos)
        {
            var ImageBatch = new ImageBatch(DataReader.ReadTrainImage());
            var LabelBatch = new LabelBatch(DataReader.ReadTrainLabel());
            var net = ResultWriter.ReadResult();
            for (int x = startPos/STEP; x < (60000/STEP); x++)
            {
                ResultWriter.WriteLog("start:" + x * STEP + " to " + (x * STEP + STEP) + "\n");
                for (; ; )
                {
                    var e=Learn(ImageBatch, LabelBatch, x, net);
                    if (e / STEP < 0.01)
                    {
                        ResultWriter.WriteLog("cost:" + e / STEP + "\n");
                        break;
                    }
                }
            }
        }

        private static double Learn(ImageBatch imb,LabelBatch lab,int x,Net net)
        {
            int CorrectNum = 0;
            double e = 0;
            for (int i = x * STEP; i < x * STEP + STEP; i++)
            {
                formChanger.ChangeImage(imb[i].GetBitmap(),lab[i]+"     ("+(i+1)+"/"+imb.Count()+")");
                formChanger.SetInfo("index:" + (i + 1)+"  Load Picture");
                net.LoadSource(imb[i], lab[i]);
                formChanger.SetInfo("index:" + (i + 1) + "  Begin Reason");
                int say;
                var res=net.BeginReason(out say);
                var cost=net.Evaluation();
                formChanger.SetInfo("Ans:"+(res?"Correct": "Wrong")+"!    Say: "+say);
                formChanger.SetInfo("Cost:"+cost);
                formChanger.SetInfo("index:" + (i + 1)+"  Begin Recall");
                formChanger.AddNeuronNote(net, lab[i], say, cost);
                net.Recall();
                e += cost;
                if (res) CorrectNum++;
            }
            //Console.WriteLine("=================================");
            Console.WriteLine("Average Cost:" + e / STEP);
            Console.WriteLine("Accuracy:"+Convert.ToDouble(CorrectNum)/STEP);
            formChanger.AddCost(e / STEP, Convert.ToDouble(CorrectNum) / STEP);
            Console.WriteLine("Learn Updating");
            net.Update(1);
            Console.WriteLine("Saving Statue");
            ResultWriter.WriteResult(net);
            Console.WriteLine("=================================");
            return e;
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

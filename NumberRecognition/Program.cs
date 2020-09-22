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

        private static FormChanger formChanger;
        static void Main(string[] args)
        {
            //BuildNewNet();
            var form = new ImageForm();
            formChanger = new FormChanger(form);
            (new Thread(formChanger.RunForm)).Start();
            BeginLearning(0);
            RunCmd2("shutdown", "-p");
            Console.ReadLine();
        }

        private static void BeginLearning(int startPos)
        {
            var ImageBatch = new ImageBatch(DataReader.ReadTrainImage());
            var LabelBatch = new LabelBatch(DataReader.ReadTrainLabel());
            var net = ResultWriter.ReadResult();
            for (int x = startPos/100; x < 600; x++)
            {
                ResultWriter.WriteLog("start:" + x * 100 + " to " + (x * 100 + 100) + "\n");
                for (; ; )
                {
                    double e = 0;
                    Learn(ImageBatch, LabelBatch, x, e, net);
                    if (e / 100 < 1)
                    {
                        ResultWriter.WriteLog("cost:" + e / 100 + "\n");
                        break;
                    }
                }
            }
        }

        private static void Learn(ImageBatch imb,LabelBatch lab,int x,double e,Net net)
        {
            int CorrectNum = 0;
            for (int i = x * 100; i < x * 100 + 100; i++)
            {
                formChanger.ChangeImage(imb[i].GetBitmap(),lab[i]+"     ("+i+"/"+imb.Count()+")");
                Console.WriteLine("index:" + (i + 1)+"  Load Picture");
                net.LoadSource(imb[i], lab[i]);
                Console.WriteLine("index:" + (i + 1) + "    Begin Reason");
                var res=net.BeginReason();
                var cost=net.Evaluation();
                Console.WriteLine("Ans:"+(res?"Correct": "Wrong")+"!");
                Console.WriteLine("Cost:"+cost);
                Console.WriteLine("index:" + (i + 1)+"  Begin Recall");
                net.Recall();
                e += cost;
                if (res) CorrectNum++;
            }
            Console.WriteLine("Average Cost:" + e / 100);
            Console.WriteLine("Accuracy:"+Convert.ToDouble(CorrectNum)/100);
            Console.WriteLine("Learn Updating");
            net.Update();
            Console.WriteLine("Saving Statue");
            ResultWriter.WriteResult(net);
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
        }
    }
}

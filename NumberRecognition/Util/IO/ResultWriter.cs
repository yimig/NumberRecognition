using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NumberRecognition.Model;
using NumberRecognition.Model.NeuronNet;

namespace NumberRecognition.Util.IO
{
    static class ResultWriter
    {
        private const string FILE_NAME = "result.json";
        private const string LOG_NAME = "log.txt";

        public static void WriteResult(Net net)
        {
            var jsonStr = JsonConvert.SerializeObject(net);
            StreamWriter sw=new StreamWriter(FILE_NAME,false);
            sw.Write(jsonStr);
            sw.Close();
        }

        public static void WriteLog(string logStr)
        {
            StreamWriter sw = new StreamWriter(LOG_NAME, true);
            logStr = DateTime.Now.ToString()+"\n==========================\n" + logStr;
            sw.Write(logStr);
            sw.Close();
        }

        public static Net ReadResult()
        {
            StreamReader sr=new StreamReader(FILE_NAME);
            var jsonStr = sr.ReadToEnd();
            sr.Close();
            return JsonConvert.DeserializeObject<Net>(jsonStr);
        }

        public static bool IsExistResult()
        {
            return File.Exists(FILE_NAME);
        }
    }
}

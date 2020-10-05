using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NumberRecognition.Model.NeuronNet
{
    public class Neuron
    {
        [JsonProperty("active")]public double Active { get; set; }

        public Neuron()
        {
            
        }

        public Neuron(double active)
        {
            Active = active;
        }

        public override string ToString()
        {
            return Active.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length">保留小数点位数</param>
        /// <returns></returns>
        public string ToString(int length)
        {
            return Active.ToString("f"+length);
        }
    }
}

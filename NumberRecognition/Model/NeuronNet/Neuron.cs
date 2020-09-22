using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NumberRecognition.Model.NeuronNet
{
    class Neuron
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
            return Active + "";
        }
    }
}

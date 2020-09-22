using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberRecognition.Model.Data
{
    interface IBatch:IEnumerable
    {
        int Count();

        byte[] GetRawBytes();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberRecognition.Model.Data
{
    class LabelBatch:IBatch
    {
        private int magicNumber, itemCount;
        private byte[] rawData;
        private List<int> labels;

        public LabelBatch(byte[] rawData)
        {
            this.rawData = rawData;
            GetMagicNumber();
            GetItemCount();
            GetLabels();
        }

        public int MagicNumber => magicNumber;

        public int ItemCount => itemCount;

        public int this[int index] => labels[index];

        private int BytesToInt32(byte[] bytes, int startPos)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] tempBytes = new byte[4];
                Buffer.BlockCopy(bytes, startPos, tempBytes, 0, 4);
                Array.Reverse(tempBytes);
                return BitConverter.ToInt32(tempBytes, 0);
            }
            else
            {
                return BitConverter.ToInt32(bytes, startPos);
            }
        }

        private void GetMagicNumber()
        {
            magicNumber = BytesToInt32(rawData, 0);
        }

        private void GetItemCount()
        {
            itemCount = BytesToInt32(rawData, 4);
        }

        private void GetLabels()
        {
            labels=new List<int>();
            for (int i = 8; i < rawData.Length; i++)
            {
                labels.Add(rawData[i]);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return labels.GetEnumerator();
        }

        public int Count()
        {
            return ItemCount;
        }

        public byte[] GetRawBytes()
        {
            return rawData;
        }
    }
}

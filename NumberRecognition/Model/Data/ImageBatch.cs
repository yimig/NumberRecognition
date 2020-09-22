using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumberRecognition.Model.NeuronNet;

namespace NumberRecognition.Model.Data
{
    class ImageBatch:IBatch
    {
        private int magicNumber, itemCount;
        private List<PixelImage> pixelImages;
        private int rows, columns;
        private byte[] rawData;

        public int MagicNumber => magicNumber;

        public int Rows => rows;

        public int Columns => columns;

        public ImageBatch(byte[] rawData)
        {
            this.rawData=rawData;
            GetMagicNumber();
            GetItemCount();
            GetRowsAndColumns();
            GetImages();
        }

        private int BytesToInt32(byte[] bytes,int startPos)
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

        private void GetRowsAndColumns()
        {
            rows = BytesToInt32(rawData, 8);
            columns = BytesToInt32(rawData, 12);
        }

        private void GetImages()
        {
            pixelImages=new List<PixelImage>();
            for (int i = 0; i < itemCount; i++)
            {
                byte[] tempBytes=new byte[rows*columns];
                Buffer.BlockCopy(rawData,(16+i*rows*columns),tempBytes,0,rows*columns);
                pixelImages.Add(new PixelImage(rows,columns,tempBytes));
            }
        }

        public int Count()
        {
            return pixelImages.Count;
        }

        public byte[] GetRawBytes()
        {
            return rawData;
        }

        public IEnumerator GetEnumerator()
        {
            return pixelImages.GetEnumerator();
        }

        public PixelImage this[int index] => pixelImages[index];
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumberRecognition.Model.NeuronNet;

namespace NumberRecognition.Model.Data
{
    class PixelImage
    {
        private int rows, columns;
        private byte[] data;

        public int Rows => rows;

        public int Columns => columns;

        public byte[] Data => data;

        public int Size => data.Length;

        public byte this[int index]
        {
            get => data[index];
        }

        public PixelImage(int rows, int columns, byte[] data)
        {
            this.rows = rows;
            this.columns = columns;
            this.data = data;
        }

        public Bitmap GetBitmap()
        {
            var temp=new Bitmap(columns,rows);
            for (int i = 0; i < data.Length; i++)
            {
                int color = 255 - data[i];
                temp.SetPixel(i%columns,i/columns,Color.FromArgb(color,color,color));
            }

            return temp;
        }
    }
}

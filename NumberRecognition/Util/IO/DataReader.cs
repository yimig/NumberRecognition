using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberRecognition.Util.IO
{
    static class DataReader
    {
        private const string TRAIN_IMAGES = "data/train-images.idx3-ubyte";
        private const string TRAIN_LABELS = "data/train-labels.idx1-ubyte";
        private const string TEST_IMAGES = "data/t10k-images.idx3-ubyte";
        private const string TEST_LABELS = "data/t10k-labels.idx1-ubyte";

        private static byte[] ReadData(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] byteArray = new byte[fs.Length];
                    fs.Read(byteArray, 0, byteArray.Length);
                    return byteArray;
                }
            }
            catch
            {
                return null;
            }
        }

        public static byte[] ReadTrainImage()
        {
            return ReadData(TRAIN_IMAGES);
        }

        public static byte[] ReadTrainLabel()
        {
            return ReadData(TRAIN_LABELS);
        }

        public static byte[] ReadTestImage()
        {
            return ReadData(TEST_IMAGES);
        }

        public static byte[] ReadTestLabel()
        {
            return ReadData(TEST_LABELS);
        }
    }
}

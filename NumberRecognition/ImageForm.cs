using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NumberRecognition
{
    public partial class ImageForm : Form
    {
        public ImageForm()
        {
            InitializeComponent();
        }

        public void SetNumber(Bitmap numPic,string title)
        {
            this.Invoke(new EventHandler(delegate
            {
                this.pictureBox.Image = numPic;
                this.Text = title;
            }));
        }
    }
}

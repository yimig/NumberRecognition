using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using NumberRecognition.Model.NeuronNet;

namespace NumberRecognition
{
    public partial class ImageForm : Form
    {
        private Label[] lbLayer1, lbLayer2, lbLayer3;

        public ImageForm()
        {
            InitializeComponent();
            InitLabelLayers();
            InitChart();
        }

        private void InitLabelLayers()
        {
            lbLayer1 = new[]
            {
                lbL1N1, lbL1N2, lbL1N3, lbL1N4, lbL1N5, lbL1N6, lbL1N7, lbL1N8, lbL1N9, lbL1N10, lbL1N11, lbL1N12,
                lbL1N13, lbL1N14, lbL1N15, lbL1N16
            };
            lbLayer2 = new[]
            {
                lbL2N1, lbL2N2, lbL2N3, lbL2N4, lbL2N5, lbL2N6, lbL2N7, lbL2N8, lbL2N9, lbL2N10, lbL2N11, lbL2N12,
                lbL2N13, lbL2N14, lbL2N15, lbL2N16
            };
            lbLayer3 = new[]
            {
                lbL3N1, lbL3N2, lbL3N3, lbL3N4, lbL3N5, lbL3N6, lbL3N7, lbL3N8, lbL3N9, lbL3N10
            };
        }

        private void InitChart()
        {
            // 定义图表区域
            this.ctCost.ChartAreas.Clear();
            ChartArea chartArea1 = new ChartArea("C1");
            this.ctCost.ChartAreas.Add(chartArea1);
            //定义存储和显示点的容器
            this.ctCost.Series.Clear();
            Series series1 = new Series("Average Cost");
            series1.ChartArea = "C1";
            this.ctCost.Series.Add(series1);
            //设置图表显示样式
            this.ctCost.ChartAreas[0].AxisY.Minimum = 0;
            this.ctCost.ChartAreas[0].AxisY.Maximum = 1;
            this.ctCost.ChartAreas[0].AxisX.Interval = 10;
            this.ctCost.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.ctCost.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //设置标题
            this.ctCost.Titles.Clear();
            this.ctCost.Titles.Add("S01");
            this.ctCost.Titles[0].Text = "Cost Table";
            this.ctCost.Titles[0].ForeColor = Color.RoyalBlue;
            this.ctCost.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            //设置图表显示样式
            this.ctCost.Series[0].ChartType = SeriesChartType.Line;
            //this.ctCost.Series[0].ChartType = SeriesChartType.Spline;
            this.ctCost.Series[0].Color = Color.Red;
            this.ctCost.Series[0].Points.Clear();

        }

        public void AddCost(double cost)
        {
            this.Invoke(new EventHandler(delegate
            {
                ctCost.Series[0].Points.AddY(cost);
            }));
        }

        public void SetNumber(Bitmap numPic,string title,Net net)
        {
            this.Invoke(new EventHandler(delegate
            {
                this.pictureBox.Image = numPic;
                this.Text = title;
                AddNeuronNote(net);
            }));
        }

        public void SetInfo(string info)
        {
            this.Invoke(new EventHandler(delegate
            {
                tbStatue.AppendText(info + "\r\n");
            }));
        }

        public void AddNeuronNote(Net net)
        {
            for (int i = 0; i < 16; i++)
            {
                lbLayer1[i].Text = net.Layers[1][i].ToString(4);
            }
            for (int i = 0; i < 16; i++)
            {
                lbLayer2[i].Text = net.Layers[2][i].ToString(4);
            }
            for (int i = 0; i < 10; i++)
            {
                lbLayer3[i].Text = net.Layers[3][i].ToString(4);
            }
        }
    }
}

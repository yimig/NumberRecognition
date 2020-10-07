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
        private Chart[] subCharts;

        public ImageForm()
        {
            InitializeComponent();
            InitLabelLayers();
            InitMainChart();
            InitSubCharts();
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

        private void InitMainChart()
        {
            ctCost.Series.Clear();
            var area = AddChartArea(1000,5, ctCost,"C1");
            AddChartSeries(ctCost,area, "Average Cost", Color.Red);
            AddChartSeries(ctCost,area, "Accuracy", Color.DarkBlue);
            //设置标题
            this.ctCost.Titles.Clear();
            this.ctCost.Titles.Add("S01");
            this.ctCost.Titles[0].Text = "Cost Table";
            this.ctCost.Titles[0].ForeColor = Color.RoyalBlue;
            this.ctCost.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        }

        private void InitSubCharts()
        {
            subCharts = new[] {chart1, chart2, chart3, chart4, chart5, chart6, chart7, chart8, chart9, chart10};
            for (int i = 0; i < subCharts.Length; i++)
            {
                InitSubChart(subCharts[i],i);
            }
        }

        private void InitSubChart(Chart chart,int id)
        {
            chart.Series.Clear();
            var area = AddChartArea(10000,5, chart,"C1");
            AddChartSeries(chart,area, "Cost", Color.Red);
            AddChartSeries(chart,area, "Confidence", Color.BlueViolet);
            AddChartSeries(chart, area, "AWC", Color.Black);
            //设置标题
            chart.Titles.Clear();
            chart.Titles.Add("S01");
            chart.Titles[0].Text = "Number "+id;
            chart.Titles[0].ForeColor = Color.RoyalBlue;
            chart.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        }

        private ChartArea AddChartArea(int intervalX,int maxY,Chart chart,string name)
        {
            // 定义图表区域
            ChartArea area = new ChartArea(name);
            chart.ChartAreas.Add(area);
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = maxY;
            area.AxisX.Interval = intervalX;
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            // Zoom into the Y axis
            //area.AxisY.ScaleView.Zoom(2, 3);

            // Enable range selection and zooming end user interface
            area.CursorY.IsUserEnabled = true;
            area.CursorY.IsUserSelectionEnabled = true;
            area.AxisY.ScaleView.Zoomable = true;

            //将滚动内嵌到坐标轴中
            area.AxisY.ScrollBar.IsPositionedInside = true;

            // 设置滚动条的大小
            area.AxisY.ScrollBar.Size = 10;

            // 设置滚动条的按钮的风格，下面代码是将所有滚动条上的按钮都显示出来
            area.AxisY.ScrollBar.ButtonStyle = ScrollBarButtonStyles.All;

            // 设置自动放大与缩小的最小量
            area.AxisY.ScaleView.SmallScrollSize = double.NaN;
            area.AxisY.ScaleView.SmallScrollMinSize = 2;
            return area;
        }

        private Series AddChartSeries(Chart chart,ChartArea area,string name,Color color)
        {
            Series series = new Series(name);
            series.ChartArea = area.Name;
            chart.Series.Add(series);
            series.ChartType = SeriesChartType.Line;
            series.Color = color;
            return series;
        }

        private void AssignDataToSubChart(Net net, int testNum, double cost)
        {
            subCharts[testNum].Series[0].Points.AddY(cost);
            subCharts[testNum].Series[1].Points.AddY(net.Layers[3][testNum].Active);
            double sum=0;
            for (int i = 0; i < 10; i++)
            {
                if(i==testNum)continue;
                sum += net.Layers[3][i].Active;
            }

            subCharts[testNum].Series[2].Points.AddY(sum / 9);
        }

        public void AddCost(double cost,double accuracy)
        {
            this.Invoke(new EventHandler(delegate
            {
                ctCost.Series[0].Points.AddY(cost);
                ctCost.Series[1].Points.AddY(accuracy);
            }));
        }

        public void SetNumber(Bitmap numPic,string title)
        {
            this.Invoke(new EventHandler(delegate
            {
                this.pictureBox.Image = numPic;
                this.Text = title;
            }));
        }

        private void ImageForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        public void SetInfo(string info)
        {
            this.Invoke(new EventHandler(delegate
            {
                tbStatue.AppendText(info + "\r\n");
            }));
        }

        public void AddNeuronNote(Net net,int testNum,int sayNum, double cost)
        {
            this.Invoke(new EventHandler(delegate
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

                foreach (var lb in lbLayer3)
                {
                    lb.BackColor = SystemColors.Control;
                }
                lbLayer3[sayNum].BackColor = Color.IndianRed;
                lbLayer3[testNum].BackColor = Color.LightGreen;
                AssignDataToSubChart(net,testNum,cost);
            }));
        }
    }
}

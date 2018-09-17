/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TCGA_Genetic_Workbench
{
    public partial class MeasureForm : Form
    {
        public List<int> curList;
        public int selectionStatus; // 0 Not, 1 Source, 2 Target

        public MeasureForm(MeasureParams mp, Filter filter, int status, Boolean evolutionStarted)
        {
            InitializeComponent();

            if (filter == null)
            {
                curList = new List<int>();
            }
            else
            {
                curList = filter.excluded;
            }

            if (evolutionStarted)
            {
                button2.Enabled = false;
            }

            string sampleFile = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(mp.barFile))) + "\\Samples.csv";

            selectionStatus = status;

            switch (status)
            {
                case 0:
                    button2.Text = "Not Selected";
                    button2.BackColor = SystemColors.Control;
                    break;
                case 1:
                    button2.Text = "Source Data";
                    button2.BackColor = Color.DarkSeaGreen;
                    break;
                case 2:
                    button2.Text = "Target Data";
                    button2.BackColor = Color.LightSteelBlue;
                    break;
            }

            FileInfo f = new FileInfo(mp.barFile);

            int size = (int)(f.Length);
            byte[] valByteArray = new byte[size];
            float[] valArray = new float[size / sizeof(float)];

            BinaryReader b = new BinaryReader(File.Open(mp.barFile, FileMode.Open));
            b.Read(valByteArray, 0, size);
            b.Close();

            Buffer.BlockCopy(valByteArray, 0, valArray, 0, size);

            int valueCount = 0;

            List<float> floatList = new List<float>();
            List<Int32> intList = new List<Int32>();

            for (int i = 0; i < valArray.Length; i++)
            {
                if (valArray[i] != float.MinValue)
                {
                    floatList.Add(valArray[i]);
                    valueCount++;
                }
            }

            floatList.Sort();

            List<string> evalList = null;
            string evlFile = mp.barFile.Replace(".bar",".evl");

            if (File.Exists(evlFile))
            {
                evalList = loadStringList(evlFile);
            }

            chart1.ChartAreas[0].AxisX.LabelStyle.IsEndLabelVisible = false;
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.IsStartedFromZero = false;

            float thresh = mp.lowThresh;
            int count = 0;
            int pointIndex = 0;
            int steps = 0;

            if (evalList == null)
            {
                string axisLabel = "< " + numFormat(thresh, mp.stepExp);

                for (int v = 0; v < floatList.Count(); v++)
                {
                    if (floatList[v] < thresh)
                    {
                        count++;
                    }
                    else
                    {
                        if (steps >= mp.numSteps)
                        {
                            break;
                        }

                        chart1.Series[0].Points.Add(count);
                        chart1.Series[0].Points[pointIndex].AxisLabel = axisLabel;
                        if (curList.Contains(pointIndex))
                        {
                            chart1.Series[0].Points[pointIndex].Color = Color.DarkRed;
                        }

                        count = 0;
                        pointIndex++;
                        axisLabel = numFormat(thresh, mp.stepExp) + " to ";
                        thresh += mp.step;
                        steps++;
                        axisLabel += numFormat(thresh, mp.stepExp);
                    }
                }

                chart1.Series[0].Points.Add(count);
                chart1.Series[0].Points[pointIndex].AxisLabel = "> " + numFormat(thresh, mp.stepExp);
                if (curList.Contains(pointIndex))
                {
                    chart1.Series[0].Points[pointIndex].Color = Color.DarkRed;
                }
            }
            else
            {
                int evalIdx = 0;

                for (int v = 0; v < floatList.Count(); v++)
                {
                    if ((int)(floatList[v]) == evalIdx)
                    {
                        count++;
                    }
                    else
                    {
                        chart1.Series[0].Points.Add(count);
                        chart1.Series[0].Points[evalIdx].AxisLabel = evalList[evalIdx];
                        if (curList.Contains(evalIdx))
                        {
                            chart1.Series[0].Points[evalIdx].Color = Color.DarkRed;
                        }

                        count = 0;
                        evalIdx++;
                    }
                }

                chart1.Series[0].Points.Add(count);
                chart1.Series[0].Points[evalIdx].AxisLabel = evalList[evalIdx];
                if (curList.Contains(evalIdx))
                {
                    chart1.Series[0].Points[evalIdx].Color = Color.DarkRed;
                }
            }

            if (mp.barFile.Contains("Probe_Expression"))
            {
                int pipeOffset = mp.name.IndexOf("|");

                if (pipeOffset > 1)
                {
                    label1.Text = scortUnconvert(mp.name.Substring(0, pipeOffset));
                    label3.Text = "(Gene: " + mp.name.Substring(pipeOffset + 1) + ")";

                    if (label3.Text.Contains("---"))
                    {
                        label3.Visible = false;
                    }
                    else
                    {
                        label3.Visible = true;
                    }
                }
            }
            else
            {
                label1.Text = mp.name;
                label3.Visible = false;
            }

            label2.Text = valueCount.ToString() + "  Values";

            List<string> sampleIDs = loadStringList(sampleFile);

            DataGridViewCell cell = new DataGridViewTextBoxCell();

            cell.Style.BackColor = Color.LightGray;
            cell.Style.SelectionBackColor = Color.LightGray;
            cell.Style.SelectionForeColor = Color.Black;
            cell.Style.Font = new Font("Times New Roman", 14);
            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            dataGridView1.ColumnCount = 2;
            dataGridView1.RowHeadersWidth = 200;
            dataGridView1.RowHeadersDefaultCellStyle.Font = new Font("Times New Roman", 10);

            dataGridView1.Columns[1].Name = "Values";
            dataGridView1.Columns[0].CellTemplate = cell;
            dataGridView1.Columns[1].CellTemplate = cell;
            dataGridView1.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.TopCenter;
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].Width = 160;
            dataGridView1.Columns[1].Width = 154;

            int ididx = 0;
            string[] row = new string[2];
            for (int i = 0; i < valArray.Length; i++)
            {
                if (valArray[i] != float.MinValue)
                {
                    row[0] = sampleIDs[i];
                    if (evalList == null)
                    {
                        row[1] = valArray[i].ToString();
                    }
                    else
                    {
                        int idx = Convert.ToInt32(valArray[i]);
                        row[1] = evalList[idx];
                    }
                    dataGridView1.Rows.Add(row);
                }
            }

            dataGridView1.CurrentCell = null;
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.RowHeadersVisible = false;
        }

        static public string scortUnconvert(string longLabel)
        {
            string slabel = longLabel;

            if (slabel.StartsWith("A-"))
            {
                slabel = slabel.Replace("A-", "Adx-");
            }
            else if (slabel.StartsWith("A."))
            {
                slabel = slabel.Replace("A.", "ADXEC.");
            }
            else if (slabel.StartsWith("AD."))
            {
                slabel = slabel.Replace("AD.", "ADXECAD.");
            }
            else if (slabel.StartsWith("ADA."))
            {
                slabel = slabel.Replace("ADA.", "ADXECADA.");
            }
            else if (slabel.StartsWith("AMU."))
            {
                slabel = slabel.Replace("AMU.", "ADXECEMUTR.");
            }
            else if (slabel.StartsWith("AM."))
            {
                slabel = slabel.Replace("AM.", "ADXECMG.");
            }
            else if (slabel.StartsWith("AN."))
            {
                slabel = slabel.Replace("AN.", "ADXECNTDJ.");
            }
            else if (slabel.StartsWith("AR."))
            {
                slabel = slabel.Replace("AR.", "ADXECRS.");
            }
            else if (slabel.StartsWith("AL."))
            {
                slabel = slabel.Replace("AL.", "ADXLCEC.");
            }
            else if (slabel.StartsWith("AO."))
            {
                slabel = slabel.Replace("AO.", "ADXOCEC.");
            }
            else if (slabel.StartsWith("AP."))
            {
                slabel = slabel.Replace("AP.", "ADXPCEC.");
            }

            slabel = slabel + "_at";

            return (slabel);
        }

        private string numFormat(float f, int stepExp)
        {
            if (stepExp >= 0)
            {
                int val = (int)f;
                for (int i = 0; i < stepExp; i++)
                {
                    val = val / 10;
                }

                for (int i = 0; i < stepExp; i++)
                {
                    val = val * 10;
                }

                return val.ToString();
            }
            else
            {
                string ft = "F" + (-stepExp).ToString();
                return (f.ToString(ft));
            }
        }

        private List<string> loadStringList(string filename)
        {
            List<string> samples = new List<string>();

            if (File.Exists(filename) == false)
            {
                return (null);
            }

            string line;
            int idx = 0;

            StreamReader reader = new StreamReader(filename);
            while ((line = reader.ReadLine()) != null)
            {
                samples.Add(line);
                idx++;
            }

            reader.Close();

            return (samples);
        }

        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (button2.Enabled == false)
            {
                return;
            }

            HitTestResult seriesHit = chart1.HitTest(e.X, e.Y);
            int idx = -1;
            if (seriesHit.ChartElementType == ChartElementType.DataPoint)
            {
                idx = seriesHit.PointIndex;
            }
            else if (seriesHit.ChartElementType == ChartElementType.AxisLabels)
            {
                CustomLabel cl = (CustomLabel)(seriesHit.Object);
                idx = (((int)(cl.ToPosition + cl.FromPosition)) / 2) - 1;
            }

            if (idx >= 0)
            {
                if (curList.Contains(idx))
                {
                    chart1.Series[0].Points[idx].Color = Color.Teal;
                    curList.Remove(idx);
                }
                else
                {
                    chart1.Series[0].Points[idx].Color = Color.DarkRed;
                    curList.Add(idx);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch (selectionStatus)
            {
                case 0:
                    button2.Text = "Source Data";
                    button2.BackColor = Color.DarkSeaGreen;
                    selectionStatus = 1;
                    break;

                case 1:
                    button2.Text = "Target Data";
                    button2.BackColor = Color.LightSteelBlue;
                    selectionStatus = 2;
                    break;

                case 2:
                    button2.Text = "Not Selected";
                    button2.BackColor = SystemColors.Control;
                    selectionStatus = 0;
                    break;
            }
        }
    }
}

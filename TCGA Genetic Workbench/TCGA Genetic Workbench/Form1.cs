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
using Evolution;
using System.Drawing.Drawing2D;
using MathNet.Numerics;

namespace TCGA_Genetic_Workbench
{
    public partial class Form1 : Form
    {
        string startDir = "C:\\firehose";
        string cancerDir = "C:\\firehose";
        int pastCancerType = 0;
        Button[] mbuttons;
        Button[] leaderbuttons;
        int mbuttonsVisible = 50;
        int curMeasureOffset = 0;
        int curNumMeasures = 0;
        int filterPercent = 100;
        string curNodeName;
        List<Filter> filters;
        List<PipelineSelection> selections;
        PipelineSelection currentSelection;
        TreeNode filterNode;
        Boolean pauseFlag = false;
        Boolean timerTicked = false;
        List<int> scatterX;
        List<int> scatterY;
        string scatterXLabel;
        string scatterYLabel;
        string scatterXPipeline;
        string scatterYPipeline;
        float scatterMinX;
        float scatterMaxX;
        float scatterMinY;
        float scatterMaxY;
        float scatterA;
        float scatterB;
        float scatterQ;
        int leaderBase = -1;
        Boolean showLeaderChart = false;
        Boolean evolutionStarted = false;
        List<string> searchPaths;
        List<int> searchIndices;
        int searchIndex = -1;
        Boolean rbChangedProgrammatically = false;
        Boolean evolutionReady = false;
        int[] leaderIndex = null;
        Correlation currentLeader = null;
        bool coad = false;
        int brafMode = 0;

        Dictionary<string, string> scortDictionary = null;

        int numSamples;

        Population population;
        ACEUniverse u;

        public Form1()
        {
            InitializeComponent();
            Timer timer = new Timer();

            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 6000;
            timer.Enabled = true; 
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timerTicked = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.SetBounds(this.Location.X, this.Location.Y, 1900, 920);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Forget All")
            {
                clearExperiment();
                treeView1.Invalidate();
            }
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                DialogResult result = fbd.ShowDialog();
                startDir = fbd.SelectedPath;
                Properties.Settings.Default.startdir = startDir;
                Properties.Settings.Default.Save();
                label3.Text = startDir;
            }
        }

        private void clearExperiment()
        {
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            filters = new List<Filter>();
            selections = new List<PipelineSelection>();
            setMbuttonsVisible(0);
            setLeaderButtonsVisible(false);
            curMeasureOffset = 0;
            curNumMeasures = 0;
            filterPercent = 0;
            pauseFlag = false;
            timerTicked = false;
            showLeaderChart = false;
            evolutionStarted = false;
            searchPaths = new List<string>();
            searchIndices = new List<int>();
            searchIndex = -1;
            evolutionReady = false;
            button1.Text = "Change Root Data Folder";
            button57.Visible = false;
            button57.Enabled = true;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;
            treeView1.Nodes.Clear();
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = false;
            label6.Visible = false;
            label7.Visible = false;
            comboBox5.Visible = false;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox1.Text = "";
            comboBox2.Text = "";
            label11.Text = "0";
            label17.Text = "0% Cached";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            startDir = Properties.Settings.Default.startdir;
            if (!Directory.Exists(startDir))
            {
                startDir = "C:\\firehose";
            }

            label3.Text = startDir;
            string[] cancerTypes = Directory.GetDirectories(startDir + "\\FAST");

            foreach (string ct in cancerTypes)
            {
                comboBox1.Items.Add(Path.GetFileName(ct));
                comboBox2.Items.Add(Path.GetFileName(ct));
            }

            comboBox3.Items.Add("Both");
            comboBox3.Items.Add("Positive");
            comboBox3.Items.Add("Negative");
            comboBox3.SelectedIndex = 0;

            comboBox5.Items.Add("All samples");
            comboBox5.Items.Add("V600E present");
            comboBox5.Items.Add("No V600E");
            comboBox5.SelectedIndex = 0;

            filters = new List<Filter>();
            selections = new List<PipelineSelection>();

            Font treeFont = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
            //treeView1.ItemHeight = 20;
            treeView1.Font = treeFont;
            setControlArrays();
            setMbuttonsVisible(0);
            setLeaderButtonsVisible(false);

            scatterX = new List<int>();
            scatterY = new List<int>();
            scatterXLabel = "";
            scatterYLabel = "";
        }

        private void setControlArrays()
        {
            mbuttons = new Button[50];

            mbuttons[0] = button2;
            mbuttons[1] = button3;
            mbuttons[2] = button4;
            mbuttons[3] = button5;
            mbuttons[4] = button6;
            mbuttons[5] = button7;
            mbuttons[6] = button8;
            mbuttons[7] = button9;
            mbuttons[8] = button10;
            mbuttons[9] = button11;

            mbuttons[10] = button12;
            mbuttons[11] = button13;
            mbuttons[12] = button14;
            mbuttons[13] = button15;
            mbuttons[14] = button16;
            mbuttons[15] = button17;
            mbuttons[16] = button18;
            mbuttons[17] = button19;
            mbuttons[18] = button20;
            mbuttons[19] = button21;

            mbuttons[20] = button22;
            mbuttons[21] = button23;
            mbuttons[22] = button24;
            mbuttons[23] = button25;
            mbuttons[24] = button26;
            mbuttons[25] = button27;
            mbuttons[26] = button28;
            mbuttons[27] = button29;
            mbuttons[28] = button30;
            mbuttons[29] = button31;

            mbuttons[30] = button32;
            mbuttons[31] = button33;
            mbuttons[32] = button34;
            mbuttons[33] = button35;
            mbuttons[34] = button36;
            mbuttons[35] = button37;
            mbuttons[36] = button38;
            mbuttons[37] = button39;
            mbuttons[38] = button40;
            mbuttons[39] = button41;

            mbuttons[40] = button42;
            mbuttons[41] = button43;
            mbuttons[42] = button44;
            mbuttons[43] = button45;
            mbuttons[44] = button46;
            mbuttons[45] = button47;
            mbuttons[46] = button48;
            mbuttons[47] = button49;
            mbuttons[48] = button50;
            mbuttons[49] = button51;

            for (int i = 0; i < 50; i++)
            {
                mbuttons[i].Click += measureButton_Click;
                mbuttons[i].Tag = i;
            }

            leaderbuttons = new Button[10];

            leaderbuttons[0] = button62;
            leaderbuttons[1] = button63;
            leaderbuttons[2] = button64;
            leaderbuttons[3] = button65;
            leaderbuttons[4] = button66;
            leaderbuttons[5] = button67;
            leaderbuttons[6] = button68;
            leaderbuttons[7] = button69;
            leaderbuttons[8] = button70;
            leaderbuttons[9] = button71;

            for (int i = 0; i < 10; i++)
            {
                leaderbuttons[i].Click += leaderButton_Click;
                leaderbuttons[i].Tag = i;
            }
        }

        private void setMbuttonsVisible(int newValue)
        {
            if (newValue > mbuttonsVisible)
            {
                for (int b = mbuttonsVisible; b < newValue; b++)
                {
                    mbuttons[b].Visible = true;
                }
            }
            else if (newValue < mbuttonsVisible)
            {
                for (int b = newValue; b < mbuttonsVisible; b++)
                {
                    mbuttons[b].Visible = false;
                }
            }

            mbuttonsVisible = newValue;
        }

        private void setLeaderButtonsVisible(Boolean newValue)
        {
            for (int b = 0; b < 10; b++)
            {
                leaderbuttons[b].Visible = newValue;
            }
        }


        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox5.SelectedIndex)
            {
                case 0:
                    label7.Text = "623 Samples";
                    brafMode = 0;
                    break;
                case 1:
                    label7.Text = "20 Samples";
                    brafMode = 1;
                    break;
                case 2:
                    label7.Text = "603 Samples";
                    brafMode = 2;
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString().Length == 0)
            {
                return;
            }

            if (comboBox1.SelectedItem.ToString().Equals("COAD"))
            {
                coad = true;
            }
            else
            {
                coad = false;
            }

            if (comboBox1.SelectedItem.ToString().Equals("SCORT"))
            {
                //makeScortGeneIndex();
            }

            cancerDir = startDir + "\\FAST\\" + comboBox1.SelectedItem.ToString();
            pastCancerType = cancerDir.Length - (startDir.Length + 5);

            treeView1.Nodes.Clear();

            TreeNode node = null;
            TreeNode first = null;

            List<string> sampleIDs = loadSampleList(cancerDir + "\\Samples.csv");
            numSamples = sampleIDs.Count();
            label7.Text = sampleIDs.Count().ToString() + " Samples";
            label7.Visible = true;
            label2.Visible = false;
            label3.Visible = false;
            button1.Visible = false;
            label18.Visible = true;
            label19.Visible = true;
            checkBox1.Visible = true;
            comboBox3.Visible = true;

            if (coad)
            {
                comboBox5.Visible = true;
            }

            filters = new List<Filter>();
            selections = new List<PipelineSelection>();

            string[] categories = Directory.GetDirectories(cancerDir);

            foreach (string category in categories)
            {
                string cname = Path.GetFileName(category);

                string catDir = cancerDir + "\\" + cname;

                string[] pipelines = Directory.GetDirectories(catDir);

                if (pipelines.Length > 0)
                {
                    node = treeView1.Nodes.Add(cname);
                    node.Name = "*" + cname;

                    if (first == null)
                    {
                        first = node;
                    }

                    foreach (string pipeline in pipelines)
                    {
                        string pname = Path.GetFileName(pipeline);
                        string shortPname = pname.Substring(pastCancerType);
                        node.Nodes.Add(shortPname).Name = pipeline;
                        selections.Add(new PipelineSelection(pipeline));
                    }
                }
            }

            node = treeView1.Nodes.Add("Filters");
            node.Name = "#Filters";
            filterNode = node;

            node = treeView1.Nodes.Add("Search");
            node.Name = "#Search";

            foreach (TreeNode n in treeView1.Nodes)
            {
                n.Expand();
            }

            if (first != null)
            {
                treeView1.SelectedNode = first;
            }

            comboBox2.SelectedIndex = comboBox1.SelectedIndex;

            dataGridView1.Visible = false;
            textBox1.Visible = false;
            button72.Visible = false;
            label13.Visible = false;
            listBox1.Visible = false;
            setMbuttonsVisible(0);
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;
            label4.Visible = false;
            button57.Visible = false;
            label6.Visible = false;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem.ToString().Length == 0)
            {
                return;
            }

            comboBox1.SelectedIndex = comboBox2.SelectedIndex;
            return;
            /*
            cancerDir = startDir + "\\" + comboBox2.SelectedItem.ToString();

            treeView1.Nodes.Clear();

            TreeNode node = null;
            TreeNode first = null;

            List<string> sampleIDs = loadSampleList(cancerDir + "\\Samples.csv");
            numSamples = sampleIDs.Count();
            label7.Text = sampleIDs.Count().ToString() + " Samples";
            label7.Visible = true;
            label2.Visible = false;
            label3.Visible = false;
            button1.Visible = false;

            filters = new List<Filter>();
            selections = new List<PipelineSelection>();

            string[] categories = Directory.GetDirectories(cancerDir);

            foreach (string category in categories)
            {
                string cname = Path.GetFileName(category);

                string catDir = cancerDir + "\\" + cname;

                string[] pipelines = Directory.GetDirectories(catDir);

                if (pipelines.Length > 0)
                {
                    node = treeView1.Nodes.Add(cname);
                    node.Name = "*" + cname;

                    if (first == null)
                    {
                        first = node;
                    }

                    foreach (string pipeline in pipelines)
                    {
                        string pname = Path.GetFileName(pipeline);
                        node.Nodes.Add(pname.Substring(5)).Name = pipeline;
                        selections.Add(new PipelineSelection(pipeline));
                    }
                }
            }

            node = treeView1.Nodes.Add("Filters");
            node.Name = "#Filters";
            filterNode = node;

            foreach (TreeNode n in treeView1.Nodes)
            {
                n.Expand();
            }

            if (first != null)
            {
                treeView1.SelectedNode = first;
            }

            comboBox1.SelectedIndex = comboBox2.SelectedIndex;
             * */
        }

        private void fixCancerType()
        {
            if (comboBox1.Enabled)
            {
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
            }

            button1.Text = "Forget All";
            button1.Visible = true;
        }

        private void getSizes(string pipeline, ref int all, ref int m30, ref int m60)
        {
            FileInfo f = new FileInfo(pipeline + "\\AllValues.faf");
            all = (int)(f.Length) / 100;

            f = new FileInfo(pipeline + "\\AllValues30.faf");
            m30 = (int)(f.Length) / 100;

            f = new FileInfo(pipeline + "\\AllValues60.faf");
            m60 = (int)(f.Length) / 100;
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Bounds.Contains(e.Location) == false)
            {
                return;
            }

            showData(e.Node.Name);
        }

        private void showData(string nodeName)
        {
            if (nodeName.StartsWith("*"))  // Category node
            {
                showPipelines(nodeName.Substring(1));
                label6.Visible = false;
                button57.Visible = false;
            }
            else if (nodeName.StartsWith("#F"))  // Filters
            {
                showFilters();
                label6.Visible = false;
                button57.Visible = false;
            }
            else if (nodeName.StartsWith("#S"))  // Filters
            {
                showSearch();
                label6.Visible = false;
                button57.Visible = false;
            }
            else
            {
                dataGridView1.Visible = false;
                textBox1.Visible = false;
                button72.Visible = false;
                label13.Visible = false;
                listBox1.Visible = false;
                curNodeName = nodeName;
                label6.Text = Path.GetFileName(nodeName).Substring(pastCancerType);
                label6.Visible = true;
                button57.Visible = true;
                foreach (PipelineSelection ps in selections)
                {
                    if (ps.path.Equals(nodeName))
                    {
                        currentSelection = ps;
                    }
                }

                switch (currentSelection.status)
                {
                    case 0:
                        button57.Text = "Pipeline not Selected";
                        button57.BackColor = SystemColors.Control;
                        break;
                    case 1:
                        button57.Text = "Pipeline is Source Data";
                        button57.BackColor = Color.DarkSeaGreen;
                        break;
                    case 2:
                        button57.Text = "Pipeline is Target Data";
                        button57.BackColor = Color.LightSteelBlue;
                        break;
                }

                filterPercent = 0;
                rbChangedProgrammatically = true;
                radioButton1.Select();
                rbChangedProgrammatically = false;
                newFilterPercent();
            }
        }

        private void newFilterPercent()
        {
            string faf = getCurrentFaf();

            FileInfo f = new FileInfo(faf);

            curNumMeasures = (int)(f.Length) / 100;
            if (searchIndex == -1)
            {
                curMeasureOffset = 0;
            }
            else
            {
                curMeasureOffset = searchIndex;
                searchIndex = -1;
            }

            showMeasures();
        }

        private string getCurrentFaf()
        {
            string faf = curNodeName + "\\AllValues.faf";

            switch (filterPercent)
            {
                case 30:
                    faf = curNodeName + "\\AllValues30.faf";
                    break;
                case 60:
                    faf = curNodeName + "\\AllValues60.faf";
                    break;
            }

            return (faf);
        }

        private string getBarFile(int measure)
        {
            string bar = curNodeName + "\\V_" + measure.ToString().PadLeft(8, '0') + ".bar";

            return (bar);
        }

        private void showMeasures()
        {
            if (filterPercent == 100)  // Special case - Selected
            {
                showExceptions();
                return;
            }

            int numToShow = curNumMeasures - curMeasureOffset;

            if (numToShow > 50)
            {
                numToShow = 50;
            }

            label4.Text = (curMeasureOffset + 1).ToString() + "  to  " +
                (curMeasureOffset + numToShow).ToString() + "  of  " + curNumMeasures.ToString();

            for (int i = 0; i < numToShow; i++)
            {
                showMeasure(i, curMeasureOffset + i);
            }

            setMbuttonsVisible(numToShow);

            if (curMeasureOffset > 0)
            {
                button52.Enabled = true;
            }
            else
            {
                button52.Enabled = false;
            }

            if (curMeasureOffset > 49)
            {
                button53.Enabled = true;
            }
            else
            {
                button53.Enabled = false;
            }

            if (curMeasureOffset + 50 < curNumMeasures)
            {
                button54.Enabled = true;
            }
            else
            {
                button54.Enabled = false;
            }

            if (curMeasureOffset + 500 < curNumMeasures)
            {
                button55.Enabled = true;
            }
            else
            {
                button55.Enabled = false;
            }

            if (curMeasureOffset + 5000 < curNumMeasures)
            {
                button59.Enabled = true;
            }
            else
            {
                button59.Enabled = false;
            }

            if ((currentSelection.unSelectedExceptions.Count() > 0) ||
            (currentSelection.sourceExceptions.Count() > 0) ||
            (currentSelection.targetExceptions.Count() > 0))
            {
                radioButton4.ForeColor = Color.OrangeRed;
            }
            else
            {
                radioButton4.ForeColor = Color.Black;
            }

            label4.Visible = true;
            panel2.Visible = true;
            panel3.Visible = true;
            panel4.Visible = true;
        }

        private void showExceptions()
        {
            label4.Text = "Pipeline Exceptions";

            int stat0Count = currentSelection.unSelectedExceptions.Count();
            int stat1Count = currentSelection.sourceExceptions.Count();
            int stat2Count = currentSelection.targetExceptions.Count();

            int ofs = 0;
            for (int i = 0; i < stat0Count; i++)
            {
                showMeasure(ofs + i, currentSelection.unSelectedExceptions[i]);
            }

            ofs = stat0Count;
            for (int i = 0; i < stat1Count; i++)
            {
                showMeasure(ofs + i, currentSelection.sourceExceptions[i]);
            }

            ofs = stat0Count + stat1Count;
            for (int i = 0; i < stat2Count; i++)
            {
                showMeasure(ofs + i, currentSelection.targetExceptions[i]);
            }

            setMbuttonsVisible(stat0Count + stat1Count + stat2Count);

            if ((stat0Count > 0) || (stat1Count > 0) || (stat2Count > 0))
            {
                radioButton4.ForeColor = Color.OrangeRed;
            }
            else
            {
                radioButton4.ForeColor = Color.Black;
            }

            button52.Enabled = false;
            button53.Enabled = false;
            button54.Enabled = false;
            button55.Enabled = false;
            button59.Enabled = false;

            label4.Visible = true;
            panel2.Visible = true;
            panel3.Visible = true;
            panel4.Visible = true;
        }

        private void showMeasure(int index, int measure)
        {
            string faf = getCurrentFaf();
            string name = "";
            Int32 numEntries = 0, numSteps = 0, stepExp = 0, barFileIndex = 0;
            float lowThresh = 0, step = 0;

            getFafInfo(faf, measure, ref name, ref numEntries, ref lowThresh, ref step, ref numSteps, ref stepExp, ref barFileIndex);

            if (curNodeName.Contains("Probe_Expression"))
            {
                int pipePos = name.IndexOf("|");

                if (pipePos > 1)
                {
                    name = name.Substring(0, pipePos);
                }
            }

            float fontSize = 10.2f;

            mbuttons[index].Text = name + Environment.NewLine + "(" + numEntries.ToString() + ")";
            if (name.Length > 13)
            {
                if (name.Length > 17)
                {
                    fontSize = 6.0f;
                }
                else
                {
                    fontSize = 8.0f;
                }
            }

            mbuttons[index].Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            switch (currentSelection.measureStatus(measure))
            {
                case 0:
                    mbuttons[index].BackColor = SystemColors.Control;
                    break;
                case 1:
                    mbuttons[index].BackColor = Color.DarkSeaGreen;
                    break;
                case 2:
                    mbuttons[index].BackColor = Color.LightSteelBlue;
                    break;
            }
        }

        private void getFafInfo(string faf, int measure, ref string name, ref Int32 numEntries, ref float lowThresh, ref float step, ref Int32 numSteps, ref Int32 stepExp, ref Int32 barFileIndex)
        {
            BinaryReader b = new BinaryReader(File.Open(faf, FileMode.Open), Encoding.Unicode);
            int pos = 100 * measure;
            b.BaseStream.Seek(pos, SeekOrigin.Begin);

            char[] buf = b.ReadChars(38);
            name = new string(buf).Trim();
            numEntries = b.ReadInt32();
            lowThresh = b.ReadSingle();
            step = b.ReadSingle();
            numSteps = b.ReadInt32();
            stepExp = b.ReadInt32();
            barFileIndex = b.ReadInt32();
            b.Close();
        }

        private void showPipelines(string cname)
        {
            setMbuttonsVisible(0);
            dataGridView1.Visible = true;
            label4.Text = "Pipelines for " + cname;
            label4.Visible = true;
            panel2.Visible = true;
            panel3.Visible = false;
            panel4.Visible = false;

            DataGridViewCell cell = new DataGridViewTextBoxCell();

            cell.Style.BackColor = Color.LightGray;
            cell.Style.SelectionBackColor = Color.LightGray;
            cell.Style.SelectionForeColor = Color.Black;
            cell.Style.Font = new Font("Times New Roman", 14);
            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewCell cell2 = new DataGridViewTextBoxCell();

            cell2.Style.BackColor = Color.LightGray;
            cell2.Style.SelectionBackColor = Color.LightGray;
            cell2.Style.SelectionForeColor = Color.Black;
            cell2.Style.Font = new Font("Times New Roman", 14);
            cell2.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            dataGridView1.ColumnCount = 4;
            dataGridView1.RowHeadersWidth = 200;
            dataGridView1.RowHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14);

            dataGridView1.Columns[0].Name = "Pipeline";
            dataGridView1.Columns[1].Name = "All";
            dataGridView1.Columns[2].Name = "> 30%";
            dataGridView1.Columns[3].Name = "> 60%";
            dataGridView1.Columns[0].CellTemplate = cell2;
            dataGridView1.Columns[1].CellTemplate = cell;
            dataGridView1.Columns[2].CellTemplate = cell;
            dataGridView1.Columns[3].CellTemplate = cell;
            dataGridView1.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].Width = 435;
            dataGridView1.Columns[1].Width = 100;
            dataGridView1.Columns[2].Width = 100;
            dataGridView1.Columns[3].Width = 100;

            string[] pipelines = Directory.GetDirectories(cancerDir + "\\" + cname);
            string[] row = new string[4];

            foreach (string pipeline in pipelines)
            {
                string pname = Path.GetFileName(pipeline);

                int all = 100, m30 = 30, m60 = 60;
                getSizes(pipeline, ref all, ref m30, ref m60);

                row[0] = pname.Substring(pastCancerType);
                row[1] = all.ToString();
                row[2] = m30.ToString();
                row[3] = m60.ToString();
                dataGridView1.Rows.Add(row);
            }

            dataGridView1.CurrentCell = null;
            dataGridView1.ColumnHeadersVisible = true;
            dataGridView1.RowHeadersVisible = false;
        }

        private void showFilters()
        {
            textBox1.Visible = false;
            button72.Visible = false;
            label13.Visible = false;
            listBox1.Visible = false;
            setMbuttonsVisible(0);
            dataGridView1.Visible = true;
            label4.Text = "Current FIlters";
            label4.Visible = true;
            panel2.Visible = true;
            panel3.Visible = false;
            panel4.Visible = false;

            DataGridViewCell cell = new DataGridViewTextBoxCell();

            cell.Style.BackColor = Color.LightGray;
            cell.Style.SelectionBackColor = Color.LightGray;
            cell.Style.SelectionForeColor = Color.Black;
            cell.Style.Font = new Font("Times New Roman", 14);
            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewCell cell2 = new DataGridViewTextBoxCell();

            cell2.Style.BackColor = Color.LightGray;
            cell2.Style.SelectionBackColor = Color.LightGray;
            cell2.Style.SelectionForeColor = Color.Black;
            cell2.Style.Font = new Font("Times New Roman", 14);
            cell2.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            dataGridView1.ColumnCount = 2;
            dataGridView1.RowHeadersWidth = 200;
            dataGridView1.RowHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14);

            dataGridView1.Columns[0].Name = "Pipeline";
            dataGridView1.Columns[1].Name = "Measure";
            dataGridView1.Columns[0].CellTemplate = cell2;
            dataGridView1.Columns[1].CellTemplate = cell;
            dataGridView1.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].Width = 435;
            dataGridView1.Columns[1].Width = 300;

            string[] row = new string[2];

            foreach (Filter filter in filters)
            {
                string pname = Path.GetFileName(filter.pipeline);
                string faf = filter.pipeline + "\\AllValues.faf";

                string name = "";
                Int32 numEntries = 0, numSteps = 0, stepExp = 0, barFileIndex = 0;
                float lowThresh = 0, step = 0;

                getFafInfo(faf, filter.measure, ref name, ref numEntries, ref lowThresh, ref step, ref numSteps, ref stepExp, ref barFileIndex);

                row[0] = pname.Substring(pastCancerType);
                row[1] = name;
                dataGridView1.Rows.Add(row);
            }

            dataGridView1.CurrentCell = null;
            dataGridView1.ColumnHeadersVisible = true;
            dataGridView1.RowHeadersVisible = false;
        }


        private void showSearch()
        {
            dataGridView1.Visible = false;
            setMbuttonsVisible(0);
            textBox1.Visible = true;
            button72.Visible = true;
            label4.Text = "Text Search";
            listBox1.Visible = true;
            label13.Visible = true;
            label4.Visible = true;
            panel2.Visible = true;
            panel3.Visible = false;
            panel4.Visible = false;
        }

        private void measureButton_Click(object sender, EventArgs e)
        {
            int idx = (Int32)(((Button)sender).Tag);

            string faf = getCurrentFaf();
            int measure;

            int stat0Count = currentSelection.unSelectedExceptions.Count();
            int stat1Count = currentSelection.sourceExceptions.Count();
            int stat2Count = currentSelection.targetExceptions.Count();

            if (filterPercent == 100)
            {
                if (idx < stat0Count)
                {
                    measure = currentSelection.unSelectedExceptions[idx];
                }
                else if (idx < stat0Count + stat1Count)
                {
                    measure = currentSelection.sourceExceptions[idx - stat0Count];
                }
                else
                {
                    measure = currentSelection.targetExceptions[idx - (stat0Count + stat1Count)];
                }
            }
            else
            {
                measure = curMeasureOffset + idx;
            }

            string name = "";
            Int32 numEntries = 0, numSteps = 0, stepExp = 0, barFileIndex = 0;
            float lowThresh = 0, step = 0;
            getFafInfo(faf, measure, ref name, ref numEntries, ref lowThresh, ref step, ref numSteps, ref stepExp, ref barFileIndex);

            MeasureParams mp = new MeasureParams();
            mp.barFile = getBarFile(barFileIndex);
            mp.name = name;
            mp.numEntries = numEntries;
            mp.lowThresh = lowThresh;
            mp.step = step;
            mp.numSteps = numSteps;
            mp.stepExp = stepExp;

            Filter filter = null;
            string pipeline = Path.GetDirectoryName(faf);

            foreach (Filter f in filters)
            {
                if (f.pipeline.Equals(pipeline) && (f.measure == measure))
                {
                    filter = f;
                }
            }

            int status = currentSelection.measureStatus(measure);

            MeasureForm measureForm = new MeasureForm(mp, filter, status, evolutionStarted);


            measureForm.ShowDialog();

            List<int> newList = measureForm.curList;
            if (filter == null)
            {
                if (newList.Count() > 0)
                {
                    Filter nf = new Filter();
                    nf.pipeline = pipeline;
                    nf.measure = measure;
                    nf.excluded = newList;
                    filters.Add(nf);
                    filterNode.ForeColor = Color.DarkRed;
                    fixCancerType();
                }
            }
            else
            {
                if (newList.Count() > 0)
                {
                    filter.excluded = newList;
                }
                else
                {
                    filters.Remove(filter);

                    if (filters.Count() == 0)
                    {
                        filterNode.ForeColor = Color.Black;
                    }
                }
            }

            if (measureForm.selectionStatus != status)
            {
                currentSelection.setMeasureStatus(measure, measureForm.selectionStatus);
                showMeasures();
                if ((currentSelection.unSelectedExceptions.Count() > 0) ||
                (currentSelection.sourceExceptions.Count() > 0) ||
                (currentSelection.targetExceptions.Count() > 0))
                {
                    radioButton4.ForeColor = Color.OrangeRed;
                }
                else
                {
                    radioButton4.ForeColor = Color.Black;
                }

                fixCancerType();
            }
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (rbChangedProgrammatically)
            {
                return;
            }
            // Show all
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                filterPercent = 0;
                newFilterPercent();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            // > 30%
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                filterPercent = 30;
                newFilterPercent();
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            // > 60%
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                filterPercent = 60;
                newFilterPercent();
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            // Selected
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                filterPercent = 100;
                newFilterPercent();
            }
        }

        private void button52_Click(object sender, EventArgs e)
        {
            // Back to start
            curMeasureOffset = 0;
            showMeasures();
        }

        private void button53_Click(object sender, EventArgs e)
        {
            // Back 50
            curMeasureOffset -= 50;
            showMeasures();
        }

        private void button54_Click(object sender, EventArgs e)
        {
            // Forward 50
            curMeasureOffset += 50;
            showMeasures();
        }

        private void button55_Click(object sender, EventArgs e)
        {
            // Forward 500
            curMeasureOffset += 500;
            showMeasures();
        }

        private void button59_Click(object sender, EventArgs e)
        {
            // Forward 5000
            curMeasureOffset += 5000;
            showMeasures();
        }

        private void button56_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            panel7.Visible = true;
            if (evolutionStarted == false)
            {
                prepareEvolution();
            }
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
                string digits = f.ToString();
                string ret;
                int mult = -stepExp;

                int dpt = digits.IndexOf(".");

                if (dpt == -1)
                {
                    ret = digits + ".";
                    for (int i = 0; i < mult; i++)
                    {
                        ret += "0";
                    }
                }
                else
                {
                    ret = digits.Substring(0,dpt);
                    ret += ".";
                    int idx = dpt + 1;

                    for (int i = 0; i < mult; i++)
                    {
                        if (digits.Length <= idx + i)
                        {
                            ret += "0";
                        }
                        else
                        {
                        ret += digits[idx + i];
                        }
                    }
                }

                return (ret);
            }
        }

        private void button57_Click(object sender, EventArgs e)
        {
            switch (currentSelection.status)
            {
                case 0:
                    button57.Text = "Pipeline is Source Data";
                    button57.BackColor = Color.DarkSeaGreen;
                    treeView1.SelectedNode.ForeColor = Color.MediumSeaGreen;
                    currentSelection.setPipelineStatus(1);
                    break;

                case 1:
                    button57.Text = "Pipeline is Target Data";
                    button57.BackColor = Color.LightSteelBlue;
                    treeView1.SelectedNode.ForeColor = Color.CornflowerBlue;
                    currentSelection.setPipelineStatus(2);
                    break;

                case 2:
                    button57.Text = "Pipeline not Selected";
                    button57.BackColor = SystemColors.Control;
                    treeView1.SelectedNode.ForeColor = Color.Black;
                    currentSelection.setPipelineStatus(0);
                    break;
            }

            fixCancerType();
            showMeasures();
        }

        private List<string> loadSampleList(string sampleFilename)
        {
            List<string> samples = new List<string>();

            if (File.Exists(sampleFilename) == false)
            {
                return (null);
            }

            string line;
            int idx = 0;

            StreamReader reader = new StreamReader(sampleFilename);
            while ((line = reader.ReadLine()) != null)
            {
                samples.Add(line);
                idx++;
            }

            reader.Close();

            return (samples);
        }

        private void button58_Click(object sender, EventArgs e)
        {
            panel7.Visible = false;
            panel1.Visible = true;
            label14.Text = "Selected Leader";
        }

        private void button60_Click(object sender, EventArgs e)
        {
            if (button60.Text == "Start Evolution")
            {
                leaderBase = -1;
                button74.Visible = false;

                if (evolutionReady == false)
                {
                    MessageBox.Show("Both sources and targets must be selected first");
                    return;
                }

                pauseFlag = false;
                evolutionStarted = true;
                button57.Enabled = false;
                button60.BackColor = Color.YellowGreen;
                showLeaderChart = false;
                panel12.Invalidate();
                button61.Text = "Simple Leaders";

                button60.Text = "Pause Evolution";
                while (pauseFlag == false)
                {
                    population.doGenerations(1, 0);
                    Application.DoEvents();

                    if (timerTicked)
                    {
                        showLeaders();
                        if (u.passedThreshold)
                        {
                            panel10.Invalidate();
                            u.passedThreshold = false;
                        }

                        label11.Text = u.numOrganisms.ToString();
                        label17.Text = u.cache.percentCached().ToString() + "% Cached";
                        timerTicked = false;
                    }
                }
                showLeaders();
                panel10.Invalidate();
                label11.Text = u.numOrganisms.ToString();
                button61.Text = "SAVE LEADERS";

                button60.Text = "Start Evolution";
            }
            else if (button60.Text == "Pause Evolution")
            {
                pauseFlag = true;
                button60.BackColor = Color.DarkOrange;
            }
        }

        private void prepareEvolution()
        {
            u = new ACEUniverse();
            population = new Population(u);

            Boolean noPipelineTargets = true;

            u.numSamples = numSamples;

            u.numSourceMeasures = 0;
            u.numTargetMeasures = 0;
            u.correlationType = comboBox3.SelectedIndex;

            u.brafMode = brafMode;

            comboBox4.Visible = false;

            for (int p = 0; p < selections.Count(); p++)
            {
                if (selections[p].status == 2)
                {
                    noPipelineTargets = false;
                }
            }

            u.allTargets = false;

            if (checkBox1.Checked)
            {
                if (noPipelineTargets)
                {
                    u.allTargets = true;
                }
                else
                {
                    MessageBox.Show("All Targets mode not possible when Pipeline Target is selected - ignoring it");
                }
            }

            if (u.allTargets)
            {
                u.evoSummary = "Correlation with a set of Targets" + Environment.NewLine;
            }
            else
            {
                u.evoSummary = "Standard correlation" + Environment.NewLine;
            }

            switch (u.correlationType)
            {
                case 0:
                    u.evoSummary += "Counting both positive and negative correlation" + Environment.NewLine;
                    break;
                case 1:
                    u.evoSummary += "Counting positive correlation only" + Environment.NewLine;
                    label15.Text += " (+ only)";
                    break;
                case 2:
                    u.evoSummary += "Counting negative correlation only" + Environment.NewLine;
                    label15.Text += " (- only)";
                    break;
            }

            for (int p = 0; p < selections.Count(); p++)
            {
                int all = 100, m30 = 30, m60 = 60;
                getSizes(selections[p].path, ref all, ref m30, ref m60);

                switch (selections[p].status)
                {
                    case 0:  // Not selected
                        u.numSourceMeasures += selections[p].sourceExceptions.Count();
                        u.numTargetMeasures += selections[p].targetExceptions.Count();
                        break;
                    case 1:  // Source
                        u.evoSummary += "Pipeline " + selections[p].path + " set as Source" + Environment.NewLine;
                        u.numSourceMeasures += all;
                        u.numSourceMeasures -= selections[p].unSelectedExceptions.Count();
                        u.numSourceMeasures -= selections[p].targetExceptions.Count();
                        u.numTargetMeasures += selections[p].targetExceptions.Count();

                        break;
                    case 2:  // Target
                        u.evoSummary += "Pipeline " + selections[p].path + " set as Target" + Environment.NewLine;
                        u.numTargetMeasures += all;
                        u.numTargetMeasures -= selections[p].unSelectedExceptions.Count();
                        u.numTargetMeasures -= selections[p].sourceExceptions.Count();
                        u.numSourceMeasures += selections[p].sourceExceptions.Count();
                        break;
                }

                if (selections[p].unSelectedExceptions.Count() > 0)
                {
                    u.evoSummary += "Selection exceptions for pipeline " + selections[p].path + Environment.NewLine;
                    string fafFile = selections[p].path + "\\AllValues.faf";
                    for (int ex = 0; ex < selections[p].unSelectedExceptions.Count(); ex++)
                    {
                        int val = selections[p].unSelectedExceptions[ex];
                        string name = "";
                        Int32 numEntries = 0, numSteps = 0, stepExp = 0, barFileIndex = 0;
                        float lowThresh = 0, step = 0;

                        getFafInfo(fafFile, val, ref name, ref numEntries, ref lowThresh, ref step, ref numSteps, ref stepExp, ref barFileIndex);
                        u.evoSummary += "Unselected : " + name + Environment.NewLine;
                    }
                }

                if (selections[p].sourceExceptions.Count() > 0)
                {
                    u.evoSummary += "Source exceptions for pipeline " + selections[p].path + Environment.NewLine;
                    string fafFile = selections[p].path + "\\AllValues.faf";
                    for (int ex = 0; ex < selections[p].sourceExceptions.Count(); ex++)
                    {
                        int val = selections[p].sourceExceptions[ex];
                        string name = "";
                        Int32 numEntries = 0, numSteps = 0, stepExp = 0, barFileIndex = 0;
                        float lowThresh = 0, step = 0;

                        getFafInfo(fafFile, val, ref name, ref numEntries, ref lowThresh, ref step, ref numSteps, ref stepExp, ref barFileIndex);
                        u.evoSummary += "Source : " + name + Environment.NewLine;
                    }
                }
                if (selections[p].targetExceptions.Count() > 0)
                {
                    u.evoSummary += "Target exceptions for pipeline " + selections[p].path + Environment.NewLine;
                    string fafFile = selections[p].path + "\\AllValues.faf";
                    for (int ex = 0; ex < selections[p].targetExceptions.Count(); ex++)
                    {
                        int val = selections[p].targetExceptions[ex];
                        string name = "";
                        Int32 numEntries = 0, numSteps = 0, stepExp = 0, barFileIndex = 0;
                        float lowThresh = 0, step = 0;

                        getFafInfo(fafFile, val, ref name, ref numEntries, ref lowThresh, ref step, ref numSteps, ref stepExp, ref barFileIndex);
                        u.evoSummary += "Target : " + name + Environment.NewLine;
                    }
                }
            }

            u.cache = new Cache(selections, u.numSourceMeasures, u.numTargetMeasures, 100000);

            if ((u.numSourceMeasures == 0) || (u.numTargetMeasures == 0))
            {
                button60.BackColor = Color.Red;

                return;
            }

            button60.BackColor = Color.DarkOrange;

            evolutionReady = true;

            u.coverageX = 20;
            u.coverageY = 50;

            u.coverage = new int[u.coverageX][];

            for (int i = 0; i < u.coverageX; i++)
            {
                u.coverage[i] = new int[u.coverageY];

                for (int j = 0; j < u.coverageY; j++)
                {
                    u.coverage[i][j] = 0;
                }
            }

            u.numOrganisms = 0;

            int nextSource = 0;
            int nextTarget = 0;

            for (int p = 0; p < selections.Count(); p++)
            {
                int all = 100, m30 = 30, m60 = 60;
                getSizes(selections[p].path, ref all, ref m30, ref m60);
                List<int> allExceptions;

                switch (selections[p].status)
                {
                    case 0:  // Not selected
                        foreach (int measure in selections[p].sourceExceptions)
                        {
                            u.cache.setSourceMeasureCode(nextSource++, (p * 10000000) + measure);
                        }

                        foreach (int measure in selections[p].targetExceptions)
                        {
                            u.cache.setTargetMeasureCode(nextTarget++, (p * 10000000) + measure);
                        }
                        break;
                    case 1:  // Source
                        allExceptions = new List<int>();

                        foreach (int measure in selections[p].unSelectedExceptions)
                        {
                            allExceptions.Add(measure);
                        }

                        foreach (int measure in selections[p].targetExceptions)
                        {
                            u.cache.setTargetMeasureCode(nextTarget++, (p * 10000000) + measure);
                            allExceptions.Add(measure);
                        }

                        if (allExceptions.Count() > 0)
                        {
                            allExceptions.Sort();

                            for (int e = 0; e < allExceptions.Count(); e++)
                            {
                                int first;
                                if (e == 0)
                                {
                                    first = 0;
                                }
                                else
                                {
                                    first = allExceptions[e - 1] + 1;
                                }

                                for (int measure = first; measure < allExceptions[e]; measure++)
                                {
                                    u.cache.setSourceMeasureCode(nextSource++, (p * 10000000) + measure);
                                }
                            }

                            for (int measure = allExceptions[allExceptions.Count() - 1] + 1; measure < all; measure++)
                            {
                                u.cache.setSourceMeasureCode(nextSource++, (p * 10000000) + measure);
                            }
                        }
                        else
                        {
                            for (int measure = 0; measure < all; measure++)
                            {
                                u.cache.setSourceMeasureCode(nextSource++, (p * 10000000) + measure);
                            }
                        }
                        break;
                    case 2:  // Target
                        allExceptions = new List<int>();

                        foreach (int measure in selections[p].unSelectedExceptions)
                        {
                            allExceptions.Add(measure);
                        }

                        foreach (int measure in selections[p].sourceExceptions)
                        {
                            u.cache.setSourceMeasureCode(nextSource++, (p * 10000000) + measure);
                            allExceptions.Add(measure);
                        }

                        if (allExceptions.Count() > 0)
                        {
                            allExceptions.Sort();

                            for (int e = 0; e < allExceptions.Count(); e++)
                            {
                                int first;
                                if (e == 0)
                                {
                                    first = 0;
                                }
                                else
                                {
                                    first = allExceptions[e - 1] + 1;
                                }

                                for (int measure = first; measure < allExceptions[e]; measure++)
                                {
                                    u.cache.setTargetMeasureCode(nextTarget++, (p * 10000000) + measure);
                                }
                            }

                            for (int measure = allExceptions[allExceptions.Count() - 1] + 1; measure < all; measure++)
                            {
                                u.cache.setTargetMeasureCode(nextTarget++, (p * 10000000) + measure);
                            }
                        }
                        else
                        {
                            for (int measure = 0; measure < all; measure++)
                            {
                                u.cache.setTargetMeasureCode(nextTarget++, (p * 10000000) + measure);
                            }
                        }
                        break;
                }
            }

            u.cache.okAfterFilters = new Boolean[u.numSamples];

            for (int i = 0; i < u.numSamples; i++)
            {
                u.cache.okAfterFilters[i] = true;
            }

            foreach (Filter f in filters)
            {
                applyFilter(f, ref u.cache.okAfterFilters);
            }

            label15.Text = u.numSourceMeasures.ToString() + " Source Measures, " + u.numTargetMeasures.ToString() + " Target Measures";

            switch (u.correlationType)
            {
                case 0:
                    break;
                case 1:
                    label15.Text += " (+ only)";
                    break;
                case 2:
                    label15.Text += " (- only)";
                    break;
            }

            int okCount = 0;
            for (int i = 0; i < numSamples; i++)
            {
                if (u.cache.okAfterFilters[i])
                {
                    okCount++;
                }
            }

            if (u.allTargets)
            {
                comboBox4.Items.Clear();

                for (int t = 0; t < u.numTargetMeasures; t++)
                {
                    string targetName = ""; ;
                    bool isTargetEnumerated = false;
                    float[] targetData = null;
                    u.cache.getTargetMeasureInfo(t, ref targetName, ref isTargetEnumerated, ref targetData, 0, u.brafMode);
                    comboBox4.Items.Add(targetName);
                }

                comboBox4.SelectedIndex = 0;
            }

            label16.Text = okCount.ToString() + " Samples after Filtering";

            showLeaderChart = false;
        }

        private void showLeaders()
        {
            Correlation[] leadersToShow = new Correlation[10];
            leaderIndex = new int[10];

            int nextLeader = leaderBase;

            for (int i = 0; i < 10; i++)
            {
                do
                {
                    nextLeader++;
                    leadersToShow[i] = (Correlation)(population.getLeader(nextLeader));
                    leaderIndex[i] = nextLeader;
                }
                while (existsAlready(leadersToShow, i));
            }

            label20.Text = (leaderBase + 2).ToString() + " to " + (leaderBase + 11).ToString();
            if (leaderBase < 1)
            {
                button74.Visible = false;
            }
            else
            {
                button74.Visible = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Color bcolor = Color.LightGray;

                Correlation c = leadersToShow[i];

                button73.Visible = true;

                if (c == null)
                {
                    leaderbuttons[i].Text = "";
                    leaderbuttons[i].Visible = false;
                    button73.Visible = false;
                    continue;
                }
                else
                {
                    leaderbuttons[i].Visible = true;
                }

                string sourceTstring = "";
                string targetTstring = "";

                if (c.sourceTransformation == 1)
                {
                    sourceTstring = " (l)";
                }
                else if (c.sourceTransformation == 2)
                {
                    sourceTstring = " (s)";
                }
                else if (c.sourceTransformation == 3)
                {
                    sourceTstring = " (a)";
                }

                if (c.targetTransformation == 1)
                {
                    targetTstring = " (l)";
                }
                else if (c.targetTransformation == 2)
                {
                    targetTstring = " (s)";
                }
                else if (c.targetTransformation == 3)
                {
                    targetTstring = " (a)";
                }

                string sourceName = c.sourceName;
                string targetName = c.targetName;

                if (u.cache.getSourcePipeline(c.sourceMeasure).Contains("Probe_Expression"))
                {
                    int pipeOffset = sourceName.IndexOf("|");

                    if (pipeOffset > 1)
                    {
                        sourceName = sourceName.Substring(0, pipeOffset);
                    }
                }

                if (u.cache.getTargetPipeline(c.targetMeasure).Contains("Probe_Expression"))
                {
                    int pipeOffset = targetName.IndexOf("|");

                    if (pipeOffset > 1)
                    {
                        targetName = targetName.Substring(0, pipeOffset);
                    }
                }

                if (u.allTargets)
                {
                    leaderbuttons[i].Text = sourceName + sourceTstring;
                }
                else
                {
                    leaderbuttons[i].Text = sourceName + sourceTstring + "  >>>  " + targetName + targetTstring;
                }

                if (c.rSquared < 0)
                {
                    bcolor = Color.FromArgb(200, 200, 200);
                }
                else if (c.rSquared > 0.5)
                {
                    bcolor = Color.FromArgb(0, 200, 0);
                }
                else
                {
                    int reduction = (int)(c.rSquared * 200);
                    if (reduction > 200)
                    {
                        reduction = 200;
                    }
                    if (reduction < 0)
                    {
                        reduction = 0;
                    }
                    bcolor = Color.FromArgb(200 - reduction, 200, 200 - reduction);
                }

                leaderbuttons[i].BackColor = bcolor;
            }

            setLeaderButtonsVisible(true);
        }

        private Boolean existsAlready(Correlation[] corrs, int index)
        {
            if (corrs[index] == null)
            {
                return (false);
            }

            for (int other = 0; other < index; other++)
            {
                if (corrs[index].isDuplicate(corrs[other]))
                {
                    return (true);
                }
            }

            return (false);
        }

        private void applyFilter(Filter filter, ref Boolean[] flags)
        {
            string barFile = filter.pipeline + "\\V_" + filter.measure.ToString().PadLeft(8, '0') + ".bar";
            Boolean isEnumerated = false;

            if (File.Exists(filter.pipeline + "\\V_" + filter.measure.ToString().PadLeft(8, '0') + ".evl"))
            {
                isEnumerated = true;
            }

            FileInfo f = new FileInfo(barFile);

            int size = (int)(f.Length);
            byte[] valByteArray = new byte[size];
            float[] valArray = new float[size / sizeof(float)];

            BinaryReader b = new BinaryReader(File.Open(barFile, FileMode.Open));
            b.Read(valByteArray, 0, size);
            b.Close();

            Buffer.BlockCopy(valByteArray, 0, valArray, 0, size);

            string name = "";
            Int32 numEntries = 0, numSteps = 0, stepExp = 0, barFileIndex = 0;
            float lowThresh = 0, step = 0;
            getFafInfo(filter.pipeline + "\\AllValues.faf", filter.measure, ref name, ref numEntries, ref lowThresh, ref step, ref numSteps, ref stepExp, ref barFileIndex);

            MeasureParams mp = new MeasureParams();
            //mp.barFile = getBarFile(measure);
            mp.name = name;
            mp.numEntries = numEntries;
            mp.lowThresh = lowThresh;
            mp.step = step;
            mp.numSteps = numSteps;
            mp.stepExp = stepExp;

            for (int v = 0; v < u.numSamples; v++)
            {
                if (valArray[v] == float.MinValue)
                {
                    flags[v] = false;
                }
                else if (filter.excluded.Contains(getBarIndex(valArray[v], isEnumerated, lowThresh, step, numSteps)))
                {
                    flags[v] = false;
                }
            }
        }

        private int getBarIndex(float val, Boolean isEnumerated, float lowThresh, float step, int numSteps)
        {
            if (isEnumerated)
            {
                return ((int)val);
            }
            else
            {
                for (int i = 0; i < numSteps; i++)
                {
                    if (val < lowThresh + (step * i))
                    {
                        return (i);
                    }
                }

                return (numSteps);
            }
        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            paintProgressPanel(e);
        }

        private void paintProgressPanel(PaintEventArgs e)
        {
            if (evolutionStarted == false)
            {
                return;
            }

            int xwidth = (panel10.Width / u.coverage.Length) + 1;
            int ywidth = (panel10.Height / u.coverage[0].Length) + 1;

            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    int oneToOne = u.numSourceMeasures * u.numTargetMeasures / 1000;
                    if (oneToOne == 0)
                    {
                        oneToOne = 1;
                    }

                    int scaledCoverage = ((u.coverage[x][y] * 511)/ oneToOne) - 255;

                    if (scaledCoverage < -255)
                    {
                        scaledCoverage = -255;
                    }

                    if (scaledCoverage > 255)
                    {
                        scaledCoverage = 255;
                    }

                    paintProgressSquare(e, scaledCoverage, x * xwidth, y * ywidth, xwidth, ywidth);
                }
            }
        }

        private void paintProgressSquare(PaintEventArgs e, int coverage, int xofs, int yofs, int xsize, int ysize)
        {
            int r = 0;
            int g = 0;
            int b = 0;

            if (coverage < 0)
            {
                r = -coverage;
            }
            else
            {
                g = coverage;
            }

            Brush brush = new SolidBrush(Color.FromArgb(r, g, b));
            e.Graphics.FillEllipse(brush, new Rectangle(xofs, yofs, xsize, ysize));
        }

        private void leaderButton_Click(object sender, EventArgs e)
        {
            int idx = (Int32)(((Button)sender).Tag);

            Correlation c = null;

            if (comboBox4.Items.Count > 0)
            {
                comboBox4.SelectedIndex = 0;
            }

            c = (Correlation)(population.getLeader(leaderIndex[idx]));
            currentLeader = c;

            if (c == null)
            {
                return;
            }

            string sourceName = "", targetName = "";
            Boolean isSourceEnumerated = false, isTargetEnumerated = false;
            float[] sourceData = null, targetData = null;

            int targetMeasure;

            if (u.allTargets)
            {
                targetMeasure = 0;
                comboBox4.Visible = true;
            }
            else
            {
                targetMeasure = c.targetMeasure;
            }

            u.cache.getSourceMeasureInfo(c.sourceMeasure, ref sourceName, ref isSourceEnumerated, ref sourceData, c.sourceTransformation, u.brafMode);
            u.cache.getTargetMeasureInfo(targetMeasure, ref targetName, ref isTargetEnumerated, ref targetData, c.targetTransformation, u.brafMode);

            string sourceTstring = "";
            string targetTstring = "";

            if (c.sourceTransformation == 1)
            {
                sourceTstring = " (l)";
            }
            else if (c.sourceTransformation == 2)
            {
                sourceTstring = " (s)";
            }
            else if (c.sourceTransformation == 3)
            {
                sourceTstring = " (a)";
            }

            if (c.targetTransformation == 1)
            {
                targetTstring = " (l)";
            }
            else if (c.targetTransformation == 2)
            {
                targetTstring = " (s)";
            }
            else if (c.targetTransformation == 3)
            {
                targetTstring = " (a)";
            }

            if (u.cache.getSourcePipeline(c.sourceMeasure).Contains("Probe_Expression"))
            {
                int pipeOffset = sourceName.IndexOf("|");

                if (pipeOffset > 1)
                {
                    string firstBit = MeasureForm.scortUnconvert(sourceName.Substring(0, pipeOffset));
                    string geneBit = sourceName.Substring(pipeOffset + 1);

                    if (geneBit.Contains("---"))
                    {
                        sourceName = firstBit;
                    }
                    else
                    {
                        sourceName = firstBit + " (" + geneBit + ")";
                    }
                }
            }

            if (u.cache.getTargetPipeline(c.targetMeasure).Contains("Probe_Expression"))
            {
                int pipeOffset = targetName.IndexOf("|");

                if (pipeOffset > 1)
                {
                    string firstBit = MeasureForm.scortUnconvert(targetName.Substring(0, pipeOffset));
                    string geneBit = targetName.Substring(pipeOffset + 1);

                    if (geneBit.Contains("---"))
                    {
                        targetName = firstBit;
                    }
                    else
                    {
                        targetName = firstBit + " (" + geneBit + ")";
                    }
                }
            }

            scatterXLabel = sourceName + sourceTstring;
            scatterYLabel = targetName + targetTstring;

            scatterXPipeline = u.cache.getSourcePipeline(c.sourceMeasure);
            scatterYPipeline = u.cache.getTargetPipeline(targetMeasure);

            scatterX = new List<int>();
            scatterY = new List<int>();

            scatterMinX = float.MaxValue;
            scatterMaxX = float.MinValue;
            scatterMinY = float.MaxValue;
            scatterMaxY = float.MinValue;

            int okCount = 0;

            for (int i = 0; i < sourceData.Length; i++)
            {
                if (u.cache.okAfterFilters[i] && (sourceData[i] != float.MinValue) && (targetData[i] != float.MinValue))
                {
                    okCount++;

                    if (sourceData[i] < scatterMinX)
                    {
                        scatterMinX = sourceData[i];
                    }

                    if (sourceData[i] > scatterMaxX)
                    {
                        scatterMaxX = sourceData[i];
                    }

                    if (targetData[i] < scatterMinY)
                    {
                        scatterMinY = targetData[i];
                    }

                    if (targetData[i] > scatterMaxY)
                    {
                        scatterMaxY = targetData[i];
                    }
                }
            }

            double[] sd = new double[okCount];
            double[] td = new double[okCount];
            int didx = 0;

            for (int i = 0; i < sourceData.Length; i++)
            {
                if (u.cache.okAfterFilters[i] && (sourceData[i] != float.MinValue) && (targetData[i] != float.MinValue))
                {
                    scatterX.Add(calcScatterOffset(sourceData[i], scatterMinX, scatterMaxX));
                    scatterY.Add(calcScatterOffset(targetData[i], scatterMinY, scatterMaxY));
                    sd[didx] = sourceData[i];
                    td[didx++] = targetData[i];
                }
            }

            scatterA = c.regressionMetrics[0].lineSlope;
            scatterB = c.regressionMetrics[0].lineIntercept;
            scatterQ = c.regressionMetrics[0].rSquared;
            showLeaderChart = true;
            label14.Text = "Selected Leader (" + okCount.ToString() + ")";
            panel12.Invalidate();
        }

        private int calcScatterOffset(float val, float minVal, float maxVal)
        {
            float plus = val - minVal;
            float range = maxVal - minVal;
            return ((int)((plus * 10000) / range));
        }

        private void panel12_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            paintLeaderChart(e);
        }

        private void paintLeaderChart(PaintEventArgs e)
        {
            if (showLeaderChart == false)
            {
                return;
            }

            Brush textBrush = new SolidBrush(Color.DarkRed);
            Brush smallTextBrush = new SolidBrush(Color.Black);
            Font textFont = new Font("Microsoft Sans Serif", 14.0f);
            Font smallTextFont = new Font("Microsoft Sans Serif", 11.0f);
            Font tinyTextFont = new Font("Microsoft Sans Serif", 8.0f);
            int lhs = 64;
            int rhs = panel12.Width - 48;
            int top = 54;
            int bottom = panel12.Height - 68;
            Font xFont = calcFontScale(scatterXLabel);
            Font yFont = calcFontScale(scatterYLabel);

            e.Graphics.FillRectangle(new SolidBrush(Color.LightBlue), new Rectangle(lhs - 4, top - 4, rhs + 8 - lhs, bottom + 8 - top));

            SizeF stringSize1 = e.Graphics.MeasureString(scatterYLabel, yFont);

            if (u.allTargets)
            {
                e.Graphics.DrawString(scatterYPipeline, smallTextFont, smallTextBrush, lhs + 150, 14);
            }
            else
            {
                e.Graphics.DrawString(scatterYPipeline, smallTextFont, smallTextBrush, lhs + stringSize1.Width + 30, 14);
            }

            SizeF stringSize2 = e.Graphics.MeasureString(scatterXLabel, xFont);

            if (scatterXLabel.Contains("(") && (scatterXLabel.Length > 16))
            {
                stringSize2 = e.Graphics.MeasureString(scatterXLabel, tinyTextFont);
                e.Graphics.DrawString(scatterXLabel, tinyTextFont, textBrush, rhs - stringSize2.Width, bottom + 30);
            }
            else
            {
                e.Graphics.DrawString(scatterXLabel, textFont, textBrush, rhs - stringSize2.Width, bottom + 30);
            }

            SizeF stringSize3 = e.Graphics.MeasureString(scatterXPipeline, smallTextFont);
            e.Graphics.DrawString(scatterXPipeline, smallTextFont, smallTextBrush, rhs - (stringSize2.Width + stringSize3.Width + 50), bottom + 34);

            if (u.allTargets == false)
            {
                if (scatterYLabel.Contains("(") && (scatterYLabel.Length > 16))
                {
                    e.Graphics.DrawString(scatterYLabel, tinyTextFont, textBrush, 30, 10);
                }
                else
                {
                    e.Graphics.DrawString(scatterYLabel, textFont, textBrush, 30, 10);
                }
            }

            e.Graphics.DrawString(floatString(scatterMaxY), smallTextFont, smallTextBrush, 10, top);
            e.Graphics.DrawString(floatString(scatterMinY), smallTextFont, smallTextBrush, 10, bottom - 25);

            e.Graphics.DrawString(floatString(scatterMaxX), smallTextFont, smallTextBrush, rhs - 25, bottom + 10);
            e.Graphics.DrawString(floatString(scatterMinX), smallTextFont, smallTextBrush, lhs - 20, bottom + 10);

            e.Graphics.DrawString("Score : " + scatterQ.ToString(), textFont, smallTextBrush, rhs - 150, 10);

            Brush dotBrush = new SolidBrush(Color.DarkGreen);
            int dotRadius = 3;
            for (int i = 0; i < scatterX.Count(); i++)
            {
                e.Graphics.FillEllipse(dotBrush, new Rectangle(lhs + (((rhs - lhs) * scatterX[i]) / 10000) - dotRadius,
                                                        (bottom - (((bottom - top) * scatterY[i]) / 10000)) - dotRadius,
                                                                            dotRadius + dotRadius, dotRadius + dotRadius));
            }

            float xfStart = scatterMinX;
            float xfEnd = scatterMaxX;
            float yfStart = (scatterMinX * scatterA) + scatterB;
            float yfEnd = (scatterMaxX * scatterA) + scatterB;

            if (scatterA != 0)
            {
                if (yfStart > scatterMaxY)
                {
                    yfStart = scatterMaxY;
                    xfStart = (yfStart - scatterB) / scatterA;
                }

                if (yfStart < scatterMinY)
                {
                    yfStart = scatterMinY;
                    xfStart = (yfStart - scatterB) / scatterA;
                }

                if (yfEnd > scatterMaxY)
                {
                    yfEnd = scatterMaxY;
                    xfEnd = (yfEnd - scatterB) / scatterA;
                }

                if (yfEnd < scatterMinY)
                {
                    yfEnd = scatterMinY;
                    xfEnd = (yfEnd - scatterB) / scatterA;
                }
            }

            int xStart = calcScatterOffset(xfStart, scatterMinX, scatterMaxX);
            int xEnd = calcScatterOffset(xfEnd, scatterMinX, scatterMaxX);
            int yStart = calcScatterOffset(yfStart, scatterMinY, scatterMaxY);
            int yEnd = calcScatterOffset(yfEnd, scatterMinY, scatterMaxY);

            e.Graphics.DrawLine(new Pen(Color.RoyalBlue,3), lhs + (((rhs - lhs) * xStart) / 10000), bottom - (((bottom - top) * yStart) / 10000), lhs + (((rhs - lhs) * xEnd) / 10000), bottom - (((bottom - top) * yEnd) / 10000));
        }

        private Font calcFontScale(string label)
        {
            Font textFont = new Font("Microsoft Sans Serif", 14.0f);

            int maxBig = 16;
            int maxMedium = 24;
            int maxSmall = 36;

            if (label.Length > maxBig)
            {
                if (label.Length > maxMedium)
                {
                    if (label.Length > maxSmall)
                    {
                        textFont = new Font("Microsoft Sans Serif", 5.0f);
                    }
                    else
                    {
                        textFont = new Font("Microsoft Sans Serif", 8.0f);
                    }
                }
                else
                {
                    textFont = new Font("Microsoft Sans Serif", 11.0f);
                }
            }

            return (textFont);
        }

        private string floatString(float f)
        {
            string raw = f.ToString();
            int digits = 0;
            int dplace = 99999;

            for (int i = 0; i < raw.Length; i++)
            {
                if ((raw[i] >= '1') && (raw[i] <= '9'))
                {
                    digits++;
                }
                else if (raw[i] == '.')
                {
                    dplace = i;
                }
            }

            int toRemove = digits - 3;
            if ((toRemove > 0) && (dplace < (raw.Length - toRemove) - 1))
            {
                return (raw.Substring(0, raw.Length - toRemove));
            }

            return (raw);
        }

        private void button72_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                return;
            }

            string ss = textBox1.Text.ToUpper();
            char[] nameChars = new char[38];
            int found = 0;
            listBox1.Items.Clear();
            searchPaths = new List<string>();
            searchIndices = new List<int>();

            foreach (PipelineSelection ps in selections)
            {
                string faf = ps.path + "\\AllValues.faf";
                byte[] fafData = File.ReadAllBytes(faf);

                int chunks = fafData.Length / 100;

                for (int ch = 0; ch < chunks; ch++)
                {
                    int ofs = ch * 100;
                    Buffer.BlockCopy(fafData, ofs, nameChars, 0, 76);
                    string name = new string(nameChars).Trim().ToUpper();

                    if (name.Contains(ss))
                    {
                        string pipeline = Path.GetFileName(ps.path).Substring(pastCancerType);
                        listBox1.Items.Add(name + "   (" + pipeline + ")");
                        searchPaths.Add(ps.path);
                        searchIndices.Add(ch);
                        found++;
                    }
                }

                label13.Visible = true;
                label13.Text = found.ToString() + " found";
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb != null)
            {
                searchIndex = searchIndices[lb.SelectedIndex];
                filterPercent = 0;
                showData(searchPaths[lb.SelectedIndex]);
            }
        }

        private void button61_Click(object sender, EventArgs e)
        {
            // Save Leaders
            if (button61.Text == "SAVE LEADERS")
            {
                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                saveFileDialog1.InitialDirectory = "mydocs";
                saveFileDialog1.Filter = "csv files|*.csv";
                saveFileDialog1.Title = "Save results to file";
                saveFileDialog1.RestoreDirectory = false;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        saveToStream(myStream, false);
                        string fname = saveFileDialog1.FileName;
                        fname = fname.Substring(0, fname.Length - 3) + "txt";
                        System.IO.File.WriteAllText(fname, u.evoSummary);
                    }
                }
            }
        }

        private void saveToStream(Stream myStream, Boolean batch)
        {
            string resString = "Source Measure,Source Pipeine,Target Measure, Target Pipeline, RSquared, Line Intercept, Line Slope" + Environment.NewLine;

            Correlation[] leadersToShow = new Correlation[1200];

            int nextLeader = -1;

            for (int i = 0; i < 1200; i++)
            {
                do
                {
                    nextLeader++;
                    leadersToShow[i] = (Correlation)(population.getLeader(nextLeader));
                }
                while (existsAlready(leadersToShow, i));
            }

            for (int i = 0; i < 1200; i++)
            {
                Correlation c = leadersToShow[i];

                if (c == null)
                {
                    continue;
                }

                string sourcePipeline = u.cache.getSourcePipeline(c.sourceMeasure);
                string targetPipeline = u.cache.getTargetPipeline(c.targetMeasure);
                resString += c.sourceName;
                resString += ",";
                resString += sourcePipeline;
                resString += ",";
                resString += c.targetName;
                resString += ",";
                resString += targetPipeline;
                resString += ",";
                resString += c.rSquared.ToString();
                resString += ",";
                resString += c.regressionMetrics[0].lineIntercept.ToString();
                resString += ",";
                resString += c.regressionMetrics[0].lineSlope.ToString();
                resString += Environment.NewLine;
            }

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(resString);
            myStream.Write(bytes, 0, bytes.Length);
            myStream.Close();
        }

        private void makeScortGeneIndex()
        {
            scortDictionary = new Dictionary<string, string>();

            string[] cancerTypes = Directory.GetDirectories(startDir + "\\FAST");
            string probeFile = startDir + "\\FAST\\SCORT\\Gene Expression\\SCORT_Gene_Expression_Probes.txt";
            string geneFile = startDir + "\\FAST\\SCORT\\Gene Expression\\SCORT_Gene_Annotated.txt";

            StreamReader preader = new StreamReader(probeFile);
            StreamReader greader = new StreamReader(probeFile);
            string pline;
            string gline;
            char[] sep = new char[] { '\t' };

            while (((pline = preader.ReadLine()) != null) && ((gline = greader.ReadLine()) != null))
            {
                string[] pbits = pline.Split(sep);
                string[] gbits = gline.Split(sep);

                string probe = pbits[0];
                string gene = gbits[0];

                scortDictionary.Add(probe, gene);
            }

            preader.Close();
            greader.Close();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            Correlation c = currentLeader;

            if (c == null)
            {
                return;
            }

            string sourceName = "", targetName = "";
            Boolean isSourceEnumerated = false, isTargetEnumerated = false;
            float[] sourceData = null, targetData = null;

            int targetMeasure = comboBox4.SelectedIndex;

            u.cache.getSourceMeasureInfo(c.sourceMeasure, ref sourceName, ref isSourceEnumerated, ref sourceData, c.sourceTransformation, u.brafMode);
            u.cache.getTargetMeasureInfo(targetMeasure, ref targetName, ref isTargetEnumerated, ref targetData, c.targetTransformation, u.brafMode);

            scatterXLabel = sourceName;
            scatterYLabel = targetName;

            scatterXPipeline = u.cache.getSourcePipeline(c.sourceMeasure);
            scatterYPipeline = u.cache.getTargetPipeline(targetMeasure);

            scatterX = new List<int>();
            scatterY = new List<int>();

            scatterMinX = float.MaxValue;
            scatterMaxX = float.MinValue;
            scatterMinY = float.MaxValue;
            scatterMaxY = float.MinValue;

            int okCount = 0;

            for (int i = 0; i < sourceData.Length; i++)
            {
                if (u.cache.okAfterFilters[i] && (sourceData[i] != float.MinValue) && (targetData[i] != float.MinValue))
                {
                    okCount++;

                    if (sourceData[i] < scatterMinX)
                    {
                        scatterMinX = sourceData[i];
                    }

                    if (sourceData[i] > scatterMaxX)
                    {
                        scatterMaxX = sourceData[i];
                    }

                    if (targetData[i] < scatterMinY)
                    {
                        scatterMinY = targetData[i];
                    }

                    if (targetData[i] > scatterMaxY)
                    {
                        scatterMaxY = targetData[i];
                    }
                }
            }

            double[] sd = new double[okCount];
            double[] td = new double[okCount];
            int didx = 0;

            for (int i = 0; i < sourceData.Length; i++)
            {
                if (u.cache.okAfterFilters[i] && (sourceData[i] != float.MinValue) && (targetData[i] != float.MinValue))
                {
                    scatterX.Add(calcScatterOffset(sourceData[i], scatterMinX, scatterMaxX));
                    scatterY.Add(calcScatterOffset(targetData[i], scatterMinY, scatterMaxY));
                    sd[didx] = sourceData[i];
                    td[didx++] = targetData[i];
                }
            }

            scatterA = c.regressionMetrics[targetMeasure].lineSlope;
            scatterB = c.regressionMetrics[targetMeasure].lineIntercept;
            scatterQ = c.regressionMetrics[targetMeasure].rSquared;
            showLeaderChart = true;
            panel12.Invalidate();
        }

        private void button73_Click(object sender, EventArgs e)
        {
            leaderBase += 10;
            showLeaders();
        }

        private void button74_Click(object sender, EventArgs e)
        {
            leaderBase -= 10;
            showLeaders();
        }
    }
}

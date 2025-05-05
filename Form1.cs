using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OS
{
    public partial class Form1 : Form
    {
        private List<Process> processes = new List<Process>();
        private Panel ganttChartPanel;
        private Panel resultsPanel;
        private TableLayoutPanel inputTable;
        private Panel inputScrollPanel;
        private List<(string Process, int Start, int Duration)> ganttData = new List<(string, int, int)>();
        private NumericUpDown nudProcessCount, nudQuantum;
        private Button btnCalculate;
        private GroupBox gbInputs;

        public Form1()
        {
            InitializeComponent();
            this.Text = "🕒 Round Robin Scheduler Simulator";
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.WhiteSmoke;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1100, 750);

            // Header
            Label lblHeader = new Label
            {
                Text = "Round Robin Scheduler Simulator",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Teal,
                AutoSize = true,
                Location = new Point(30, 20)
            };
            this.Controls.Add(lblHeader);

            Label lblDesc = new Label
            {
                Text = "Easily visualize and analyze CPU scheduling!",
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(32, 60)
            };
            this.Controls.Add(lblDesc);

            // Input Group
            gbInputs = new GroupBox
            {
                Text = "Inputs",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(400, 340), // Increased height for more space
                Location = new Point(30, 100),
                BackColor = Color.White
            };
            this.Controls.Add(gbInputs);

            Label lblProc = new Label { Text = "Number of Processes:", Location = new Point(20, 35), AutoSize = true };
            nudProcessCount = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 3, Location = new Point(200, 32), Width = 60 };
            gbInputs.Controls.Add(lblProc);
            gbInputs.Controls.Add(nudProcessCount);

            Label lblQuantum = new Label { Text = "Quantum:", Location = new Point(20, 75), AutoSize = true };
            nudQuantum = new NumericUpDown { Minimum = 1, Maximum = 9, Value = 3, Location = new Point(200, 72), Width = 60 };
            gbInputs.Controls.Add(lblQuantum);
            gbInputs.Controls.Add(nudQuantum);

            // Panel to enable scrolling for the input table
            inputScrollPanel = new Panel
            {
                Location = new Point(20, 110),
                Size = new Size(350, 160), // Shows about 4-5 rows, rest scrolls
                AutoScroll = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            gbInputs.Controls.Add(inputScrollPanel);

            // Table for process input
            inputTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Location = new Point(0, 0),
                Size = new Size(330, 500), // Tall enough for many rows
                AutoSize = false,
                AutoScroll = false,
                BackColor = Color.White
            };
            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputScrollPanel.Controls.Add(inputTable);

            // Button
            btnCalculate = new Button
            {
                Text = "Calculate",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.Teal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Location = new Point(250, 280),
                Cursor = Cursors.Hand
            };
            gbInputs.Controls.Add(btnCalculate);

            btnCalculate.Click += BtnCalculate_Click;
            nudProcessCount.ValueChanged += (s, e) => GenerateProcessInputs();
            GenerateProcessInputs();

            // Gantt Chart Panel
            ganttChartPanel = new Panel
            {
                Name = "ganttChartPanel",
                Size = new Size(900, 180),
                Location = new Point(30, gbInputs.Bottom + 20), // Place below input box!
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                AutoScroll = true,
                Visible = false
            };
            this.Controls.Add(ganttChartPanel);

            // Results Panel
            resultsPanel = new Panel
            {
                Name = "resultsPanel",
                Size = new Size(320, 350),
                Location = new Point(700, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke,
                AutoScroll = true
            };
            this.Controls.Add(resultsPanel);
        }

        private void GenerateProcessInputs()
        {
            inputTable.Controls.Clear();
            int rowCount = (int)nudProcessCount.Value + 1;
            inputTable.RowCount = rowCount;

            // Header Row
            inputTable.Controls.Add(new Label { Text = "Process", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true }, 0, 0);
            inputTable.Controls.Add(new Label { Text = "Burst Time", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true }, 1, 0);
            inputTable.Controls.Add(new Label { Text = "Arrival Time", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true }, 2, 0);

            int rowHeight = 35;
            inputTable.Height = rowCount * rowHeight;

            for (int i = 1; i < rowCount; i++)
            {
                inputTable.Controls.Add(new Label { Text = $"P{i}", AutoSize = true, TextAlign = ContentAlignment.MiddleCenter }, 0, i);
                inputTable.Controls.Add(new TextBox { Name = $"txtBurst{i}", Width = 60, Anchor = AnchorStyles.Left }, 1, i);
                inputTable.Controls.Add(new TextBox { Name = $"txtArrival{i}", Width = 60, Anchor = AnchorStyles.Left }, 2, i);
            }
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            CollectProcessData();
            RunRoundRobinSimulation();
        }

        private bool ValidateInputs()
        {
            StringBuilder errorMsg = new StringBuilder();
            bool valid = true;
            for (int i = 1; i <= nudProcessCount.Value; i++)
            {
                var burstBox = inputTable.Controls.Find($"txtBurst{i}", true).FirstOrDefault() as TextBox;
                var arrivalBox = inputTable.Controls.Find($"txtArrival{i}", true).FirstOrDefault() as TextBox;
                if (burstBox == null || !int.TryParse(burstBox.Text, out int burst) || burst < 0)
                {
                    burstBox.BackColor = Color.LightPink;
                    errorMsg.AppendLine($"• P{i} Burst Time must be a non-negative integer");
                    valid = false;
                }
                else burstBox.BackColor = Color.White;

                if (arrivalBox == null || !int.TryParse(arrivalBox.Text, out int arrival) || arrival < 0)
                {
                    arrivalBox.BackColor = Color.LightPink;
                    errorMsg.AppendLine($"• P{i} Arrival Time must be a non-negative integer");
                    valid = false;
                }
                else arrivalBox.BackColor = Color.White;
            }
            if (!valid)
                MessageBox.Show(errorMsg.ToString(), "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return valid;
        }

        private void CollectProcessData()
        {
            processes.Clear();
            for (int i = 1; i <= nudProcessCount.Value; i++)
            {
                var burstBox = inputTable.Controls.Find($"txtBurst{i}", true).FirstOrDefault() as TextBox;
                var arrivalBox = inputTable.Controls.Find($"txtArrival{i}", true).FirstOrDefault() as TextBox;
                int burst = int.Parse(burstBox.Text);
                int arrival = int.Parse(arrivalBox.Text);
                processes.Add(new Process
                {
                    Name = $"P{i}",
                    BurstTime = burst,
                    ArrivalTime = arrival,
                    RemainingTime = burst
                });
            }
        }

        private void RunRoundRobinSimulation()
        {
            int quantum = (int)nudQuantum.Value;
            int currentTime = 0;
            Queue<Process> queue = new Queue<Process>();
            ganttData.Clear();

            var arrived = new List<Process>();
            var remaining = processes.OrderBy(p => p.ArrivalTime).ToList();

            while (processes.Any(p => p.RemainingTime > 0))
            {
                var newArrivals = remaining.Where(p => p.ArrivalTime <= currentTime).ToList();
                foreach (var p in newArrivals)
                {
                    queue.Enqueue(p);
                    remaining.Remove(p);
                }

                if (queue.Count == 0)
                {
                    currentTime++;
                    continue;
                }

                var current = queue.Dequeue();

                if (current.ResponseTime == -1)
                    current.ResponseTime = currentTime - current.ArrivalTime;

                int timeSlice = Math.Min(quantum, current.RemainingTime);
                ganttData.Add((current.Name, currentTime, timeSlice));
                current.RemainingTime -= timeSlice;
                currentTime += timeSlice;

                foreach (var p in remaining.Where(p => p.ArrivalTime <= currentTime).ToList())
                {
                    queue.Enqueue(p);
                    remaining.Remove(p);
                }

                if (current.RemainingTime > 0)
                    queue.Enqueue(current);
                else
                    current.TurnaroundTime = currentTime - current.ArrivalTime;
            }

            foreach (var p in processes)
                p.WaitingTime = p.TurnaroundTime - p.BurstTime;

            DrawGanttChart();
            DisplayResults();
        }

        private void DrawGanttChart()
        {
            ganttChartPanel.Controls.Clear();
            ganttChartPanel.Visible = true;

            int scale = 35;
            int blockHeight = 48;
            int yPos = 30;
            int totalTime = ganttData.Last().Start + ganttData.Last().Duration;

            // Draw Gantt blocks
            Color[] colors = { Color.LightSkyBlue, Color.LightGreen, Color.LightSalmon, Color.LightYellow, Color.LightPink, Color.LightCyan };
            for (int i = 0; i < ganttData.Count; i++)
            {
                var item = ganttData[i];
                Panel block = new Panel
                {
                    Width = item.Duration * scale,
                    Height = blockHeight,
                    Location = new Point(item.Start * scale, yPos),
                    BackColor = colors[i % colors.Length],
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(2)
                };
                block.Controls.Add(new Label
                {
                    Text = $"{item.Process}\n({item.Duration})",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                });
                ganttChartPanel.Controls.Add(block);
            }

            // Draw time labels at every process switch point
            var labelTimes = ganttData.Select(g => g.Start).ToList();
            labelTimes.Add(ganttData.Last().Start + ganttData.Last().Duration);
            labelTimes = labelTimes.Distinct().OrderBy(x => x).ToList();

            foreach (var t in labelTimes)
            {
                ganttChartPanel.Controls.Add(new Label
                {
                    Text = t.ToString(),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.DimGray,
                    Location = new Point(t * scale - 4, yPos + blockHeight + 8),
                    AutoSize = true,
                    BackColor = Color.Transparent
                });
            }

            ganttChartPanel.AutoScrollMinSize = new Size(totalTime * scale + 50, 0);
        }

        private void DisplayResults()
        {
            resultsPanel.Controls.Clear();

            resultsPanel.Controls.Add(new Label
            {
                Text = "Process Metrics",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.Teal,
                Location = new Point(12, 10),
                AutoSize = true
            });

            int yPos = 50;
            foreach (var p in processes.OrderBy(p => p.Name))
            {
                resultsPanel.Controls.Add(new Label
                {
                    Text = $"{p.Name}:   Turnaround = {p.TurnaroundTime},   Waiting = {p.WaitingTime},   Response = {p.ResponseTime}",
                    Location = new Point(12, yPos),
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    AutoSize = true
                });
                yPos += 28;
            }

            resultsPanel.Controls.Add(new Label
            {
                Text = $"Averages:\nTurnaround: {processes.Average(p => p.TurnaroundTime):F2}\n" +
                       $"Waiting: {processes.Average(p => p.WaitingTime):F2}\n" +
                       $"Response: {processes.Average(p => p.ResponseTime):F2}",
                Location = new Point(12, yPos + 10),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                AutoSize = true
            });
        }

        public class Process
        {
            public string Name { get; set; }
            public int BurstTime { get; set; }
            public int ArrivalTime { get; set; }
            public int RemainingTime { get; set; }
            public int WaitingTime { get; set; }
            public int ResponseTime { get; set; } = -1;
            public int TurnaroundTime { get; set; }
        }
    }
}

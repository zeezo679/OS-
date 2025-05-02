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
        private int x;
        private int y;
        private int processCount = 0;
        private List<Process> processes = new List<Process>();
        private Panel ganttChartPanel;
        private Panel resultsPanel = new Panel();
        private List<(string Process, int Start, int Duration)> ganttData = new List<(string, int, int)>();

        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScroll = true;

            ganttChartPanel = new Panel
            {
                Name = "ganttChartPanel",
                Size = new Size(600, 150),
                Location = new Point(120, 500),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                Visible = false
            };
            this.Controls.Add(ganttChartPanel);

            resultsPanel = new Panel
            {
                Name = "resultsPanel",
                Size = new Size(300, 300),
                Location = new Point(650, 200),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            this.Controls.Add(resultsPanel);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out int processNumUserInput) ||
                !int.TryParse(textBox2.Text, out int quantumNumUserInput))
            {
                MessageBox.Show("Please enter valid numbers for process count and quantum",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            processCount = processNumUserInput;

            if (!ValidateProcessesInput(processNumUserInput) || !ValidateQuantum(quantumNumUserInput))
            {
                MessageBox.Show("Invalid inputs. Process count must be positive and quantum between 1-9",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ClearPreviousResults();
            GenerateProcessInputs(processNumUserInput);
            GenerateArrivalTimeInputs(processNumUserInput);
            GenerateButton();
        }

        private void ClearPreviousResults()
        {
            ganttChartPanel.Controls.Clear();
            ganttChartPanel.Visible = false;
            resultsPanel.Controls.Clear();
            processes.Clear();
            ganttData.Clear();

            // Remove dynamically added controls (TextBoxes)
            foreach (var ctrl in this.Controls.OfType<TextBox>().Where(t => t.Name.StartsWith("textBox") && t != textBox1 && t != textBox2).ToList())
            {
                this.Controls.Remove(ctrl);
            }

            // Remove calculate button if exists
            var btnOld = this.Controls.Find("btnDynamic", true).FirstOrDefault();
            if (btnOld != null)
                this.Controls.Remove(btnOld);
        }

        private bool ValidateProcessesInput(int userInput) => userInput > 0;
        private bool ValidateQuantum(int userInput) => userInput > 0 && userInput < 10;

        private void GenerateProcessInputs(int numberOfProcesses)
        {
            var topInitial = 100;
            var leftInitial = 120;

            for (int i = 1; i <= numberOfProcesses; i++)
            {
                TextBox txt = new TextBox
                {
                    Top = topInitial += 50,
                    Left = leftInitial,
                    Name = "textBox" + (i + 2)
                };
                this.Controls.Add(txt);
                this.x = txt.Top;
                this.y = txt.Left;
            }
        }

        private void GenerateArrivalTimeInputs(int numberOfProcesses)
        {
            int topInitial = 100;
            int leftInitial = 420;

            for (int i = 1; i <= numberOfProcesses; i++)
            {
                TextBox txt = new TextBox
                {
                    Top = topInitial += 50,
                    Left = leftInitial,
                    Name = "textBox" + (i + numberOfProcesses + 2)
                };
                this.Controls.Add(txt);
            }
        }

        private void GenerateButton()
        {
            Button btn = new Button
            {
                Name = "btnDynamic",
                Text = "Calculate",
                Top = x + 50,
                Left = y + 141,
                Size = new Size(100, 40)
            };

            btn.Click += (sender, e) =>
            {
                if (!ValidateInputs())
                    return;

                CollectProcessData();
                RunRoundRobinSimulation();
            };

            this.Controls.Add(btn);
        }

        private bool ValidateInputs()
        {
            bool inputsValid = true;
            StringBuilder errorMsg = new StringBuilder();

            for (int i = 1; i <= processCount; i++)
            {
                if (!ValidateTextBox($"textBox{i + 2}", $"Process {i} burst time", errorMsg, out _) ||
                    !ValidateTextBox($"textBox{i + processCount + 2}", $"Process {i} arrival time", errorMsg, out _))
                {
                    inputsValid = false;
                }
            }

            if (!inputsValid)
            {
                MessageBox.Show(errorMsg.ToString(), "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return inputsValid;
        }

        private bool ValidateTextBox(string name, string fieldName, StringBuilder errorMsg, out int value)
        {
            value = 0;
            TextBox textBox = this.Controls.Find(name, true).FirstOrDefault() as TextBox;
            if (textBox == null || !int.TryParse(textBox.Text, out value) || value < 0)
            {
                if (textBox != null) textBox.BackColor = Color.LightPink;
                errorMsg.AppendLine($"• {fieldName} must be a valid non-negative integer");
                return false;
            }

            textBox.BackColor = SystemColors.Window;
            return true;
        }

        private void CollectProcessData()
        {
            processes.Clear();

            for (int i = 1; i <= processCount; i++)
            {
                ValidateTextBox($"textBox{i + 2}", "", new StringBuilder(), out int burstTime);
                ValidateTextBox($"textBox{i + processCount + 2}", "", new StringBuilder(), out int arrivalTime);

                processes.Add(new Process
                {
                    Name = $"P{i}",
                    BurstTime = burstTime,
                    ArrivalTime = arrivalTime,
                    RemainingTime = burstTime
                });
            }
        }

        private void RunRoundRobinSimulation()
        {
            int quantum = int.Parse(textBox2.Text);
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
            {
                p.WaitingTime = p.TurnaroundTime - p.BurstTime;
            }

            DrawGanttChart();
            DisplayResults();
        }

        private void DrawGanttChart()
        {
            ganttChartPanel.Controls.Clear();
            ganttChartPanel.Visible = true;

            int scale = 30;
            int blockHeight = 40;
            int yPos = 20;
            int totalTime = ganttData.Last().Start + ganttData.Last().Duration;

            for (int i = 0; i <= totalTime; i += Math.Max(1, totalTime / 10))
            {
                ganttChartPanel.Controls.Add(new Label
                {
                    Text = i.ToString(),
                    Location = new Point(i * scale + 5, yPos + blockHeight + 5),
                    AutoSize = true
                });
            }

            Color[] colors = { Color.LightBlue, Color.LightGreen, Color.LightSalmon,
                               Color.LightYellow, Color.LightPink, Color.LightCyan };

            for (int i = 0; i < ganttData.Count; i++)
            {
                var item = ganttData[i];
                Panel block = new Panel
                {
                    Width = item.Duration * scale,
                    Height = blockHeight,
                    Location = new Point(item.Start * scale, yPos),
                    BackColor = colors[i % colors.Length],
                    BorderStyle = BorderStyle.FixedSingle
                };

                block.Controls.Add(new Label
                {
                    Text = $"{item.Process}\n({item.Duration})",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                });

                ganttChartPanel.Controls.Add(block);
            }

            ganttChartPanel.AutoScrollMinSize = new Size(totalTime * scale + 50, 0);
        }

        private void DisplayResults()
        {
            resultsPanel.Controls.Clear();

            resultsPanel.Controls.Add(new Label
            {
                Text = "Process Metrics",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            });

            int yPos = 40;
            foreach (var p in processes.OrderBy(p => p.Name))
            {
                resultsPanel.Controls.Add(new Label
                {
                    Text = $"{p.Name}: TA={p.TurnaroundTime}, WT={p.WaitingTime}, RT={p.ResponseTime}",
                    Location = new Point(10, yPos),
                    AutoSize = true
                });
                yPos += 25;
            }

            resultsPanel.Controls.Add(new Label
            {
                Text = $"Averages:\nTA: {processes.Average(p => p.TurnaroundTime):F2}\n" +
                       $"WT: {processes.Average(p => p.WaitingTime):F2}\n" +
                       $"RT: {processes.Average(p => p.ResponseTime):F2}",
                Location = new Point(10, yPos + 10),
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

        private void Form1_Load(object sender, EventArgs e) { }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}





/*
 using System.Diagnostics;
using System.Text;

namespace OS
{
    public partial class Form1 : Form
    {
        private int x;
        private int y;
        private TextBox lastGenerated;
        private int processCount = 0;

        private List<Process> processes = new List<Process>();
        private Panel ganttChartPanel = new Panel();
        private Panel resultsPanel = new Panel();
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            this.AutoScroll = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       
        private void button1_Click(object sender, EventArgs e)
        {
            int processNumUserInput;
            int quantumNumUserInput;
            int.TryParse(textBox1.Text, out processNumUserInput);
            int.TryParse(textBox2.Text, out quantumNumUserInput);

            processCount = processNumUserInput;


            if (!ValidateProcessesInput(processNumUserInput) || !ValidateQuantum(quantumNumUserInput))
            {
                MessageBox.Show("The inputs are invalid, Please make sure the process number is not a negative number, and the quantum is not negative," +
                    "also consider that the maximum quantum is 10", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                GenerateProcessInputs(processNumUserInput);
                GenerateArrivalTimeInputs(processNumUserInput);
                GenerateButton();
            }
        }

        private bool ValidateProcessesInput(int userInput)
        {
            return userInput > 0;
        }
        private bool ValidateQuantum(int userInput)
        {
            return userInput > 0 && userInput < 10;
        }

        private void GenerateProcessInputs(int numberOfProcesses)
        {
            var topInitial = 100;
            var leftInitial = 120;

            for (int i = 1; i <= numberOfProcesses; i++)
            {
                TextBox txt = new TextBox();
                this.Controls.Add(txt);
                txt.Top = topInitial += 50;
                txt.Left = leftInitial;
                txt.Name = "textBox" + (i+2);
                this.x = txt.Top;
                this.y = txt.Left;
            }

        }

        private void GenerateArrivalTimeInputs(int numberOfProcesses)
        {
            int topInitial = 100;
            int leftInitial = 420;
            for (int i = 1; i <= numberOfProcesses; i++)
            {
                TextBox txt = new TextBox();
                this.Controls.Add(txt);
                txt.Top = topInitial += 50;
                txt.Left = leftInitial;
                txt.Name = "textBox" + (i + numberOfProcesses + 2);
            }
        }

        private void GenerateButton()
        {
            Button btn = new Button();

            btn.Name = "btnDynamic";
            btn.Text = "Calculate";
            btn.Top = x + 50;
            btn.Left = y + 141;
            btn.Click += (sender, e) =>
            {
                bool inputsValid = true;
                StringBuilder errorMsg = new StringBuilder();

                for (int i = 1; i <= processCount; i++)
                {
                    TextBox burstTextBox = this.Controls.Find($"textBox{i+2}", true).FirstOrDefault() as TextBox;
                    if (burstTextBox == null || !int.TryParse(burstTextBox.Text, out int burstTime) || burstTime <= 0)
                    {
                        inputsValid = false;
                        burstTextBox.BackColor = Color.LightPink; // Highlight invalid field
                        errorMsg.AppendLine($"• Process {i}: Burst time must be a positive number");
                    }
                    else
                    {
                        burstTextBox.BackColor = SystemColors.Window; // Reset color if valid
                    }

                }

                for (int i = 1; i <= processCount; i++)
                {
                    TextBox arrivalTextBox = this.Controls.Find($"textBox{i + processCount + 2}", true).FirstOrDefault() as TextBox;
                    if (arrivalTextBox == null || !int.TryParse(arrivalTextBox.Text, out int arrivalTime) || arrivalTime < 0)
                    {
                        inputsValid = false;
                        arrivalTextBox.BackColor = Color.LightPink;
                        errorMsg.AppendLine($"• Process {i}: Arrival time cannot be negative");
                    }
                    else
                    {
                        arrivalTextBox.BackColor = SystemColors.Window;
                    }
                }

                if (!inputsValid)
                {
                    MessageBox.Show(errorMsg.ToString(), "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                CollectProcessData();

            };

            this.Controls.Add(btn);
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
            public bool HasResponded { get; set; } = false;
        }
        private void CollectProcessData()
        {
            processes.Clear();
            int quantum = int.Parse(textBox2.Text);

            for (int i = 1; i <= processCount; i++)
            {
                TextBox burstTextBox = this.Controls.Find($"textBox{i + 2}", true).FirstOrDefault() as TextBox;
                TextBox arrivalTextBox = this.Controls.Find($"textBox{i + processCount + 2}", true).FirstOrDefault() as TextBox;

                processes.Add(new Process
                {
                    Name = $"P{i}",
                    BurstTime = int.Parse(burstTextBox.Text),
                    ArrivalTime = int.Parse(arrivalTextBox.Text),
                    RemainingTime = int.Parse(burstTextBox.Text)
                });
            }
        }
        private void RunRoundRobinSimulation()
        {
            var quantum = int.Parse(textBox2.Text);
            var currentTime = 0; //What is the purpose of this??
            Queue<Process> queue = new Queue<Process>();
            List<(string Process, int Start, int Duration)> ganttData = new List<(string, int, int)>();

            foreach (var p in processes)
            {
                p.RemainingTime = p.BurstTime;
                p.WaitingTime = 0;
            }
        }
        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

    }
}

 */
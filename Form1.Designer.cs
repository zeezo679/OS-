namespace OS
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            button1 = new Button();
            label3 = new Label();
            label4 = new Label();
            SuspendLayout();
           
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(668, 38);
            label2.Name = "label2";
            label2.Size = new Size(88, 15);
            label2.TabIndex = 1;
            label2.Text = "Enter Quantum";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(205, 35);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(92, 23);
            textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(762, 35);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(69, 23);
            textBox2.TabIndex = 3;
            
           
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(383, 126);
            label4.Name = "label4";
            label4.Size = new Size(168, 15);
            label4.TabIndex = 6;
            label4.Text = "Enter Arrival Times Respectivly";
            
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button button1;
        private Label label3;
        private Label label4;
    }
}

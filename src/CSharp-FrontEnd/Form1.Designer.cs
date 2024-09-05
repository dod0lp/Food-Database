namespace CSharp_FrontEnd
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
            dataGridViewAllFood = new DataGridView();
            textBox1 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAllFood).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridViewAllFood.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewAllFood.Location = new Point(3, 3);
            dataGridViewAllFood.Name = "dataGridView1";
            dataGridViewAllFood.Size = new Size(1772, 356);
            dataGridViewAllFood.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(62, 523);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(1059, 23);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1878, 914);
            Controls.Add(textBox1);
            Controls.Add(dataGridViewAllFood);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dataGridViewAllFood).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridViewAllFood;
        private TextBox textBox1;
    }
}

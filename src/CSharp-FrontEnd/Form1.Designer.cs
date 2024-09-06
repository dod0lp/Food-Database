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
            dataGridFood1 = new DataGridView();
            dataGridFood2 = new DataGridView();
            dataGridFoodFinal = new DataGridView();
            btnCombineFoods = new Button();
            btnInsertIntoDB = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAllFood).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFood1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFood2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFoodFinal).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewAllFood
            // 
            dataGridViewAllFood.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewAllFood.Location = new Point(3, 3);
            dataGridViewAllFood.Name = "dataGridViewAllFood";
            dataGridViewAllFood.Size = new Size(1772, 356);
            dataGridViewAllFood.TabIndex = 0;
            // 
            // dataGridFood1
            // 
            dataGridFood1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridFood1.Location = new Point(3, 533);
            dataGridFood1.Name = "dataGridFood1";
            dataGridFood1.Size = new Size(1511, 56);
            dataGridFood1.TabIndex = 1;
            // 
            // dataGridFood2
            // 
            dataGridFood2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridFood2.Location = new Point(3, 613);
            dataGridFood2.Name = "dataGridFood2";
            dataGridFood2.Size = new Size(1511, 57);
            dataGridFood2.TabIndex = 2;
            // 
            // dataGridFoodFinal
            // 
            dataGridFoodFinal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridFoodFinal.Location = new Point(3, 739);
            dataGridFoodFinal.Name = "dataGridFoodFinal";
            dataGridFoodFinal.Size = new Size(1511, 55);
            dataGridFoodFinal.TabIndex = 3;
            // 
            // btnCombineFoods
            // 
            btnCombineFoods.Location = new Point(1690, 533);
            btnCombineFoods.Name = "btnCombineFoods";
            btnCombineFoods.Size = new Size(157, 23);
            btnCombineFoods.TabIndex = 4;
            btnCombineFoods.Text = "Combine foods";
            btnCombineFoods.UseVisualStyleBackColor = true;
            btnCombineFoods.Click += buttonCombineFood_Click;
            // 
            // btnInsertIntoDB
            // 
            btnInsertIntoDB.Location = new Point(1690, 771);
            btnInsertIntoDB.Name = "btnInsertIntoDB";
            btnInsertIntoDB.Size = new Size(157, 23);
            btnInsertIntoDB.TabIndex = 5;
            btnInsertIntoDB.Text = "Insert into database";
            btnInsertIntoDB.UseVisualStyleBackColor = true;
            btnInsertIntoDB.Click += btnInsertIntoDB_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1878, 914);
            Controls.Add(btnInsertIntoDB);
            Controls.Add(btnCombineFoods);
            Controls.Add(dataGridFoodFinal);
            Controls.Add(dataGridFood2);
            Controls.Add(dataGridFood1);
            Controls.Add(dataGridViewAllFood);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dataGridViewAllFood).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFood1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFood2).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFoodFinal).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridViewAllFood;
        private DataGridView dataGridFood1;
        private DataGridView dataGridFood2;
        private DataGridView dataGridFoodFinal;
        private Button btnCombineFoods;
        private Button btnInsertIntoDB;
    }
}

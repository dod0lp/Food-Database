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
            refreshDatabaseButton = new Button();
            textBoxFood1 = new TextBox();
            textBoxFood2 = new TextBox();
            textBoxFoodInsert = new TextBox();
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
            dataGridFood1.Size = new Size(1374, 56);
            dataGridFood1.TabIndex = 1;
            // 
            // dataGridFood2
            // 
            dataGridFood2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridFood2.Location = new Point(3, 624);
            dataGridFood2.Name = "dataGridFood2";
            dataGridFood2.Size = new Size(1374, 57);
            dataGridFood2.TabIndex = 2;
            // 
            // dataGridFoodFinal
            // 
            dataGridFoodFinal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridFoodFinal.Location = new Point(3, 739);
            dataGridFoodFinal.Name = "dataGridFoodFinal";
            dataGridFoodFinal.Size = new Size(1374, 55);
            dataGridFoodFinal.TabIndex = 3;
            // 
            // btnCombineFoods
            // 
            btnCombineFoods.Location = new Point(1476, 533);
            btnCombineFoods.Name = "btnCombineFoods";
            btnCombineFoods.Size = new Size(157, 23);
            btnCombineFoods.TabIndex = 4;
            btnCombineFoods.Text = "Combine foods";
            btnCombineFoods.UseVisualStyleBackColor = true;
            btnCombineFoods.Click += buttonCombineFood_Click;
            // 
            // btnInsertIntoDB
            // 
            btnInsertIntoDB.Location = new Point(1476, 771);
            btnInsertIntoDB.Name = "btnInsertIntoDB";
            btnInsertIntoDB.Size = new Size(157, 23);
            btnInsertIntoDB.TabIndex = 5;
            btnInsertIntoDB.Text = "Insert into database";
            btnInsertIntoDB.UseVisualStyleBackColor = true;
            btnInsertIntoDB.Click += buttonInsertIntoDB_Click;
            // 
            // refreshDatabaseButton
            // 
            refreshDatabaseButton.Location = new Point(1690, 365);
            refreshDatabaseButton.Name = "refreshDatabaseButton";
            refreshDatabaseButton.Size = new Size(85, 23);
            refreshDatabaseButton.TabIndex = 6;
            refreshDatabaseButton.Text = "Refresh";
            refreshDatabaseButton.UseVisualStyleBackColor = true;
            refreshDatabaseButton.Click += refreshDatabaseButton_Click;
            // 
            // textBoxFood1
            // 
            textBoxFood1.Enabled = false;
            textBoxFood1.Location = new Point(3, 504);
            textBoxFood1.Name = "textBoxFood1";
            textBoxFood1.Size = new Size(100, 23);
            textBoxFood1.TabIndex = 7;
            textBoxFood1.Text = "Food 1";
            // 
            // textBoxFood2
            // 
            textBoxFood2.Enabled = false;
            textBoxFood2.Location = new Point(3, 595);
            textBoxFood2.Name = "textBoxFood2";
            textBoxFood2.Size = new Size(100, 23);
            textBoxFood2.TabIndex = 8;
            textBoxFood2.Text = "Food 2";
            // 
            // textBoxFoodInsert
            // 
            textBoxFoodInsert.Enabled = false;
            textBoxFoodInsert.Location = new Point(3, 710);
            textBoxFoodInsert.Name = "textBoxFoodInsert";
            textBoxFoodInsert.Size = new Size(100, 23);
            textBoxFoodInsert.TabIndex = 9;
            textBoxFoodInsert.Text = "Food to Insert";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1878, 914);
            Controls.Add(textBoxFoodInsert);
            Controls.Add(textBoxFood2);
            Controls.Add(textBoxFood1);
            Controls.Add(refreshDatabaseButton);
            Controls.Add(btnInsertIntoDB);
            Controls.Add(btnCombineFoods);
            Controls.Add(dataGridFoodFinal);
            Controls.Add(dataGridFood2);
            Controls.Add(dataGridFood1);
            Controls.Add(dataGridViewAllFood);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewAllFood).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFood1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFood2).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridFoodFinal).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridViewAllFood;
        private DataGridView dataGridFood1;
        private DataGridView dataGridFood2;
        private DataGridView dataGridFoodFinal;
        private Button btnCombineFoods;
        private Button btnInsertIntoDB;
        private Button refreshDatabaseButton;
        private TextBox textBoxFood1;
        private TextBox textBoxFood2;
        private TextBox textBoxFoodInsert;
    }
}

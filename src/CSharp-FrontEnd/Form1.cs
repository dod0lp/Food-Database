using Food_Database_Base;
using Food;
using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;
using System.Drawing.Text;

namespace CSharp_FrontEnd
{
    public partial class Form1 : Form
    {
        private FoodDbContext dbContext;
        private DB_DataParser dbParser;
        private int foodEdit1 = -1;
        private int foodEdit2 = -1;
        const string btnTextFoodEdit1 = "Food1";
        const string btnTextFoodEdit2 = "Food2";

        public Form1()
        {
            dbContext = new FoodDbContext();
            dbParser = new DB_DataParser(dbContext);
            InitializeComponent();

            Load += Form1_Load;
        }

        private void InitDatagrid1()
        {
            dataGridView1.Columns.Add("Id", "ID");
            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("Weight", "Weight");
            dataGridView1.Columns.Add("Kcal", "Kcal");
            dataGridView1.Columns.Add("kJ", "kJ");

            dataGridView1.Columns.Add("Fat", "Fat");
            dataGridView1.Columns.Add("SaturatedFat", "Saturated Fat");

            dataGridView1.Columns.Add("Carbs", "Carbs");
            dataGridView1.Columns.Add("Sugars", "Sugars");

            dataGridView1.Columns.Add("Protein", "Protein");
            dataGridView1.Columns.Add("Salt", "Salt");

            dataGridView1.Columns.Add("Description", "Description");
            dataGridView1.Columns.Add("Contains", "Contains");

            AddEditFoodButtons();

            // Add event handler for cell content click
            dataGridView1.CellContentClick += DataGridView1_CheckFood;
        }

        private void AddEditFoodButtons()
        {
            DataGridViewButtonColumn buttonFood1 = new DataGridViewButtonColumn
            {
                Name = btnTextFoodEdit1,
                HeaderText = btnTextFoodEdit1,
                Text = btnTextFoodEdit1,
                UseColumnTextForButtonValue = true
            };
            dataGridView1.Columns.Add(buttonFood1);

            DataGridViewButtonColumn buttonFood2 = new DataGridViewButtonColumn
            {
                Name = btnTextFoodEdit2,
                HeaderText = btnTextFoodEdit2,
                Text = btnTextFoodEdit2,
                UseColumnTextForButtonValue = true
            };
            dataGridView1.Columns.Add(buttonFood2);
        }

        private void DataGridView1_CheckFood(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns[btnTextFoodEdit1].Index && e.RowIndex >= 0)
            {
                var rowId = dataGridView1.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                foodEdit1 = Convert.ToInt32(rowId);
            }

            if (e.ColumnIndex == dataGridView1.Columns[btnTextFoodEdit2].Index && e.RowIndex >= 0)
            {
                var rowId = dataGridView1.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                foodEdit1 = Convert.ToInt32(rowId);
            }
        }

        private void AddFoodDomainToDatagrid1(Food.Food foodDomain)
        {
            var foodInfoList = Food.Food.ToStringList(foodDomain);

            var row = new DataGridViewRow();
            row.CreateCells(dataGridView1);

            if (foodInfoList.Count > row.Cells.Count)
            {
                for (int i = row.Cells.Count; i < foodInfoList.Count; i++)
                {
                    row.Cells.Add(new DataGridViewTextBoxCell());
                }
            }

            for (int i = 0; i < foodInfoList.Count; i++)
            {
                row.Cells[i].Value = foodInfoList[i];
            }

            row.Cells[dataGridView1.Columns[btnTextFoodEdit1].Index].Value = btnTextFoodEdit1;
            row.Cells[dataGridView1.Columns[btnTextFoodEdit2].Index].Value = btnTextFoodEdit2;
            dataGridView1.Rows.Add(row);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            LoadAllFoods();
        }

        private void LoadAllFoods()
        {
            try
            {
                using (dbContext)
                {
                    List<FoodEntity> foodsWithNutrients = dbParser.GetAllFoodEntity();
                    dataGridView1.Rows.Clear();

                    InitDatagrid1();

                    if (foodsWithNutrients == null || foodsWithNutrients.Count == 0)
                    {
                        MessageBox.Show("No data found.");
                        return;
                    }

                    foreach (FoodEntity foodEntity in foodsWithNutrients)
                    {
                        Food.Food foodDomain = foodEntity.MapToDomain();
                        AddFoodDomainToDatagrid1(foodDomain);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Dispose of the DbContext when the form is closing
            dbContext?.Dispose();
            base.OnFormClosing(e);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

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
        private int foodEditID1 = -1;
        private int foodEditID2 = -1;
        const string btnTextFoodEdit1 = "Food1";
        const string btnTextFoodEdit2 = "Food2";

        public Form1()
        {
            dbContext = new FoodDbContext();
            dbParser = new DB_DataParser(dbContext);
            InitializeComponent();

            Load += Form1_Load;
        }

        private void InitDatagridFoodInfo(DataGridView datagrid, bool readOnly = true)
        {
            datagrid.Columns.Add("Id", "ID");
            datagrid.Columns.Add("Name", "Name");
            datagrid.Columns.Add("Weight", "Weight");
            datagrid.Columns.Add("Kcal", "Kcal");
            datagrid.Columns.Add("kJ", "kJ");

            datagrid.Columns.Add("Fat", "Fat");
            datagrid.Columns.Add("SaturatedFat", "Saturated Fat");

            datagrid.Columns.Add("Carbs", "Carbs");
            datagrid.Columns.Add("Sugars", "Sugars");

            datagrid.Columns.Add("Protein", "Protein");
            datagrid.Columns.Add("Salt", "Salt");

            datagrid.Columns.Add("Description", "Description");
            datagrid.Columns.Add("Contains", "Contains");

            datagrid.ReadOnly = readOnly;
        }

        private void InitDatagridFoodInfoAllFood()
        {
            dataGridViewAllFood.Rows.Clear();
            InitDatagridFoodInfo(dataGridViewAllFood);
            dataGridViewAllFood.CellContentClick += DataGridView1_CheckFood;
            dataGridViewAllFood.ReadOnly = true;
            AddEditFoodButtons();
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
            dataGridViewAllFood.Columns.Add(buttonFood1);

            DataGridViewButtonColumn buttonFood2 = new DataGridViewButtonColumn
            {
                Name = btnTextFoodEdit2,
                HeaderText = btnTextFoodEdit2,
                Text = btnTextFoodEdit2,
                UseColumnTextForButtonValue = true
            };
            dataGridViewAllFood.Columns.Add(buttonFood2);
        }

        private void DataGridView1_CheckFood(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridViewAllFood.Columns[btnTextFoodEdit1].Index && e.RowIndex >= 0)
            {
                var rowId = dataGridViewAllFood.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                foodEditID1 = Convert.ToInt32(rowId);
            }

            if (e.ColumnIndex == dataGridViewAllFood.Columns[btnTextFoodEdit2].Index && e.RowIndex >= 0)
            {
                var rowId = dataGridViewAllFood.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                foodEditID2 = Convert.ToInt32(rowId);
            }
        }

        private void AddFoodDomainToDatagrid1(Food.Food foodDomain)
        {
            var foodInfoList = Food.Food.ToStringList(foodDomain);

            var row = new DataGridViewRow();
            row.CreateCells(dataGridViewAllFood);

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

            row.Cells[dataGridViewAllFood.Columns[btnTextFoodEdit1].Index].Value = btnTextFoodEdit1;
            row.Cells[dataGridViewAllFood.Columns[btnTextFoodEdit2].Index].Value = btnTextFoodEdit2;
            dataGridViewAllFood.Rows.Add(row);
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
                    InitDatagridFoodInfoAllFood();

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

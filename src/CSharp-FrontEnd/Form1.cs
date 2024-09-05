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

        private void DataGridFoodEditCheck(object sender, DataGridViewCellEventArgs e, string btnTextFoodEdit, ref int foodEditID, DataGridView datagrid)
        {
            if (e.ColumnIndex == dataGridViewAllFood.Columns[btnTextFoodEdit].Index && e.RowIndex >= 0)
            {
                var rowId = dataGridViewAllFood.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                foodEditID = Convert.ToInt32(rowId);

                var foodToEdit = dbParser.GetFoodDomainModelById(foodEditID);
                if (foodToEdit != null)
                {
                    EnsureSingleRowAndUpdate(datagrid, foodToEdit);
                }
            }
        }

        private void DataGridView1_CheckFood(object sender, DataGridViewCellEventArgs e)
        {
            DataGridFoodEditCheck(sender, e, btnTextFoodEdit1, ref foodEditID1, dataGridFood1);
            DataGridFoodEditCheck(sender, e, btnTextFoodEdit2, ref foodEditID2, dataGridFood2);
        }

        private void AddFoodDomainToDatagridAllFood(Food.Food foodDomain)
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

        private void ConfigureDataGridView(DataGridView datagrid)
        {
            datagrid.Rows.Clear();
        }

        private void InitializeEditDataGrids()
        {
            InitDatagridFoodInfo(dataGridFood1, false);
            InitDatagridFoodInfo(dataGridFood2, false);
            InitDatagridFoodInfo(dataGridFoodFinal, false);

            ConfigureDataGridView(dataGridFood1);
            ConfigureDataGridView(dataGridFood2);
            ConfigureDataGridView(dataGridFoodFinal);
        }

        private void EnsureSingleRowAndUpdate(DataGridView dataGridView, Food.Food foodDomain)
        {
            var foodInfoList = Food.Food.ToStringList(foodDomain);
            dataGridView.Rows.Clear();

            dataGridView.Rows.Add();

            if (dataGridView.Rows.Count > 0 && !dataGridView.Rows[0].IsNewRow)
            {
                DataGridViewRow row = dataGridView.Rows[0];

                if (foodInfoList.Count > row.Cells.Count)
                {
                    MessageBox.Show("The number of values exceeds the number of columns.");
                    return;
                }

                for (int i = 0; i < foodInfoList.Count; i++)
                {
                    if (i < row.Cells.Count)
                    {
                        row.Cells[i].Value = foodInfoList[i];
                    }
                }
            }
            else
            {
                MessageBox.Show("No rows available to update.");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeEditDataGrids();
            LoadAllFoods();
        }

        private void LoadAllFoods()
        {
            try
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
                    AddFoodDomainToDatagridAllFood(foodDomain);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }
}

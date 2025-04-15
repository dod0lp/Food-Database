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
        }

        /// <summary>
        /// Initializes the given DataGridView with columns for food information.
        /// </summary>
        /// <param name="datagrid">The DataGridView to initialize.</param>
        /// <param name="readOnly">Indicates whether the DataGridView should be read-only.</param>
        private void InitDatagridFoodInfo(DataGridView datagrid, bool readOnly = true)
        {
            datagrid.Columns.Add("Id", "ID");
            datagrid.Columns.Add("Name", "Name");
            datagrid.Columns.Add("Weight", "Weight (g)");
            datagrid.Columns.Add("Kcal", "Kcal");
            datagrid.Columns.Add("kJ", "kJ");

            datagrid.Columns.Add("Fat", "Fat (g)");
            datagrid.Columns.Add("SaturatedFat", "Saturated Fat");

            datagrid.Columns.Add("Carbs", "Carbs (g)");
            datagrid.Columns.Add("Sugars", "Sugars (g)");

            datagrid.Columns.Add("Protein", "Protein (g)");
            datagrid.Columns.Add("Salt", "Salt (g)");

            datagrid.Columns.Add("Description", "Description");
            datagrid.Columns.Add("Contains", "Contains");

            datagrid.ReadOnly = readOnly;
        }

        /// <summary>
        /// Initializes the DataGridView for displaying all food items.
        /// </summary>
        private void InitDatagridFoodInfoAllFood()
        {
            dataGridViewAllFood.Rows.Clear();
            dataGridViewAllFood.Columns.Clear();

            InitDatagridFoodInfo(dataGridViewAllFood);
            dataGridViewAllFood.CellContentClick += DataGridView1_CheckFood;
            dataGridViewAllFood.ReadOnly = true;
            AddEditFoodButtons();
        }

        /// <summary>
        /// Converts the ID in selected row from the given DataGridView to a <see cref="Food"/> - domain object.
        /// </summary>
        /// <param name="datagrid">The DataGridView containing the row with the food ID.</param>
        /// <returns>The corresponding food domain object, or null if not found.</returns>
        private Food.Food? AquireRowIDToFoodDomain(DataGridView datagrid)
        {
            if (datagrid.Rows.Count > 0 && datagrid.Rows[0].Cells["Id"].Value != null)
            {
                var idValue = datagrid.Rows[0].Cells["Id"].Value.ToString();
                if (int.TryParse(idValue, out int foodId))
                {
                    return dbParser.GetFoodDomainModelById(foodId);
                }
            }

            return null;
        }

        /// <summary>
        /// Converts the values selected row from the given DataGridView to a <see cref="Food"/> - domain object.
        /// </summary>
        /// <param name="datagrid">The DataGridView containing the row with the food ID.</param>
        /// <returns>The corresponding food domain object, or null if error.</returns>
        private Food.Food? RowValuesToDomain(DataGridView datagrid)
        {
            var list = new List<string>();
            foreach (DataGridViewCell cell in datagrid.Rows[0].Cells)
            {
                list.Add(cell.Value?.ToString() ?? string.Empty);
            }

            try
            {
                Food.Food food = Food.Food.FromStringList(list);
                return food;
            }
            catch { return null; }
        }

        /// <summary>
        /// Adds edit buttons to the <see cref="DataGridView"/> for editing food items.
        /// </summary>
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

        /// <summary>
        /// Handles the event to check and edit food items in the DataGridView.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Data about the cell click event.</param>
        /// <param name="btnTextFoodEdit">The name of the button clicked to edit the food item.</param>
        /// <param name="foodEditID">Reference to the ID of the food item being edited.</param>
        /// <param name="datagrid">The DataGridView containing the food data.</param>
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

        /// <summary>
        /// Combines two selected food items from different DataGridViews into one food object.
        /// </summary>
        /// <param name="datagrid1">The first DataGridView containing the first food item.</param>
        /// <param name="datagrid2">The second DataGridView containing the second food item.</param>
        /// <returns>The combined food object, or one of the individual food objects if only one is selected.</returns>
        private Food.Food? MakeFoodFromTwoSelected(DataGridView datagrid1, DataGridView datagrid2)
        {
            Food.Food? food1 = datagrid1.Rows.Count > 0 ?
                AquireRowIDToFoodDomain(datagrid1) : null;

            Food.Food? food2 = datagrid2.Rows.Count > 0 ?
                AquireRowIDToFoodDomain(datagrid2) : null;

            if (food1 == null && food2 == null) return null;
            // return one of them or combine them
            return food1 != null && food2 != null ? food1 + food2 : food1 ?? food2;
        }

        /// <summary>
        /// Handles the event for combining two food items into one when the button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void buttonCombineFood_Click(object sender, EventArgs e)
        {
            Food.Food? food = MakeFoodFromTwoSelected(dataGridFood1, dataGridFood2);

            if (food != null)
            {
                EnsureSingleRowAndUpdate(dataGridFoodFinal, food);
            }
        }

        /// <summary>
        /// Handles the event to insert the selected food item from the final DataGridView into the database.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void buttonInsertIntoDB_Click(object sender, EventArgs e)
        {
            Food.Food? food = RowValuesToDomain(dataGridFoodFinal);

            if (food != null)
            {
                try
                {
                    int retId = dbParser.InsertFoodFromDomain(food);
                    dbParser.InsertFoodMappingsFromEntity(retId);
                }
                catch
                {
                    MessageBox.Show("This did not work");
                }
            }
        }

        /// <summary>
        /// Handles the event for checking and editing food items when a cell is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void DataGridView1_CheckFood(object sender, DataGridViewCellEventArgs e)
        {
            DataGridFoodEditCheck(sender, e, btnTextFoodEdit1, ref foodEditID1, dataGridFood1);
            DataGridFoodEditCheck(sender, e, btnTextFoodEdit2, ref foodEditID2, dataGridFood2);
        }

        /// <summary>
        /// Adds a food domain object to the DataGridView displaying all food items.
        /// </summary>
        /// <param name="foodDomain">The food domain object to add.</param>
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

        /// <summary>
        /// Configures the <see cref="DataGridView"/> by clearing its rows.
        /// </summary>
        /// <param name="datagrid">The <see cref="DataGridView"/> to configure.</param>
        private void ConfigureDataGridView(DataGridView datagrid)
        {
            datagrid.Rows.Clear();
        }

        /// <summary>
        /// Initializes and configures the <see cref="DataGridView"/> for editing food items.
        /// </summary>
        private void InitializeEditDataGrids()
        {
            InitDatagridFoodInfo(dataGridFood1, false);
            InitDatagridFoodInfo(dataGridFood2, false);
            InitDatagridFoodInfo(dataGridFoodFinal, false);

            ConfigureDataGridView(dataGridFood1);
            ConfigureDataGridView(dataGridFood2);
            ConfigureDataGridView(dataGridFoodFinal);
        }

        /// <summary>
        /// Ensures the <see cref="DataGridView"/> contains a single row and updates it with the given food domain object.
        /// </summary>
        /// <param name="dataGridView">The <see cref="DataGridView"/> to update.</param>
        /// <param name="foodDomain">The food domain object to update with.</param>
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
            LoadAllFoodsAsync();
        }

        /// <summary>
        /// Prints all foods in database into a row/columns list.
        /// </summary>
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

        /// <summary>
        /// Prints all foods in database into a row/columns list asynchronously.
        /// </summary>
        private async void LoadAllFoodsAsync()
        {
            try
            {
                // Load data in a separate thread to avoid blocking the UI
                List<FoodEntity> foodsWithNutrients = await Task.Run(() => dbParser.GetAllFoodEntityAsync());

                InitDatagridFoodInfoAllFood();

                if (foodsWithNutrients == null || foodsWithNutrients.Count == 0)
                {
                    MessageBox.Show("No data found.");
                    return;
                }

                foreach (FoodEntity foodEntity in foodsWithNutrients)
                {
                    // Map the entity to the domain object
                    Food.Food foodDomain = foodEntity.MapToDomain();

                    // Add to the datagrid (execute on UI thread if necessary)
                    AddFoodDomainToDatagridAllFood(foodDomain);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="e">Event</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            dbContext?.Dispose();
            base.OnFormClosing(e);
        }

        private void refreshDatabaseButton_Click(object sender, EventArgs e)
        {
            LoadAllFoodsAsync();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

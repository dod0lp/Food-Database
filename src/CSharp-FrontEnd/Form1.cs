using Food_Database_Base;
using Food;
using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;

namespace CSharp_FrontEnd
{
    public partial class Form1 : Form
    {
        private FoodDbContext dbContext;
        private DB_DataParser dbParser;

        public Form1()
        {
            dbContext = new FoodDbContext();
            dbParser = new DB_DataParser(dbContext);
            InitializeComponent();

            Load += Form1_Load;
            InitDatagrid1();
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
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDatabaseData();
        }

        private void LoadDatabaseData()
        {
            try
            {
                using (dbContext)
                {
                    List<FoodEntity> foodsWithNutrients = dbParser.GetAllFoodEntity();
                    dataGridView1.Rows.Clear();
                    if (foodsWithNutrients == null || foodsWithNutrients.Count == 0)
                    {
                        MessageBox.Show("No data found.");
                        return;
                    }

                    foreach (FoodEntity foodEntity in foodsWithNutrients)
                    {
                        Food.Food foodList = foodEntity.MapToDomain();
                        var foodInfoList = Food.Food.ToStringList(foodList);

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

                        dataGridView1.Rows.Add(row);
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
    }
}

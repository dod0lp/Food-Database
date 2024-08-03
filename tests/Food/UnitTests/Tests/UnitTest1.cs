using Food;

namespace FoodTests
{
    public class NutrientsCreation
    {
        [Theory]
        [InlineData(10, 7)]
        [InlineData(15, 5)]
        [InlineData(20, 10)]
        public void MakeFats(int totalFat, int saturatedFat)
        {
            Fat fat = new(totalFat, saturatedFat);

            Assert.Equal(totalFat, fat.Total);
            Assert.Equal(saturatedFat, fat.Saturated);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void MakeSalts(int total)
        {
            Salt salt = new(total);

            Assert.Equal(total, salt.Total);
        }

        [Theory]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        public void MakeProteins(int total)
        {
            Protein protein = new(total);

            Assert.Equal(total, protein.Total);
        }
    }
}
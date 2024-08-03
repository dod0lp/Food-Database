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
    }
}
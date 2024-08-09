using Food;

namespace FoodTests
{
    public class NutrientsCreation
    {
        public static IEnumerable<object[]> TestData_Total_MultiplesOf5()
        {
            const int maxNum = 100;
            const int step = 5;

            for (int i = 0; i <= maxNum; i += step)
            {
                yield return new object[] { i };
            }
        }

        public static IEnumerable<object[]> TestData_Total_and_Other_MultiplesOf5()
        {
            const int maxNum = 100;
            const int step = 5;

            Random random = new Random();
            for (int i = 0; i <= maxNum; i += step)
            {
                double multiplier = random.NextDouble(); // Generates a random number between 0.0 and 0.99
                int secondary = (int)(i * multiplier); // Multiply the Total by this secondary number (for example Saturated fat, Sugars...)
                yield return new object[] { i, secondary };
            }
        }


        [Theory]
        [InlineData(10, 7)]
        [InlineData(15, 5)]
        [InlineData(20, 10)]
        [MemberData(nameof(TestData_Total_and_Other_MultiplesOf5))]
        public void MakeFats(int totalFat, int saturatedFat)
        {
            Fat fat = new(totalFat, saturatedFat);

            Assert.Equal(totalFat, fat.Total);
            Assert.Equal(saturatedFat, fat.Saturated);
        }

        [Theory]
        [InlineData(30, 10)]
        [InlineData(45, 15)]
        [InlineData(60, 20)]
        [MemberData(nameof(TestData_Total_and_Other_MultiplesOf5))]
        public void MakeCarbohydrates(int total, int sugar)
        {
            Carbohydrates carbs = new(total, sugar);

            Assert.Equal(total, carbs.Total);
            Assert.Equal(sugar, carbs.Sugar);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        [MemberData(nameof(TestData_Total_MultiplesOf5))]
        public void MakeSalts(int total)
        {
            Salt salt = new(total);

            Assert.Equal(total, salt.Total);
        }

        [Theory]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [MemberData(nameof(TestData_Total_MultiplesOf5))]
        public void MakeProteins(int total)
        {
            Protein protein = new(total);

            Assert.Equal(total, protein.Total);
        }

    }
}
namespace Food
{
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }
        public Nutrients NutrientContent { get; set; }
        public List<Food> Ingredients { get; set; }

        public Food(int id, string name, double weight, Nutrients nutrientContent)
        {
            Id = id;
            Name = name;
            Weight = weight;
            NutrientContent = nutrientContent;
            Ingredients = new List<Food>();
        }

        public void AddIngredient(Food food)
        {
            Ingredients.Add(food);
        }

        public bool RemoveIngredient(int foodId)
        {
            var ingredient = Ingredients.FirstOrDefault(f => f.Id == foodId);

            if (ingredient != null)
            {
                Ingredients.Remove(ingredient);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// <see cref="Nutrients"/> content per 100g of Food
    /// </summary>
    public struct Nutrients
    {
        public Fat FatContent { get; set; }
        public Carbohydrates CarbohydrateContent { get; set; }
        public double Protein { get; set; }
        public double Salt { get; set; }

        public Nutrients(Fat fatContent, Carbohydrates carbohydrateContent, double protein, double salt)
        {
            FatContent = fatContent;
            CarbohydrateContent = carbohydrateContent;
            Protein = protein;
            Salt = salt;
        }
    }

    public struct Fat
    {
        public double Total { get; set; }
        public double Saturated { get; set; }

        public Fat(double total, double saturated)
        {
            Total = total;
            Saturated = saturated;
        }

        public static Fat operator +(Fat f1, Fat f2)
        {
            return new Fat(f1.Total + f2.Total, f1.Saturated + f2.Saturated);
        }

        public static Fat operator -(Fat f1, Fat f2)
        {
            return new Fat(f1.Total - f2.Total, f1.Saturated - f2.Saturated);
        }
    }

    public struct Carbohydrates
    {
        public double Total { get; set; }
        public double Sugar { get; set; }

        public Carbohydrates(double total, double sugar)
        {
            Total = total;
            Sugar = sugar;
        }

        public static Carbohydrates operator +(Carbohydrates c1, Carbohydrates c2)
        {
            return new Carbohydrates(c1.Total + c2.Total, c1.Sugar + c2.Sugar);
        }

        public static Carbohydrates operator -(Carbohydrates c1, Carbohydrates c2)
        {
            return new Carbohydrates(c1.Total - c2.Total, c1.Sugar - c2.Sugar);
        }
    }
}

public class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello World!");
    }
}
namespace Food
{
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Nutrients NutrientContent { get; set; }
        // TODO: Maybe make this into just list of FoodIDs for simpler database work?
        public List<Food> Ingredients { get; set; }

        public Food(int id, string name, Nutrients nutrientContent)
        {
            Id = id;
            Name = name;
            NutrientContent = nutrientContent;
            Ingredients = new List<Food>();
        }

        public void AddIngredient(Food food)
        {
            Ingredients.Add(food);
        }
    }

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
    }
}
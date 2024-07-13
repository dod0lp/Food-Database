using Food;

namespace Food
{
    // There is no check if some values are negative, for example negative amount of Protein, Fat...
    // If there is value not set, the default value in most, if not all, cases will be 0
    // for easier setting up of values, so we can't say when value was not set, but we can set
    // value of something to -1 to explicitly specify, that this value is not set yet
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
    /// <see cref="Nutrients"/> content of certain <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>
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

        public static Nutrients operator +(Nutrients n1, Nutrients n2)
        {
            return new Nutrients(
                n1.FatContent + n2.FatContent,
                n1.CarbohydrateContent + n2.CarbohydrateContent,
                n1.Protein + n2.Protein,
                n1.Salt + n2.Salt
            );
        }

        public static Nutrients operator -(Nutrients n1, Nutrients n2)
        {
            return new Nutrients(
                n1.FatContent - n2.FatContent,
                n1.CarbohydrateContent - n2.CarbohydrateContent,
                n1.Protein - n2.Protein,
                n1.Salt - n2.Salt
            );
        }
    }

    public struct Energy
    {
        private static readonly double KcalToKjFactor = 4.184;

        private int kcal;
        private int kj;

        public double Kcal
        {
            get => kcal;
            set
            {
                kcal = Convert.ToInt32(value);
                kj = Convert.ToInt32(value * KcalToKjFactor);
            }
        }

        public double Kj
        {
            get => kj;
            set
            {
                kj = Convert.ToInt32(value);
                kcal = Convert.ToInt32(value / KcalToKjFactor);
            }
        }

        public Energy(double value, bool isKcal = true)
        {
            if (isKcal)
            {
                Kcal = value;
            }

            else
            {
                Kj = value;
            }
        }

        public override string ToString()
        {
            return $"{kcal} kcal ({kj} kJ)";
        }
    }


    public struct Fat
    {
        public double Total { get; set; }
        public double Saturated { get; set; }

        public Fat(double total = 0, double saturated = 0)
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

        public Carbohydrates(double total = 0, double sugar = 0)
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

        Fat fat1 = new(0, 240);
        Console.WriteLine(fat1.Total);
        Console.WriteLine(fat1.Saturated);

        Energy en1 = new(100);
        Console.WriteLine(en1);
    }
}
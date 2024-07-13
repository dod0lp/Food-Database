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
        public Energy Energy { get; set; }
        public Fat FatContent { get; set; }
        public Carbohydrates CarbohydrateContent { get; set; }
        public double Protein { get; set; }
        public double Salt { get; set; }

        public Nutrients(Energy energy, Fat fatContent, Carbohydrates carbohydrateContent, double protein, double salt)
        {
            Energy = energy;
            FatContent = fatContent;
            CarbohydrateContent = carbohydrateContent;
            Protein = protein;
            Salt = salt;
        }

        public static Nutrients operator +(Nutrients n1, Nutrients n2)
        {
            return new Nutrients(
                n1.Energy + n2.Energy,
                n1.FatContent + n2.FatContent,
                n1.CarbohydrateContent + n2.CarbohydrateContent,
                n1.Protein + n2.Protein,
                n1.Salt + n2.Salt
            );
        }

        public static Nutrients operator -(Nutrients n1, Nutrients n2)
        {
            return new Nutrients(
                n1.Energy - n2.Energy,
                n1.FatContent - n2.FatContent,
                n1.CarbohydrateContent - n2.CarbohydrateContent,
                n1.Protein - n2.Protein,
                n1.Salt - n2.Salt
            );
        }

        public static Nutrients operator *(double factor, Nutrients nutrients)
        {
            return new Nutrients(
                factor * nutrients.Energy,
                factor * nutrients.FatContent,
                factor * nutrients.CarbohydrateContent,
                factor * nutrients.Protein,
                factor * nutrients.Salt
            );
        }

        public static Nutrients operator *(Nutrients nutrients, double factor)
        {
            return factor * nutrients;
        }

        public override string ToString()
        {
            return $"Energy: {Energy}, Fat: {FatContent}, Carbohydrates: {CarbohydrateContent}, Protein: {Protein}, Salt: {Salt}";
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

        public static Energy operator +(Energy e1, Energy e2)
        {
            return new Energy(e1.Kcal + e2.Kcal);
        }

        public static Energy operator -(Energy e1, Energy e2)
        {
            return new Energy(e1.Kcal - e2.Kcal);
        }

        public static Energy operator *(double factor, Energy e)
        {
            return new Energy(e.Kcal * factor);
        }

        public static Energy operator *(Energy e, double factor)
        {
            return factor * e;
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

        public static Fat operator *(double factor, Fat f)
        {
            return new Fat(f.Total * factor, f.Saturated * factor);
        }

        public static Fat operator *(Fat f, double factor)
        {
            return factor * f;
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

        public static Carbohydrates operator *(double factor, Carbohydrates c)
        {
            return new Carbohydrates(c.Total * factor, c.Sugar * factor);
        }

        public static Carbohydrates operator *(Carbohydrates c, double factor)
        {
            return factor * c;
        }
    }
}

public class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello World!");

        bool tests = true;
        if (tests)
        {
            Fat fat1 = new(0, 240);
            Console.WriteLine(fat1.Total);
            Console.WriteLine(fat1.Saturated);

            Energy en1 = new(100);
            Energy en2 = new(50);
            Console.WriteLine(en1 + en2);


            Nutrients nutrients1 = new Nutrients(
                new Energy(150),
                new Fat(30, 15),
                new Carbohydrates(70, 40),
                20,
                3.5
            );

            Nutrients nutrients2 = new Nutrients(
                new Energy(100),
                new Fat(20, 10),
                new Carbohydrates(50, 30),
                15,
                2.5
            );

            Nutrients resultAddNutrients = nutrients1 + nutrients2;
            Nutrients resultSubtractNutrients = nutrients1 - nutrients2;
            Nutrients resultScaleNutrients = 0.5 * nutrients1;

            Console.WriteLine(nutrients1);
            Console.WriteLine(nutrients2);

            Console.WriteLine($"Result of addition (Nutrients):\n{resultAddNutrients}");
            Console.WriteLine($"Result of subtraction (Nutrients):\n{resultSubtractNutrients}");
            Console.WriteLine($"Result of scaling (Nutrients):\n{resultScaleNutrients}");
        }
    }
}
namespace Food
{
    // There is no check if some values are negative, for example negative amount of Protein, Fat...
    // If there is value not set, the default value in most, if not all, cases will be 0
    // for easier setting up of values, so we can't say when value was not set, but we can set
    // value of something to -1 to explicitly specify, that this value is not set yet

    /// <summary>
    /// Class for specific operations with numbers.
    /// </summary>
    public static class NumberOperations
    {
        public static int[] lookup_powers_10 = new int[9];

        static NumberOperations()
        {
            for (int i = 0; i < lookup_powers_10.Length; i++)
            {
                lookup_powers_10[i] = (int)Math.Pow(10, i);
            }
        }

        /// <summary>
        /// Round up double to <see cref="N"/> decimal places.<br></br>
        /// </summary>
        /// <param name="number">Number to be rounded up.</param>
        /// <param name="N">Number of total decimal places.</param>
        /// <returns></returns>
        public static double RoundUpToNDecimalPlaces(double number, int N)
        {
            return Math.Ceiling(number * lookup_powers_10[N]) / lookup_powers_10[N];
        }

        /// <summary>
        /// Specialized class for rounding up to 2decimal places.
        /// </summary>
        /// <param name="number">Number to be rounded up.</param>
        /// <returns></returns>
        public static double RoundUpTo2DecimalPlaces(double number)
        {
            // return RoundUpToNDecimalPlaces(number, 2);
            return Math.Ceiling(number * 100) / 100;
        }
    }

    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }
        public Nutrients NutrientContent { get; set; }
        public List<Food> Ingredients { get; set; }
        public string Description { get; set; }

        public Food(int id, string name, double weight, Nutrients nutrientContent, string description = "")
        {
            Id = id;
            Name = name;
            Weight = weight;
            NutrientContent = nutrientContent;
            Ingredients = new List<Food>();
            Description = description;
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
    /// <br></br>
    /// Represents nutritional information including <see cref="Energy"/>, <see cref="FatContent"/>, <see cref="CarbohydrateContent"/>, <see cref="Protein"/>, and <see cref="Salt"/> content.
    /// </summary>
    public struct Nutrients
    {
        /// <summary>
        /// Gets or sets the energy content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>.
        /// </summary>
        public Energy Energy { get; set; }

        /// <summary>
        /// Gets or sets the fat content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>..
        /// </summary>
        public Fat FatContent { get; set; }

        /// <summary>
        /// Gets or sets the carbohydrate content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>..
        /// </summary>
        public Carbohydrates CarbohydrateContent { get; set; }

        /// <summary>
        /// Gets or sets the protein content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>..
        /// </summary>
        public Protein Protein { get; set; }

        /// <summary>
        /// Gets or sets the salt content of the <see cref="Food"/>, arbitrary <see cref="Food.Weight"/> - defined by <see cref="Food"/>..
        /// </summary>
        public Salt Salt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nutrients"/> struct with specified nutritional values.
        /// </summary>
        /// <param name="energy">The energy content.</param>
        /// <param name="fatContent">The fat content.</param>
        /// <param name="carbohydrateContent">The carbohydrate content.</param>
        /// <param name="protein">The protein content.</param>
        /// <param name="salt">The salt content.</param>
        public Nutrients(Energy energy, Fat fatContent, Carbohydrates carbohydrateContent, Protein protein, Salt salt)
        {
            Energy = energy;
            FatContent = fatContent;
            CarbohydrateContent = carbohydrateContent;
            Protein = protein;
            Salt = salt;
        }

        /// <summary>
        /// Adds two instances of <see cref="Nutrients"/>, combining their respective nutritional values.
        /// </summary>
        /// <param name="n1">The first <see cref="Nutrients"/> instance.</param>
        /// <param name="n2">The second <see cref="Nutrients"/> instance.</param>
        /// <returns>A new <see cref="Nutrients"/> instance with summed nutritional values.</returns>
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

        /// <summary>
        /// Subtracts one instance of <see cref="Nutrients"/> from another, subtracting their respective nutritional values.
        /// </summary>
        /// <param name="n1">The first <see cref="Nutrients"/> instance.</param>
        /// <param name="n2">The second <see cref="Nutrients"/> instance.</param>
        /// <returns>A new <see cref="Nutrients"/> instance with subtracted nutritional values.</returns>
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

        /// <summary>
        /// Scales the nutritional values of a <see cref="Nutrients"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="nutrients">The <see cref="Nutrients"/> instance to scale.</param>
        /// <returns>A new <see cref="Nutrients"/> instance with scaled nutritional values.</returns>
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

        /// <summary>
        /// Scales the nutritional values of a <see cref="Nutrients"/> instance by a specified factor.
        /// </summary>
        /// <param name="nutrients">The <see cref="Nutrients"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Nutrients"/> instance with scaled nutritional values.</returns>
        public static Nutrients operator *(Nutrients nutrients, double factor)
        {
            return factor * nutrients;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Nutrients"/> instance, displaying its nutritional values.
        /// </summary>
        /// <returns>A string representation of the <see cref="Nutrients"/> instance.</returns>
        public override readonly string ToString()
        {
            return $"Energy: {Energy}\nFat: {FatContent}\nCarbohydrates: {CarbohydrateContent}\nProtein: {Protein}\nSalt: {Salt}\n";
        }
    }

    /// <summary>
    /// Represents energy information in both <see cref="kcal"/> (kilocalories) and <see cref="kj"/> (kilojoules).
    /// </summary>
    public struct Energy
    {
        private static readonly double KcalToKjFactor = 4.184;

        private int kcal;
        private int kj;

        /// <summary>
        /// Gets or sets the energy value in <see cref="kcal"/> (kilocalories).
        /// </summary>
        public double Kcal
        {
            readonly get => kcal;

            set
            {
                kcal = (int) Math.Ceiling(value);
                kj = (int) Math.Ceiling(value * KcalToKjFactor);
            }
        }

        /// <summary>
        /// Gets or sets the energy value in <see cref="kj"/> (kilojoules).
        /// </summary>
        public double KJ
        {
            readonly get => kj;

            set
            {
                kj = (int) Math.Ceiling(value);
                kcal = (int) Math.Ceiling(value / KcalToKjFactor);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Energy"/> struct with a specified energy value.<br></br>
        /// Can initialize with either <see cref="Kcal"/> or <see cref="KJ"/>
        /// </summary>
        /// <param name="value">The energy value.</param>
        /// <param name="isKcal">Specify true if the provided value is in kcal (default), false if in kJ.</param>
        public Energy(double value, bool isKcal = true)
        {
            if (isKcal)
            {
                Kcal = value;
            }
            else
            {
                KJ = value;
            }
        }

        /// <summary>
        /// Adds two instances of <see cref="Energy"/>, combining their kcal values.
        /// </summary>
        /// <param name="e1">The first <see cref="Energy"/> instance.</param>
        /// <param name="e2">The second <see cref="Energy"/> instance.</param>
        /// <returns>A new <see cref="Energy"/> instance with summed energy values in kcal.</returns>
        public static Energy operator +(Energy e1, Energy e2)
        {
            return new Energy(e1.Kcal + e2.Kcal);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Carbohydrates"/> from another, subtracting their <see cref="Carbohydrates.Total"/> and <see cref="Carbohydrates.Sugar"/> amounts.
        /// <br></br><br ></br>
        /// In order for values to not become negative, you need to ensure that you are subtracting <see cref="Food.NutrientContent"/>.<see cref="Carbohydrates"/>
        /// <br></br>
        /// contained in other <see cref="Food.NutrientContent"/>.<see cref="Carbohydrates"/>
        /// <br></br>
        /// </summary>
        /// <param name="e1">The first <see cref="Energy"/> instance.</param>
        /// <param name="e2">The second <see cref="Energy"/> instance.</param>
        /// <returns>A new <see cref="Energy"/> instance with subtracted energy values in kcal.</returns>
        public static Energy operator -(Energy e1, Energy e2)
        {
            return new Energy(e1.Kcal - e2.Kcal);
        }

        /// <summary>
        /// Scales the energy value of a <see cref="Energy"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="e">The <see cref="Energy"/> instance to scale.</param>
        /// <returns>A new <see cref="Energy"/> instance with scaled energy values in kcal.</returns>
        public static Energy operator *(double factor, Energy e)
        {
            return new Energy(e.Kcal * factor);
        }

        /// <summary>
        /// Scales the energy value of a <see cref="Energy"/> instance by a specified factor.
        /// </summary>
        /// <param name="e">The <see cref="Energy"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Energy"/> instance with scaled energy values in kcal.</returns>
        public static Energy operator *(Energy e, double factor)
        {
            return factor * e;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Energy"/> instance, displaying its energy values in kcal and kJ.
        /// </summary>
        /// <returns>A string representation of the <see cref="Energy"/> instance.</returns>
        public override readonly string ToString()
        {
            return $"{Kcal} kcal ({KJ} kJ)";
        }
    }

    // TODO: figure out how this shit will work to have Total, Operators inherit....
    public struct Nutrient
    {

    }

    /// <summary>
    /// Represents nutritional information related to <see cref="Fat"/>.
    /// </summary>
    public struct Fat
    {
        /// <summary>
        /// Gets or sets the total amount of fat in grams.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Gets or sets the amount of saturated fat in grams.
        /// </summary>
        public double Saturated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fat"/> struct with specified values.
        /// </summary>
        /// <param name="total">The total amount of fat in grams (default is 0).</param>
        /// <param name="saturated">The amount of saturated fat in grams (default is 0).</param>
        public Fat(double total = 0, double saturated = 0)
        {
            Total = NumberOperations.RoundUpTo2DecimalPlaces(total);
            Saturated = NumberOperations.RoundUpTo2DecimalPlaces(saturated);
        }

        /// <summary>
        /// Adds two instances of <see cref="Fat"/>, combining their total and saturated <see cref="Fat"/> amounts.
        /// </summary>
        /// <param name="f1">The first <see cref="Fat"/> instance.</param>
        /// <param name="f2">The second <see cref="Fat"/> instance.</param>
        /// <returns>A new <see cref="Fat"/> instance with summed <see cref="Fat"/> values.</returns>
        public static Fat operator +(Fat f1, Fat f2)
        {
            return new Fat(f1.Total + f2.Total, f1.Saturated + f2.Saturated);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Fat"/> from another, subtracting their <see cref="Fat.Total"/> and <see cref="Fat.Saturated"/> amounts.
        /// <br></br><br></br>
        /// In order for values to not become negative, you need to ensure that you are subtracting <see cref="Food.NutrientContent"/>.<see cref="Fat"/>
        /// <br></br>
        /// contained in other <see cref="Food.NutrientContent"/>.<see cref="Fat"/>
        /// <br></br>
        /// </summary>
        /// <param name="f1">The first <see cref="Fat"/> instance.</param>
        /// <param name="f2">The second <see cref="Fat"/> instance.</param>
        /// <returns>A new <see cref="Fat"/> instance with subtracted <see cref="Fat"/> values.</returns>
        public static Fat operator -(Fat f1, Fat f2)
        {
            return new Fat(f1.Total - f2.Total, f1.Saturated - f2.Saturated);
        }

        /// <summary>
        /// Scales the fat values of a <see cref="Fat"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="f">The <see cref="Fat"/> instance to scale.</param>
        /// <returns>A new <see cref="Fat"/> instance with scaled <see cref="Fat"/> values.</returns>
        public static Fat operator *(double factor, Fat f)
        {
            return new Fat(f.Total * factor, f.Saturated * factor);
        }

        /// <summary>
        /// Scales the fat values of a <see cref="Fat"/> instance by a specified factor.
        /// </summary>
        /// <param name="f">The <see cref="Fat"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Fat"/> instance with scaled <see cref="Fat"/> values.</returns>
        public static Fat operator *(Fat f, double factor)
        {
            return factor * f;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Fat"/> instance, displaying its <see cref="Fat.Total"/> and <see cref="Fat.Saturated"/> amounts.
        /// </summary>
        /// <returns>A string representation of the <see cref="Fat"/> instance.</returns>
        public override readonly string ToString()
        {
            return $"Total: {Total}, Saturated: {Saturated}";
        }
    }

    /// <summary>
    /// Represents nutritional information related to <see cref="Carbohydrates"/>.
    /// </summary>
    public struct Carbohydrates
    {
        /// <summary>
        /// Gets or sets the total amount of carbohydrates in grams.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Gets or sets the amount of sugar in grams.
        /// </summary>
        public double Sugar { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Carbohydrates"/> struct with specified values.
        /// </summary>
        /// <param name="total">The total amount of carbohydrates in grams (default is 0).</param>
        /// <param name="sugar">The amount of sugar in grams (default is 0).</param>
        public Carbohydrates(double total = 0, double sugar = 0)
        {
            Total = NumberOperations.RoundUpTo2DecimalPlaces(total);
            Sugar = NumberOperations.RoundUpTo2DecimalPlaces(sugar);
        }

        /// <summary>
        /// Adds two instances of <see cref="Carbohydrates"/>, combining their <see cref="Carbohydrates.Total"/> and <see cref="Carbohydrates.Sugar"/> amounts.
        /// </summary>
        /// <param name="c1">The first <see cref="Carbohydrates"/> instance.</param>
        /// <param name="c2">The second <see cref="Carbohydrates"/> instance.</param>
        /// <returns>A new <see cref="Carbohydrates"/> instance with summed carbohydrate values.</returns>
        public static Carbohydrates operator +(Carbohydrates c1, Carbohydrates c2)
        {
            return new Carbohydrates(c1.Total + c2.Total, c1.Sugar + c2.Sugar);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Carbohydrates"/> from another, subtracting their <see cref="Carbohydrates.Total"/> and <see cref="Carbohydrates.Sugar"/> amounts.
        /// <br></br><br ></br>
        /// In order for values to not become negative, you need to ensure that you are subtracting <see cref="Food.NutrientContent"/>.<see cref="Carbohydrates"/>
        /// <br></br>
        /// contained in other <see cref="Food.NutrientContent"/>.<see cref="Carbohydrates"/>
        /// <br></br>
        /// </summary>
        /// <param name="c1">The first <see cref="Carbohydrates"/> instance.</param>
        /// <param name="c2">The second <see cref="Carbohydrates"/> instance.</param>
        /// <returns>A new <see cref="Carbohydrates"/> instance with subtracted <see cref="Carbohydrates"/> values.</returns>
        public static Carbohydrates operator -(Carbohydrates c1, Carbohydrates c2)
        {
            return new Carbohydrates(c1.Total - c2.Total, c1.Sugar - c2.Sugar);
        }

        /// <summary>
        /// Scales the carbohydrate values of a <see cref="Carbohydrates"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="c">The <see cref="Carbohydrates"/> instance to scale.</param>
        /// <returns>A new <see cref="Carbohydrates"/> instance with scaled carbohydrate values.</returns>
        public static Carbohydrates operator *(double factor, Carbohydrates c)
        {
            return new Carbohydrates(c.Total * factor, c.Sugar * factor);
        }

        /// <summary>
        /// Scales the carbohydrate values of a <see cref="Carbohydrates"/> instance by a specified factor.
        /// </summary>
        /// <param name="c">The <see cref="Carbohydrates"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Carbohydrates"/> instance with scaled carbohydrate values.</returns>
        public static Carbohydrates operator *(Carbohydrates c, double factor)
        {
            return factor * c;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Carbohydrates"/> instance, displaying its <see cref="Carbohydrates.Total"/> and <see cref="Carbohydrates.Sugar"/> amounts.
        /// </summary>
        /// <returns>A string representation of the <see cref="Carbohydrates"/> instance.</returns>
        public override readonly string ToString()
        {
            return $"Total: {Total}, Sugar: {Sugar}";
        }
    }

    /// <summary>
    /// Represents nutritional information related to <see cref="Protein"/>.
    /// </summary>
    public struct Protein
    {
        /// <summary>
        /// Gets or sets the total amount of protein in grams.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Protein"/> struct with specified value.
        /// </summary>
        /// <param name="total">The total amount of protein in grams (default is 0).</param>
        public Protein(double total = 0)
        {
            Total = NumberOperations.RoundUpTo2DecimalPlaces(total);
        }

        /// <summary>
        /// Adds two instances of <see cref="Protein"/>, combining their <see cref="Protein.Total"/> amounts.
        /// </summary>
        /// <param name="p1">The first <see cref="Protein"/> instance.</param>
        /// <param name="p2">The second <see cref="Protein"/> instance.</param>
        /// <returns>A new <see cref="Protein"/> instance with summed protein values.</returns>
        public static Protein operator +(Protein p1, Protein p2)
        {
            return new Protein(p1.Total + p2.Total);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Protein"/> from another, subtracting their <see cref="Protein.Total"/> amounts.
        /// </summary>
        /// <param name="p1">The first <see cref="Protein"/> instance.</param>
        /// <param name="p2">The second <see cref="Protein"/> instance.</param>
        /// <returns>A new <see cref="Protein"/> instance with subtracted protein values.</returns>
        public static Protein operator -(Protein p1, Protein p2)
        {
            return new Protein(p1.Total - p2.Total);
        }

        /// <summary>
        /// Scales the protein value of a <see cref="Protein"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="p">The <see cref="Protein"/> instance to scale.</param>
        /// <returns>A new <see cref="Protein"/> instance with scaled <see cref="Protein.Total"/> value.</returns>
        public static Protein operator *(double factor, Protein p)
        {
            return new Protein(p.Total * factor);
        }

        /// <summary>
        /// Scales the protein value of a <see cref="Protein"/> instance by a specified factor.
        /// </summary>
        /// <param name="p">The <see cref="Protein"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Protein"/> instance with scaled <see cref="Protein.Total"/> value.</returns>
        public static Protein operator *(Protein p, double factor)
        {
            return factor * p;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Protein"/> instance, displaying its <see cref="Protein.Total"/> amount.
        /// </summary>
        /// <returns>A string representation of the <see cref="Protein"/> instance.</returns>
        public override readonly string ToString()
        {
            return $"Total: {Total}";
        }
    }

    /// <summary>
    /// Represents nutritional information related to <see cref="Salt"/>.
    /// </summary>
    public struct Salt
    {
        /// <summary>
        /// Gets or sets the total amount of salt in grams.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Salt"/> struct with specified value.
        /// </summary>
        /// <param name="total">The total amount of salt in grams (default is 0).</param>
        public Salt(double total = 0)
        {
            Total = NumberOperations.RoundUpTo2DecimalPlaces(total);
        }

        /// <summary>
        /// Adds two instances of <see cref="Salt"/>, combining their <see cref="Salt.Total"/> amounts.
        /// </summary>
        /// <param name="s1">The first <see cref="Salt"/> instance.</param>
        /// <param name="s2">The second <see cref="Salt"/> instance.</param>
        /// <returns>A new <see cref="Salt"/> instance with summed salt values.</returns>
        public static Salt operator +(Salt s1, Salt s2)
        {
            return new Salt(s1.Total + s2.Total);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Salt"/> from another, subtracting their <see cref="Salt.Total"/> amounts.
        /// </summary>
        /// <param name="s1">The first <see cref="Salt"/> instance.</param>
        /// <param name="s2">The second <see cref="Salt"/> instance.</param>
        /// <returns>A new <see cref="Salt"/> instance with subtracted salt values.</returns>
        public static Salt operator -(Salt s1, Salt s2)
        {
            return new Salt(s1.Total - s2.Total);
        }

        /// <summary>
        /// Scales the salt value of a <see cref="Salt"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="s">The <see cref="Salt"/> instance to scale.</param>
        /// <returns>A new <see cref="Salt"/> instance with scaled <see cref="Salt.Total"/> value.</returns>
        public static Salt operator *(double factor, Salt s)
        {
            return new Salt(s.Total * factor);
        }

        /// <summary>
        /// Scales the salt value of a <see cref="Salt"/> instance by a specified factor.
        /// </summary>
        /// <param name="s">The <see cref="Salt"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Salt"/> instance with scaled <see cref="Salt.Total"/> value.</returns>
        public static Salt operator *(Salt s, double factor)
        {
            return factor * s;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Salt"/> instance, displaying its <see cref="Salt.Total"/> amount.
        /// </summary>
        /// <returns>A string representation of the <see cref="Salt"/> instance.</returns>
        public override readonly string ToString()
        {
            return $"Total: {Total}";
        }
    }


    public class Program_Food
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

                Nutrients nutrients1 = new(
                    new Energy(150),
                    new Fat(30, 15),
                    new Carbohydrates(70, 40),
                    new Protein(20),
                    new Salt(3.5)
                );

                Nutrients nutrients2 = new(
                    new Energy(100),
                    new Fat(20, 10),
                    new Carbohydrates(50, 30),
                    new Protein(15),
                    new Salt(2.5)
                );

                Nutrients resultAddNutrients = nutrients1 + nutrients2;
                Nutrients resultSubtractNutrients = nutrients1 - nutrients2;
                Nutrients resultScaleNutrients = 0.5 * nutrients1;

                Console.WriteLine(nutrients1);
                Console.WriteLine(nutrients2);

                Console.WriteLine($"Result of addition (Nutrients):\n{resultAddNutrients}");
                Console.WriteLine($"Result of subtraction (Nutrients):\n{resultSubtractNutrients}");
                Console.WriteLine($"Result of scaling (Nutrients):\n{resultScaleNutrients}");

                // Rounding up tests
                Fat fat2 = new Fat(10.22222, 10.333);
                Console.WriteLine(fat2);
                Carbohydrates carbs1 = new(1.1321321, 325.43891);
                Console.WriteLine(carbs1);

                for (int i = 0; i < 9; i++)
                {
                    Console.WriteLine(NumberOperations.RoundUpToNDecimalPlaces(10.123456789, i));
                }
            }
        }
    }
}
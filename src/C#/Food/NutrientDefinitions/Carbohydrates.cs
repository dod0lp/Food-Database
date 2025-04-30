using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food
{
    /// <summary>
    /// Represents nutritional information related to <see cref="Carbohydrates"/>.
    /// </summary>
    public class Carbohydrates : Nutrient
    {
        /// <summary>
        /// Gets or sets the amount of sugar in grams.
        /// </summary>
        public double Sugar { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Carbohydrates"/> struct with specified values.
        /// </summary>
        /// <param name="total">The total amount of carbohydrates in grams (default is 0).</param>
        /// <param name="sugar">The amount of sugar in grams (default is 0).</param>
        public Carbohydrates(double total = 0, double sugar = 0) : base(total)
        {
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
        public override string ToString()
        {
            return $"{Str_Total(Total)}, Sugar: {Sugar}";
        }
    }
}
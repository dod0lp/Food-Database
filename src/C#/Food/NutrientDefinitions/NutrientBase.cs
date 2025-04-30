using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food
{
    /// <summary>
    /// Represents a nutrient with a specific total amount. 
    /// Supports various arithmetic operations such as addition, subtraction, and scaling through overloaded operators.
    /// </summary>
    /// <remarks>
    /// The <see cref="Nutrient"/> class allows users to perform the following operations:
    /// <list type="bullet">
    ///     <item><description>Addition (+): Combines the total amounts of two <see cref="Nutrient"/> instances.</description></item>
    ///     <item><description>Subtraction (-): Subtracts the total amount of one <see cref="Nutrient"/> from another.</description></item>
    ///     <item><description>Multiplication (*): Scales a <see cref="Nutrient"/> instance by a specified factor.</description></item>
    /// </list>
    /// This class ensures that the total amount of the nutrient is always rounded to two decimal places when initialized or modified.
    /// </remarks>
    public class Nutrient
    {
        /// <summary>
        /// Gets or sets the total amount of <see cref="Nutrient"/> in grams.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nutrient"/> class with specified values.
        /// </summary>
        /// <param name="total">The total amount of nutrient in grams (default is 0).</param>
        public Nutrient(double total = 0)
        {
            Total = NumberOperations.RoundUpTo2DecimalPlaces(total);
        }

        /// <summary>
        /// String for setting how total value of <see cref="Nutrient"/> is displayed
        /// </summary>
        /// <param name="total">Simply put <see cref="Total"/> here.</param>
        /// <returns></returns>
        protected static string Str_Total(double total)
        {
            return $"Total: {total}";
        }

        /// <summary>
        /// Adds two instances of <see cref="Nutrient"/>, combining their <see cref="Nutrient.Total"/> amounts.
        /// </summary>
        /// <param name="n1">The first <see cref="Nutrient"/> instance.</param>
        /// <param name="n2">The second <see cref="Nutrient"/> instance.</param>
        /// <returns>A new <see cref="Nutrient"/> instance with summed protein values.</returns>
        public static Nutrient operator +(Nutrient n1, Nutrient n2)
        {
            return new Nutrient(n1.Total + n2.Total);
        }

        /// <summary>
        /// Subtracts one instance of <see cref="Nutrient"/> from another, subtracting their <see cref="Nutrient.Total"/> amounts.
        /// </summary>
        /// <param name="n1">The first <see cref="Nutrient"/> instance.</param>
        /// <param name="n2">The second <see cref="Nutrient"/> instance.</param>
        /// <returns>A new <see cref="Nutrient"/> instance with subtracted nutrient values.</returns>
        public static Nutrient operator -(Nutrient n1, Nutrient n2)
        {
            return new Nutrient(n1.Total - n2.Total);
        }

        /// <summary>
        /// Scales the nutrient value of a <see cref="Nutrient"/> instance by a specified factor.
        /// </summary>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="n">The <see cref="Nutrient"/> instance to scale.</param>
        /// <returns>A new <see cref="Nutrient"/> instance with scaled <see cref="Nutrient.Total"/> value.</returns>
        public static Nutrient operator *(double factor, Nutrient n)
        {
            return new Nutrient(n.Total * factor);
        }

        /// <summary>
        /// Scales the nutrient value of a <see cref="Nutrient"/> instance by a specified factor.
        /// </summary>
        /// <param name="n">The <see cref="Nutrient"/> instance to scale.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <returns>A new <see cref="Nutrient"/> instance with scaled <see cref="Nutrient.Total"/> value.</returns>
        public static Nutrient operator *(Nutrient n, double factor)
        {
            return factor * n;
        }

        public override string ToString()
        {
            return $"{Str_Total(Total)}";
        }
    }
}

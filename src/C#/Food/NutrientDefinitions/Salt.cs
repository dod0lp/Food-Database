using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food
{
    /// <summary>
    /// Represents nutritional information related to <see cref="Salt"/>.
    /// </summary>
    public class Salt : Nutrient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Salt"/> class with specified values.
        /// </summary>
        /// <param name="total">The total amount of salt in grams (default is 0).</param>
        public Salt(double total = 0) : base(total) { }

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
        /// <returns>A new <see cref="Salt"/> instance with subtracted protein values.</returns>
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

    }
}

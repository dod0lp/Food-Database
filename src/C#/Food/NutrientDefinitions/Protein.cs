using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food
{
    /// <summary>
    /// Represents nutritional information related to <see cref="Protein"/>.
    /// </summary>
    public class Protein : Nutrient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Protein"/> class with specified values.
        /// </summary>
        /// <param name="total">The total amount of protein in grams (default is 0).</param>
        public Protein(double total = 0) : base(total) { }

        /// <summary>
        /// Adds two instances of <see cref="Protein"/>, combining their <see cref="Protein.Total"/> amounts.
        /// </summary>
        /// <param name="p1">The first <see cref="Protein"/> instance.</param>
        /// <param name="p2">The second <see cref="Protein"/> instance.</param>
        /// <returns>A new <see cref="Protein"/> instance with summed protein values.</returns>
        public static Protein operator +(Protein p1, Protein p2)
        {
            // return new Protein(((Nutrient)p1 + (Nutrient)p2).Total);
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
    }
}
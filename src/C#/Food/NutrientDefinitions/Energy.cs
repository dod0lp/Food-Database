using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food
{
    /// <summary>
    /// Represents energy information in both <see cref="kcal"/> (kilocalories) and <see cref="kj"/> (kilojoules).
    /// </summary>
    public struct Energy
    {
        private static readonly double KcalToKjFactor = 4.184F;

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
                kcal = (int)Math.Ceiling(value);
                kj = (int)Math.Ceiling(value * KcalToKjFactor);
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
                kj = (int)Math.Ceiling(value);
                kcal = (int)Math.Ceiling(value / KcalToKjFactor);
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
}

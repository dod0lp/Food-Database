namespace Food
{

    /// <summary>
    /// Represents nutritional information related to <see cref="Fat"/>.
    /// </summary>
    public class Fat : Nutrient
    {
        /// <summary>
        /// Gets or sets the amount of saturated fat in grams.
        /// </summary>
        public double Saturated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fat"/> struct with specified values.
        /// </summary>
        /// <param name="total">The total amount of fat in grams (default is 0).</param>
        /// <param name="saturated">The amount of saturated fat in grams (default is 0).</param>
        public Fat(double total = 0, double saturated = 0) : base(total)
        {
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
        public override string ToString()
        {
            return $"{Str_Total(Total)}, Saturated: {Saturated}";
        }
    }
}
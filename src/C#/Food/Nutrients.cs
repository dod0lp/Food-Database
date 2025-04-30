namespace Food
{
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
}
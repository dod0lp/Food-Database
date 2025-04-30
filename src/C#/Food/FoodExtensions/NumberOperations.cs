namespace Food
{
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
}
using System.Collections.Generic;

namespace PayeGames.AsalYolculuk.Core
{
    public static class PrimeMathEngine
    {
        private const int MaxCachedNumber = 10000;

        private static readonly HashSet<int> PrimeCache = BuildSieve(MaxCachedNumber);

        private static HashSet<int> BuildSieve(int max)
        {
            bool[] isComposite = new bool[max + 1];
            HashSet<int> primes = new HashSet<int>();

            for (int number = 2; number <= max; number++)
            {
                if (isComposite[number]) continue;

                primes.Add(number);
                for (int multiple = number * number; multiple <= max; multiple += number)
                {
                    isComposite[multiple] = true;
                }
            }

            return primes;
        }

        public static bool IsPrime(int number)
        {
            if (number < 2) return false;
            if (number <= MaxCachedNumber) return PrimeCache.Contains(number);

            for (int divisor = 2; divisor * divisor <= number; divisor++)
            {
                if (number % divisor == 0) return false;
            }

            return true;
        }

        public static List<int> GetPrimeFactors(int number)
        {
            List<int> factors = new List<int>();
            int remaining = number;

            for (int divisor = 2; divisor * divisor <= remaining; divisor++)
            {
                while (remaining % divisor == 0)
                {
                    factors.Add(divisor);
                    remaining /= divisor;
                }
            }

            if (remaining > 1) factors.Add(remaining);

            return factors;
        }

        public static (int factorA, int factorB) GetDivisionPair(int compositeNumber)
        {
            for (int divisor = 2; divisor * divisor <= compositeNumber; divisor++)
            {
                if (compositeNumber % divisor == 0)
                {
                    return (divisor, compositeNumber / divisor);
                }
            }

            return (1, compositeNumber);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace PN_MRalg
{
    class MRalg
    {
        private Random random;

        public MRalg(int seed)
        {
            random = new Random(seed);
        }
        // HAC A4.24
        public bool Composite(BigInteger n, int t)
        {
            if (n == 2 || n == 3)
                return false;

            BigInteger m = n % 2;

            if (m == 0)
                return true;

            BigInteger n1 = n - 1;
            BigInteger r = n1;
            long s = 0;

            m = r % 2;

            while (m == 0)
            {
                r = r / 2;
                m = r % 2;
                s++;
            }

            BigInteger n2 = n - 2;

            for (int i = 1; i <= t; i++)
            {
                BigInteger a = RandomRange(2, n2);
                BigInteger y = BigInteger.ModPow(a, r, n);

                if (y != 1 && y != n1)
                {
                    int j = 1;

                    while (j <= s && y != n1)
                    {
                        y = BigInteger.ModPow(y, 2, n);

                        if (y == 1)
                            return true;

                        j++;
                    }

                    if (y != n1)
                        return true;
                }
            }

            return false;
        }

        public BigInteger RandomRange(
          BigInteger lower, BigInteger upper)
        {
            if (lower <= long.MaxValue && upper <= long.MaxValue && lower < upper)
            {
                BigInteger r;

                while (true)
                {
                    r = lower + (long)(((long)upper - (long)lower) * random.NextDouble());

                    if (r >= lower && r <= upper)
                        return r;
                }
            }

            BigInteger delta = upper - lower;
            byte[] deltaBytes = delta.ToByteArray();
            byte[] buffer = new byte[deltaBytes.Length];

            while (true)
            {
                random.NextBytes(buffer);

                BigInteger r = new BigInteger(buffer) + lower;

                if (r >= lower && r <= upper)
                    return r;
            }
        }
    }
}

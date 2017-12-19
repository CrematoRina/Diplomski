using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PN_SSalg
{
    class SSalg
    {

        private Random random;

        public SSalg(int seed)
        {
            random = new Random(seed);
        }
        // HAC A2.149
        public int Jacobi(BigInteger a, BigInteger n)
        {

            if (a == 0)
                return 0;

            if (a == 1)
                return 1;

            int e = 0;
            BigInteger a1 = a;
            BigInteger quotient = a1 / 2;
            BigInteger remainder = a1 % 2;

            while (remainder == 0)
            {
                e++;
                a1 = quotient;
                quotient = a1 / 2;
                remainder = a1 % 2;
            }

            int s = 0;

            if (e % 2 == 0)
                s = 1;

            else
            {
                BigInteger mod8 = n % 8;

                if (mod8 == 1 || mod8 == 7)
                    s = +1;

                if (mod8 == 3 || mod8 == 5)
                    s = -1;
            }

            BigInteger mod4 = n % 4;
            BigInteger a14 = a1 % 4;

            if (mod4 == 3 && a14 == 3)
                s = -s;

            BigInteger n1 = n % a1;

            if (a1 == 1)
                return s;

            else
                return s * Jacobi(n1, a1);
        }

       
        public BigInteger RandomRange(BigInteger lower, BigInteger upper)
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
        //HAC A4.18
        public bool Composite(BigInteger n, int t)
        {
            BigInteger n1 = n - 1, n2 = n - 2, n12 = n1 / 2;

            for (int i = 1; i <= t; i++)
            {
                BigInteger a = RandomRange(2, n2);
                BigInteger r = BigInteger.ModPow(a, n12, n);

                if (r != 1 && r != n1)
                    return false;

                int s = Jacobi(a, n);

                if (!(r == s) && !(s == -1 && r == n1))
                    return false;
            }

            return true;
        }
    }
}

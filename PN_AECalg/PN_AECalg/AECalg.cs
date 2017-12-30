using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.ComponentModel;

namespace PN_AECalg
{
    class AECalg
    {
     private int seed;
        private long B0;
        private BackgroundWorker bw;
        private EllipticCurve ecs;
        private Algorithms hcs;
        private PolynomialMethods pms;
        private Random random;
        private Action<string> SetText;
        private List<long> primes;

        public AECalg(int seed, long B0, 
            BackgroundWorker bw, Action<string> SetText)
        {
            this.seed = seed;
            this.B0 = B0;
            this.bw = bw;
            this.SetText = SetText;
            random = new Random(seed);
            ecs = new EllipticCurve();
            hcs = new Algorithms(seed);
            pms = new PolynomialMethods(hcs);
            GetPrimes();
        }

        public struct DiophantinePoint
        {
            public BigInteger x, y;
        }
        
        //Digit-by-digit algorithm for square root (wiki)
        private BigInteger Sqrt(BigInteger n)
        {
            if (n == 0)
                return 0;

            BigInteger r = Sqrt(n >> 2);
            BigInteger r2 = r << 1;
            BigInteger s = r2 + 1;

            if (n < s * s)
                return r2;

            else
                return s;
        }

        //Cohen A1.7.3
        private bool SquareTest(BigInteger n, ref BigInteger s)
        {
            bool square = true;
            long[] q11 = new long[11];
            long[] q63 = new long[63];
            long[] q64 = new long[64];
            long[] q65 = new long[65];
            long k, r, t;

            for (k = 0; k < 11; k++) q11[k] = 0;
            for (k = 0; k < 6; k++) q11[(k * k) % 11] = 1;
            for (k = 0; k < 63; k++) q63[k] = 0;
            for (k = 0; k < 32; k++) q63[(k * k) % 63] = 1;
            for (k = 0; k < 64; k++) q64[k] = 0;
            for (k = 0; k < 32; k++) q64[(k * k) % 64] = 1;
            for (k = 0; k < 65; k++) q65[k] = 0;
            for (k = 0; k < 33; k++) q65[(k * k) % 65] = 1;
            t = (long)(n % 64);
            r = (long)(n % 45045);
            if (q64[t] == 0) square = false;
            if (square && q63[r % 63] == 0) square = false;
            if (square && q65[r % 65] == 0) square = false;
            if (square && q11[r % 11] == 0) square = false;
            if (square)
            {
                s = Sqrt(n);
                if (s * s != n)
                    square = false;
            }
            return square;
        }
        //Cohen A1.5.3
        private bool ModCornacchia(long D, BigInteger p, ref DiophantinePoint dp)
        {
            long D2 = D % 2;

            if (D2 < 0)
                D2 += 2;

            dp = new DiophantinePoint();

            if (p == 2)
            {
                if (D + 8 >= 0)
                {
                    BigInteger s = 0;

                    if (SquareTest(D + 8, ref s))
                    {
                        dp.x = s;
                        dp.y = 1;
                        return true;
                    }
                }

                return false;
            }

            int k = hcs.Jacobi(-D, p);

            if (k == -1)
                return false;

            BigInteger x0 = hcs.SquareRootModPrime(-D, p);

            if (x0 == 0)
                return false;

            if (x0 % 2 != D2)
                x0 = p - x0;

            BigInteger a = 2 * p, b = x0;
            BigInteger l = 2 * Sqrt(p);
            BigInteger r = -1;

            while (b > l)
            {
                r = a % b;
                if (r < 0)
                    r += b;
                a = b;
                b = r;
            }

            long aD = -D;
            BigInteger c = (((4 * p) - (b * b)) / aD) % p, sc = 0;
            if (c < 0) c += p;
            bool test = SquareTest(c, ref sc);
            if (c <= 1 || ((4 * p) % p - (b * b) % p) % aD == 0 || !test)
                return false;
            dp.x = b;
            dp.y = sc;
            return true;
        }

        private void GetPrimes()
        {
            bool[] sieve = new bool[B0];

            primes = new List<long>();

            // sieve of Eratosthenes

            int limit = (int)B0 - 1;
            int i, k, n, nn, sqrtLimit = (int)Math.Sqrt(limit);

            // initialize the prime number sieve

            for (n = 2; n <= limit; n++)
                sieve[n] = true;

            // eliminate the multiples of n

            for (n = 2; n <= sqrtLimit; n++)
                for (i = 2; i <= n - 1; i++)
                    sieve[i * n] = false;

            // eliminate squares

            for (n = 2; n <= sqrtLimit; n++)
            {
                if (sieve[n])
                {
                    k = 0;
                    nn = n * n;
                    i = nn + k * n;

                    while (i <= limit)
                    {
                        sieve[i] = false;
                        i = nn + k * n;
                        k++;
                    }
                }
            }

            for (n = 2; n <= limit; n++)
                if (sieve[n])
                    primes.Add(n);
        }
        // Cohen A8.1.1 (Trial Division)
        public BigInteger FindFactor(
           BigInteger m, BigInteger Ni)
        {
            long p, upper = 0;
            BigInteger ms = Sqrt(m), q, s = Sqrt(Ni), t, t2;

            if (!hcs.Composite(m, 20))
                return -1;

            if (s * s < Ni)
                s++;

            t = Sqrt(s);

            if (t * t < s)
                t++;

            t2 = t * t;

            if (ms > B0)
                upper = B0;

            else
                upper = (long)s;

            for (int i = 0; i < primes.Count; i++)
            {
                p = primes[i];

                if (p > upper)
                    return -1;

                if (m % p == 0)
                {
                    q = m / p;

                    if (!hcs.Composite(q, 20) && q > t2)
                        return q;

                    m = q;

                    while (m > 1 && m % p == 0)
                        m /= p;

                    if (m == 1)
                        return -1;

                    if (!hcs.Composite(m, 20))
                    {
                        if (m > t2)
                            return m;

                        else
                            return -1;
                    }
                }
            }

            return -1;
        }
        //Cohen P5.3.1
        private int w(long D)
        {
            // class number of an imaginary quadratic field
            if (D < -4)
                return 2;
            else if (D == -4)
                return 4;
            return 6;
        }
       //Cohen S7.6.1
        private Complex Delta(Complex tau)
        {
            int n = 1, sign = -1;
            Complex arg = tau * new Complex(0.0, 2.0 * Math.PI);
            Complex q = Complex.Exp(arg);
            Complex sum = 1.0;

            while (true)
            {
                Complex term1 = Complex.Pow(q, (n * (3.0 * n - 1)) / 2.0);
                Complex term2 = Complex.Pow(q, (n * (3.0 * n + 1)) / 2.0);
                Complex terms = term1 + term2;

                sum += sign * terms;

                if (Complex.Abs(terms) < 1.0e-16)
                    break;

                n++;
                sign = -sign;
            }

            return q * Complex.Pow(sum, 24.0);
        }
        //Cohen S7.6.1
        private Complex j(Complex tau)
        {
            Complex delta1 = Delta(tau);
            Complex delta2 = Delta(2.0 * tau);
            Complex ftau, numer;

            if (Complex.Abs(delta1) < 1.0e-8 && Complex.Abs(delta2) < 1.0e-8)
            {
                delta1 = 1.0e-8;
                ftau = 0;
            }

            else
                ftau = delta2 / delta1;

            numer = 256 * ftau + 1.0;
            return Complex.Pow(numer, 3) / delta1;
        }

        private Complex[] PolyMul(int r, int s, Complex[] u, Complex[] v)
        {
            Complex[] w = new Complex[r + s + 1];

            for (int k = r + s; k >= 0; k--)
            {
                Complex sum = 0;

                for (int j = 0; j <= k; j++)
                {
                    Complex uj = 0;
                    Complex vk = 0;

                    if (j <= r)
                        uj = u[j];

                    if (k - j <= s)
                        vk = v[k - j];

                    sum += uj * vk;
                }

                w[k] = sum;
            }

            return w;
        }
        // Cohen A7.6.1
        public List<BigInteger> HilbertClassPolynomial(long D, BigInteger p)
        {
            double aj = 0.0, t = 0, B = 0;
            long a = 0, b = 0;
            int cs = 2;
            Complex J = 0, tau = 0;
            Complex sD = Complex.Sqrt(D);
            Complex[] P = new Complex[1];
            Complex[] Q = new Complex[2];
            Complex[] R = new Complex[3];
            BigInteger lpi = 0;
            List<BigInteger> lp = new List<BigInteger>();

            P[0] = 1.0;
            Q[1] = 1.0;
            R[2] = 1.0;
            b = D % 2;
            if (b < 0) b += 2;
            B = (long)Math.Floor(Math.Sqrt(Math.Abs(D) / 3.0));

            switch (cs)
            {
                case 2:
                    t = (b * b - D) / 4;
                    a = Math.Max(b, 1);
                    goto case 3;
                case 3:
                    if (t % a != 0)
                        goto case 4;
                    tau = (-b + sD) / (2.0 * a);
                    J = j(tau);
                    J = (Complex)((BigInteger)J.Real % p);

                    if (a == b || a * a == t || b == 0)
                    {
                        Q[0] = -J;
                        P = PolyMul(P.Length - 1, 1, P, Q);

                        for (int i = 0; i < P.Length; i++)
                        {
                            lpi = ((BigInteger)Math.Round(P[i].Real)) % p;
                            P[i] = (Complex)lpi;
                        }
                    }
                    else
                    {
                        aj = Complex.Abs(J);
                        R[0] = new Complex(aj * aj, 0.0);
                        R[1] = new Complex(2.0 * J.Real, 0.0);
                        P = PolyMul(P.Length - 1, 2, P, R);

                        for (int i = 0; i < P.Length; i++)
                        {
                            lpi = ((BigInteger)Math.Round(P[i].Real)) % p;
                            P[i] = (Complex)lpi;
                        }
                    }
                    goto case 4;
                case 4:
                    a++;

                    if (a * a <= t)
                        goto case 3;
                    goto case 5;
                case 5:
                    b += 2;

                    if (b <= B)
                        goto case 2;

                    for (int i = 0; i < P.Length; i++)
                    {
                        lpi = ((BigInteger)Math.Round(P[i].Real)) % p;
                        if (lpi < 0)
                            lpi += p;
                        lp.Add(lpi);
                    }

                    return lp;
            }

            return null;
        }

        private long NextDiscriminant(ref long previous)
        {
            long D, modD4;

            while (true)
            {
                previous++;
                D = previous;
                modD4 = D % 4;

                if (modD4 == 0 || modD4 == 1)
                    return -D;
            }
        }
        //Cohen A9.2.4
        public int Atkin(BigInteger N)
        {
            if (N % 2 == 0 || hcs.Composite(N, 20))
                return 0;
            if(N != 3)
            { 
                BigInteger greatest = BigInteger.GreatestCommonDivisor(N, 6);

                if (greatest != 1)
                    return -2;
            }

            bool rm = false;
            int cs = 2, i = 0, jac = 0, n = -1;
            long D = 0, k = 0, lNi, slNi, previous = 3;
            string text = string.Empty;
            BigInteger a = 0, ainv = 0, b = 0, c = 0, sc = 0, jRoot = 0, m = 0;
            BigInteger g = 0, Ni = N, q = 0, x = 0, y = 0;
            DiophantinePoint dp = new DiophantinePoint();
            ECPoint P = new ECPoint(), P1 = new ECPoint(), P2 = new ECPoint();
            List<BigInteger> T = null, root = null;
            List<BigInteger> printed = new List<BigInteger>();

            switch (cs)
            {
                case 2:
                    if (bw.CancellationPending)
                        return -1;
                    if (Ni <= B0)
                    {
                        if (Ni == 3 || Ni == 5)
                            return 1;
                        lNi = (long)Ni;
                        slNi = (long)Math.Sqrt(lNi);
                        for (int r = 0; r <= slNi; r++)
                        {
                            if (lNi % primes[r] == 0)
                                return 0;
                        }
                        SetText(text);
                        return 1;
                    }
                    rm = hcs.Composite(Ni, 20);
                    goto case 3;
                case 3:
                    if (bw.CancellationPending)
                        return -1;
                    n++;
                    if (n == 1000000000)
                    {
                        SetText(text);
                        return -3;
                    }
                    D = NextDiscriminant(ref previous);
                    if (-D > Ni)
                        return -3;
                    jac = hcs.Jacobi(D, Ni);
                    if (jac == -1)
                        goto case 3;
                    if (!ModCornacchia(D, Ni, ref dp))
                        goto case 3;
                    goto case 4;
                case 4:
                    if (bw.CancellationPending)
                        return -1;
                    m = Ni + 1 + dp.x;
                    q = FindFactor(m, Ni);
                    if (q != -1)
                        goto case 5;
                    m = Ni + 1 - dp.x;
                    q = FindFactor(m, Ni);
                    if (q != -1)
                        goto case 5;
                    if (D == -4)
                    {
                        m = Ni + 1 + 2 * dp.y;
                        q = FindFactor(m, Ni);
                        if (q != -1)
                            goto case 5;
                        m = Ni + 1 - 2 * dp.y;
                        q = FindFactor(m, Ni);
                        if (q != -1)
                            goto case 5;
                    }
                    if (D == -3)
                    {
                        m = Ni + 1 + (dp.x + 3 * dp.y);
                        q = FindFactor(m, Ni);
                        if (q != -1)
                            goto case 5;
                        m = Ni + 1 - (dp.x + 3 * dp.y);
                        q = FindFactor(m, Ni);
                        if (q != -1)
                            goto case 5;
                        m = Ni + 1 + (dp.x - 3 * dp.y);
                        q = FindFactor(m, Ni);
                        if (q != -1)
                            goto case 5;
                        m = Ni + 1 - (dp.x - 3 * dp.y);
                        q = FindFactor(m, Ni);
                        if (q != -1)
                            goto case 5;
                    }
                    goto case 3;
                case 5:
                    if (bw.CancellationPending)
                        return -1;
                    goto case 6;
                case 6:
                    if (bw.CancellationPending)
                        return -1;
                    if (D == -4)
                    {
                        a = -1;
                        b = +0;
                    }
                    else if (D == -3)
                    {
                        a = +0;
                        b = -1;
                    }
                    else
                    {
                        T = HilbertClassPolynomial(D, Ni);
                        pms.FindRootsModuloPrime(Ni, T, ref root);
                        if (root.Count == 0)
                            goto case 3;
                        jRoot = root[random.Next(root.Count)];
                        a = jRoot - 1728;
                        if (a < 0)
                            a += Ni;
                        ainv = pms.Inverse(a, Ni);
                        c = (jRoot * ainv) % Ni;
                        if (c == 0)
                            goto case 3;
                        if (c < 0)
                            c += Ni;
                        a = (-3 * c) % Ni;
                        if (a < 0)
                            a += Ni;
                        b = (2 * c) % Ni;
                        if (b < 0)
                            b += Ni;
                    }
                    goto case 7;
                case 7:
                    if (bw.CancellationPending)
                        return -1;
                    g = hcs.RandomRange(1, Ni - 1);
                    while (hcs.Jacobi(g, Ni) != -1)
                        g = hcs.RandomRange(1, Ni - 1);
                    if (D == -3)
                    {
                        if (BigInteger.ModPow(g, (Ni - 1) / 3, Ni) == -1)
                            goto case 7;
                    }
                    goto case 8;
                case 8:
                    if (bw.CancellationPending)
                        return -1;
                    x = hcs.RandomRange(1, Ni - 1);
                    y = ((((x * x) % Ni) * x) % Ni + (a * x) % Ni + b) % Ni;
                    if (hcs.Jacobi(y, Ni) == -1)
                        goto case 8;
                    if (y < 0)
                        y += Ni;
                    y = hcs.SquareRootModPrime(y, Ni);
                    if (y == 0)
                        goto case 8;
                    k = 0;
                    goto case 9;
                case 9:
                    if (bw.CancellationPending)
                        return -1;
                    P.x = x;
                    P.y = y;
                    ecs.Multiply(a, m / q, Ni, P, ref P2);
                    ecs.Multiply(a, q, Ni, P2, ref P1);
                    if (ecs.PointO(P1))
                        goto case 12;
                    goto case 10;
                case 10:
                    if (bw.CancellationPending)
                        return -1;
                    k++;
                    if (k >= w(D))
                        goto case 14;
                    if (D < -4)
                    {
                        a = (a * (g * g) % Ni) % Ni;
                        b = (b * (g * g * g) % Ni) % Ni;
                    }
                    else if (D == -4)
                        a = (a * g) % Ni;
                    else
                        b = (b * g) % Ni;
                    if (a < 0)
                        a += Ni;
                    if (b < 0)
                        b += Ni;
                    goto case 8;
                case 11:
                    if (bw.CancellationPending)
                        return -1;
                    x = hcs.RandomRange(1, Ni - 1);
                    y = ((((x * x) % Ni) * x) % Ni + (a * x) % Ni + b) % Ni;
                    if (hcs.Jacobi(y, Ni) == -1)
                        goto case 8;
                    y = hcs.SquareRootModPrime(y, Ni);
                    if (y == 0)
                        goto case 11;
                    P.x = x;
                    P.y = y;
                    ecs.Multiply(a, m / q, Ni, P, ref P2);
                    ecs.Multiply(a, q, Ni, P2, ref P1);
                    if (!ecs.PointO(P1))
                        goto case 10;
                    goto case 12;
                case 12:
                    if (ecs.PointO(P2)) goto case 11;
                    if (!printed.Contains(Ni))
                    {
                        text += "N[" + i.ToString() + "] = " + Ni.ToString() + "\r\n";
                        text += "D = " + D.ToString() + "\r\n";
                        text += "a = " + a.ToString() + "\r\n";
                        text += "b = " + b.ToString() + "\r\n";
                        text += "m = " + m.ToString() + "\r\n";
                        text += "q = " + q.ToString() + "\r\n";
                        text += "P = (" + P.x + ", " + P.y + ")\r\n";
                        text += "P1 = (" + P1.x + ", " + P1.y + ")\r\n";
                        text += "P2 = (" + P2.x + ", " + P2.y + ")\r\n";
                        printed.Add(Ni);
                    }
                    goto case 13;
                case 13:
                    i++;
                    n = -1;
                    Ni = q;
                    goto case 2;
                case 14:
                    if (i == 0) return 0;
                    i--;
                    n = -1;
                    goto case 2;
            }
            return -4;
        }
    }
}

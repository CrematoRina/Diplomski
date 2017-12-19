using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PN_AECalg
{
    public struct ECPoint
    {
        public BigInteger x, y;
    }

    class EllipticCurve
    {
        public int Legendre(
          BigInteger x, BigInteger a, BigInteger b, BigInteger p)
        {
            int i = 0;
            BigInteger p1 = p - 1;
            BigInteger y = Weierstrass(a, b, x, p);
            BigInteger l = BigInteger.ModPow(y, p1 / 2, p);

            if (l == p1)
                i = -1;
            else
                i = (int)l;

            return i;
        }

        public BigInteger Weierstrass(
            BigInteger a, BigInteger b, BigInteger x, BigInteger p)
        {
            return ((((x * x) % p) * x) % p + (a * x) % p + b) % p;
        }

        public bool PointO(ECPoint P)
        {
            return P.x.IsZero && P.y.IsOne;
        }
        //HAC A2.221
        public void ExtEuclid(BigInteger a, BigInteger b,
            ref BigInteger x, ref BigInteger y, ref BigInteger d)
        {
            if (b == 0)
            {
                d = a;
                x = 1;
                y = 0;
                return;
            }

            BigInteger q, r, x2 = 1, x1 = 0, y2 = 0, y1 = 1;

            while (b != 0)
            {
                q = a / b;
                r = a - q * b;
                x = x2 - q * x1;
                y = y2 - q * y1;
                a = b;
                b = r;
                x2 = x1;
                x1 = x;
                y2 = y1;
                y1 = y;
            }

            d = a;
            x = x2;
            y = y2;
        }
       //HAC A2.226
        public BigInteger Inverse(BigInteger a, BigInteger p)
        {
            BigInteger d = 0, x = 0, y = 0, res = 0;

            ExtEuclid(a, p, ref x, ref y, ref d);

            if (d == 1)
                res = x;
            else
                res = 0;

            return res;
        }

        public bool Add(BigInteger a, BigInteger p, ECPoint P,
            ECPoint Q, ref ECPoint R)
        // elliptic curve point partial addition 
        {
            BigInteger lambda = -1;

            if (PointO(P) && PointO(Q))
            {
                R = new ECPoint();
                R.x = new BigInteger(0);
                R.y = new BigInteger(1);
                return true;
            }

            BigInteger ps = (p - Q.y) % p;

            if (P.x == Q.x && P.y == ps)
            {
                R = new ECPoint();
                R.x = new BigInteger(0);
                R.y = new BigInteger(1);
                return true;
            }

            if (PointO(P))
            {
                R = Q;
                return true;
            }

            if (PointO(Q))
            {
                R = P;
                return true;
            }

            if (P.x != Q.x)
            {
                BigInteger i = (Q.x - P.x) % p;
                BigInteger y = (Q.y - P.y) % p;

                if (i < 0)
                    i += p;

                i = Inverse(i, p);

                if (i == 0)
                    return false;

                lambda = (i * y) % p;
            }
            else
            {
                BigInteger y2 = (2 * P.y) % p;
                BigInteger i = Inverse(y2, p);

                if (i < 0)
                    i += p;

                if (i == 0)
                    return false;

                BigInteger x3 = (3 * P.x) % p;

                x3 = (x3 * P.x) % p;
                x3 = (x3 + a) % p;
                lambda = (x3 * i) % p;
            }

            BigInteger lambda2 = (lambda * lambda) % p;
            BigInteger delta1 = (P.x + Q.x) % p;

            R.x = (lambda2 - delta1) % p;

            BigInteger delta2 = (P.x - R.x) % p;

            delta2 = (lambda * delta2) % p;
            R.y = (delta2 - P.y) % p;

            if (R.x < 0)
                R.x += p;

            if (R.y < 0)
                R.y += p;

            return true;
        }

        public bool Multiply(
            BigInteger a, long n, BigInteger p,
            ECPoint P, ref ECPoint R)
        {
            long k = n;
            ECPoint S;

            R = new ECPoint();
            R.x = 0;
            R.y = 1;
            S = P;

            while (k != 0)
            {
                if ((k & 1) == 1)
                    if (!Add(a, p, R, S, ref R))
                        return false;

                k >>= 1;

                if (k != 0)
                    if (!Add(a, p, S, S, ref S))
                        return false;
            }

            return true;
        }

        public bool Multiply(
            BigInteger a, BigInteger n, BigInteger p,
            ECPoint P, ref ECPoint R)
        {
            BigInteger k = n;
            ECPoint S;

            R = new ECPoint();
            R.x = 0;
            R.y = 1;
            S = P;

            while (k != 0)
            {
                BigInteger m = k % 2;

                if (m == 1)
                    if (!Add(a, p, R, S, ref R))
                        return false;

                k = k / 2;

                if (k != 0)
                    if (!Add(a, p, S, S, ref S))
                        return false;
            }

            return true;
        }

        public ECPoint Negate(BigInteger p, ECPoint P)
        {
            ECPoint Q = new ECPoint();

            Q.x = P.x;
            Q.y = (p - P.y) % p;

            if (Q.y < 0)
                Q.y += p;

            return Q;
        }
    }
}

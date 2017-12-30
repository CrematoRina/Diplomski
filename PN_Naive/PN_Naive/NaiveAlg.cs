using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.ComponentModel;


namespace PN_Naive
{
    
    class NaiveAlg
    {
        private BackgroundWorker bw;
        private int prime = 1;

       public NaiveAlg(BackgroundWorker bw)
        {
            this.bw = bw;
        }

       public BigInteger Sqrt(BigInteger n)
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

        public int Composite(BigInteger num)
        {
            BigInteger sqrtn = Sqrt(num);
            if (num == 1)
                return 0;
            for (BigInteger i = 2; i <= sqrtn; i++)
            {
                if (bw.CancellationPending)
                    return -1;

                if (num % i == 0) prime = 0;
            }

            return prime;
        }

    }
}

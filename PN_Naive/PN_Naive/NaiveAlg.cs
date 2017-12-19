using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PN_Naive
{
    
    class NaiveAlg
    {
       // private BigInteger num;
        private bool prime = true;

        public bool Composite(BigInteger num)
        {
            for (BigInteger i = 2; i < num; i++)
                if (num % i == 0) prime = false;

            return prime;
        }

    }
}

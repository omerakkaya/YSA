using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YapaySinirAglarıOdevi
{
    class CikisNoron : Noron, INoron
    {
        public void toplamaBirimi(double girdi, double agirlik)
        {
            setNetGirdi(getNetGirdi() + girdi * agirlik);
        }

        public double transferBirimi()
        {
            return (double)(1 / (1 + Math.Exp(-getNetGirdi())));
        }
    }
}

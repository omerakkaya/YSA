using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YapaySinirAglarıOdevi
{
    class Noron
    {
        double netGirdi = 0;
        double delta = 0;

        public double getNetGirdi()
        {
            return netGirdi;
        }

        public void setNetGirdi(double netGirdi)
        {
            this.netGirdi = netGirdi;
        }

        public void netGirdiSifirla()
        {
            netGirdi = 0;
        }

        public void setDelta(double delta)
        {
            this.delta = delta;
        }

        public double getDelta()
        {
            return delta;
        }
    }
}

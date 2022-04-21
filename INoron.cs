using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YapaySinirAglarıOdevi
{
    interface INoron
    {
        void toplamaBirimi(double girdi, double agirlik);

        double transferBirimi();

        void setDelta(double delta);

        double getDelta();
    }
}

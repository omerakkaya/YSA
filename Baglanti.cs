using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YapaySinirAglarıOdevi
{
    class Baglanti
    {
        Noron baglantiNoron1;
        Noron baglantiNoron2;
        double agAgirligi = 0;
        double degisimMiktari = 0;

        public Baglanti(Noron baglantiNoron1, Noron baglantiNoron2)
        {
            this.baglantiNoron1 = baglantiNoron1;
            this.baglantiNoron2 = baglantiNoron2;
        }

        public void setAgirlik(double agAgirligi)
        {
            this.agAgirligi = agAgirligi;
        }

        public double getAgirlik()
        {
            return agAgirligi;
        }

        public Noron getBaglantiNoron1()
        {
            return baglantiNoron1;
        }

        public Noron getBaglantiNoron2()
        {
            return baglantiNoron2;
        }

        public void setDegisim(double degisimMiktari)
        {
            this.degisimMiktari = degisimMiktari;
        }

        public double getDegisim()
        {
            return degisimMiktari;
        }
    }
}

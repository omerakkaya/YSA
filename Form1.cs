using IronXL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace YapaySinirAglarıOdevi
{
    public partial class Form1 : Form
    {
        List<List<double>> egitimDataSet = new List<List<double>>(); // eğitim data setini tutan liste
        List<List<double>> testDataSet = new List<List<double>>(); // test data setini tutan liste
        List<GirisNoron> girisNoron = new List<GirisNoron>(); // giriş katmanı nöronlarını tutan liste
        List<GizliNoron> gizliNoron = new List<GizliNoron>(); // gizli katman nöronlarını tutan liste
        List<Bias> biaslar = new List<Bias>(); // biasları tutan liste
        List<Baglanti> baglantilar = new List<Baglanti>(); // ağ bağlantılarını tutan liste
        List<double> enBuyuk = new List<double>(); // excel tablosunda sütunlardaki en büyük değerleri tutan liste
        List<double> enKucuk = new List<double>(); // excel tablosunda sütunlardaki en küçük değerleri tutan liste
        CikisNoron cikisNoron; // çıkış nöronu
        const double ogrenmeKatsayisi = 0.6; // sabit öğrenme katsayısı
        const double momentumKatsayisi = 0.8; // sabit momentum katsayısı
        const int epochSayisi = 100; // sabit epoch sayısı
        Random random = new Random();
        double toplamHata = 0;
        double toplamHataTest = 0;
        double toplamKarelerHatasi = 0;
        double toplamKarelerHatasiTest = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            WorkBook workBook = new WorkBook("dataSet.xls"); // dataSet.xls dosyasını okur
            WorkSheet workSheet = workBook.WorkSheets.First();
            List<int> testVeriSetiIndexs = new List<int>();
            int testIndex = ((workSheet.RowCount - 1) * 3) / 10;

            for (int testSetiVeriIndex = 0; testSetiVeriIndex < testIndex; testSetiVeriIndex++) // random test verilerini seçer
            {
                while (true)
                {
                    int randomVeriSatiri = random.Next(1, workSheet.RowCount - 1);
                    if (!testVeriSetiIndexs.Contains(randomVeriSatiri))
                    {
                        testVeriSetiIndexs.Add(randomVeriSatiri);
                        break;
                    }
                }
            }

            for(int i = 0; i < workSheet.ColumnCount; i++) // sütunlardaki en büyük ve en küçük değerleri bulur
            {
                double max = (double)workSheet.GetColumn(i).Max();
                double min = (double)workSheet.GetColumn(i).Min();
                enBuyuk.Add(max);
                enKucuk.Add(min);
            }

            for (int rowCount = 1; rowCount < workSheet.RowCount; rowCount++)
            {
                List<double> testVeriSetiAltList = new List<double>();
                List<double> egitimVeriSetiAltList = new List<double>();
                for (int columnCount = 0; columnCount < workSheet.ColumnCount ; columnCount++) // veri normalizasyonu
                {
                    double normalizeDeger = (double)(workSheet.GetCellAt(rowCount, columnCount).DoubleValue - enKucuk[columnCount]) / (enBuyuk[columnCount] - enKucuk[columnCount]);

                    if (testVeriSetiIndexs.Contains(rowCount))
                    {
                        testVeriSetiAltList.Add(normalizeDeger);
                    }
                    else
                    {
                        egitimVeriSetiAltList.Add(normalizeDeger);
                    }
                }

                if(testVeriSetiAltList.Count != 0) // veriler eğitim veya test data setlerine yazılır
                {
                    if(testVeriSetiAltList.Last() != 0)
                    testDataSet.Add(testVeriSetiAltList);
                }
                if(egitimVeriSetiAltList.Count != 0)
                {
                    if(egitimVeriSetiAltList.Last() != 0)
                    egitimDataSet.Add(egitimVeriSetiAltList);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button3.Enabled = true;
            girisNoron.Clear();
            gizliNoron.Clear();
            baglantilar.Clear();
            WorkBook workBook = new WorkBook("dataSet.xls"); // dataSet.xls dosyasını okur
            WorkSheet workSheet = workBook.WorkSheets.First();

            int girisKatmaniNoronSayisi = workSheet.ColumnCount - 1; // giriş katmanı nöron sayısı belirlenir
            label2.Text = "Giriş Katmanı Nöron Sayısı : " + girisKatmaniNoronSayisi.ToString();
            for (int girdiNoronSayisi = 0; girdiNoronSayisi < girisKatmaniNoronSayisi; girdiNoronSayisi++) // girisNoron listesine nöron eklenir
            {
                girisNoron.Add(new GirisNoron());
            }

            int gizliKatmandakiNoronSayisi = Convert.ToInt32(textBox1.Text); // gizli katman nöron sayısı belirlenir
            label3.Text = "Gizli Katman Nöron Sayısı : " + gizliKatmandakiNoronSayisi.ToString();
            for (int gizliKatmanNoronSayisi = 0; gizliKatmanNoronSayisi < gizliKatmandakiNoronSayisi; gizliKatmanNoronSayisi++) // gizliNoron listesine nöron eklenir
            {
                gizliNoron.Add(new GizliNoron());
            }

            cikisNoron = new CikisNoron(); // çıkış nöronu tanımlanır
            label4.Text = "Çıkış Katmanı Nöron Sayısı : " + 1.ToString();

            int biasSayisi = gizliKatmandakiNoronSayisi + 1; // bias sayısı belirlenir
            for(int biasSayilari = 0; biasSayilari < biasSayisi; biasSayilari++) // biaslar listesine bias eklenir
            {
                biaslar.Add(new Bias(random.NextDouble()));
            }

            for (int girdiNoronIndex = 0; girdiNoronIndex < girisNoron.Count; girdiNoronIndex++) // giriş katmanı nöronları ile gizli katman nöronları arasındaki bağlantılar kurulur
            {
                for (int gizliKatmanNoronIndex = 0; gizliKatmanNoronIndex < gizliNoron.Count; gizliKatmanNoronIndex++)
                {
                    Baglanti baglanti1 = new Baglanti(girisNoron[girdiNoronIndex],gizliNoron[gizliKatmanNoronIndex]);
                    baglanti1.setAgirlik(random.NextDouble());
                    baglantilar.Add(baglanti1);
                }
            }

            for (int gizliNoronIndex = 0; gizliNoronIndex < gizliNoron.Count; gizliNoronIndex++) // gizli katman ile çıkış nöron arasındaki bağlantılar kurulur
            {
                Baglanti baglanti2 = new Baglanti(gizliNoron[gizliNoronIndex], cikisNoron);
                baglanti2.setAgirlik(random.NextDouble());
                baglantilar.Add(baglanti2);
            }

            for(int gizliNoronIndex = 0; gizliNoronIndex < gizliNoron.Count; gizliNoronIndex++) // gizli katman biasları ile gizli katmandaki nöronlar arasındaki bağlantılar kurulur
            {
                Baglanti baglanti3 = new Baglanti(biaslar[gizliNoronIndex], gizliNoron[gizliNoronIndex]);
                baglanti3.setAgirlik(random.NextDouble());
                baglantilar.Add(baglanti3);
            }

            Baglanti baglanti = new Baglanti(biaslar.Last(), cikisNoron); // çıkış katmanı biası ile çıkış nöronu arasındaki bağlantı kurulur
            baglanti.setAgirlik(random.NextDouble());
            baglantilar.Add(baglanti);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            List<List<double>> egitimVeriSetiKarisik = egitimDataSet.OrderBy(x => Guid.NewGuid()).ToList(); // eğitim data setini karıştırır
            egitimDataSet = egitimVeriSetiKarisik;
            for (int i = 0; i < epochSayisi; i++) // eğitim aşaması
            {
                for (int t = 0; t < egitimDataSet.Count; t++)
                {
                    // ***********************ileri besleme***********************
                    for (int j = 0; j < gizliNoron.Count; j++)
                    {
                        gizliNoron[j].netGirdiSifirla();
                    }

                    cikisNoron.netGirdiSifirla();

                    for (int j = 0; j < girisNoron.Count; j++) // giriş nöronlarına girdiler okutulur
                    {
                        girisNoron[j].setNetGirdi(egitimDataSet[t][j]);
                    }

                    for (int j = 0; j < girisNoron.Count; j++) // giriş nöronlarında gizli katman nöronlarına çıktılar gönderilir
                    {
                        for (int k = 0; k < gizliNoron.Count; k++)
                        {
                            gizliNoron[k].toplamaBirimi(girisNoron[j].getNetGirdi(), AgBul(girisNoron[j], gizliNoron[k]).getAgirlik());
                        }
                    }

                    for (int k = 0; k < gizliNoron.Count; k++) // biaslardan gizli katman nöronlarına çıktılar gönderilir
                    {
                        gizliNoron[k].toplamaBirimi(biaslar[k].getNetGirdi(), AgBul(biaslar[k], gizliNoron[k]).getAgirlik());
                    }

                    for (int j = 0; j < gizliNoron.Count; j++) // gizli katman nöronlarıdan çıkış katmanına çıktılar gönderilir
                    {
                        cikisNoron.toplamaBirimi(gizliNoron[j].transferBirimi(), AgBul(gizliNoron[j], cikisNoron).getAgirlik());
                    }
                    
                    cikisNoron.toplamaBirimi(biaslar.Last().getNetGirdi(), AgBul(biaslar.Last(), cikisNoron).getAgirlik()); // biasdan çıkış katmanına çıktılar gönderilir

                    // ***********************geri yayılım***********************
                    cikisNoron.setDelta((egitimDataSet[t].Last() - cikisNoron.transferBirimi()) * cikisNoron.transferBirimi() * (1 - cikisNoron.transferBirimi())); // çıkış nöronu deltası hesaplanır

                    for (int j = 0; j < gizliNoron.Count; j++) // gizli katman nöronlarının deltası heaplanır
                    {
                        gizliNoron[j].setDelta(cikisNoron.getDelta() * AgBul(gizliNoron[j], cikisNoron).getAgirlik() * gizliNoron[j].transferBirimi() * (1 - gizliNoron[j].transferBirimi()));
                    }

                    for (int j = 0; j < baglantilar.Count; j++) // ağırlıklar güncellenir
                    {
                        if (baglantilar[j].getBaglantiNoron2() == cikisNoron && !biaslar.Contains(baglantilar[j].getBaglantiNoron1()))
                        {
                            baglantilar[j].setDegisim(ogrenmeKatsayisi * baglantilar[j].getBaglantiNoron2().getDelta() * (1 / (1 + Math.Exp(-baglantilar[j].getBaglantiNoron1().getNetGirdi()))) + momentumKatsayisi * baglantilar[j].getDegisim());
                        }
                        else
                        {
                            baglantilar[j].setDegisim(ogrenmeKatsayisi * baglantilar[j].getBaglantiNoron2().getDelta() * baglantilar[j].getBaglantiNoron1().getNetGirdi() + momentumKatsayisi * baglantilar[j].getDegisim());
                        }
                        baglantilar[j].setAgirlik(baglantilar[j].getAgirlik() + baglantilar[j].getDegisim());
                    }

                    toplamHata += Math.Abs((egitimDataSet[t].Last() - cikisNoron.transferBirimi()) / egitimDataSet[t].Last()); // toplam hata hesaplanması

                    toplamKarelerHatasi += Math.Pow(egitimDataSet[t].Last() - cikisNoron.transferBirimi(), 2); // toplam kareler hatası hesaplanması
                }
                double mape = 100 * (toplamHata / egitimDataSet.Count); // mapenin bulunması
                double mse = toplamKarelerHatasi / egitimDataSet.Count; // msenin bulunması
                Console.WriteLine("MAPE = " + mape.ToString());
                Console.WriteLine("------------------------------------------------------------------");
                chart1.Series[0].Points.Add(mse);
                toplamHata = 0;
                toplamKarelerHatasi = 0;
                Application.DoEvents();
                if(mape <= 3)
                {
                    break;
                }
            }
            Console.WriteLine("***************EĞİTİM TAMAMLANDI****************");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                if (Convert.ToInt32(textBox1.Text) <= 20 && Convert.ToInt32(textBox1.Text) >= 2)
                {
                    button1.Enabled = true;
                }
                else
                {
                    button1.Enabled = false;
                }
            }
            else
            {
                button1.Enabled = false;
            }
        }

        private Baglanti AgBul(Noron baglantiNoron1, Noron baglantiNoron2) // iki nöron arasındaki ağ bulunur
        {
            for (int i = 0; i < baglantilar.Count; i++)
            {
                if (baglantilar[i].getBaglantiNoron1() == baglantiNoron1 && baglantilar[i].getBaglantiNoron2() == baglantiNoron2)
                {
                    return baglantilar[i];
                }
            }

            return null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("*************TEST*************");
            for (int i = 0; i < testDataSet.Count; i++) // test data setinin algoritmadan geçirilmesi
            {
                for (int j = 0; j < gizliNoron.Count; j++)
                {
                    gizliNoron[j].netGirdiSifirla();
                }

                cikisNoron.netGirdiSifirla();

                for (int j = 0; j < girisNoron.Count; j++)
                {
                    girisNoron[j].setNetGirdi(testDataSet[i][j]);
                }

                for (int j = 0; j < girisNoron.Count; j++)
                {
                    for (int k = 0; k < gizliNoron.Count; k++)
                    {
                        gizliNoron[k].toplamaBirimi(girisNoron[j].getNetGirdi(), AgBul(girisNoron[j], gizliNoron[k]).getAgirlik());
                    }
                }

                for (int k = 0; k < gizliNoron.Count; k++)
                {
                    gizliNoron[k].toplamaBirimi(biaslar[k].getNetGirdi(), AgBul(biaslar[k], gizliNoron[k]).getAgirlik());
                }

                for (int j = 0; j < gizliNoron.Count; j++)
                {
                    cikisNoron.toplamaBirimi(gizliNoron[j].transferBirimi(), AgBul(gizliNoron[j], cikisNoron).getAgirlik());
                }

                cikisNoron.toplamaBirimi(biaslar.Last().getNetGirdi(), AgBul(biaslar.Last(), cikisNoron).getAgirlik());

                toplamHataTest += Math.Abs((testDataSet[i].Last() - cikisNoron.transferBirimi()) / testDataSet[i].Last());

                toplamKarelerHatasiTest += Math.Pow(testDataSet[i].Last() - cikisNoron.transferBirimi(), 2);
            }

            Console.WriteLine("MAPE = " + (100 * (toplamHataTest / testDataSet.Count)).ToString());
            label5.Text = ": %" + (100 * (toplamHataTest / testDataSet.Count)).ToString();
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("MSE = " + toplamKarelerHatasiTest / testDataSet.Count);
            label5.Text = ": %" + (toplamKarelerHatasiTest / testDataSet.Count).ToString();
            Console.WriteLine("------------------------------------------------------------------");
            toplamHataTest = 0;
            toplamKarelerHatasiTest = 0;
        }
    }
}

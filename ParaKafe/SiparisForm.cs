using ParaKafe.Data;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace ParaKafe
{
    public partial class SiparisForm : Form
    {
        public event EventHandler<MasaTasindiEventArgs> MasaTasindi;

        private readonly Siparis siparis;
        private readonly KafeVeri db;
        private readonly BindingList<SiparisDetay> blDetaylar;

        public SiparisForm(Siparis siparis, KafeVeri db)
        {
            this.siparis = siparis;
            this.db = db;
            blDetaylar = new BindingList<SiparisDetay>(siparis.SiparisDetaylar);
            blDetaylar.ListChanged += BlDetaylar_ListChanged;
            InitializeComponent();
            dgvSiparisDetaylar.AutoGenerateColumns = false;
            dgvSiparisDetaylar.DataSource = blDetaylar;
            UrunleriListele();
            MasaNoyuGuncelle();
            OdemeTutariniGuncelle();
        }

        private void BlDetaylar_ListChanged(object sender, ListChangedEventArgs e)
        {
            OdemeTutariniGuncelle();
        }

        private void OdemeTutariniGuncelle()
        {
            lblOdemeTutari.Text = siparis.ToplamTutarTL;
        }

        private void MasaNoyuGuncelle()
        {
            Text = $"Masa {siparis.MasaNo} (Açılış: {siparis.AcilisZamani})";
            lblMasaNo.Text = siparis.MasaNo.ToString("00");
            int[] doluMasalar = db.AktifSiparisler.Select(x => x.MasaNo).ToArray();
            cboMasaNo.DataSource = Enumerable
                .Range(1, db.MasaAdet)
                .Where(x => !doluMasalar.Contains(x))
                .ToList();
        }

        private void UrunleriListele()
        {
            cboUrun.DataSource = db.Urunler;
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            SiparisDetay sd = new SiparisDetay();
            Urun urun = (Urun)cboUrun.SelectedItem;
            sd.UrunAd = urun.UrunAd;
            sd.BirimFiyat = urun.BirimFiyat;
            sd.Adet = (int)nudAdet.Value;
            blDetaylar.Add(sd);
        }

        private void btnAnasayfayaDon_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSiparisIptal_Click(object sender, EventArgs e)
        {
            SiparisiKapat(SiparisDurum.Iptal, 0);
        }

        private void btnOdemeAl_Click(object sender, EventArgs e)
        {
            SiparisiKapat(SiparisDurum.Odendi, siparis.ToplamTutar());
        }

        private void SiparisiKapat(SiparisDurum durum, decimal odenenTutar)
        {
            siparis.KapanisZamani = DateTime.Now;
            siparis.Durum = durum;
            siparis.OdenenTutar = odenenTutar;
            db.AktifSiparisler.Remove(siparis);
            db.GecmisSiparisler.Add(siparis);
            Close();
        }

        private void btnMasaTasi_Click(object sender, EventArgs e)
        {
            if (cboMasaNo.SelectedIndex == -1) return;

            int eskiMasaNo = siparis.MasaNo;
            int hedefMasaNo = (int)cboMasaNo.SelectedItem;
            siparis.MasaNo = hedefMasaNo;
            MasaNoyuGuncelle();
            if (MasaTasindi != null)
                MasaTasindi(this, new MasaTasindiEventArgs(eskiMasaNo, hedefMasaNo));
        }
    }
}

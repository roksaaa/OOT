using BotanickaBasta;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private ObservableCollection<Biljka> biljke = new ObservableCollection<Biljka>();
        private CollectionViewSource viewSource = new CollectionViewSource();
        private int trenutnaStranica = 1;
        private int stavkiPoStranici = 16;
        private int ukupnoStranica => (int)Math.Ceiling((double)biljke.Count / stavkiPoStranici);
        public MainWindow()
        {
            InitializeComponent();
            ucitajFajlove("ulaz.txt");
            foreach (var b in biljke)
                b.PropertyChanged += Biljka_PropertyChanged;

            viewSource.Source = biljke;
            viewSource.Filter += FilterStranice;
            biljkeDG.ItemsSource = viewSource.View;

            PrikaziStranicu(1);
        }


        private void Biljka_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SacuvajUFajl();

            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    biljkeDG.SelectedItem = null;
            //    biljkeDG.UnselectAll();
            //}), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SacuvajUFajl()
        {
            using (StreamWriter sw = new StreamWriter("ulaz.txt", false))
            {
                for (int i = 0; i < biljke.Count; i++)
                {
                    sw.Write(biljke[i].ToString());
                    if (i < biljke.Count - 1)
                        sw.WriteLine();
                }
            }
        }

        private void FilterStranice(object sender, FilterEventArgs e)
        {
            Biljka b = e.Item as Biljka;
            if (b != null)
            {
                int index = biljke.IndexOf(b);
                int start = (trenutnaStranica - 1) * stavkiPoStranici;
                int end = start + stavkiPoStranici;
                e.Accepted = index >= start && index < end;
            }
        }
        private void PrikaziStranicu(int stranica)
        {
            if (stranica < 1) stranica = 1;
            if (stranica > ukupnoStranica) stranica = ukupnoStranica == 0 ? 1 : ukupnoStranica;

            trenutnaStranica = stranica;
            viewSource.View.Refresh();
            labelStranica.Content = $"Stranica {trenutnaStranica} / {ukupnoStranica}";
        }

        private void btnPrethodna_Click(object sender, RoutedEventArgs e)
        {
            PrikaziStranicu(trenutnaStranica - 1);
        }

        private void btnSledeca_Click(object sender, RoutedEventArgs e)
        {
            PrikaziStranicu(trenutnaStranica + 1);
        }

        private void ucitajFajlove(string putanja)
        {
            StreamReader sr = null;
            try
            {
                string linija;
                sr = new StreamReader(putanja);
                while ((linija = sr.ReadLine()) != null)
                {
                    string[] delovi = linija.Split(',');
                    //int sifra = int.Parse(delovi[0]);
                    int sifra = biljke.Count;
                    string naucniNaziv = delovi[1];
                    string uobicajeniNaziv = delovi[2];
                    string porodica = delovi[3];
                    string datumNabavke = delovi[4];
                    string lokacija = delovi[5];
                    string status = delovi[6];
                    string slika = delovi[7];

                    Biljka biljka = new Biljka(sifra, naucniNaziv, uobicajeniNaziv, porodica, datumNabavke, lokacija, status, slika);
                    biljke.Add(biljka);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sr?.Close();
            }
        }

        private void ObrisiBTN_Click(object sender, RoutedEventArgs e)
        {
            if (biljkeDG.SelectedItem is Biljka b)
            {
                for (int i = 0; i < biljke.Count; i++)
                {
                    if (biljke[i].Sifra == b.Sifra)
                    {
                        biljke.RemoveAt(i);
                        break;
                    }
                }
                PrikaziStranicu(trenutnaStranica);
                //SacuvajUFajl();
                using (StreamWriter sw = new StreamWriter("ulaz.txt", false))
                {
                    foreach (Biljka biljka in biljke)
                    {
                        sw.WriteLine(biljka.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Selektujte biljku!");
            }
        }

        private void biljkeDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (biljkeDG.SelectedItem is Biljka b)
            //{
            //    sifraUnos.Text = b.Sifra.ToString();
            //    naucniUnos.Text = b.NaucniNaziv;
            //    uobicajeniUnos.Text = b.UobicajeniNaziv;
            //    porodicaUnos.Text = b.Porodica;
            //    datumUnos.Text = b.DatumNabavke;
            //    lokacijaUnos.Text = b.Lokacija;
            //    statusUnos.Text = b.Status;
            //}
            this.DataContext = biljkeDG.SelectedItem as Biljka;
        }

        private void promeniSliku_Click(object sender, RoutedEventArgs e)
        {
            if (biljkeDG.SelectedItem is Biljka b)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                bool? success = fileDialog.ShowDialog();
                if (success == true)
                {
                    string absolutePath = fileDialog.FileName;
                    string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                    string relativePath = System.IO.Path.GetRelativePath(appFolder, absolutePath);

                    b.Slika = new BitmapImage(new Uri(absolutePath, UriKind.Absolute));
                    b.SlikaPath = relativePath;
                }
                PrikaziStranicu(trenutnaStranica);
                //SacuvajUFajl();
                using (StreamWriter sw = new StreamWriter("ulaz.txt", false))
                {
                    foreach (Biljka biljka in biljke)
                    {
                        sw.WriteLine(biljka.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Selektujte biljku!");
            }
        }

        private void DodajBTN_Click(object sender, RoutedEventArgs e)
        {
            int moze = 1;
            int sifra = -1;
            string path = "";
            DateTime d = DateTime.Today;
            string[] razdeljenDatum;

            if (string.IsNullOrWhiteSpace(sifraUnos.Text) || string.IsNullOrWhiteSpace(naucniUnos.Text) ||
                string.IsNullOrWhiteSpace(uobicajeniUnos.Text) || string.IsNullOrWhiteSpace(porodicaUnos.Text) ||
                string.IsNullOrWhiteSpace(datumUnos.Text) || string.IsNullOrWhiteSpace(lokacijaUnos.Text) ||
                string.IsNullOrWhiteSpace(statusUnos.Text))
            {
                moze = 0;
                MessageBox.Show("Popunite sva polja!");
            }

            if (moze == 1)
            {
                if (!int.TryParse(sifraUnos.Text, out sifra))
                {
                    moze = 0;
                    MessageBox.Show("Sifra moze da sadrzi samo brojeve!");
                }
            }

            if (moze == 1)
            {
                foreach (Biljka biljka in biljke)
                {
                    if (biljka.Sifra == sifra)
                    {
                        MessageBox.Show("Postoji biljka sa sifrom!");
                        moze = 0;
                        break;
                    }
                }
            }

            if (moze == 1)
            {
                try
                {
                    string separator = datumUnos.Text.Contains('.') ? "." :
                                       datumUnos.Text.Contains('/') ? "/" : " ";
                    razdeljenDatum = datumUnos.Text.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    DateTime d1 = new DateTime(int.Parse(razdeljenDatum[2]), int.Parse(razdeljenDatum[1]), int.Parse(razdeljenDatum[0]));
                    if (d1 > DateTime.Today)
                    {
                        moze = 0;
                        MessageBox.Show("Ne mozete uneti datum iz buducnosti!");
                    }
                    else
                    {
                        d = d1;
                    }
                }
                catch (Exception ex)
                {
                    moze = 0;
                    MessageBox.Show("Nevalidan datum: " + ex.Message);
                }
            }

            if (moze == 1)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                bool? success = fileDialog.ShowDialog();
                if (success == true)
                {
                    string absolutePath = fileDialog.FileName;
                    string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                    path = System.IO.Path.GetRelativePath(appFolder, absolutePath);
                }

                Biljka b = new Biljka(sifra, naucniUnos.Text, uobicajeniUnos.Text, porodicaUnos.Text, d.ToString("dd/MM/yyyy"), lokacijaUnos.Text, statusUnos.Text, path);
                biljke.Add(b);
                b.PropertyChanged += Biljka_PropertyChanged;
                PrikaziStranicu(ukupnoStranica);
                sifraUnos.Text = naucniUnos.Text = uobicajeniUnos.Text = porodicaUnos.Text = datumUnos.Text = lokacijaUnos.Text = statusUnos.Text = "";
                
                try
                {
                    using (StreamWriter sw = new StreamWriter("ulaz.txt", false))
                    {
                        for (int i = 0; i < biljke.Count; i++)
                        {
                            sw.Write(biljke[i].ToString());
                            if (i < biljke.Count - 1)
                                sw.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void promeniSadrzajBTN_Click(object sender, RoutedEventArgs e)
        {
            int moze = 1;
            int sifra = -1;
            DateTime d = DateTime.Today;
            string[] razdeljenDatum;

            if (string.IsNullOrWhiteSpace(sifraUnos.Text) || string.IsNullOrWhiteSpace(naucniUnos.Text) ||
                string.IsNullOrWhiteSpace(uobicajeniUnos.Text) || string.IsNullOrWhiteSpace(porodicaUnos.Text) ||
                string.IsNullOrWhiteSpace(datumUnos.Text) || string.IsNullOrWhiteSpace(lokacijaUnos.Text) ||
                string.IsNullOrWhiteSpace(statusUnos.Text))
            {
                moze = 0;
                MessageBox.Show("Popunite sva polja!");
            }

            if (moze == 1)
            {
                if (!int.TryParse(sifraUnos.Text, out sifra))
                {
                    moze = 0;
                    MessageBox.Show("Sifra moze da sadrzi samo brojeve!");
                }
            }

            if (moze == 1)
            {
                foreach (Biljka biljka in biljke)
                {
                    if (biljka.Sifra == sifra && biljkeDG.SelectedItem is Biljka b && b.Sifra != sifra)
                    {
                        MessageBox.Show("Postoji biljka sa sifrom!");
                        moze = 0;
                        break;
                    }
                }
            }

            if (moze == 1)
            {
                try
                {
                    string separator = datumUnos.Text.Contains('.') ? "." :
                                       datumUnos.Text.Contains('/') ? "/" : " ";
                    razdeljenDatum = datumUnos.Text.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    DateTime d1 = new DateTime(int.Parse(razdeljenDatum[2]), int.Parse(razdeljenDatum[1]), int.Parse(razdeljenDatum[0]));
                    if (d1 > DateTime.Today)
                    {
                        moze = 0;
                        MessageBox.Show("Ne mozete uneti datum iz buducnosti!");
                    }
                    else
                    {
                        d = d1;
                    }
                }
                catch (Exception ex)
                {
                    moze = 0;
                    MessageBox.Show("Nevalidan datum: " + ex.Message);
                }
            }

            if (moze == 1)
            {
                if (biljkeDG.SelectedItem is Biljka biljka)
                {
                    biljka.Sifra = sifra;
                    biljka.NaucniNaziv = naucniUnos.Text;
                    biljka.UobicajeniNaziv = uobicajeniUnos.Text;
                    biljka.Porodica = porodicaUnos.Text;
                    biljka.DatumNabavke = datumUnos.Text;
                    biljka.Lokacija = lokacijaUnos.Text;
                    biljka.Status = statusUnos.Text;
                }
                else
                {
                    MessageBox.Show("Selektujte biljku!");
                }

                sifraUnos.Text = naucniUnos.Text = uobicajeniUnos.Text = porodicaUnos.Text = datumUnos.Text = lokacijaUnos.Text = statusUnos.Text = "";
                PrikaziStranicu(trenutnaStranica);
                SacuvajUFajl();
                using (StreamWriter sw = new StreamWriter("ulaz.txt", false))
                {
                    foreach (Biljka bilj in biljke)
                    {
                        sw.WriteLine(bilj.ToString());
                    }
                }
            }
        }

        private void eksportBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("izlaz.csv", false, Encoding.UTF8))
                {
                    sw.WriteLine("Sifra,NaucniNaziv,UobicajeniNaziv,Porodica,DatumNabavke,Lokacija,Status,SlikaPath");

                    foreach (Biljka biljka in biljke)
                    {
                        sw.WriteLine(biljka.ToString());
                    }
                }
                MessageBox.Show("Eksport uspešan!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom eksportovanja: " + ex.Message);
            }
        }

        private void sifraUnos_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(sifraUnos.Text, out int novaSifra))
            {
                // Ako je duplikat (osim ako je ista selektovana biljka)
                if (biljke.Any(b => b != biljkeDG.SelectedItem && b.Sifra == novaSifra))
                {
                    MessageBox.Show("Već postoji biljka sa ovom šifrom!");
                    sifraUnos.Text = "";  // ili resetuj na prethodnu vrednost
                    sifraUnos.Focus();
                }
            }
        }

        private void biljkeDG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Dohvati element na koji je kliknuto
            var dep = (DependencyObject)e.OriginalSource;

            // Penji se kroz vizualno stablo dok ne nađeš DataGridRow ili null
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            // Ako nije DataGridRow, znači da je kliknuto u prazno
            if (dep == null)
            {
                biljkeDG.UnselectAll();
                biljkeDG.SelectedItem = null;
            }
        }

    }

    public class DatumValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                return new ValidationResult(false, "Datum je obavezan");

            DateTime temp;
            string[] formati = new[]
            {
        "d.M.yyyy",
        "dd.MM.yyyy",
        "d/M/yyyy",
        "dd/MM/yyyy",
        "d M yyyy",
        "dd MM yyyy"
    };

            if (!DateTime.TryParseExact(s, formati, cultureInfo, DateTimeStyles.None, out temp))
                return new ValidationResult(false, "Nevalidan format datuma");

            if (temp > DateTime.Today)
                return new ValidationResult(false, "Datum ne može biti u budućnosti");

            return ValidationResult.ValidResult;
        }
    }

}

using SpeechLib;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace kelimeEzberle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        ArrayList did;
        ArrayList ingkel;
        ArrayList trkel;
        ArrayList diger;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        DispatcherTimer dispatcherTimer2 = new DispatcherTimer();
        BackgroundWorker bckgrndworker = new BackgroundWorker();
        private void Window_Initialized(object sender, EventArgs e)        {
            
            bckgrndworker.WorkerReportsProgress = true;
            bckgrndworker.DoWork += work;
            bckgrndworker.ProgressChanged += worker_ProgressChanged;
            bckgrndworker.RunWorkerCompleted += worker_RunWorkerCompleted;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer2.Tick += new EventHandler(dispatcherTimer_Tick2);
            dispatcherTimer2.Interval = new TimeSpan(0, 0, 4);
            bckgrndworker.RunWorkerAsync();
            lblknt.Visibility = Visibility.Hidden;
        }

        private void work(object sender, DoWorkEventArgs e)
        {
            did = new ArrayList();
            ingkel = new ArrayList();
            trkel = new ArrayList();
            diger = new ArrayList();
            int process = 0;
            int id = 0;
            string[] data;
            StreamReader sr;
            sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\db.txt");
            string satir = "";
            while ((satir = sr.ReadLine()) != null)
            {
                data = satir.Split('	');
                ingkel.Add(data[0]);
                trkel.Add(data[1]);
                did.Add(id);
                if(data.Length == 3)
                    diger.Add(data[2]);
                else
                    diger.Add("");
                id++;

                //reported
                process++;
                (sender as BackgroundWorker).ReportProgress(process);
                if (process >= 99)
                    process = 0;
                
            }
            sr.Close();
        }
      
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pbar.Value = 100;
            lblyukleniyor.Content = "Kelimeler Yüklendi.";
            dispatcherTimer.Start();
            rasgele();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbar.Value = e.ProgressPercentage;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            lblyukleniyor.Visibility = Visibility.Hidden;
            pbar.Visibility = Visibility.Hidden;
            lbldiger.Visibility = Visibility.Hidden;
            dispatcherTimer.Stop();
        }

        private void dispatcherTimer_Tick2(object sender, EventArgs e)
        {
            lblknt.Visibility = Visibility.Hidden;
            lbldiger.Visibility = Visibility.Hidden;
        }

        ArrayList cid = new ArrayList();
        Random rnd = new Random();
        string cvp = "";
        string digeranlam = "";
        string secilenKelime = "look at me now";
        void rasgele()
        {
            bool rsgdrm = true;
            int srnd = 0;
            while (rsgdrm)
            {
                bool drm = false;
                srnd = rnd.Next(0, did.Count);                
                foreach (int item in cid)
                {
                    if (item == srnd)
                    {
                        drm = true;
                        break;
                    }

                }

                if (!drm)
                    break;
            }
            if (chktring.IsChecked == true)
            {
                lblKelime.Content = trkel[srnd];
                cvp = ingkel[srnd].ToString();
                digeranlam = diger[srnd].ToString();
                new Thread(dinlet).Start();
                secilenKelime = trkel[srnd].ToString(); 
            }
            else
            {
                lblKelime.Content = ingkel[srnd];
                cvp = trkel[srnd].ToString();
                digeranlam = diger[srnd].ToString();
                new Thread(dinlet).Start();
                secilenKelime = ingkel[srnd].ToString();
            }
            cid.Add(srnd);
            if (cid.Count >= did.Count)
                cid.Clear();
        }

        private void btnileri_Click(object sender, RoutedEventArgs e)
        {
            click();
        }

        bool cvpknt = false;
        bool digerAnlamknt = false;

        void click()
        {
            cvpknt = false;
            digerAnlamknt = false;
            if (txtKelime.Text != "")
            {
                string[] cvplar = cvp.ToLower().Split(',');
                foreach (string item in cvplar)
                {
                    if (item.Trim() == txtKelime.Text.ToLower())
                        cvpknt = true;
                }

                string[] digerAnlamCvp = digeranlam.ToLower().Split(',');
                foreach (string item in digerAnlamCvp)
                {
                    if (item.Trim() == txtKelime.Text.ToLower())
                        digerAnlamknt = true;
                }


                if (cvpknt || digerAnlamknt)
                {
                    lblknt.Content = "DOĞRU - "+ cvp;
                    lbldiger.Text = "Diğer Anlamlar: " + digeranlam;
                    lblknt.Foreground = Brushes.Green;
                    lblknt.Visibility = Visibility.Visible;
                    lbldiger.Visibility = Visibility.Visible;
                    dispatcherTimer2.Stop();
                    dispatcherTimer2.Start();
                    txtKelime.Text = "";
                }
                else
                {
                    lblknt.Content = "Yanlış - " + cvp;
                    lbldiger.Text = "Diğer Anlamlar: " + digeranlam;
                    lblknt.Foreground = Brushes.Red;
                    lblknt.Visibility = Visibility.Visible;
                    lbldiger.Visibility = Visibility.Visible;
                    dispatcherTimer2.Stop();
                    dispatcherTimer2.Start();
                    txtKelime.Text = "";
                }
                rasgele();
            }
            else
                MessageBox.Show("Boş değer girdiniz.", "Kelime Ezberle");
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                click();
        }
                
        SpVoice metniOku = new SpVoice();
        private void image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            new Thread(dinlet).Start();
        }
        bool speechAktif = true;
        void dinlet()
        {
            if (speechAktif)
            {
                try
                {
                    metniOku.Speak(secilenKelime, SpeechVoiceSpeakFlags.SVSFDefault);
                }
                catch
                {
                    if (MessageBox.Show("Sesin Gelmesi İçin\nSpeech SDK Yüklemeniz Gerekiyor.\nYüklemek İçin Evet.\nBu İletiyi Tekrar Görmek İstemiyorsanız Hayır Basın.", "Kelime Ezberle", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=10121");
                    }
                    else
                    {
                        speechAktif = false;
                    }

                }
            }
        }

        private void refresh_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            lblyukleniyor.Visibility = Visibility.Visible;
            pbar.Visibility = Visibility.Visible;
            bckgrndworker.RunWorkerAsync();
        }
    }
}

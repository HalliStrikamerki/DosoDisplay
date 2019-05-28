using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DosoDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// test 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Cursor = Cursors.None;

          
            FillList(); // Fylla á datasett á skjá
                      
            GetFileList(); // skanna folder og fá lista yfir skrár (myndir)

            changeImage();

            List_Timer();  //Timer fyrir Tiltektarlista

            Image_Timer(); // Tímer fyrir slideshow

            ShowWeather();

        }

        public class Customer
        {
            public string CustomerName { get; set; }
            public string Status { get; set; }
            public string Color { get; set; }
        }

        public static string[] filePaths;
        public static int max;
        public static int curr;
        public static int weathertimer;
     
        public static string Path = ConfigurationManager.AppSettings.Get("Path");
        public static string Slipp = ConfigurationManager.AppSettings.Get("Slipp");
        public static string ImageTime = ConfigurationManager.AppSettings.Get("ImageTime");

        public void ShowWeather()
        {
            string curDir = Directory.GetCurrentDirectory();

            string path = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
            
            //wb_Weather.Navigate(new Uri(string.Format("file:///{0}/HTMLPage1.html", curDir)));
            //wb_Weather.Navigate(new Uri(string.Format("file://127.0.0.1/C$/Users/User/source/repos/DosoDisplay/DosoDisplay/bin/Debug/HTMLPage1.html", curDir)));
            wb_Weather.Navigate(new Uri(string.Format("file://127.0.0.1/C$/HTMLPage1.html", curDir)));
            

        }
            
        public void changeImage()
        {
            curr++;
            if (curr >= max) { GetFileList(); }

            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(filePaths[curr], UriKind.Relative);
                image.CacheOption = BitmapCacheOption.OnLoad;              
                myImage.Width = ImageGrid.Width;
                myImage.Height = ImageGrid.Height;
                image.EndInit();

                myImage.Source = image;
                               
            }
            catch (Exception) { }
        }

        public void Image_Timer()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(int.Parse(ImageTime));
            timer.Tick += image_Tick;
            timer.Start();

            
        }

        void image_Tick(object sender, EventArgs e)
        {
            try
            {
                changeImage();
            }
            catch (Exception) { }
            //changeImage();
        }

        public void GetFileList()
        {
            try
            {
                filePaths = Directory.GetFiles(Path);
                max = filePaths.Length;
                curr = 0;
                //MessageBox.Show(filePaths[0].ToString());
            }
            catch (Exception ex) { WritetoLog("Næ ekki að lesa skrár fyrir myndir"); }
        }

        public void Slideshow()
        {
            curr++;

            if (curr >= max) { GetFileList(); }

            try
            {
                me_slideshow.Source = new Uri(filePaths[curr]);
                me_slideshow.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
                me_slideshow.Play();
            }
            catch (Exception) { }


        }

        private void Me_slideshow_MediaEnded(object sender, RoutedEventArgs e)
        {
           
            MediaPlay();
        }

        public void List_Timer()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                FillList();
            }
            catch (Exception ex) { WritetoLog("Næ ekki að tengjast við sql grunn"); }
            //FillList();
        }

        public void FillList() //Listi yfir viðskiptavini og stöðu tiltektar
        {

            weathertimer++;
            if (weathertimer>360)
            {
                
                weathertimer = 0;
                wb_Weather.Refresh();
            }

            
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["IceLinkWareHouseConnectionString"].ToString());
            //SqlConnection conn = new SqlConnection(@"Data Source=localhost\sqlexpress;Initial Catalog=IceLinkWareHouse;Integrated Security=True");

            
            SqlCommand command = new SqlCommand("SET ARITHABORT OFF " +
                                                "SET ANSI_WARNINGS OFF " +
                                                "Select h.CustomerName as 'Viðskiptavinur                                     -', " +
                                                "h.status, " +
                                                "cast(round(sum(l.Picked) / (sum(l.ToPick) + 0.001) * 100, 0) as int) as 'Status', " +
                                                "MAX(l.Colour) as 'Litun' " +
                                                "from " +
                                                "IceLink_Headers h inner join IceLink_Lines l on h.HeaderNo = l.HeaderNo " +
                                                "left outer join IceLink_StatusText s on h.Status = s.ID " +
                                                "where " +
                                                "type = 3 and " +
                                                "Status < 2 and " +
                                                "Priority = 1 " +
                                                "group by " +
                                                "h.HeaderNo, " +
                                                "h.CustomerName, " +
                                                "s.Text, " +
                                                "h.Status, " +
                                                "h.DeliveryDate " +
                                                "--l.Colour " +
                                                "order by h.HeaderNo "
                                                , conn);
                                                
            

            GridViewListDoso.Items.Clear();
            conn.Open();
            
            
            SqlDataReader reader = command.ExecuteReader();
           


            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Customer c = new Customer();

                    c.CustomerName = reader[0].ToString();

                    /*
                    if (reader[2].ToString() == "0" && reader[1].ToString() == "0") { c.Status = "Ekki hafin";}
                    else if (reader[2].ToString() == "100") { c.Status = "Í afhendingu"; }
                    
                    else{c.Status = "" + reader[2].ToString()+"%";}
                
                    if (reader[3].ToString() == "0") { c.Color = ""; }
                    else { c.Color = "Já"; c.Status = "Í litun"; }
                    */

                    // Status
                    if (reader[2].ToString() == "0" && reader[1].ToString() == "0") { c.Status = "Ekki hafin"; }
                    else if (reader[2].ToString() == "100" && (reader[3].ToString() == "0")) { c.Status = "Í afhendingu"; }
                    else if (reader[2].ToString() == "100" && (reader[3].ToString() == "1")) { c.Status = "Í litun"; }
                    else { c.Status = "" + reader[2].ToString() + "%"; }

                    // Litun eða ekki
                    if (reader[3].ToString() == "0") { c.Color = ""; }
                    else { c.Color = "Já"; }

                    

                    if (Slipp == "1")
                    {
                        if (c.CustomerName.Contains("Slippfélagið")) { }
                        else { GridViewListDoso.Items.Add(c); }
                    }
                    else { GridViewListDoso.Items.Add(c); }

                    

                }
            }

            

            conn.Close();

        } 

        private void Me_slideshow_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MediaPlay();
        }

        private void MediaPlay()
        {
            

            curr++;
            if (curr >= max) { GetFileList(); }
            try
            {
                me_slideshow.Source = new Uri(filePaths[curr]);
                me_slideshow.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
                me_slideshow.Play();
                
                
                
              
            }
            catch (Exception) { Slideshow(); }

        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
            if (e.Key == System.Windows.Input.Key.Escape)
            {

                Application.Current.Shutdown();
            }

        }

        public void WritetoLog (string error)
        {
            File.AppendAllText(Environment.CurrentDirectory + @"\ErrorLog.txt", DateTime.Now.ToString() + " - " + error + Environment.NewLine);
        }
   
    }

}

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DosoDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            FillList();

            GetFileList();

            Slideshow();

            List_Timer();
        }

        public class Customer
        {
            public string CustomerName { get; set; }
            public string status { get; set; }
            public string Color { get; set; }

        }

        public static string[] filePaths;
        public static int max;
        public static int curr;
        public static string Path = ConfigurationManager.AppSettings.Get("Path");


        public void GetFileList()
        {
            filePaths = Directory.GetFiles(Path);
            max = filePaths.Length;
            curr = 0;
            //MessageBox.Show(filePaths[0].ToString());
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
            curr++;
            if (curr >= max) { GetFileList(); }
            //me_slideshow.Source = new Uri(@"c:\test\2.mp4");
            try
            {
                me_slideshow.Source = new Uri(filePaths[curr]);
                me_slideshow.LoadedBehavior = System.Windows.Controls.MediaState.Manual;

                me_slideshow.Play();
            }
            catch (Exception) { Slideshow(); }
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
            FillList();
        }




        public void FillList()
        {

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["IceLinkWareHouseConnectionString"].ToString());
            //SqlConnection conn = new SqlConnection(@"Data Source=localhost\sqlexpress;Initial Catalog=IceLinkWareHouse;Integrated Security=True");

            SqlCommand command = new SqlCommand("SET ARITHABORT OFF " +
                                                "SET ANSI_WARNINGS OFF " +
                                                "Select h.CustomerName as 'Viðskiptavinur                                     -', " +
                                                //"s.Text as 'Staða         -', " +
                                                "cast(round(sum(l.Picked) / (sum(l.ToPick) + 0.001) * 100, 0) as int) as 'Status', " +
                                                "MAX(l.Colour) as 'Litun' " +
                                                "from " +
                                                "IceLink_Headers h inner join IceLink_Lines l on h.HeaderNo = l.HeaderNo " +
                                                "left outer join IceLink_StatusText s on h.Status = s.ID " +
                                                "where " +
                                                "type = 3 and " +
                                                "Status < 2 and " +
                                                "Priority > 0 " +
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

                    



                    if (reader[1].ToString() == "0") { c.status = "Í bið";}
                    else if (reader[1].ToString() == "100") { c.status = "Lokið"; }
                    else{c.status = "Tiltekt " + reader[1].ToString()+"%";}
                
                    if (reader[2].ToString() == "0") { c.Color = ""; }
                    else { c.Color = "X"; }

                    //if (c.CustomerName.Contains("Slippfélagið")) { }
                    //else { GridViewListDoso.Items.Add(c); }

                    GridViewListDoso.Items.Add(c);

                }
            }
            conn.Close();
        

            /*
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(dt);
            this.GridViewListDoso.ItemsSource = dt.DefaultView;
            */






        }
    }
}

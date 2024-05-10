using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using Price_Checker.Services;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Data;
using System.Linq;

namespace Price_Checker
{
    public partial class mainForm : Form
    {

        private readonly ImagesManagerService imageManager;
        private readonly ScanBarcodeService scanBarcodeService;
        private readonly BarcodeTimer barcodeTimer;
        private readonly FontManagerService fontManager;
        private readonly VideoManagerService videoManager;
        private readonly DataRetrievalService dataRetrievalService;
        private readonly string connstring;
        private DatabaseConfig _config;

        public mainForm()
        {
            InitializeComponent();
            lbl_barcode.KeyDown += Lbl_barcode_KeyDown;
            KeyPreview = true;
            this.Shown += MainForm_Shown;


            timer1.Start();
            timer1.Interval = 1000; // 1000 milliseconds = 1 second
            timer1.Tick += timer1_Tick;

            UpdateStatusLabel(lbl_status, bottomPanel);
            Appname(lbl_appname);


            scanBarcodeService = new ScanBarcodeService();
            scanBarcodeService.BarcodeScanned += ScanBarcodeService_BarcodeScanned;
            barcodeTimer = new BarcodeTimer(lbl_barcode);
            imageManager = new ImagesManagerService(pictureBox1);
            imageManager.ImageSlideshow();

            fontManager = new FontManagerService();
            lbl_barcode.Font = fontManager.GetCustomFont();

            videoManager = new VideoManagerService(axWindowsMediaPlayer1);
           


            DatabaseConfig databaseConfig = new DatabaseConfig();

            //// Prepare the message to be displayed
            //string message = $"Server: {databaseConfig.Server}\n" +
            //                  $"Uid: {databaseConfig.Uid}\n" +
            //                  $"Pwd: {databaseConfig.Pwd}\n" +
            //                  $"Database: {databaseConfig.Database}";

            //// Show the MessageBox
            //MessageBox.Show(message, "Database Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information);

            dataRetrievalService = new DataRetrievalService(databaseConfig);
            dataRetrievalService.SetInterval(5, DataRetrievalService.TimeUnit.Seconds);

            //EmbedPriceForm();
        }

        //private void EmbedPriceForm()
        //{
        //    PriceCheckerForm priceForm = new PriceCheckerForm();
        //    priceForm.FormBorderStyle = FormBorderStyle.None; // Remove border
        //    priceForm.TopLevel = false; // Set as non-top-level form
        //    priceForm.Dock = DockStyle.Fill; // Fill the panel
        //    panel1.Controls.Add(priceForm); // Add PriceForm to a panel on the main form
        //    priceForm.Show();
        //}

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // Set focus to lbl_barcode when the form is shown
            lbl_barcode.Focus();
        }

        private DateTime lastOnlineTime = DateTime.MinValue;
        private bool wasOnlinePreviously = false;

        private void UpdateStatusLabel(Label lbl_status, Panel bottomPanel)
        {
            _config = new DatabaseConfig();
            string connstring = $"server={_config.Server};port={_config.Port};uid={_config.Uid};pwd={_config.Pwd};database={_config.Database}";
            string status = "Server Offline"; // Default status
            Color panelColor = Color.Red; // Default color for bottom panel when server is offline

            try
            {
                using (MySqlConnection con = new MySqlConnection(connstring))
                {
                    con.Open();
                    string sql = "SELECT set_status FROM settings";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int statusValue = reader.GetInt32(0);
                        if (statusValue == 1)
                        {
                            lastOnlineTime = DateTime.Now;
                            wasOnlinePreviously = true;
                            status = "Server Online";
                            panelColor = Color.FromArgb(22, 113, 192); // Set custom color (22, 113, 192) when server is online

                        }
                        else
                        {
                            if (wasOnlinePreviously)
                            {
                                status = "Server Offline";
                            }
                            else
                            {
                                status = "Server Offline";
                            }
                        }
                    }
                }

            }
            catch (MySqlException ex)
            {
                // Handle MySQL-specific exceptions
                status = "Error";
                panelColor = Color.Red; // Set panel color to red in case of error
                MessageBox.Show(ex.Message);
            }

            lbl_status.Text = $"{status} as of {(status == "Server Offline" ? lastOnlineTime.ToString() : DateTime.Now.ToString())}";
            bottomPanel.BackColor = panelColor; // Set bottom panel's back color
        }


        private void Lbl_barcode_KeyDown(object sender, KeyEventArgs e)
        {
            scanBarcodeService.HandleBarcodeInput(e, lbl_barcode, scanPanel, this);
        }
        private void ScanBarcodeService_BarcodeScanned(object sender, string barcode)
        {
            barcodeTimer.StartTimer();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateStatusLabel(lbl_status, bottomPanel);
        }

        private void Appname(Label lbl_appname)
        {
            _config = new DatabaseConfig();
            string connstring = $"server={_config.Server};port={_config.Port};uid={_config.Uid};pwd={_config.Pwd};database={_config.Database}";
            using (MySqlConnection con = new MySqlConnection(connstring))
            {
                con.Open();
                string sql = "SELECT set_appname FROM settings";
                MySqlCommand cmd = new MySqlCommand(sql, con);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string appName = reader.GetString(0);

                    reader.Close();

                    lbl_appname.Text = appName;
                }
                else
                {
                    lbl_appname.Text = "No app name found";
                }
            }
        }
    }
}

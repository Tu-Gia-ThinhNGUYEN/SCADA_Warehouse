//Connect MX Component
using ActUtlTypeLib;
using AForge.Video;
using AForge.Video.DirectShow;
//Firebase Config
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Globalization;
//ZXing for Barcode
using ZXing;
//MutilThread
using System.Threading;

namespace SCADA_WAREHOUSE
{
    public partial class Form1 : Form
    {
        //Define Firebase's variable
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "dZ3CBlLKT06LV1ZvS6NKeiWu4gzLGFrunPPdJLBZ",
            BasePath = "https://meca-spksalt-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;
        //Define PLC's variable
        public ActUtlType plc = new ActUtlType();
        //ZXing's variable
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        //System's variable
        DataTable dt = new DataTable();
        static int enableReadFirebase = new int();
        //SQL's variable
        string connectionString = @"Data Source=GIA-THINH\CITADEL;Initial Catalog=WAREHOUSE_VER2;Integrated Security=True;MultipleActiveResultSets=true;";
        SqlConnection sqlConnection = new SqlConnection();
        SqlCommand sqlCommand = new SqlCommand();
        //PLC tag
        //Write tag
        bool plcOnBtn;
        bool plcOffBtn;
        bool plcAutoBtn;
        bool plcManualBtn;


        //Read tag
        int plcErrorNumber;
        int plcErrorX;
        int plcErrorY1;
        int plcErrorY2;
        int plcErrorZ;

        int plcRunLight;
        int plcStopLight;
        int plcAlarmLight;
        int plcAutoLight;
        int plcManualLight;
        int plcImportLight;
        int plcExportLight;

        int plcSpeedConveyor;
        int plcHomingDone;
        int plcJogXSpeed;
        int plcJogYSpeed;
        int plcJogZSpeed;

        int plcXSpeed;
        int plcY1Speed;
        int plcY2Speed;
        int plcZSpeed;
        int plcXPosition;
        int plcY1Position;
        int plcY2Position;
        int plcZPosition;

        int plcWrongBarcode;
        int plcCurrentBarcode;

        int plcHomeComplete;
        int plcEntrySensor;
        int plcPackageSensor;
        int plcCylinder;
        int plcSuctionCup;

        int[] plcPosition = new int[18];
        string[] positionName = new string[] {"A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9"};

        int[] plcBarcode = new int[18];

        public static bool[] isLoaded = new bool[18];
        static long noTableSQL = new long();

        public Form1()
        {
            InitializeComponent();

            txtBoxTimeStorage.Text = "6";
            cbIntervalStorage.SelectedIndex = 2;
            enableReadFirebase = 0;
            ThreadStart ts = new ThreadStart(updateCycle);
            Thread updateCycleThread = new Thread(ts);
            updateCycleThread.IsBackground = true;
            updateCycleThread.Start();
            ThreadStart tsFirebase = new ThreadStart(updateCycleFirebase);
            Thread updateCycleFirebaseThread = new Thread(tsFirebase);
            updateCycleFirebaseThread.IsBackground = true;
            updateCycleFirebaseThread.Start();
            //FirebaseDataForm firebaseForm = new FirebaseDataForm();
            //firebaseForm.Show();
            sqlConnection.ConnectionString = connectionString;
            try
            {
                txtBoxLogicalSation.Text = "1";
                plc.ActLogicalStationNumber = Convert.ToInt16(txtBoxLogicalSation.Text);
                plc.Open(); //Open connection
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Connection");
                lbStatus.Text = "Disconnected";
                lbStatus.ForeColor = Color.Red;
            }
            //txtBarcode.Text = "0";
            if (plc.Open() == -268435453)
            {
                lbStatus.Text = "Connected";
                //lbStatus.Text = plc.Open().ToString();
                lbStatus.ForeColor = Color.Green;
            }
            else
            {
                lbStatus.Text = "Disconnected";
                //lbStatus.Text = plc.Open().ToString();
                lbStatus.ForeColor = Color.Red;
            }
            
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
            client = new FireSharp.FirebaseClient(config); //Firebase declare
            }catch
            {
                MessageBox.Show("There was problem in the internet.", "Firebase Data");
            }
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in filterInfoCollection)
                cboCamera.Items.Add(device.Name);
                cboCamera.SelectedIndex = 0;

            dt.Columns.Add("hoten");
            dt.Columns.Add("lop");
            dt.Columns.Add("truong");

            //dtgDSHS.DataSource = dt;

            //dtgDSHS.Columns[0].HeaderText = "Họ và tên";
            //dtgDSHS.Columns[1].HeaderText = "Lớp";
            //dtgDSHS.Columns[2].HeaderText = "Trường";

        }
        private async void RetrieveData()
        {
            //dt.Rows.Clear();
            //FirebaseResponse resp2 = await client.GetTaskAsync("Information/");
            //Data obj2 = resp2.ResultAs<Data>();

            //DataRow row = dt.NewRow();
            //row["hoten"] = obj2.HoTen;
            //row["lop"] = obj2.Lop;
            //row["truong"] = obj2.Truong;

            //dt.Rows.Add(row);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            plc.ActLogicalStationNumber = Convert.ToInt16(txtBoxLogicalSation.Text); //Station of PLC in MX Component
            plc.Open(); //Open connection
            if(plc.Open() == -268435453)
            {
                lbStatus.Text = "Connected";
                //lbStatus.Text = plc.Open().ToString();
                lbStatus.ForeColor = Color.Green;
                MessageBox.Show("Communication is Successful.", "Connection");
            }
            else
            {
                lbStatus.Text = "Disconnected";
                //lbStatus.Text = plc.Open().ToString();
                lbStatus.ForeColor = Color.Red;
                MessageBox.Show("Communication is Fail.", "Connection");
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            plc.Close(); //Close connection
            lbStatus.Text = "Disconnected";
            lbStatus.ForeColor = Color.Red;
            MessageBox.Show("Communication is Disconnected.", "Connection");
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            int read_result;
            int read_msb;
            int read_lsb;
            string temp;
            string tempReg;
            plc.ReadDeviceBlock(txtBoxAdress.Text, 2, out read_lsb);
                //temp = txtBoxAdress.Text.Remove(0, 1);
                //tempReg = txtBoxAdress.Text.Remove(1, txtBoxAdress.MaxLength);
            if(txtBoxAdress.Text == "D5000")
            { 
                plc.ReadDeviceBlock("D5001", 2, out read_msb);

                read_result = (read_msb << 16) | read_lsb;
                txtBoxValue.Text = read_result.ToString();
            }
            else
            {
                txtBoxValue.Text = read_lsb.ToString();
            }
        }

        private void btnRetrieveData_Click(object sender, EventArgs e)
        {
            RetrieveData();
        }

        private async void btnAddDB_Click(object sender, EventArgs e)
        {
            var data = new Data
            {
                HoTen = "Gia Thinh",
                Lop = "191511A",
                Truong = "UTE"
            };

            SetResponse response = await client.SetTaskAsync("Information/" + "id1/" + "abc", data);
            Data result = response.ResultAs<Data>();

            MessageBox.Show("Write data complete.");
            //RetrieveData();

        }

        private void btnOpenFirebase_Click(object sender, EventArgs e)
        {
            FirebaseDataForm firebaseWindow = new FirebaseDataForm();
            firebaseWindow.Show();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cboCamera.SelectedIndex].MonikerString);
            //videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            //videoCaptureDevice.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoCaptureDevice.IsRunning)
                videoCaptureDevice.Stop();
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            int barcode = new int();
            if(txtBoxAdress.Text != "D5000")
            {
                plc.WriteDeviceBlock(txtBoxAdress.Text, 2, Convert.ToInt32(txtBoxValue.Text));
                MessageBox.Show("Write data completed.", "Connection");
            }
            else if(txtBoxAdress.Text == "D5000")
            {
                barcode = Convert.ToInt32(txtBoxValue.Text);
                plc.WriteDeviceBlock("D5000", 2, barcode);
                plc.WriteDeviceBlock("D5001", 2, barcode>>16);
                MessageBox.Show("Write data completed.", "Connection");
            }
            else
            {
                MessageBox.Show("Please press adress input.", "Connection");
            }
        }

        

        private void CycleUpdate_Tick(object sender, EventArgs e)
        {


            //plc.GetDevice(txtBoxAdress.Text, out plcRunLight);
            //txtBoxValue.Text = plcRunLight.ToString();

            plc.GetDevice("M10", out plcErrorNumber);
            plc.GetDevice("D90", out plcErrorX);
            plc.GetDevice("D92", out plcErrorZ);
            plc.GetDevice("D94", out plcErrorY1);
            plc.GetDevice("D96", out plcErrorY2);

            plc.GetDevice("D800", out plcXPosition);
            plc.GetDevice("D802", out plcZPosition);
            plc.GetDevice("D804", out plcY1Position);
            plc.GetDevice("D806", out plcY2Position);
            plc.GetDevice("D238", out plcSpeedConveyor);

            plc.GetDevice("D74", out plcXSpeed);
            plc.GetDevice("D78", out plcZSpeed);
            plc.GetDevice("D82", out plcY1Speed);
            plc.GetDevice("D86", out plcY2Speed);

            plc.GetDevice("Y70", out plcRunLight);
            plc.GetDevice("Y71", out plcStopLight);
            plc.GetDevice("M8001", out plcAlarmLight);
            plc.GetDevice("Y72", out plcAutoLight);
            plc.GetDevice("Y73", out plcManualLight);
            plc.GetDevice("M1406", out plcImportLight);
            plc.GetDevice("M1411", out plcExportLight);

            plc.GetDevice("D5000", out plcCurrentBarcode);
            plc.GetDevice("M6000", out plcWrongBarcode);

            plc.GetDevice("M8000", out plcHomeComplete);
            plc.GetDevice("X2E", out plcEntrySensor);
            plc.GetDevice("X2D", out plcPackageSensor);
            plc.GetDevice("Y75", out plcCylinder);
            plc.GetDevice("Y77", out plcSuctionCup);

            plc.GetDevice("M500", out plcPosition[0]);
            plc.GetDevice("M501", out plcPosition[1]);
            plc.GetDevice("M502", out plcPosition[2]);
            plc.GetDevice("M503", out plcPosition[3]);
            plc.GetDevice("M504", out plcPosition[4]);
            plc.GetDevice("M505", out plcPosition[5]);
            plc.GetDevice("M506", out plcPosition[6]);
            plc.GetDevice("M507", out plcPosition[7]);
            plc.GetDevice("M508", out plcPosition[8]);
            plc.GetDevice("M509", out plcPosition[9]);
            plc.GetDevice("M510", out plcPosition[10]);
            plc.GetDevice("M511", out plcPosition[11]);
            plc.GetDevice("M512", out plcPosition[12]);
            plc.GetDevice("M513", out plcPosition[13]);
            plc.GetDevice("M514", out plcPosition[14]);
            plc.GetDevice("M515", out plcPosition[15]);
            plc.GetDevice("M516", out plcPosition[16]);
            plc.GetDevice("M517", out plcPosition[17]);

            plc.GetDevice("D130", out plcBarcode[0]);
            plc.GetDevice("D112", out plcBarcode[1]);
            plc.GetDevice("D114", out plcBarcode[2]);
            plc.GetDevice("D116", out plcBarcode[3]);
            plc.GetDevice("D118", out plcBarcode[4]);
            plc.GetDevice("D120", out plcBarcode[5]);
            plc.GetDevice("D122", out plcBarcode[6]);
            plc.GetDevice("D124", out plcBarcode[7]);
            plc.GetDevice("D126", out plcBarcode[8]);
            plc.GetDevice("D128", out plcBarcode[9]);
            plc.GetDevice("D132", out plcBarcode[10]);
            plc.GetDevice("D134", out plcBarcode[11]);
            plc.GetDevice("D136", out plcBarcode[12]);
            plc.GetDevice("D138", out plcBarcode[13]);
            plc.GetDevice("D140", out plcBarcode[14]);
            plc.GetDevice("D142", out plcBarcode[15]);
            plc.GetDevice("D144", out plcBarcode[16]);
            plc.GetDevice("D146", out plcBarcode[17]);

            //changePositionImage(plcPosition[0], picBoxPositionA1);
            //changePositionImage(plcPosition[1], picBoxPositionA2);
            //changePositionImage(plcPosition[2], picBoxPositionA3);
            //changePositionImage(plcPosition[3], picBoxPositionA4);
            //changePositionImage(plcPosition[4], picBoxPositionA5);
            //changePositionImage(plcPosition[5], picBoxPositionA6);
            //changePositionImage(plcPosition[6], picBoxPositionA7);
            //changePositionImage(plcPosition[7], picBoxPositionA8);
            //changePositionImage(plcPosition[8], picBoxPositionA9);
                                           
            //changePositionImage(plcPosition[9], picBoxPositionB1);
            //changePositionImage(plcPosition[10], picBoxPositionB2);
            //changePositionImage(plcPosition[11], picBoxPositionB3);
            //changePositionImage(plcPosition[12], picBoxPositionB4);
            //changePositionImage(plcPosition[13], picBoxPositionB5);
            //changePositionImage(plcPosition[14], picBoxPositionB6);
            //changePositionImage(plcPosition[15], picBoxPositionB7);
            //changePositionImage(plcPosition[16], picBoxPositionB8);
            //changePositionImage(plcPosition[17], picBoxPositionB9);

            txtBoxXaxisSpeed.Text = (plcXSpeed/10).ToString();
            txtBoxZaxisSpeed.Text = (plcZSpeed/10).ToString();
            txtBoxY1axisSpeed.Text = (plcY1Speed/10).ToString();
            txtBoxY2axisSpeed.Text = (plcY2Speed/10).ToString();
            txtBoxConveyorSpeed.Text = (plcSpeedConveyor/10).ToString();

            txtBoxXaxisPosition.Text = (plcXPosition/10).ToString();
            txtBoxZaxisPosition.Text = (plcZPosition/10).ToString();
            txtBoxY1axisPosition.Text = (plcY1Position/10).ToString();
            txtBoxY2axisPosition.Text = (plcY2Position/10).ToString();

            txtBoxXerror.Text = String.Format("{0:X}H", plcErrorX).ToString();
            txtBoxZerror.Text = String.Format("{0:X}H", plcErrorZ).ToString();
            txtBoxY1error.Text = String.Format("{0:X}H", plcErrorY1).ToString();
            txtBoxY2error.Text = String.Format("{0:X}H", plcErrorY2).ToString();

            if (plcRunLight == 1)
            {
                btnOnLight.BackColor = Color.Green;
                lbON.ForeColor = Color.Green;
            }
            else
            {
                btnOnLight.BackColor = Color.White;
                lbON.ForeColor = Color.LightGray;
            }
            if (plcStopLight == 1)
            {
                btnOffLight.BackColor = Color.Red;
                lbOFF.ForeColor = Color.Red;
            }
            else
            {
                btnOffLight.BackColor = Color.White;
                lbOFF.ForeColor = Color.LightGray;
            }
            if (plcAutoLight == 1)
            {
                btnAutoLight.BackColor = Color.LightSlateGray;
                lbAUTO.ForeColor = Color.LightSlateGray;
            }
            else
            {
                btnAutoLight.BackColor = Color.White;
                lbAUTO.ForeColor = Color.LightGray;
            }
            if (plcManualLight == 1)
            {
                btnManualLight.BackColor = Color.LightSlateGray;
                lbMANUAL.ForeColor = Color.LightSlateGray;
            }
            else
            {
                btnManualLight.BackColor = Color.White;
                lbMANUAL.ForeColor = Color.LightGray;
            }
            if (plcImportLight == 1)
            {
                btnImportLight.BackColor = Color.DarkSlateBlue;
                lbIMPORT.ForeColor = Color.DarkSlateBlue;
            }
            else
            {
                btnImportLight.BackColor = Color.White;
                lbIMPORT.ForeColor = Color.LightGray;
            }
            if (plcExportLight == 1)
            {
                btnExportLight.BackColor = Color.DarkSlateBlue;
                lbEXPORT.ForeColor = Color.DarkSlateBlue;
            }
            else
            {
                btnExportLight.BackColor = Color.White;
                lbEXPORT.ForeColor = Color.LightGray;
            }
            if (plcAlarmLight == 1)
            {
                btnAlarmLight.BackColor = Color.Yellow;
                //MessageBox.Show("There are some errors in system", "Error");
            }
            else
            {
                btnAlarmLight.BackColor = Color.White;
            }
            if (plcHomeComplete == 1)
            {
                btnHomeComplete.BackColor = Color.Green;
            }
            else
            {
                btnHomeComplete.BackColor = Color.White;
            }
            if (plcEntrySensor == 1)
            {
                btnEntrySensor.BackColor = Color.Green;
            }
            else
            {
                btnEntrySensor.BackColor = Color.White;
            }
            if (plcPackageSensor == 1)
            {
                btnPackageSensor.BackColor = Color.Green;
            }
            else
            {
                btnPackageSensor.BackColor = Color.White;
            }
            if (plcCylinder == 1)
            {
                btnCylinder.BackColor = Color.Red;
            }
            else
            {
                btnCylinder.BackColor = Color.White;
            }
            if (plcSuctionCup == 1)
            {
                btnSuctionCup.BackColor = Color.Red;
            }
            else
            {
                btnSuctionCup.BackColor = Color.White;
            }
            if (plcWrongBarcode == 1)
            {
                btnWrongBarcode.BackColor = Color.Red;
            }
            else
            {
                btnWrongBarcode.BackColor = Color.White;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (plc.Open() == -268435453)
            {
                lbStatus.Text = "Connected";
                lbStatus.ForeColor = Color.Green;
                MessageBox.Show("Communication is Successful.", "Connection");
            }
            else
            {
                lbStatus.Text = "Disconnected";
                lbStatus.ForeColor = Color.Red;
                MessageBox.Show("Communication is Fail.", "Connection");
            }
        }

        async void SetDataToFirebase() //**************************************************************************************************************
        {
            enableReadFirebase = 0;
            SystemDataFirebase sysdata = new SystemDataFirebase()
            {
                isOn = plcRunLight.ToString(),
                isOff = plcStopLight.ToString(),
                isImport = plcImportLight.ToString(),
                isExport = plcExportLight.ToString(),
                isAuto = plcAutoLight.ToString(),
                isManual = plcManualLight.ToString(),
                loadedA1 = plcPosition[0].ToString(),
                loadedA2 = plcPosition[1].ToString(),
                loadedA3 = plcPosition[2].ToString(),
                loadedA4 = plcPosition[3].ToString(),
                loadedA5 = plcPosition[4].ToString(),
                loadedA6 = plcPosition[5].ToString(),
                loadedA7 = plcPosition[6].ToString(),
                loadedA8 = plcPosition[7].ToString(),
                loadedA9 = plcPosition[8].ToString(),
                loadedB1 = plcPosition[9].ToString(),
                loadedB2 = plcPosition[10].ToString(),
                loadedB3 = plcPosition[11].ToString(),
                loadedB4 = plcPosition[12].ToString(),
                loadedB5 = plcPosition[13].ToString(),
                loadedB6 = plcPosition[14].ToString(),
                loadedB7 = plcPosition[15].ToString(),
                loadedB8 = plcPosition[16].ToString(),
                loadedB9 = plcPosition[17].ToString(),
                barcodeA1 = plcBarcode[0].ToString(),
                barcodeA2 = plcBarcode[1].ToString(),
                barcodeA3 = plcBarcode[2].ToString(),
                barcodeA4 = plcBarcode[3].ToString(),
                barcodeA5 = plcBarcode[4].ToString(),
                barcodeA6 = plcBarcode[5].ToString(),
                barcodeA7 = plcBarcode[6].ToString(),
                barcodeA8 = plcBarcode[7].ToString(),
                barcodeA9 = plcBarcode[8].ToString(),
                barcodeB1 = plcBarcode[9].ToString(),
                barcodeB2 = plcBarcode[10].ToString(),
                barcodeB3 = plcBarcode[11].ToString(),
                barcodeB4 = plcBarcode[12].ToString(),
                barcodeB5 = plcBarcode[13].ToString(),
                barcodeB6 = plcBarcode[14].ToString(),
                barcodeB7 = plcBarcode[15].ToString(),
                barcodeB8 = plcBarcode[16].ToString(),
                barcodeB9 = plcBarcode[17].ToString(),
                statusClick = enableReadFirebase.ToString(),
            };
            FirebaseResponse responseFi = await client.UpdateTaskAsync("System/", sysdata);
        }
        private void btnTurnOn_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M7000", 1);
            plc.SetDevice("M7000", 0);
            plc.GetDevice("Y70", out plcRunLight);
            plc.GetDevice("Y71", out plcStopLight);
            SetDataToFirebase();
        }

        private void btnTurnOff_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M7002", 1);
            plc.SetDevice("M7002", 0);
            plc.GetDevice("Y70", out plcRunLight);
            plc.GetDevice("Y71", out plcStopLight);
            SetDataToFirebase();
        }

        private void btnAuto_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M6006", 1);
            plc.SetDevice("M6006", 0);
            plc.GetDevice("Y72", out plcAutoLight);
            plc.GetDevice("Y73", out plcManualLight);
            SetDataToFirebase();
        }

        private void btnManual_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M6005", 1);
            plc.SetDevice("M6005", 0);
            plc.GetDevice("Y72", out plcAutoLight);
            plc.GetDevice("Y73", out plcManualLight);
            SetDataToFirebase();
        }
        private void btnImport_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M1405", 1);
            plc.SetDevice("M1405", 0);
            plc.GetDevice("M1406", out plcImportLight);
            plc.GetDevice("M1411", out plcExportLight);
            SetDataToFirebase();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M1410", 1);
            plc.SetDevice("M1410", 0);
            plc.GetDevice("M1406", out plcImportLight);
            plc.GetDevice("M1411", out plcExportLight);
            SetDataToFirebase();
        }

        private async void btnTestSQL_Click(object sender, EventArgs e)
        {
            //sqlConnection.Open();
            //SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Data", sqlConnection);
            //DataTable dtbl = new DataTable();
            //sqlDa.Fill(dtbl);

            //dtGridViewSQL.DataSource = dtbl;
            //var culture = new CultureInfo("de-DE");

            //sqlConnection.Open();
            //sqlCommand.Connection = sqlConnection;
            //sqlCommand.CommandText = @"
            //                            IF NOT EXISTS (SELECT *FROM LoadPlace WHERE ScrollNo=1)
            //                            BEGIN
            //                            INSERT INTO LoadPlace(ScrollNo,Position,Barcode,Time) VALUES (1, 'A1', '123', '"+DateTime.Now +@"')
            //                            END";
            //sqlCommand.ExecuteNonQuery();

            //SqlCommand cmd1 = new SqlCommand(@"SELECT No
            //                                  ,Barcode
            //                                  ,Product
            //                                  ,MFGDate
            //                                  ,EXPDate
            //                              FROM ProductList WHERE Barcode="+ "8931101199916", sqlConnection);
            ////cmd1.Parameters.AddWithValue("barcode",);
            //SqlDataReader reader1;
            //reader1 = cmd1.ExecuteReader();
            //if(reader1.Read())
            //{
            //    var data = new DataFirebase
            //    {
            //        ScrollNo = "1",
            //        Barcode = "8931101199916",
            //        Position = "A1",
            //        Product = reader1["Product"].ToString(),
            //        Time = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
            //        MFGDate = reader1["MFGDate"].ToString(),
            //        EXPDate = reader1["EXPDate"].ToString(),
            //        Status = "Imported",
            //    };
            //    SetResponse response = await client.SetTaskAsync("Warehouse/" + "Import/" + DateTime.Now.ToString("yyyy/") + DateTime.Now.ToString("MMMM/") + DateTime.Now.ToString("dd/") + DateTime.Now.ToString("HH:mm:ss"), data);
            //    Data result = response.ResultAs<Data>();
            //sqlConnection.Close();
            //}
            checkMaxNoSQL();
            pushDataToFirebase(1101199916,3,"Imported",noTableSQL);


            //MessageBox.Show("Write data complete.");
        }
        private async void uploadDatatoDatabase(int i, string status)
        {
            //if (isLoaded[i])
            //    return;
            //isLoaded[i] = true;
            //MessageBox.Show("OK "+i.ToString()+ isLoaded[i].ToString());
            //sqlConnection.Open();
            //sqlCommand.Connection = sqlConnection;
            //sqlCommand.CommandText = @"
            //                            IF NOT EXISTS (SELECT *FROM LoadPlace WHERE ScrollNo=" + i.ToString() + @")
            //                            BEGIN
            //                            INSERT INTO LoadPlace(ScrollNo,Position,Barcode,Time) VALUES (" + i.ToString() + @", '" + positionName[i] + @"', '123', '" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + @"')
            //                            END";
            //sqlCommand.ExecuteNonQuery();
            //sqlConnection.Close();

            //var data = new DataFirebase
            //{
            //    ScrollNo = i.ToString(),
            //    Barcode = "123",
            //    Position = positionName[i].ToString(),
            //    Product = "Flower",
            //    Time = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
            //    MFGDate = "04/05/22",
            //    EXPDate = "06/05/22",
            //    Status = status,
            //};

            //SetResponse response = await client.SetTaskAsync("Warehouse/" + "" + status + @"/" + DateTime.Now.ToString("MMMM dd") + "/" + DateTime.Now.ToString("HH:mm:ss"), data);
            //Data result = response.ResultAs<Data>();
        }
        private void SQLcomunication_Tick(object sender, EventArgs e)
        {
            
            string[] status = new string[] { "Imported", "Exported", "Expired" };
            if (plcImportLight==1)
            {
                for(int i=0; i<18; i++)
                {
                    if (plcPosition[i]==1)
                    {
                        //if(isLoaded[i]!=true)
                        //{
                        //    uploadDatatoDatabase(i, status[0]);
                        //}
                    }    
                }
            }
            
        }
        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            BarcodeReader reader = new BarcodeReader();
            var result = reader.Decode(bitmap);
            var resultTemp = reader.Decode(bitmap);
            ulong barcode = new ulong();
            int countryCode = new int();
            int companyCode = new int();
            int productCode = new int();
            int checkSum = new int();
            int barcodeWritetoPLC = new int();
            if (result != null)
            {
                
                if(result!=resultTemp)
                {
                    resultTemp = result;
                    txtBarcode.Invoke(new MethodInvoker(delegate ()
                    {
                        txtBarcode.Text = result.ToString();
                        barcode = Convert.ToUInt64(txtBarcode.Text);
                        countryCode = Convert.ToInt16(barcode / 10000000000);
                        companyCode = Convert.ToInt32(barcode % 10000000000 / 100000);
                        productCode = Convert.ToInt16(barcode % 100000 / 10);
                        checkSum = Convert.ToInt16(barcode % 10);
                        barcodeWritetoPLC = Convert.ToInt32(barcode % 10000000000);
                        txtBoxContryCode.Text = countryCode.ToString();
                        txtBoxCompanyCode.Text = companyCode.ToString();
                        txtBoxProductCode.Text = productCode.ToString();
                        txtBoxCheckSum.Text = checkSum.ToString();
                        if(plcAutoLight == 1)
                        {
                            plc.WriteDeviceBlock("D160", 2, barcodeWritetoPLC);
                            plc.WriteDeviceBlock("D161", 2, barcodeWritetoPLC >> 16);
                            if(countryCode==893 && companyCode==11011)
                            {
                                //1 pulse truebarcode bit
                                plc.SetDevice("M22", 1); //true barcode
                                plc.SetDevice("M22", 0); //true barcode
                            }
                            else
                            {
                                plc.SetDevice("M25", 1); //wrongbarcode
                                plc.SetDevice("M25", 0);
                            }
                        }
                        else if(plcManualLight == 1)
                        {
                            plc.WriteDeviceBlock("D5000", 2, barcodeWritetoPLC);
                            plc.WriteDeviceBlock("D5001", 2, barcodeWritetoPLC >> 16);
                        }
                        //MessageBox.Show("Write data completed.", "Connection");
                    }));
                }
                else
                {
                    txtBarcode.Invoke(new MethodInvoker(delegate ()
                    {
                        txtBarcode.Text = "0";
                        txtBoxContryCode.Text = "0";
                        txtBoxCompanyCode.Text = "0";
                        txtBoxProductCode.Text = "0";
                        txtBoxCheckSum.Text = "0";
                    }));
                }

            }
            picBoxCamera.Image = bitmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cboCamera.SelectedIndex].MonikerString);
            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        private void picBoxCamera_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            cboCamera.Items.Clear();
            foreach (FilterInfo device in filterInfoCollection)
                cboCamera.Items.Add(device.Name);
            cboCamera.SelectedIndex = 0;
        }
        void checkMaxNoSQL()
        {
            if(sqlConnection.State != ConnectionState.Open)
                sqlConnection.Open();

            SqlCommand cmd0 = new SqlCommand(@"SELECT MAX(No) AS MaxNo
                                                FROM Data;");
            cmd0.Connection = sqlConnection;
            SqlDataReader reader2;
            reader2 = cmd0.ExecuteReader();
            if (reader2.Read())
            {
                noTableSQL = Convert.ToInt64(reader2["MaxNo"].ToString());
            }
            if (sqlConnection.State == ConnectionState.Open)
                sqlConnection.Close();
            noTableSQL++;
        }
        async void pushDataToFirebase(int barcode, int position, string status, long no)
        {
            if (sqlConnection.State != ConnectionState.Open)
                sqlConnection.Open();
            //SqlCommand cmd1 = new SqlCommand(@"SELECT [No], [Barcode], [Product] FROM [WAREHOUSE_VER2].[dbo].[ProductList] WHERE Barcode='893" + barcode.ToString() + @"'", sqlConnection);
            //SqlCommand cmd1 = new SqlCommand(@"SELECT [No], [Barcode], [Product] FROM [WAREHOUSE_VER2].[dbo].[ProductList] WHERE Barcode = @barcode", sqlConnection);
            //cmd1.Parameters.AddWithValue("@barcode","893"+barcode.ToString());
            //SqlDataReader reader1 = cmd1.ExecuteReader();
            using var cmd1 = new SqlCommand();
            cmd1.Connection = sqlConnection;
            cmd1.CommandText = @"SELECT *FROM ProductList WHERE Barcode=@barcodeRead";
            var barcodeSQL = "893" + barcode.ToString();
            //cmd1.Parameters.AddWithValue("@barcodeRead", barcodeSQL);
            cmd1.Parameters.Add("@barcodeRead", SqlDbType.NVarChar);
            cmd1.Parameters["@barcodeRead"].Value = barcodeSQL;
            using var reader1 = cmd1.ExecuteReader();

            if (reader1.HasRows)
            {
                if(reader1.Read())
                {
                    var data = new DataFirebase
                    {
                        ScrollNo = position.ToString(),
                        Barcode = "893"+barcode.ToString(),
                        Position = positionName[position],
                        Product = reader1.GetString("Product"),
                        Time = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                        Status = status,
                    };
                    SqlCommand cmd2 = new SqlCommand();
                    cmd2.Connection = sqlConnection;
                    cmd2.CommandText = @"
                                            IF NOT EXISTS (SELECT *FROM LoadPlace WHERE ScrollNo=" + position.ToString() + @")
                                            BEGIN
                                            INSERT INTO LoadPlace(ScrollNo,Position,Barcode,Time) VALUES (" + position.ToString() + @", '" + positionName[position] + @"', '893" + barcode.ToString() + @"', '" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + @"')
                                            END";
                    cmd2.ExecuteNonQuery();
                    SqlCommand cmd3 = new SqlCommand();
                    cmd3.Connection = sqlConnection;
                    cmd3.CommandText = @"
                                            IF NOT EXISTS (SELECT *FROM Data WHERE No=" + no.ToString() + @")
                                            BEGIN
                                            INSERT INTO Data(No,Position,Barcode,Product,Time,Status) VALUES ("+no.ToString()+@", '" + positionName[position] + @"', '893" + barcode.ToString() + @"','"+ reader1["Product"].ToString() + @"', '" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + @"','"+status+@"')
                                            END";
                    cmd3.ExecuteNonQuery();
                    SetResponse response = await client.SetTaskAsync("Warehouse/2022/May/12/" + no.ToString(), data);
                    Data result = response.ResultAs<Data>();

                }
            }
            else
            {
                MessageBox.Show("No data of barcode", "SQL Connection");
            }
            if (sqlConnection.State == ConnectionState.Open)
                sqlConnection.Close();
        }
        bool checkExpiredFirebase(int scrollNo,string interval,int timeLimit)
        {
            sqlConnection.Open();
            while (sqlConnection.State != ConnectionState.Open) ;

            SqlCommand cmd4 = new SqlCommand(@"SELECT DATEDIFF("+ interval + @",(SELECT [Time] FROM LoadPlace WHERE ScrollNo="+scrollNo.ToString()+ @"),CURRENT_TIMESTAMP) AS EXPIREDTIME;");
            cmd4.Connection = sqlConnection;
            SqlDataReader reader3;
            reader3 = cmd4.ExecuteReader();
            if (reader3.Read())
            {
                if (Convert.ToInt64(reader3["EXPIREDTIME"].ToString()) > timeLimit)
                {
                    sqlConnection.Close();
                    return true;
                }
            }
            sqlConnection.Close();
            return false;
        }
        void checkPositionAndBarcode()
        {
            plc.GetDevice("M500", out plcPosition[0]);
            plc.GetDevice("M501", out plcPosition[1]);
            plc.GetDevice("M502", out plcPosition[2]);
            plc.GetDevice("M503", out plcPosition[3]);
            plc.GetDevice("M504", out plcPosition[4]);
            plc.GetDevice("M505", out plcPosition[5]);
            plc.GetDevice("M506", out plcPosition[6]);
            plc.GetDevice("M507", out plcPosition[7]);
            plc.GetDevice("M508", out plcPosition[8]);
            plc.GetDevice("M509", out plcPosition[9]);
            plc.GetDevice("M510", out plcPosition[10]);
            plc.GetDevice("M511", out plcPosition[11]);
            plc.GetDevice("M512", out plcPosition[12]);
            plc.GetDevice("M513", out plcPosition[13]);
            plc.GetDevice("M514", out plcPosition[14]);
            plc.GetDevice("M515", out plcPosition[15]);
            plc.GetDevice("M516", out plcPosition[16]);
            plc.GetDevice("M517", out plcPosition[17]);

            int vartemp = new int();
            plc.GetDevice("D130", out plcBarcode[0]);
            plc.GetDevice("D131", out vartemp);
            plcBarcode[0] = (vartemp << 16) | plcBarcode[0];
            plc.GetDevice("D112", out plcBarcode[1]);
            plc.GetDevice("D113", out vartemp);
            plcBarcode[1] = (vartemp << 16) | plcBarcode[1];
            plc.GetDevice("D114", out plcBarcode[2]);
            plc.GetDevice("D115", out vartemp);
            plcBarcode[2] = (vartemp << 16) | plcBarcode[2];
            plc.GetDevice("D116", out plcBarcode[3]);
            plc.GetDevice("D117", out vartemp);
            plcBarcode[3] = (vartemp << 16) | plcBarcode[3];
            plc.GetDevice("D118", out plcBarcode[4]);
            plc.GetDevice("D119", out vartemp);
            plcBarcode[4] = (vartemp << 16) | plcBarcode[4];
            plc.GetDevice("D120", out plcBarcode[5]);
            plc.GetDevice("D121", out vartemp);
            plcBarcode[5] = (vartemp << 16) | plcBarcode[5];
            plc.GetDevice("D122", out plcBarcode[6]);
            plc.GetDevice("D123", out vartemp);
            plcBarcode[6] = (vartemp << 16) | plcBarcode[6];
            plc.GetDevice("D124", out plcBarcode[7]);
            plc.GetDevice("D125", out vartemp);
            plcBarcode[7] = (vartemp << 16) | plcBarcode[7];
            plc.GetDevice("D126", out plcBarcode[8]);
            plc.GetDevice("D127", out vartemp);
            plcBarcode[8] = (vartemp << 16) | plcBarcode[8];
            plc.GetDevice("D128", out plcBarcode[9]);
            plc.GetDevice("D129", out vartemp);
            plcBarcode[9] = (vartemp << 16) | plcBarcode[9];
            plc.GetDevice("D132", out plcBarcode[10]);
            plc.GetDevice("D133", out vartemp);
            plcBarcode[10] = (vartemp << 16) | plcBarcode[10];
            plc.GetDevice("D134", out plcBarcode[11]);
            plc.GetDevice("D135", out vartemp);
            plcBarcode[11] = (vartemp << 16) | plcBarcode[11];
            plc.GetDevice("D136", out plcBarcode[12]);
            plc.GetDevice("D137", out vartemp);
            plcBarcode[12] = (vartemp << 16) | plcBarcode[12];
            plc.GetDevice("D138", out plcBarcode[13]);
            plc.GetDevice("D139", out vartemp);
            plcBarcode[13] = (vartemp << 16) | plcBarcode[13];
            plc.GetDevice("D140", out plcBarcode[14]);
            plc.GetDevice("D141", out vartemp);
            plcBarcode[14] = (vartemp << 16) | plcBarcode[14];
            plc.GetDevice("D142", out plcBarcode[15]);
            plc.GetDevice("D143", out vartemp);
            plcBarcode[15] = (vartemp << 16) | plcBarcode[15];
            plc.GetDevice("D144", out plcBarcode[16]);
            plc.GetDevice("D145", out vartemp);
            plcBarcode[16] = (vartemp << 16) | plcBarcode[16];
            plc.GetDevice("D146", out plcBarcode[17]);
            plc.GetDevice("D147", out vartemp);
            plcBarcode[17] = (vartemp << 16) | plcBarcode[17];
        }
        void changePositionImage(int scrollNo, int var, PictureBox position)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                if (var == 1)
                {
                    if (isLoaded[scrollNo])
                        return;
                    position.Image = Properties.Resources.SCADA_isProduct;
                    position.Refresh();
                    checkPositionAndBarcode();
                    SetDataToFirebase();
                    checkMaxNoSQL();
                    pushDataToFirebase(plcBarcode[scrollNo], scrollNo, "Imported", noTableSQL);
                    isLoaded[scrollNo] = true;
                }
                else
                {
                    if (!isLoaded[scrollNo])
                        return;
                    position.Image = Properties.Resources.SCADA_noProduct;
                    position.Refresh();
                    SetDataToFirebase();
                    checkMaxNoSQL();
                    while (txtBarcode.Text==null || txtBarcode.Text=="0") ;
                    pushDataToFirebase(Convert.ToInt32(txtBarcode.Text.Remove(0,3)), scrollNo, "Exported", noTableSQL);
                    checkPositionAndBarcode();
                    isLoaded[scrollNo] = false;
                }
                
            }));
        }
        void updateCycle() //Thread cycle**************************************************************************************************************
        {
            while (true)
            {
                plc.GetDevice("M10", out plcErrorNumber);
                plc.GetDevice("D90", out plcErrorX);
                plc.GetDevice("D92", out plcErrorZ);
                plc.GetDevice("D94", out plcErrorY1);
                plc.GetDevice("D96", out plcErrorY2);

                plc.GetDevice("D800", out plcXPosition);
                plc.GetDevice("D802", out plcZPosition);
                plc.GetDevice("D804", out plcY1Position);
                plc.GetDevice("D806", out plcY2Position);
                plc.GetDevice("D238", out plcSpeedConveyor);

                plc.GetDevice("D74", out plcXSpeed);
                plc.GetDevice("D78", out plcZSpeed);
                plc.GetDevice("D82", out plcY1Speed);
                plc.GetDevice("D86", out plcY2Speed);

                plc.GetDevice("Y70", out plcRunLight);
                plc.GetDevice("Y71", out plcStopLight);
                plc.GetDevice("M8001", out plcAlarmLight);
                plc.GetDevice("Y72", out plcAutoLight);
                plc.GetDevice("Y73", out plcManualLight);
                plc.GetDevice("M1406", out plcImportLight);
                plc.GetDevice("M1411", out plcExportLight);

                plc.GetDevice("D5000", out plcCurrentBarcode);
                plc.GetDevice("M6000", out plcWrongBarcode);

                plc.GetDevice("M8000", out plcHomeComplete);
                plc.GetDevice("X2E", out plcEntrySensor);
                plc.GetDevice("X2D", out plcPackageSensor);
                plc.GetDevice("Y75", out plcCylinder);
                plc.GetDevice("Y77", out plcSuctionCup);

                plc.GetDevice("M500", out plcPosition[0]);
                plc.GetDevice("M501", out plcPosition[1]);
                plc.GetDevice("M502", out plcPosition[2]);
                plc.GetDevice("M503", out plcPosition[3]);
                plc.GetDevice("M504", out plcPosition[4]);
                plc.GetDevice("M505", out plcPosition[5]);
                plc.GetDevice("M506", out plcPosition[6]);
                plc.GetDevice("M507", out plcPosition[7]);
                plc.GetDevice("M508", out plcPosition[8]);
                plc.GetDevice("M509", out plcPosition[9]);
                plc.GetDevice("M510", out plcPosition[10]);
                plc.GetDevice("M511", out plcPosition[11]);
                plc.GetDevice("M512", out plcPosition[12]);
                plc.GetDevice("M513", out plcPosition[13]);
                plc.GetDevice("M514", out plcPosition[14]);
                plc.GetDevice("M515", out plcPosition[15]);
                plc.GetDevice("M516", out plcPosition[16]);
                plc.GetDevice("M517", out plcPosition[17]);

                int vartemp = new int();
                plc.GetDevice("D130", out plcBarcode[0]);
                plc.GetDevice("D131", out vartemp);
                plcBarcode[0] = (vartemp << 16) | plcBarcode[0];
                plc.GetDevice("D112", out plcBarcode[1]);
                plc.GetDevice("D113", out vartemp);
                plcBarcode[1] = (vartemp << 16) | plcBarcode[1];
                plc.GetDevice("D114", out plcBarcode[2]);
                plc.GetDevice("D115", out vartemp);
                plcBarcode[2] = (vartemp << 16) | plcBarcode[2];
                plc.GetDevice("D116", out plcBarcode[3]);
                plc.GetDevice("D117", out vartemp);
                plcBarcode[3] = (vartemp << 16) | plcBarcode[3];
                plc.GetDevice("D118", out plcBarcode[4]);
                plc.GetDevice("D119", out vartemp);
                plcBarcode[4] = (vartemp << 16) | plcBarcode[4];
                plc.GetDevice("D120", out plcBarcode[5]);
                plc.GetDevice("D121", out vartemp);
                plcBarcode[5] = (vartemp << 16) | plcBarcode[5];
                plc.GetDevice("D122", out plcBarcode[6]);
                plc.GetDevice("D123", out vartemp);
                plcBarcode[6] = (vartemp << 16) | plcBarcode[6];
                plc.GetDevice("D124", out plcBarcode[7]);
                plc.GetDevice("D125", out vartemp);
                plcBarcode[7] = (vartemp << 16) | plcBarcode[7];
                plc.GetDevice("D126", out plcBarcode[8]);
                plc.GetDevice("D127", out vartemp);
                plcBarcode[8] = (vartemp << 16) | plcBarcode[8];
                plc.GetDevice("D128", out plcBarcode[9]);
                plc.GetDevice("D129", out vartemp);
                plcBarcode[9] = (vartemp << 16) | plcBarcode[9];
                plc.GetDevice("D132", out plcBarcode[10]);
                plc.GetDevice("D133", out vartemp);
                plcBarcode[10] = (vartemp << 16) | plcBarcode[10];
                plc.GetDevice("D134", out plcBarcode[11]);
                plc.GetDevice("D135", out vartemp);
                plcBarcode[11] = (vartemp << 16) | plcBarcode[11];
                plc.GetDevice("D136", out plcBarcode[12]);
                plc.GetDevice("D137", out vartemp);
                plcBarcode[12] = (vartemp << 16) | plcBarcode[12];
                plc.GetDevice("D138", out plcBarcode[13]);
                plc.GetDevice("D139", out vartemp);
                plcBarcode[13] = (vartemp << 16) | plcBarcode[13];
                plc.GetDevice("D140", out plcBarcode[14]);
                plc.GetDevice("D141", out vartemp);
                plcBarcode[14] = (vartemp << 16) | plcBarcode[14];
                plc.GetDevice("D142", out plcBarcode[15]);
                plc.GetDevice("D143", out vartemp);
                plcBarcode[15] = (vartemp << 16) | plcBarcode[15];
                plc.GetDevice("D144", out plcBarcode[16]);
                plc.GetDevice("D145", out vartemp);
                plcBarcode[16] = (vartemp << 16) | plcBarcode[16];
                plc.GetDevice("D146", out plcBarcode[17]);
                plc.GetDevice("D147", out vartemp);
                plcBarcode[17] = (vartemp << 16) | plcBarcode[17];

                while (!this.IsHandleCreated)
                    System.Threading.Thread.Sleep(100);
                this.Invoke(new MethodInvoker(delegate ()
                {
                    changePositionImage(0,plcPosition[0], picBoxPositionA1);
                    changePositionImage(1,plcPosition[1], picBoxPositionA2);
                    changePositionImage(2,plcPosition[2], picBoxPositionA3);
                    changePositionImage(3,plcPosition[3], picBoxPositionA4);
                    changePositionImage(4,plcPosition[4], picBoxPositionA5);
                    changePositionImage(5,plcPosition[5], picBoxPositionA6);
                    changePositionImage(6,plcPosition[6], picBoxPositionA7);
                    changePositionImage(7,plcPosition[7], picBoxPositionA8);
                    changePositionImage(8,plcPosition[8], picBoxPositionA9);

                    changePositionImage(9,plcPosition[9], picBoxPositionB1);
                    changePositionImage(10,plcPosition[10], picBoxPositionB2);
                    changePositionImage(11,plcPosition[11], picBoxPositionB3);
                    changePositionImage(12,plcPosition[12], picBoxPositionB4);
                    changePositionImage(13,plcPosition[13], picBoxPositionB5);
                    changePositionImage(14,plcPosition[14], picBoxPositionB6);
                    changePositionImage(15,plcPosition[15], picBoxPositionB7);
                    changePositionImage(16,plcPosition[16], picBoxPositionB8);
                    changePositionImage(17,plcPosition[17], picBoxPositionB9);
                    txtBoxXaxisSpeed.Text = (plcXSpeed / 10).ToString();
                    txtBoxZaxisSpeed.Text = (plcZSpeed / 10).ToString();
                    txtBoxY1axisSpeed.Text = (plcY1Speed / 10).ToString();
                    txtBoxY2axisSpeed.Text = (plcY2Speed / 10).ToString();
                    txtBoxConveyorSpeed.Text = (plcSpeedConveyor / 10).ToString();

                    txtBoxXaxisPosition.Text = (plcXPosition / 10).ToString();
                    txtBoxZaxisPosition.Text = (plcZPosition / 10).ToString();
                    txtBoxY1axisPosition.Text = (plcY1Position / 10).ToString();
                    txtBoxY2axisPosition.Text = (plcY2Position / 10).ToString();

                    txtBoxXerror.Text = String.Format("{0:X}H", plcErrorX).ToString();
                    txtBoxZerror.Text = String.Format("{0:X}H", plcErrorZ).ToString();
                    txtBoxY1error.Text = String.Format("{0:X}H", plcErrorY1).ToString();
                    txtBoxY2error.Text = String.Format("{0:X}H", plcErrorY2).ToString();

                    if (plcRunLight == 1)
                    {
                        btnOnLight.BackColor = Color.Green;
                        lbON.ForeColor = Color.Green;
                    }
                    else
                    {
                        btnOnLight.BackColor = Color.White;
                        lbON.ForeColor = Color.LightGray;
                    }
                    if (plcStopLight == 1)
                    {
                        btnOffLight.BackColor = Color.Red;
                        lbOFF.ForeColor = Color.Red;
                    }
                    else
                    {
                        btnOffLight.BackColor = Color.White;
                        lbOFF.ForeColor = Color.LightGray;
                    }
                    if (plcAutoLight == 1)
                    {
                        btnAutoLight.BackColor = Color.LightSlateGray;
                        lbAUTO.ForeColor = Color.LightSlateGray;
                    }
                    else
                    {
                        btnAutoLight.BackColor = Color.White;
                        lbAUTO.ForeColor = Color.LightGray;
                    }
                    if (plcManualLight == 1)
                    {
                        btnManualLight.BackColor = Color.LightSlateGray;
                        lbMANUAL.ForeColor = Color.LightSlateGray;
                    }
                    else
                    {
                        btnManualLight.BackColor = Color.White;
                        lbMANUAL.ForeColor = Color.LightGray;
                    }
                    if (plcImportLight == 1)
                    {
                        btnImportLight.BackColor = Color.DarkSlateBlue;
                        lbIMPORT.ForeColor = Color.DarkSlateBlue;
                    }
                    else
                    {
                        btnImportLight.BackColor = Color.White;
                        lbIMPORT.ForeColor = Color.LightGray;
                    }
                    if (plcExportLight == 1)
                    {
                        btnExportLight.BackColor = Color.DarkSlateBlue;
                        lbEXPORT.ForeColor = Color.DarkSlateBlue;
                    }
                    else
                    {
                        btnExportLight.BackColor = Color.White;
                        lbEXPORT.ForeColor = Color.LightGray;
                    }
                    if (plcAlarmLight == 1)
                    {
                        btnAlarmLight.BackColor = Color.Yellow;
                        //MessageBox.Show("There are some errors in system", "Error");
                    }
                    else
                    {
                        btnAlarmLight.BackColor = Color.White;
                    }
                    if (plcHomeComplete == 1)
                    {
                        btnHomeComplete.BackColor = Color.Green;
                    }
                    else
                    {
                        btnHomeComplete.BackColor = Color.White;
                    }
                    if (plcEntrySensor == 1)
                    {
                        btnEntrySensor.BackColor = Color.Green;
                    }
                    else
                    {
                        btnEntrySensor.BackColor = Color.White;
                    }
                    if (plcPackageSensor == 1)
                    {
                        btnPackageSensor.BackColor = Color.Green;
                    }
                    else
                    {
                        btnPackageSensor.BackColor = Color.White;
                    }
                    if (plcCylinder == 1)
                    {
                        btnCylinder.BackColor = Color.Red;
                    }
                    else
                    {
                        btnCylinder.BackColor = Color.White;
                    }
                    if (plcSuctionCup == 1)
                    {
                        btnSuctionCup.BackColor = Color.Red;
                    }
                    else
                    {
                        btnSuctionCup.BackColor = Color.White;
                    }
                    if (plcWrongBarcode == 1)
                    {
                        btnWrongBarcode.BackColor = Color.Red;
                    }
                    else
                    {
                        btnWrongBarcode.BackColor = Color.White;
                    }
                    //DataToFirebase(true);
                    //SetDataToFirebase(0);
                    ReadDataToFirebase();
                }));

            }

        }
        void updateCycleFirebase()
        {
            
            while (true)
            {

                //SystemDataFirebase sysDataFirebase = new SystemDataFirebase()
                //{
                //    isImport = "false",
                //    isManual = "false",
                //    isOn = "false",
                //    loadedA1 = "false",
                //    loadedA2 = "false",
                //    loadedA3 = "false",
                //    loadedA4 = "false",
                //    loadedA5 = "false",
                //    loadedA6 = "false",
                //    loadedA7 = "false",
                //    loadedA8 = "false",
                //    loadedA9 = "false",
                //    loadedB1 = "false",
                //    loadedB2 = "false",
                //    loadedB3 = "false",
                //    loadedB4 = "false",
                //    loadedB5 = "false",
                //    loadedB6 = "false",
                //    loadedB7 = "false",
                //    loadedB8 = "false",
                //    loadedB9 = "false",
                //};
                string[] status = new string[] { "Imported", "Exported", "Expired" };
                if (plcImportLight == 1)
                {
                    for (int i = 0; i < 18; i++)
                    {
                        if (plcPosition[i] == 1)
                        {
                            //if (isLoaded[i] != true)
                            //{
                            //    uploadDatatoDatabase(i, status[0]);
                            //}
                        }
                    }
                }

                //while (!this.IsHandleCreated)
                //    System.Threading.Thread.Sleep(100);
                //this.Invoke(new MethodInvoker(delegate ()
                //{
                //    //var result = client.Get("System/");
                //    //SystemDataFirebase sysData = result.ResultAs<SystemDataFirebase>();

                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA1)), picBoxPositionA1);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA2)), picBoxPositionA2);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA3)), picBoxPositionA3);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA4)), picBoxPositionA4);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA5)), picBoxPositionA5);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA6)), picBoxPositionA6);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA7)), picBoxPositionA7);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA8)), picBoxPositionA8);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedA9)), picBoxPositionA9);

                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB1)), picBoxPositionB1);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB2)), picBoxPositionB2);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB3)), picBoxPositionB3);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB4)), picBoxPositionB4);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB5)), picBoxPositionB5);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB6)), picBoxPositionB6);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB7)), picBoxPositionB7);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB8)), picBoxPositionB8);
                //    //changePositionImage(Convert.ToInt16(Convert.ToBoolean(sysData.loadedB9)), picBoxPositionB9);

                //    //if(Convert.ToBoolean(sysData.isOn))
                //    //{
                //    //    btnTurnOn.PerformClick();
                //    //}
                //    //if (sysData.isOff == "true")
                //    //{

                //    //}
                //    //if (sysData.isAuto == "true")
                //    //{

                //    //}
                //    //if (sysData.isManual == "true")
                //    //{

                //    //}
                //    //if (sysData.isImport == "true")
                //    //{

                //    //}
                //    //if (sysData.isExport == "true")
                //    //{

                //    //}
                //}));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var result = client.Get("System/");
            SystemDataFirebase sysData = result.ResultAs<SystemDataFirebase>();
            if(Convert.ToBoolean(sysData.isOn))
            {
                btnTurnOn.PerformClick();
            }
        }
        async void ReadDataToFirebase()
        {
            if(client!=null)
            {
                FirebaseResponse resp2 = await client.GetTaskAsync("System/");
                SystemDataFirebase sysData = resp2.ResultAs<SystemDataFirebase>();
                if(sysData == null)
                {
                    MessageBox.Show("Error of Internet", "Connection");
                }
                if(Convert.ToBoolean(Convert.ToInt16(sysData.statusClick)))
                {
                    if (Convert.ToBoolean(Convert.ToInt16(sysData.isOn))&&(plcRunLight!=1))
                    {
                        btnTurnOn.PerformClick();
                        return;
                    }
                    else if (Convert.ToBoolean(Convert.ToInt16(sysData.isOff))&&(plcStopLight!=1))
                    {
                        btnTurnOff.PerformClick();
                        return;
                    }
                    if (Convert.ToBoolean(Convert.ToInt16(sysData.isAuto))&&(plcAutoLight!=1))
                    {
                        btnAuto.PerformClick();
                        return;
                    }
                    else if (Convert.ToBoolean(Convert.ToInt16(sysData.isManual))&&(plcManualLight!=1))
                    {
                        btnManual.PerformClick();
                        return;
                    }
                    if (Convert.ToBoolean(Convert.ToInt16(sysData.isImport))&&(plcImportLight!=1))
                    {
                        btnImport.PerformClick();
                        return;
                    }
                    else if (Convert.ToBoolean(Convert.ToInt16(sysData.isExport))&&(plcExportLight!=1))
                    {
                        btnExport.PerformClick();
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Connection to Firebase failed.","Connection");
            }
        }

        private void label65_Click(object sender, EventArgs e)
        {

        }

        private void btnUpdateStorageTime_Click(object sender, EventArgs e)
        {
            //txtBoxTimeStorage.Text = cbIntervalStorage.Text;
            if (checkExpiredFirebase(1, cbIntervalStorage.Text, Convert.ToInt32(txtBoxTimeStorage.Text)))
                MessageBox.Show("oke");
        }
    }
}

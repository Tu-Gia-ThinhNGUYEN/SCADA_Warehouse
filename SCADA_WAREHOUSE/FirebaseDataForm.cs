using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ActUtlTypeLib; //Connect MX Component
//Firebase Config
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace SCADA_WAREHOUSE
{
    public partial class FirebaseDataForm : Form
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
        //System's variable
        DataTable dt = new DataTable();

        public FirebaseDataForm()
        {
            InitializeComponent();
        }

        private void FirebaseDataForm_Load(object sender, EventArgs e)
        {
            client = new FireSharp.FirebaseClient(config);

            if (client != null)
            {
                MessageBox.Show("Firebase Connection Successful!");
            }

            if (plc.Open() == 0)
            {
                lbStatus.Text = "Connected";
                lbStatus.ForeColor = Color.Green;
            }
            else
            {
                lbStatus.Text = "Disconnected";
                lbStatus.ForeColor = Color.Red;
            }
            client = new FireSharp.FirebaseClient(config);

            dt.Columns.Add("hoten");
            dt.Columns.Add("lop");
            dt.Columns.Add("truong");

            dtgDSHS.DataSource = dt;

            dtgDSHS.Columns[0].HeaderText = "Họ và tên";
            dtgDSHS.Columns[1].HeaderText = "Lớp";
            dtgDSHS.Columns[2].HeaderText = "Trường";
        }
        private async void RetrieveData()
        {
            dt.Rows.Clear();
            FirebaseResponse resp2 = await client.GetTaskAsync("Information/");
            Data obj2 = resp2.ResultAs<Data>();

            DataRow row = dt.NewRow();
            row["hoten"] = obj2.HoTen;
            row["lop"] = obj2.Lop;
            row["truong"] = obj2.Truong;

            dt.Rows.Add(row);
        }
        
    }
}

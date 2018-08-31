using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GRC_Clinical_Genetics_Application
{
    public partial class Dashboard : Form
    {
        private int userID;
        private string GRCNumber;
        private string ApplicationStatus;
        private string GRCStatus;
        private string patientFirstName;
        private string patientLastName;
        private int personalHealthNumber;
        private bool isUrgent = false;
        private bool listAll = false;
        private bool defaultData = true;
        private int openMetricsID = 1;
        private int urgentMetricsID = 2;
        private bool logOut = false;
        DashboardClass dashboard;

        private bool isClinicalApp = true;
        Connections grcConnect = new Connections();
        public Dashboard(int id)
        {
            InitializeComponent();
            this.userID = id;
            dashboard = new DashboardClass(userID);
            this.NameLabel.Text = dashboard.UpdateGreeting();
            InitializeDataTable();
        }

        public void InitializeDataTable()
        {
            defaultData = true;
            StatusComboBox.SelectedItem = "Any";
            AppStatus.SelectedItem = "Any";
            DataTable dt = dashboard.UpdateAppTable(defaultData);
            ApplicationListTableView.DataSource = dt;

            UpdateMetricLabels();
        }
        internal void UpdateMetricLabels()
        {
            NumLabel1.Text = dashboard.UpdateMetrics(openMetricsID).ToString();
            NumLabel2.Text = dashboard.UpdateMetrics(urgentMetricsID).ToString();
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            logOut = true;
            this.Close();
            Application.Restart();
        }

        private void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!logOut)
            {
                Application.Exit();
            }
        }

        //limits textbox to numbers and '-'
        private void GRCNumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }
        }

        private void PHNTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            GRCNumber = GRCNumberTextBox.Text;
            ApplicationStatus = AppStatus.SelectedItem.ToString();
            GRCStatus = StatusComboBox.SelectedItem.ToString();
            patientFirstName = PatientFirstNameTextBox.Text;
            patientLastName = PatientLastNameTextBox.Text;
            personalHealthNumber = (PHNTextBox.Text != "") ? Convert.ToInt32(PHNTextBox.Text) : 0;
            isUrgent = (UrgentCheckBox.CheckState == CheckState.Checked) ? true : false;
            listAll = (listAllCheckBox.CheckState == CheckState.Checked) ? true : false;

            defaultData = (GRCNumber == "" && ApplicationStatus == "Any" && GRCStatus == "Any" && patientFirstName == "" && patientLastName == "" && PHNTextBox.Text == "" && !isUrgent && !listAll) ? true : false;
            ApplicationListTableView.DataSource = null;
            DataTable dt = dashboard.UpdateAppTable(defaultData, GRCNumber, GRCStatus, patientFirstName, patientLastName, personalHealthNumber, isUrgent, listAll, ApplicationStatus);
            ApplicationListTableView.DataSource = dt;
            UpdateMetricLabels();
        }

        private void NewApplicationButton_Click(object sender, EventArgs e)
        {
            ApplicationForm newApp = new ApplicationForm(this, userID, false);
            newApp.Show();
        }

        private void ApplicationListTableView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex < 0)
            {
                return;
            }

            if (isClinicalApp)
            {
                int applicationNum = (ApplicationListTableView.Rows[e.RowIndex].Cells[0].Value != DBNull.Value) ? Convert.ToInt32(ApplicationListTableView.Rows[e.RowIndex].Cells[0].Value) : 0;
                if (applicationNum == 0)
                {
                    return;
                }
                //open existing application
                if (e.ColumnIndex == 1)
                {
                    Console.WriteLine(ApplicationListTableView.Rows[e.RowIndex].Cells[1].Value);
                    return;
                }
                ApplicationForm newApp = new ApplicationForm(this, userID, true, applicationNum);
                newApp.Show();
            }else
            {
                string GRCID = (ApplicationListTableView.Rows[e.RowIndex].Cells[0].Value != DBNull.Value) ? ApplicationListTableView.Rows[e.RowIndex].Cells[0].Value.ToString() : "";
                int orderID = 0;

                grcConnect.GRC_Connection.Open();
                SqlCommand cmd = grcConnect.GetOrderID(GRCID);
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    orderID = Convert.ToInt32(sdr[0]);
                }
                grcConnect.GRC_Connection.Close();
                if (GRCID == "")
                {
                    return;
                }
                GRCForm form = new GRCForm(userID, true, orderID);
                form.Show();
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            foreach(Control c in this.Controls)
            {
                if(c is TextBox)
                {
                    ((TextBox)c).Text = "";
                }
                if(c is CheckBox)
                {
                    ((CheckBox)c).CheckState = CheckState.Unchecked;
                }
                if(c is ComboBox)
                {
                    ((ComboBox)c).SelectedItem = "Any";
                }
            }
        }

        private void GRCDashboardButton_Click(object sender, EventArgs e)
        {
            isClinicalApp = !isClinicalApp;
            if (!isClinicalApp)
            {
                SearchButton.Hide();
                GRCSearchButton.Show();
                GRCDashboardButton.Text = "Clinical Application";
                HeaderLabel1.Text = "GRC Dashboard";
                HeaderLabel1.ForeColor = Color.OrangeRed;
                Bar1.BackColor = Color.Wheat;
                HelloLabel.BackColor = Color.Wheat;
                NameLabel.BackColor = Color.Wheat;
                ApplicationListTableView.BackgroundColor = Color.Cornsilk;
                listAllCheckBox.Checked = true;

                label9.Hide();
                AppStatus.Hide();

                defaultData = (GRCNumber == "" && ApplicationStatus == "Any" && GRCStatus == "Any" && patientFirstName == "" && patientLastName == "" && PHNTextBox.Text == "" && !isUrgent && !listAll) ? true : false;
                ApplicationListTableView.DataSource = null;
                DataTable dt = dashboard.UpdateGRCTable(true);
                ApplicationListTableView.DataSource = dt;
            }
            else
            {
                SearchButton.Show();
                GRCSearchButton.Hide();
                GRCDashboardButton.Text = "GRC Application";
                HeaderLabel1.Text = "Clinical Genetics Dashboard";
                HeaderLabel1.ForeColor = SystemColors.HotTrack;
                Bar1.BackColor = SystemColors.GradientInactiveCaption;
                HelloLabel.BackColor = SystemColors.GradientInactiveCaption;
                NameLabel.BackColor = SystemColors.GradientInactiveCaption;
                ApplicationListTableView.BackgroundColor = Color.AliceBlue;
                listAllCheckBox.Checked = false;
                label9.Show();
                AppStatus.Show();

                defaultData = (GRCNumber == "" && ApplicationStatus == "Any" && GRCStatus == "Any" && patientFirstName == "" && patientLastName == "" && PHNTextBox.Text == "" && !isUrgent && !listAll) ? true : false;
                ApplicationListTableView.DataSource = null;
                DataTable dt = dashboard.UpdateAppTable(defaultData, GRCNumber, GRCStatus, patientFirstName, patientLastName, personalHealthNumber, isUrgent, listAll, ApplicationStatus);
                ApplicationListTableView.DataSource = dt;
            }

        }

        private void GRCSearchButton_Click(object sender, EventArgs e)
        {
            GRCNumber = GRCNumberTextBox.Text;
            ApplicationStatus = AppStatus.SelectedItem.ToString();
            GRCStatus = StatusComboBox.SelectedItem.ToString();
            patientFirstName = PatientFirstNameTextBox.Text;
            patientLastName = PatientLastNameTextBox.Text;
            personalHealthNumber = (PHNTextBox.Text != "") ? Convert.ToInt32(PHNTextBox.Text) : 0;

            isUrgent = (UrgentCheckBox.CheckState == CheckState.Checked) ? true : false;
            listAll = (listAllCheckBox.CheckState == CheckState.Checked) ? true : false;

            defaultData = (GRCNumber == "" && ApplicationStatus == "Any" && GRCStatus == "Any" && patientFirstName == "" && patientLastName == "" && PHNTextBox.Text == "" && !isUrgent && !listAll) ? true : false;
            ApplicationListTableView.DataSource = null;
            DataTable dt = dashboard.UpdateGRCTable(defaultData, GRCNumber, GRCStatus, patientFirstName, patientLastName, personalHealthNumber, isUrgent, listAll, ApplicationStatus);
            ApplicationListTableView.DataSource = dt;
        }
    }
}

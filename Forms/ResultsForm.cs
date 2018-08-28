using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GRC_Clinical_Genetics_Application
{
    public partial class ResultsForm : Form
    {
        private int appID;
        private int empID;
        private int orderID;
        private string employeeName;
        Connections con = new Connections();
        public ResultsForm(int a, int e)
        {
            appID = a;
            empID = e;
            InitializeComponent();
        }

        private void ResultsForm_Load(object sender, EventArgs e)
        {
            con.GRC_Connection.Open();
            SqlCommand cmd = con.NameCommand(empID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                employeeName = sdr[1].ToString() + " " + sdr[2].ToString();
            }
            con.GRC_Connection.Close();

            //get order id and insert into results
            con.GRC_Connection.Open();
            int labID = 0;
            cmd = con.GetOrderID(appID);
            sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                orderID = Convert.ToInt32(sdr[0]);
                labID = Convert.ToInt32(sdr[1]);
            }
            con.GRC_Connection.Close();

            con.GRC_Connection.Open();
            //create result record
            cmd = con.NewResult(orderID, labID, empID);
            cmd.ExecuteNonQuery();
            con.GRC_Connection.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openFileDialog.Title = "Results Upload";
            openFileDialog.Filter = "(*.pdf; *.doc; *.xlsx)|*.pdf; *.docx; *.xlsx";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog.CheckFileExists)
                {
                    string path = Path.GetFullPath(openFileDialog.FileName);
                    PathBox.Text = path;
                }
            }
        }

        private void UploadReportButton_Click(object sender, EventArgs e)
        {
            if (PathBox.Text == "")
            {
                MessageBox.Show("There is no file to upload!");
                return;
            }
            string filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            string ext = Path.GetExtension(openFileDialog.FileName);
            int documentID = 0;
            string path = "\\jeeves.crha-health.ab.ca\\Genetics\\GRC\\PTLL Pilot\\Results";
            if (filename == null)
            {
                MessageBox.Show("Please select a valid document.");
            }
            else
            {
                //SAVE in database real document name, SAVE in directory DocumentType_AppID_DocumentID_PHN/Name.pdf/doc/xls
                con.GRC_Connection.Open();
                SqlCommand cmd = new SqlCommand("insert into [GRC].[dbo].[Result Documents] ([OrderID], [Document Name], [Document Type], [Document Ext], [Update By], [Created Date]) values (" +
                    orderID + ", '" + filename + ext + "', 'Result', '" + ext + "', '" + employeeName + "', Convert(VARCHAR(10), GETDATE(), 126))", con.GRC_Connection);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("select top 1 [DocumentID], [Document Type] from [GRC].[dbo].[Result Documents] where [OrderID] = '" +
                    orderID + "' order by [DocumentID] desc", con.GRC_Connection);
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    documentID = Convert.ToInt32(sdr[0]);
                }
                con.GRC_Connection.Close();

                path = path + "_" + orderID + "_" + documentID + ext;
                con.GRC_Connection.Open();
                cmd = con.UpdateResultsDestination(documentID, path);
                cmd.ExecuteNonQuery();
                con.GRC_Connection.Close();
                File.Copy(openFileDialog.FileName, path);            
                MessageBox.Show("Document uploaded.");
            }
        }

        private void ResultsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            con.GRC_Connection.Open();
            SqlCommand cmd = con.DeleteResult(orderID, 0);
            cmd.ExecuteNonQuery();
            con.GRC_Connection.Close();

            con.GRC_Connection.Open();
            cmd = con.DeleteResult(orderID, 1);
            cmd.ExecuteNonQuery();
            con.GRC_Connection.Close();
        }
    }
}

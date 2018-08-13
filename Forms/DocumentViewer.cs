using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GRC_Clinical_Genetics_Application
{
    public partial class DocumentViewer : Form
    {
        private int applicationID = 0;
        private string savedFileName = "";
        Connections docConnection = new Connections();
        public DocumentViewer(int appID, string fileName)
        {
            InitializeComponent();
            applicationID = appID;
            savedFileName = fileName;
        }

        private void DocumentViewer_Load(object sender, EventArgs e)
        {
            DataTable dt = UpdateDocumentsList();
            DocumentListComboBox.DisplayMember = "Document Name";
            DocumentListComboBox.DataSource = dt;
        }

        private DataTable UpdateDocumentsList()
        {
            DataTable docList = new DataTable();
            docConnection.GRC_Connection.Open();
            SqlDataAdapter adapt = docConnection.GetDocumentList(applicationID);
            adapt.Fill(docList);
            docConnection.GRC_Connection.Close();
            return docList;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            DataRowView drv = DocumentListComboBox.SelectedItem as DataRowView;
            string documentName = "";
            if(drv != null)
            {
                documentName = drv.Row["Document Name"] as string;
            }

            string docPath = "";
            docConnection.GRC_Connection.Open();
            SqlCommand cmd = docConnection.GetDocPath(applicationID, documentName);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                docPath = sdr[0].ToString();
            }
            docConnection.GRC_Connection.Close();

            if(docPath == "")
            {
                MessageBox.Show("There are no files to open!");
                return;
            }
            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = true;
            info.Verb = "open";
            info.FileName = docPath;
            Process.Start(info);
            
        }

    }
}

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
        private int ID = 0;
        private bool is_GRC = false;
        Connections docConnection = new Connections();
        public DocumentViewer(int iD, bool isGRC)
        {
            InitializeComponent();
            ID = iD;
            is_GRC = isGRC;
        }

        private void DocumentViewer_Load(object sender, EventArgs e)
        {
            DataTable dt = UpdateDocumentsList();
            if (!is_GRC)
            {
                DocumentListComboBox.DisplayMember = "Document Name";
            }else
            {
                DocumentListComboBox.DisplayMember = "Archive Letter";
            }
            DocumentListComboBox.DataSource = dt;
        }

        private DataTable UpdateDocumentsList()
        {
            DataTable docList = new DataTable();
            docConnection.GRC_Connection.Open();
            SqlDataAdapter adapt;
            if (!is_GRC)
            {
                adapt = docConnection.GetDocumentList(ID);
            }else
            { 
                 adapt = docConnection.GetNotificationList(ID);
            }

            adapt.Fill(docList);
            docConnection.GRC_Connection.Close();
            return docList;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            DataRowView drv = DocumentListComboBox.SelectedItem as DataRowView;
            string documentName = "";
            if (drv != null)
            {
                documentName = (!is_GRC) ? drv.Row["Document Name"] as string : drv.Row["Archive Letter"] as string;
            }

            string docPath = "";
            docConnection.GRC_Connection.Open();

            SqlCommand cmd;
            if (!is_GRC)
            {
                cmd = docConnection.GetDocPath(ID, documentName);
            }
            else
            { 
                cmd = docConnection.GetNotPath(ID, documentName);
            }

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

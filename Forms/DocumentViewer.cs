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
        private int typeOfTable;
        Connections docConnection = new Connections();
        public DocumentViewer(int id, int t)
        {
            InitializeComponent();
            ID = id;
            typeOfTable = t;
        }

        private void DocumentViewer_Load(object sender, EventArgs e)
        {
            DataTable dt = UpdateDocumentsList();

            if(typeOfTable == 3)
            {
                DocumentListComboBox.DisplayMember = "Archive Letter";
            }
            else
            {
                DocumentListComboBox.DisplayMember = "Document Name";
            }
            DocumentListComboBox.DataSource = dt;
        }

        private DataTable UpdateDocumentsList()
        {
            DataTable docList = new DataTable();
            docConnection.GRC_Connection.Open();
            SqlDataAdapter adapt;

            if (typeOfTable == 3)
            {
                adapt = docConnection.GetNotificationList(ID);
            }
            else
            {
                adapt = docConnection.GetDocumentList(ID, typeOfTable);
            }
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
                documentName = (typeOfTable != 3) ? drv.Row["Document Name"] as string : drv.Row["Archive Letter"] as string;
            }

            string docPath = "";
            docConnection.GRC_Connection.Open();
            SqlCommand cmd;
            if (typeOfTable == 3)
            {
                cmd = docConnection.GetNotPath(ID, documentName);
            }
            else
            {
                cmd = docConnection.GetDocPath(ID, documentName, typeOfTable);
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

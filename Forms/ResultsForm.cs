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
        private bool saved;
        private int labID;
        private int numGenes = 1;
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

            OutcomeComboBox.DisplayMember = "Label Name";
            OutcomeComboBox.DataSource = GetResultList(1);
            Variant1.DisplayMember = "Value Name";
            Variant1.DataSource = GetResultList(2);

            con.GRC_Connection.Open();
            cmd = con.GetOrderID(appID);
            sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                orderID = Convert.ToInt32(sdr[0]);
                labID = Convert.ToInt32(sdr[1]);
            }
            con.GRC_Connection.Close();
            CreateResult();
        }
        private DataTable GetResultList(int t)
        {
            con.GRC_Connection.Open();
            DataTable dt = new DataTable();
            SqlDataAdapter adapt = new SqlDataAdapter();
            if(t == 1)
            {
                adapt = con.GetOutcomeList();
            }else if(t == 2)
            {
                adapt = con.GetVariantList();
            }
            adapt.Fill(dt);
            con.GRC_Connection.Close();
            return dt;
        }

        private void CreateResult()
        {
            if (HasRecord())
            {
                saved = true;
                //populate information
            }else
            {
                con.GRC_Connection.Open();
                //create result record
                SqlCommand cmd = con.NewResult(orderID, labID, empID);
                cmd.ExecuteNonQuery();
                con.GRC_Connection.Close();
            }
           
        }

        private bool HasRecord()
        {
            bool hasResult = false;
            con.GRC_Connection.Open();
            SqlCommand cmd = con.HasResult(orderID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                if (Convert.ToInt32(sdr[0]) != 0)
                {
                    hasResult = true;
                }
            }
            con.GRC_Connection.Close();
            return hasResult;
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
            string path = "\\\\jeeves.crha-health.ab.ca\\Genetics\\GRC\\PTLL Pilot\\Results\\";
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

                path = path + filename + "_" + orderID + "_" + documentID + ext;
                con.GRC_Connection.Open();
                cmd = con.UpdateResultsDestination(documentID, path);
                cmd.ExecuteNonQuery();
                con.GRC_Connection.Close();
                File.Copy(openFileDialog.FileName, path);            
                MessageBox.Show("Document uploaded.");
            }
        }

        private void ResultsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved)
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

        private void ViewReportLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DocumentViewer Docs = new DocumentViewer(orderID, 2);
            Docs.Show();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            saved = true;
            this.Close();
            //save details
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            saved = false;
            this.Close();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Would you like to add a new Gene/Variant?", "Add Gene", MessageBoxButtons.YesNo);
            if(dr == DialogResult.Yes)
            {
                AddControls();
            }
        }

        private void AddControls()
        {
            int y = Gene1.Location.Y;
            const int offset = 26;
            Size BoxSize = new Size(Gene1.Width, Gene1.Height);
            numGenes++;

            TextBox txt = new TextBox();
            this.Controls.Add(txt);
            txt.Name = "Gene" + numGenes;
            txt.Location = new Point(Gene1.Location.X, y + (offset * (numGenes - 1)));
            txt.Size = BoxSize;
            txt.BringToFront();

            Label lbl1 = new Label();
            lbl1.Name = "GeneLabel" + numGenes;
            lbl1.Text = GeneLabel1.Text;
            lbl1.Location = new Point(GeneLabel1.Location.X, y + (offset * (numGenes - 1)));
            this.Controls.Add(lbl1);
            lbl1.BackColor = Color.Gainsboro;
            lbl1.Font = new Font("Microsoft Sans Serif", 9);
            lbl1.Size = GeneLabel1.Size;
            lbl1.BringToFront();

            ComboBox cmb = new ComboBox();
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.FlatStyle = FlatStyle.Popup;
            cmb.Name = "Variant" + numGenes;
            cmb.Location = new Point(Variant1.Location.X, y + (offset * (numGenes - 1)));
            this.Controls.Add(cmb);
            cmb.Size = BoxSize;
            cmb.DisplayMember = "Value Name";
            cmb.DataSource = GetResultList(2);
            cmb.BringToFront();

            Label lbl2 = new Label();
            lbl2.Name = "VariantLabel" + numGenes;
            lbl2.Text = VariantLabel1.Text;
            lbl2.Location = new Point(VariantLabel1.Location.X, y + (offset * (numGenes - 1)));
            this.Controls.Add(lbl2);
            lbl2.BackColor = Color.Gainsboro;
            lbl2.Font = new Font("Microsoft Sans Serif", 9);
            lbl2.Size = VariantLabel1.Size;
            lbl2.BringToFront();

            AddButton.Location = new Point(AddButton.Location.X, AddButton.Location.Y + offset);
            CommentsLabel.Location = new Point(CommentsLabel.Location.X, CommentsLabel.Location.Y + offset);
            CommentsTextBox.Location = new Point(CommentsTextBox.Location.X, CommentsTextBox.Location.Y + offset);
            DeleteButton.Location = new Point(DeleteButton.Location.X, DeleteButton.Location.Y + offset);
            SaveButton.Location = new Point(SaveButton.Location.X, SaveButton.Location.Y + offset);
            pictureBox2.Size = new Size(pictureBox2.Width, pictureBox2.Height + offset);
        }
    }
}

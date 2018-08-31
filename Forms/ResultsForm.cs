using System;
using System.Collections;
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
        private int resultID;
        private string employeeName;
        Connections con = new Connections();
        private bool saved;
        private int labID;
        private int numGenes = 1;
        private string outcome;
        private string otherOutcome;
        private string notes;
        private string receivedDate;
        private ArrayList ids = new ArrayList();
        private bool newRes = true;
        private int numberOfBoxes;
        int n = 1;

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
            SqlCommand cmd;
            SqlDataReader sdr;
            if (HasRecord())
            {
                saved = true;
                newRes = false;
                //populate information and get resultID
                con.GRC_Connection.Open();
                cmd = con.GetResult(orderID);
                sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    resultID = Convert.ToInt32(sdr[0]);
                    outcome = sdr[1].ToString();
                    otherOutcome = sdr[2].ToString();
                    notes = sdr[3].ToString();
                    receivedDate = Convert.ToDateTime(sdr[4]).ToString("yyyy/MM/dd");
                }
                con.GRC_Connection.Close();

                con.GRC_Connection.Open();
                cmd = con.CountDetails(resultID, true);
                sdr = sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    ids.Add(Convert.ToInt32(sdr[0]));
                }
                con.GRC_Connection.Close();

                con.GRC_Connection.Open();
                cmd = con.CountDetails(resultID, false);
                sdr = sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    numberOfBoxes = Convert.ToInt32(sdr[0]);
                }
                con.GRC_Connection.Close();
                numGenes = numberOfBoxes;
                FillInformation();
            }else
            {
                newRes = true;
                con.GRC_Connection.Open();
                //create result record
                cmd = con.NewResult(orderID, labID, empID);
                cmd.ExecuteNonQuery();
                con.GRC_Connection.Close();

                con.GRC_Connection.Open();
                cmd = con.GetResult(orderID);
                sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    resultID = Convert.ToInt32(sdr[0]);
                }
                con.GRC_Connection.Close();

                InsertDetail();
            }
           
        }

        private void InsertDetail()
        {
            con.GRC_Connection.Open();
            SqlCommand cmd = con.InsertResultDetail(resultID, employeeName);
            cmd.ExecuteNonQuery();
            con.GRC_Connection.Close();

            con.GRC_Connection.Open();             //save detail ID in array
            cmd = new SqlCommand("select top 1 [ID] FROM [GRC].[dbo].[Result Order Details] where [Result ID] = '" + resultID + "' order by [ID] desc", con.GRC_Connection);
            SqlDataReader sdr = cmd.ExecuteReader();
            while(sdr.Read())
            {
                ids.Add(Convert.ToInt32(sdr[0]));
            }
            con.GRC_Connection.Close();
        }

        private void FillInformation()
        {
            DateBox.Text = receivedDate;
            OutcomeComboBox.SelectedIndex = OutcomeComboBox.FindString(outcome);
            OtherTextBox.Text = otherOutcome;
            CommentsTextBox.Text = notes;

            for (int i = 0; i < numberOfBoxes - 1; i++)
            {
                AddControls(false);
            }
            SqlCommand cmd;
            SqlDataReader sdr;
            int a = 0;
            foreach (Control c in this.Controls)
            {//update gene, update variant
                if (c is TextBox)
                {
                    if (c.Name.Contains("Gene"))
                    {
                        con.GRC_Connection.Open();
                        cmd = con.GetGeneVariantResults(Convert.ToInt32(ids[a]));
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            c.Text = sdr[0].ToString();
                        }
                        con.GRC_Connection.Close();

                        Console.WriteLine(c.Name);
                        Console.WriteLine(ids[a]);
                        a++;
                    }
                }
            }
            a = 0;
            foreach (Control c in this.Controls)
            {
                if (c is ComboBox)
                {
                    if (c.Name.Contains("Variant"))
                    {
                        con.GRC_Connection.Open();
                        cmd = con.GetGeneVariantResults(Convert.ToInt32(ids[a]));
                        sdr = cmd.ExecuteReader();
                        while (sdr.Read())
                        {
                            ((ComboBox)c).SelectedIndex = ((ComboBox)c).FindString(sdr[1].ToString());
                        }
                        con.GRC_Connection.Close();
                        a++;
                    }
                }
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
                SqlCommand cmd = con.DeleteResult(orderID, 0, resultID);
                cmd.ExecuteNonQuery();
                con.GRC_Connection.Close();

                con.GRC_Connection.Open();
                cmd = con.DeleteResult(orderID, 2, resultID);
                cmd.ExecuteNonQuery();
                con.GRC_Connection.Close();

                con.GRC_Connection.Open();
                cmd = con.DeleteResult(orderID, 1, resultID);
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
            UpdateResultInformation();
            this.Close();
            //save details
        }

        private void UpdateResultInformation()
        {
            foreach (Control c in this.Controls)
            {
                if (c is TextBox)
                {
                    ((TextBox)c).Text = ((TextBox)c).Text.Replace("'", "");
                }
            }

            DataRowView drv = OutcomeComboBox.SelectedItem as DataRowView;
            outcome = (drv != null) ? drv.Row["Label Name"] as string : "";
            otherOutcome = OtherTextBox.Text;
            notes = CommentsTextBox.Text;
            receivedDate = DateBox.Text;
            DateTime result;
            receivedDate = (receivedDate != "" && DateTime.TryParse(receivedDate, out result)) ? receivedDate : "";

            con.GRC_Connection.Open();
            SqlCommand cmd = con.SaveResults(outcome, otherOutcome, notes, receivedDate, employeeName, orderID);
            cmd.ExecuteNonQuery();
            con.GRC_Connection.Close();

            int i = 0;
            string vc;
            foreach (Control c in this.Controls)
            {//update gene, update variant
                if (c is TextBox)
                {
                    if (c.Name.Contains("Gene"))
                    {
                        Console.WriteLine(c.Name);
                        Console.WriteLine(ids[i]);

                        con.GRC_Connection.Open();
                        cmd = new SqlCommand("update [GRC].[dbo].[Result Order Details] set [Gene] = '" + c.Text + "' where [Result ID] = " + resultID + " and [ID] = " + ids[i], con.GRC_Connection);
                        cmd.ExecuteNonQuery();
                        con.GRC_Connection.Close();
                        i++;
                    }
                }
            }
            i = 0;
            foreach (Control c in this.Controls)
            {
                if (c is ComboBox)
                {
                    if (c.Name.Contains("Variant"))
                    {
                        Console.WriteLine(c.Name);
                        Console.WriteLine(ids[i]);

                        drv = ((ComboBox)c).SelectedItem as DataRowView;
                        vc = (drv != null) ? drv.Row["Value Name"] as string : "";

                        con.GRC_Connection.Open();
                        cmd = new SqlCommand("update [GRC].[dbo].[Result Order Details] set [Variant Class] = '" + vc
                            + "', [Updated Date] = Convert(VARCHAR(10), GETDATE(), 126) where [Result ID] = " + resultID + " and [ID] = " + ids[i], con.GRC_Connection);
                        cmd.ExecuteNonQuery();
                        con.GRC_Connection.Close();
                        i++;
                    }
                }
            }
                    

                
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
                AddControls(true);
            }
        }

        private void AddControls(bool insert)
        {
            if(numGenes == 20)
            {
                return;
            }
            int y = Gene1.Location.Y;
            const int offset = 26;
            Size BoxSize = new Size(Gene1.Width, Gene1.Height);
            if (insert)
            {
                numGenes++;
                InsertDetail();
            }
            n++;
            TextBox txt = new TextBox();
            this.Controls.Add(txt);
            txt.Name = "Gene" + n;
            txt.Location = new Point(Gene1.Location.X, y + (offset * (n - 1)));
            txt.Size = BoxSize;
            txt.BringToFront();

            Label lbl1 = new Label();
            lbl1.Name = "GeneLabel" + n;
            lbl1.Text = GeneLabel1.Text;
            lbl1.Location = new Point(GeneLabel1.Location.X, y + (offset * (n - 1)));
            this.Controls.Add(lbl1);
            lbl1.BackColor = Color.Gainsboro;
            lbl1.Font = new Font("Microsoft Sans Serif", 9);
            lbl1.Size = GeneLabel1.Size;
            lbl1.BringToFront();

            ComboBox cmb = new ComboBox();
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.FlatStyle = FlatStyle.Popup;
            cmb.Name = "Variant" + n;
            cmb.Location = new Point(Variant1.Location.X, y + (offset * (n - 1)));
            this.Controls.Add(cmb);
            cmb.Size = BoxSize;
            cmb.DisplayMember = "Value Name";
            cmb.DataSource = GetResultList(2);
            cmb.BringToFront();

            Label lbl2 = new Label();
            lbl2.Name = "VariantLabel" + n;
            lbl2.Text = VariantLabel1.Text;
            lbl2.Location = new Point(VariantLabel1.Location.X, y + (offset * (n - 1)));
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

        private void DateBox_Leave(object sender, EventArgs e)
        {
            string date = DateBox.Text;
            ParseTime(date);
        }

        private void ParseTime(string s)
        {
            DateTime result;
            if (s == "")
            {
                return;
            }
            else if (DateTime.TryParse(s, out result))
            {
                s = result.ToString("yyyy/MM/dd");
                DateBox.Text = s;
            }
        }

        private void ResultsForm_Shown(object sender, EventArgs e)
        {
            CreateResult();
        }
    }
}

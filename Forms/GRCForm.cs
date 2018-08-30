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
    public partial class GRCForm : Form
    {
        #region VARIABLES
        private string PHN = "";
        private string firstName = "";
        private string lastName = "";
        private string postalCode = "";
        private string DOB = "";
        private string alternateID = "";
        private string alternateExplanation = "";
        private string gender = "";
        private int genderID = 0;

        private string orderingPhysician = "";
        private string primaryContact = "";
        private string secondaryContact = "";

        private string urgentExpl = "";
        private string otherReasonExpl = "";
        private string diagnosis = "";
        private string clinicalSpecialty = "";
        private string PTLLTest = "";
        private string gene = "";
        private string otherLabDetail = "";
        private bool otherLab = false;
        private string comments = "";

        string sampleType = "";
        string labName = "";
        bool[] reasonCheckboxes = new bool[4];
        bool[] rationaleCheckboxes = new bool[4];

        string newTestReq = "";
        string newPrefMethod = "";
        string newPrefLab = "";

        private string famHistExpl = "";
        private string ethRiskExpl = "";
        private string otherTstExpl = "";
        private string otherRationaleExpl = "";

        private bool noPHN = false;
        private bool createNewPatient = false;
        private bool isUrgent = false;
        private bool otherReason = false;

        private bool familyHistory = false;
        private bool ethnicityRisk = false;
        private bool otherTesting = false;
        private bool otherRationale = false;

        private int demographics = 1;
        private int clinicalInfo = 2;

        private int currentOrderID;
        private bool finalized = false;
        private bool deleted = false;
        private bool saved = false;
        private int employee_ID = 0;
        private string savedFileName = "";

        private bool canFinalize = false;
        private bool newTest = false;
        private bool loadExisting = false;
        private int existOrderID = 0;
        private int existGRCID = 0;
        private int existApplicationID = 0;
        private string urgentSelection;
        private string geneticsID;
        private string sendOutLab;
        private string MRN;
        private string subtype;
        private string additional;
        #endregion
        GRCFormClass GRC = new GRCFormClass();
        Connections formConnection = new Connections();
        const int buttonWidth = 95;
        const int buttonHeight = 41;
        const int textboxWidth = 818;
        const int backgroundWidth = 840;

        public GRCForm(int empID, bool existingApplication = true, int existingOrderID = 0) {

            InitializeComponent();
            employee_ID = empID;
            existOrderID = existingOrderID;
            
            PHNTextBox.AutoCompleteCustomSource = GRC.Search(demographics, 0);
            FirstNameTextBox.AutoCompleteCustomSource = GRC.Search(demographics, 1);
            LastNameTextBox.AutoCompleteCustomSource = GRC.Search(demographics, 2);
            OrderingPhysicianTextBox.AutoCompleteCustomSource = GRC.Search(clinicalInfo);

            if (existingApplication && existingOrderID != 0)
            {
                loadExisting = true;
                currentOrderID = existingOrderID;
            }
            foreach (Control c in this.Controls)
            {
                if (c is ComboBox)
                {
                    ((ComboBox)c).MouseWheel += new MouseEventHandler(comboBox_MouseWheel);
                }
            }
        }

        // OrderDetail Initialize Data Table
        //public void InitializeOderDetailDataTable()
        //{
        //    defaultData = true;
        //    DataTable dt = dashboard.UpdateAppTable(defaultData);
        //    ApplicationListTableView.DataSource = dt;

        //    UpdateMetricLabels();
        //}


        private void ApplicationForm_Load(object sender, EventArgs e)
        {

            DataTable dt;

            dt = GRC.GetList(1);
            DocumentTypeComboBox.DisplayMember = "Document Type Name";
            DocumentTypeComboBox.DataSource = dt;

            dt = GRC.GetList(2);
            GenderComboBox.DisplayMember = "Gender";
            GenderComboBox.DataSource = dt;

            dt = GRC.GetList(4);
            OtherReasonTextBox.DisplayMember = "Reason";
            OtherReasonTextBox.DataSource = dt;

            dt = GRC.GetList(5);
            UrgentComboBox.DisplayMember = "Reason";
            UrgentComboBox.DataSource = dt;

            dt = GRC.GetList(6);
            ClinicSubTypeComboBox.DisplayMember = "SubType Name";
            ClinicSubTypeComboBox.DataSource = dt;

            dt = GRC.GetList(7);
            ShippingViaComboBox.DisplayMember = "Shipping Via";
            ShippingViaComboBox.DataSource = dt;



            dt = GRC.UpdateClinicalContacts(employee_ID);
            DataTable secondary = GRC.UpdateClinicalContacts(employee_ID);
            PrimaryClinicalContactComboBox.DisplayMember = "Clinical Contact";
            PrimaryClinicalContactComboBox.DataSource = dt;
            AltClinicalContactComboBox.DisplayMember = "Clinical Contact";
            AltClinicalContactComboBox.DataSource = secondary;

            GRC.GetGRCApplication(existOrderID); // Call GRC Class

            dt = GRC.OrderDetailDataTable(existOrderID);
            OrderDetailListTableView.DataSource = dt;
            GRC.FillOrderDetails(existOrderID);

            DataGridViewComboBoxColumn testName = new DataGridViewComboBoxColumn();
            DataRowView drv;
            string s;

        }
        private void CustomList(int row, int[] data)
        {
            DataGridViewComboBoxCell comboCell = OrderDetailListTableView[3, row] as DataGridViewComboBoxCell;
            comboCell.DataSource = new BindingSource(data, null);

        }
        private void ApplicationForm_Shown(object sender, EventArgs e)
        {

            BrowseButton.Visible = !finalized;
            UploadButton.Visible = !finalized;

            FillApplication();
            
            if (!GRC.IsGRCStatusOpen())
            {
                foreach (Control c in this.Controls)
                {
                    if (c is TextBox)
                    {
                        ((TextBox)c).ReadOnly = true;
                    }else if (c is ComboBox)
                    {
                        ((ComboBox)c).DropDownHeight = 1;
                    }else if (c is CheckBox)
                    {
                        ((CheckBox)c).AutoCheck = false;
                    }else if (c is RadioButton)
                    {
                        ((RadioButton)c).AutoCheck = false;
                    
                    }else if (c is DateTimePicker)
                    {
                    ((DateTimePicker)c).Enabled = false;
                    }
            }
            }


    }

        #region CONTROL HANDLERS
        private void PHNTextBox_KeyPress(object sender, KeyPressEventArgs e){
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)){
                e.Handled = true;
            }
        }
        private void ReferenceNumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        void comboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void PHNTextBox_TextChanged(object sender, EventArgs e)
        {
            PHN = PHNTextBox.Text;
            GRC.UpdateDemographics(PHN);
            MRNTextBox.Text = GRC.GetMRN();
            FirstNameTextBox.Text = GRC.GetFirstName();
            LastNameTextBox.Text = GRC.GetLastName();
            PostalCodeTextBox.Text = GRC.GetZIP();
            //DOBPicker.Value = GRC.GetDOB(DOBPicker.MinDate);


            DOBDateTextbox.Text = GRC.GetDOBDate(); // (Convert.ToDateTime(dob)).ToString("yyyy-MM-dd");


            gender = GRC.GetGender();
            if(gender != null)
            {
                GenderComboBox.SelectedIndex = GenderComboBox.FindStringExact(gender);
            }
            
        }
        private void OrderingPhysicianTextBox_Leave(object sender, EventArgs e)
        {
            orderingPhysician = OrderingPhysicianTextBox.Text;
            GRC.UpdatePhysician(orderingPhysician);
        }

        #endregion

        #region CLICK EVENTS

        private void NewPatientButton_Click(object sender, EventArgs e)
        {
            PHN = PHNTextBox.Text;
            noPHN = (NoPHNCheckBox.CheckState == CheckState.Checked) ? true : false;
            alternateID = AlternateIDTextbox.Text;
            alternateExplanation = AlternateIDExplanationTextbox.Text;
            firstName = FirstNameTextBox.Text;
            lastName = LastNameTextBox.Text;
            postalCode = PostalCodeTextBox.Text;
           // DOB = DOBPicker.Value.ToString();
            DataRowView drv = GenderComboBox.SelectedItem as DataRowView;
            if (drv != null)
            {
                gender = drv.Row["Gender"] as string;
            }
            genderID = GRC.GetGenderID(gender);
            CheckPatient();
        }

        private void FinalizeButton_Click(object sender, EventArgs e)
        {
            CaptureInformation();
            
            if (GRC.DemographicFieldsCorrect(PHN, noPHN, alternateID, alternateExplanation, firstName, lastName, postalCode)
                && !GRC.OrderPhysicianFieldEmpty(orderingPhysician)
                && GRC.TestInfoCorrect(isUrgent, otherReason, urgentExpl, otherReasonExpl, diagnosis, clinicalSpecialty, PTLLTest, gene, newTest, otherLab, otherLabDetail, urgentSelection))
            {
                canFinalize = true;

                if (newTest && !GRC.OtherTestInfoCorrect(newTestReq, familyHistory, ethnicityRisk, otherTesting, otherRationale, famHistExpl, ethRiskExpl, otherTstExpl, otherRationaleExpl))
                {
                    canFinalize = false;
                }
            }
            else{
                canFinalize = false;
            }
            Console.WriteLine(canFinalize);
           
            if (canFinalize)
            {
                CheckPatient();
                if (createNewPatient && !noPHN)
                {
                    GRC.CreateNewPatient(PHN, firstName, lastName, genderID, DOB, postalCode, MRN, alternateID, alternateExplanation);
                    MessageBox.Show("Patient " + firstName + " " + lastName + " has been created!");
                }

                if (MessageBox.Show("Are you sure you want to finalize this application? You will not be able to make any more changes to this application if you proceed.",
                        "Finalize application",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Information) == DialogResult.No){
                        return;
                }

                finalized = true;
                GRC.SetTestID(PTLLTest, labName);
                GRC.CreateNewApplication(PHN, primaryContact, secondaryContact, isUrgent, urgentExpl, reasonCheckboxes, 
                    otherReason, otherReasonExpl, diagnosis, gene, comments, 
                    urgentSelection, newTest, finalized, 2, geneticsID, subtype, sendOutLab, otherLab, otherLabDetail,
                    newTestReq, newPrefMethod, newPrefLab, famHistExpl, ethRiskExpl, otherTstExpl, otherRationaleExpl, familyHistory, ethnicityRisk, otherTesting, otherRationale, additional, rationaleCheckboxes);
                MessageBox.Show("Application created!");
                this.Close();   
            }

        }
        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (finalized)
            {
                if (MessageBox.Show("Submitting this application will generate a GRC ID and will be open for review. Do you wish to proceed?",
                         "Submit application",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Information) == DialogResult.No)
                {
                    return;
                }
                CaptureInformation();
                GRC.SetTestID(PTLLTest, labName);
                MessageBox.Show("Application Submitted!");
                GRC.SubmitApplication(currentOrderID, employee_ID);
                this.Close();
            }
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            CaptureInformation();
            if(PHN == "" || orderingPhysician == "")
            {
                MessageBox.Show("Please enter patient demographics and or the Ordering Physician before saving!");
                return;
            }
            deleted = false;
            saved = true;
            finalized = false;
            CheckPatient();
            if (createNewPatient) {
                GRC.CreateNewPatient(PHN, firstName, lastName, genderID, DOB, postalCode, MRN, alternateID, alternateExplanation);
                MessageBox.Show("Patient " + firstName + " " + lastName + " has been created!");
            }
            
            GRC.CreateNewApplication(PHN, primaryContact, secondaryContact, isUrgent, urgentExpl, 
                reasonCheckboxes, otherReason, otherReasonExpl, diagnosis, gene, comments, urgentSelection, 
                newTest, finalized, 1, geneticsID, subtype, sendOutLab, otherLab, otherLabDetail,
                newTestReq, newPrefMethod, newPrefLab, famHistExpl, ethRiskExpl, otherTstExpl, otherRationaleExpl, familyHistory, ethnicityRisk, otherTesting, otherRationale, additional, rationaleCheckboxes);
            this.Close();
        }
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            deleted = true;
            this.Close();
        }

        private void NoPHNCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (NoPHNCheckBox.CheckState == CheckState.Checked)
            {
                AlternateIDLabel.Show();
                ExplanationAltIDLabel.Show();
                AlternateIDTextbox.Show();
                AlternateIDExplanationTextbox.Show();
                PHNTextBox.Text = "";
                PHNTextBox.ReadOnly = true;
            }
            else if (NoPHNCheckBox.CheckState == CheckState.Unchecked)
            {
                AlternateIDLabel.Hide();
                ExplanationAltIDLabel.Hide();
                AlternateIDTextbox.Hide();
                AlternateIDExplanationTextbox.Hide();
                AlternateIDTextbox.Text = "";
                AlternateIDExplanationTextbox.Text = "";
                PHNTextBox.ReadOnly = false;
            }

        }
        private void UrgentCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UrgentCheckBox.CheckState == CheckState.Checked)
            {
                UrgentExplLabel.Show();
                UrgentExplTextBox.Show();
                UrgentComboBox.Show();
            }
            else if (UrgentCheckBox.CheckState == CheckState.Unchecked)
            {
                UrgentExplLabel.Hide();
                UrgentExplTextBox.Hide();
                UrgentComboBox.Hide();
            }
        }
        private void OtherReasonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(OtherReasonCheckBox.CheckState == CheckState.Checked)
            {
                OtherReasonTextBox.Show();
            }else if (OtherReasonCheckBox.CheckState == CheckState.Unchecked)
            {
                OtherReasonTextBox.Hide();
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {           
            DataRowView drv = DocumentTypeComboBox.SelectedItem as DataRowView;
            string documentType =  (drv != null) ? drv.Row["Document Type Name"] as string : "";
            documentType = documentType.Trim();

            OpenDocumentDialog.InitialDirectory = GRC.GetDirectory(0, documentType); //Initial Directory
            OpenDocumentDialog.Title = "File Upload";
            OpenDocumentDialog.Filter = "(*.pdf; *.doc; *.xlsx)|*.pdf; *.docx; *.xlsx";
            OpenDocumentDialog.FilterIndex = 1;
            
            if (OpenDocumentDialog.ShowDialog() == DialogResult.OK)
            {
                if (OpenDocumentDialog.CheckFileExists)
                {
                    string path = Path.GetFullPath(OpenDocumentDialog.FileName);
                    PathTextBox.Text = path;
                }
            }

        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            if(PathTextBox.Text == "")
            {
                MessageBox.Show("There is no file to upload!");
                return;
            }
            if (GRC.GetApplicationId() == 0)
            {
                MessageBox.Show("You can't upload a File since there is no an Application form Created!");
                return;
            }

            string filename = Path.GetFileNameWithoutExtension(OpenDocumentDialog.FileName);
            string ext = Path.GetExtension(OpenDocumentDialog.FileName);
            int documentID = 0;
            string employeeName = "";
            string documentType = "";
            DataRowView drv = DocumentTypeComboBox.SelectedItem as DataRowView;
            if (drv != null)
            {
                documentType = drv.Row["Document Type Name"] as string;
            }
            documentType = documentType.Trim();
            string path = GRC.GetDirectory(1, documentType);
            //If file has same name as another document associated with this application, replace old document
            if (filename == null){
                MessageBox.Show("Please select a valid document.");
            }else{ 
                formConnection.GRC_Connection.Open();
                SqlCommand cmd = formConnection.NameCommand(employee_ID);
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    employeeName = sdr[1].ToString() + " " + sdr[2].ToString();
                }
                formConnection.GRC_Connection.Close();
                //SAVE in database real document name, SAVE in directory DocumentType_AppID_DocumentID_PHN/Name.pdf/doc/xls
                formConnection.GRC_Connection.Open();

                cmd = new SqlCommand("insert into [GRC].[dbo].[Application Documents] ([ApplicationID], [Document Name], [Document Type], [Document Ext], [Update By]) values (" + 
                    GRC.GetApplicationId() + ", '" + filename + ext + "', '" + documentType + "', '" + ext +"', '" + employeeName + "')", formConnection.GRC_Connection);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("select top 1 [DocumentID], [Document Type] from [GRC].[dbo].[Application Documents] where [ApplicationID] = '" + 
                    GRC.GetApplicationId() + "' order by DocumentID desc", formConnection.GRC_Connection);
                sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    documentID = Convert.ToInt32(sdr[0].ToString());
                }
                path = path + documentType + "_" + currentOrderID + "_" + documentID + "_" + PHN + ext;
               
                File.Copy(OpenDocumentDialog.FileName, path);
                formConnection.GRC_Connection.Close();
                GRC.UpdateDocumentDestination(documentID, path);
                MessageBox.Show("Document uploaded.");
               
            }
            
        }
        private void ViewDocumentsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DocumentViewer appDocs = new DocumentViewer(GRC.GetApplicationId(), false);
            appDocs.Show();
        }

        private void ViewNotificationLinkLabe_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DocumentViewer appDocs = new DocumentViewer(currentOrderID, true);
            appDocs.Show();
        }
        #endregion

        #region OTHER FUNCTIONS
        private void CheckPatient()
        {
            if (GRC.DemographicFieldsCorrect(PHN, noPHN, alternateID, alternateExplanation, firstName, lastName, postalCode))
            {
                if (!GRC.PatientExists(PHN, firstName, lastName, DOB, genderID) && !noPHN)
                {
                    DialogResult dr = MessageBox.Show("Patient does not exist! Would you like to create a new record for this patient?", "New Patient!", MessageBoxButtons.YesNo);
                    createNewPatient = (dr == DialogResult.Yes) ? true : false;
                }
                else
                {
                    if (!canFinalize && !saved)
                    {
                        MessageBox.Show("Patient already exists!");
                    }
                    createNewPatient = false;
                    //ADD: update postal code if different
                }
            }
        }
        
        private void CaptureInformation()
        {
            PHN = PHNTextBox.Text;
            MRN = MRNTextBox.Text;
            noPHN = (NoPHNCheckBox.CheckState == CheckState.Checked) ? true : false;
            alternateID = AlternateIDTextbox.Text;
            alternateExplanation = AlternateIDExplanationTextbox.Text;
            firstName = FirstNameTextBox.Text;
            lastName = LastNameTextBox.Text;
            postalCode = PostalCodeTextBox.Text;
           // DOB = DOBPicker.Value.ToString();

            DataRowView drv = GenderComboBox.SelectedItem as DataRowView;
            gender = (drv != null) ? drv.Row["Gender"] as string : "";
            genderID = GRC.GetGenderID(gender);
            geneticsID = ReferenceNumberTextBox.Text;

            orderingPhysician = OrderingPhysicianTextBox.Text;
            GRC.UpdatePhysician(orderingPhysician);
            drv = PrimaryClinicalContactComboBox.SelectedItem as DataRowView;
            primaryContact = (drv != null) ? drv.Row["Clinical Contact"] as string : "";
            drv = AltClinicalContactComboBox.SelectedItem as DataRowView;
            secondaryContact = (drv != null) ? drv.Row["Clinical Contact"] as string : "";

            isUrgent = (UrgentCheckBox.CheckState == CheckState.Checked) ? true : false;
            otherReason = (OtherReasonCheckBox.CheckState == CheckState.Checked) ? true : false;

            urgentExpl = UrgentExplTextBox.Text;
            drv = OtherReasonTextBox.SelectedItem as DataRowView;
            otherReasonExpl = (drv != null && otherReason) ? drv.Row["Reason"] as string : "";
            drv = UrgentComboBox.SelectedItem as DataRowView;
            urgentSelection = (drv != null && isUrgent) ? drv.Row["Reason"] as string : "";

            drv = ClinicSubTypeComboBox.SelectedItem as DataRowView;
            subtype = (drv != null) ? drv.Row["SubType Name"] as string : "";

            reasonCheckboxes[0] = (ClinicallyAffectedCheckBox.CheckState == CheckState.Checked) ? true : false;
            reasonCheckboxes[1] = (FamilyMutationCheckBox.CheckState == CheckState.Checked) ? true : false;
            reasonCheckboxes[2] = (PrenatalTestCheckBox.CheckState == CheckState.Checked) ? true : false;
            reasonCheckboxes[3] = (PostmortemTestCheckBox.CheckState == CheckState.Checked) ? true : false;
            comments = AdditionalCommentsTextBox.Text;

            GRC.SetSampleID(sampleType);

            var checkedButton = this.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
            sendOutLab = (checkedButton != null) ? checkedButton.Text : "";
            GRC.SetTestID(PTLLTest, labName);



        }
        private void FillApplication()
        {
            OrderIDLabel.Text = OrderIDLabel.Text + existOrderID;
            GRCIDLabel.Text = "GRC # " + GRC.GetGRCId() + "  (" +  GRC.GRCStatusName(existOrderID) + ")";
            // GRC.GetApplicationId

            if (GRC.GetApplicationId() == 0)
            {
                ApplicationNumberLabel.Text = ApplicationNumberLabel.Text + " NA";
            }
            else {
                ApplicationNumberLabel.Text =  ApplicationNumberLabel.Text + GRC.GetApplicationId();
            }
    

            PHNTextBox.Text = GRC.GetPHN();
            ReferenceNumberTextBox.Text = GRC.GetGeneticsID();
            OrderingPhysicianTextBox.Text = GRC.GetOrderingPhysician();
            PrimaryClinicalContactComboBox.SelectedIndex = PrimaryClinicalContactComboBox.FindString(GRC.GetPrimaryContact());
            AltClinicalContactComboBox.SelectedIndex = AltClinicalContactComboBox.FindString(GRC.GetSecondaryContact());

            ClinicSubTypeComboBox.SelectedIndex = ClinicSubTypeComboBox.FindString(GRC.GetClinicSubtype());
            // Shipping via ShippingViaComboBox
            ShippingViaComboBox.SelectedIndex = ShippingViaComboBox.FindString(GRC.GetShippingName());

            bool[] check = GRC.GetCheckBoxes();
            UrgentCheckBox.CheckState = (check[0] == true) ? CheckState.Checked : CheckState.Unchecked;
            ClinicallyAffectedCheckBox.CheckState = (check[1] == true) ? CheckState.Checked : CheckState.Unchecked;
            FamilyMutationCheckBox.CheckState = (check[2] == true) ? CheckState.Checked : CheckState.Unchecked;
            PrenatalTestCheckBox.CheckState = (check[3] == true) ? CheckState.Checked : CheckState.Unchecked;
            PostmortemTestCheckBox.CheckState = (check[4] == true) ? CheckState.Checked : CheckState.Unchecked;
            OtherReasonCheckBox.CheckState = (check[5] == true) ? CheckState.Checked : CheckState.Unchecked;

            UrgentExplTextBox.Text = GRC.GetFreeTextboxes(12);
            IsSampleShipCheckBox.CheckState = (GRC.GetIsSampleShip()) ? CheckState.Checked : CheckState.Unchecked;
            ShippingDateTextbox.Text = GRC.GetShippingDate();
            ShipperRefTextbox.Text = GRC.GetShippingRef();

            AdditionalCommentsTextBox.Text = GRC.GetFreeTextboxes(105); 

            string[] combo = GRC.GetTestComboBoxes();
            UrgentComboBox.SelectedIndex = UrgentComboBox.FindString(combo[6]);
            OtherReasonTextBox.SelectedIndex = OtherReasonTextBox.FindString(combo[5]);
        }

        #endregion

        private void ApplicationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (deleted)
            {
                if (MessageBox.Show("Are you sure you want to delete this application?",
                       "Delete application",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information) == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    GRC.ClearApplication();
                }
            }else if (saved)
            {
                MessageBox.Show("Application Saved!");
            }
            else if (!loadExisting) 
            {
                if (MessageBox.Show("Your unsaved changes will be lost, do you want to exit?",
                       "Exit application",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information) == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    GRC.ClearApplication();
                }

            }
            
        }

    }
}

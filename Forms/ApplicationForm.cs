using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GRC_Clinical_Genetics_Application
{
    public partial class ApplicationForm : Form
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

        private int currentAppID;
        private bool finalized = false;
        private bool deleted = false;
        private bool saved = false;
        private int employee_ID = 0;
        private string savedFileName = "";

        private bool canFinalize = false;
        private bool newTest = false;
        private bool loadExisting = false;
        private int existApp = 0;
        private string urgentSelection;
        private string geneticsID;
        private string sendOutLab;
        private string MRN;
        private string subtype;
        private string additional;
        #endregion
        ApplicationFormClass app = new ApplicationFormClass();
        Connections formConnection = new Connections();
        DashboardClass dsbClass;
        Dashboard dashBoard;
        const int textboxWidth = 818;

        public ApplicationForm(Dashboard dsb, int empID, bool existingApplication, int existingAppID = 0) {
            InitializeComponent();
            dashBoard = dsb;
            employee_ID = empID;
            existApp = existingAppID;
            dsbClass = new DashboardClass(employee_ID);

            SecondDeleteButton.Width = DeleteButton.Width + 17;
            SecondSaveButton.Width = SaveButton.Width + 17;
            SecondFinalizeButton.Width = FinalizeButton.Width + 17;
            SecondSubmitButton.Width = SubmitButton.Width + 17;
            AdditionalDetailsTextBox.Width = textboxWidth + 17;
            TestContinuedBackGround.Width = ClinicianInformationBox.Width + 17;

            PHNTextBox.AutoCompleteCustomSource = app.Search(demographics, 0);
            FirstNameTextBox.AutoCompleteCustomSource = app.Search(demographics, 1);
            LastNameTextBox.AutoCompleteCustomSource = app.Search(demographics, 2);
            OrderingPhysicianTextBox.AutoCompleteCustomSource = app.Search(clinicalInfo);
            DOBPicker.CustomFormat = "yyyy/MM/dd";
            if (existingApplication && existingAppID != 0)
            {
                loadExisting = true;
                currentAppID = existingAppID;
            } else
            {
                finalized = false;
                currentAppID = app.GenerateApplicationID(empID);
            }
            foreach (Control c in this.Controls)
            {
                if (c is ComboBox)
                {
                    ((ComboBox)c).MouseWheel += new MouseEventHandler(comboBox_MouseWheel);
                }
            }
        }
        private void ApplicationForm_Load(object sender, EventArgs e)
        {

            DataTable dt = app.GetList(0);
            ClinicalCategoryComboBox.DisplayMember = "Test Type  Description";
            ClinicalCategoryComboBox.DataSource = dt;

            dt = app.GetList(1);
            DocumentTypeComboBox.DisplayMember = "Document Type Name";
            DocumentTypeComboBox.DataSource = dt;

            dt = app.GetList(2);
            GenderComboBox.DisplayMember = "Gender";
            GenderComboBox.DataSource = dt;

            dt = app.GetList(3);
            SampleTypeComboBox.DisplayMember = "Specimen Type";
            SampleTypeComboBox.DataSource = dt;

            dt = app.GetList(4);
            OtherReasonTextBox.DisplayMember = "Reason";
            OtherReasonTextBox.DataSource = dt;

            dt = app.GetList(5);
            UrgentComboBox.DisplayMember = "Reason";
            UrgentComboBox.DataSource = dt;

            dt = app.GetList(6);
            ClinicSubTypeComboBox.DisplayMember = "SubType Name";
            ClinicSubTypeComboBox.DataSource = dt;

            dt = app.UpdateClinicalContacts(employee_ID);
            DataTable secondary = app.UpdateClinicalContacts(employee_ID);
            PrimaryClinicalContactComboBox.DisplayMember = "Clinical Contact";
            PrimaryClinicalContactComboBox.DataSource = dt;
            AltClinicalContactComboBox.DisplayMember = "Clinical Contact";
            AltClinicalContactComboBox.DataSource = secondary;

            DataRowView drv = ClinicalCategoryComboBox.SelectedItem as DataRowView;
            if (drv != null)
            {
                clinicalSpecialty = drv.Row["Test Type  Description"] as string;
            }
            PTLLTextBox.DisplayMember = "Product Name";
            PTLLTextBox.DataSource = app.GetTestList(clinicalSpecialty);

            if (loadExisting)
            {
                app.GetApplication(existApp);
                finalized = app.IsReadOnly();
            }
            SubmitButton.Enabled = app.CanSubmit(employee_ID);
            SecondSubmitButton.Enabled = app.CanSubmit(employee_ID);
        }
        private void ApplicationForm_Shown(object sender, EventArgs e)
        {
            SubmitButton.Visible = (finalized && app.GetStatusID() != 3) ? true : false; 
            FinalizeButton.Visible = !finalized;
            DeleteButton.Visible = !finalized;
            SaveButton.Visible = !finalized;
            NewTestReqLinkLabel.Visible = !finalized;
            NonPTLLLabel.Visible = NewTestReqLinkLabel.Visible;

            BrowseButton.Visible = !finalized;
            UploadButton.Visible = !finalized;

            SecondSubmitButton.Visible = (finalized && app.GetStatusID() != 3 && newTest) ? true : false;
            SecondFinalizeButton.Visible = !finalized && newTest;
            SecondDeleteButton.Visible = !finalized && newTest;
            SecondSaveButton.Visible = !finalized && newTest;

            if (loadExisting)
            {
                FillApplication();
            }
            if (finalized)
            {
                GeneWarningLabel.Visible = false;
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
        void comboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void PHNTextBox_TextChanged(object sender, EventArgs e)
        {
            PHN = PHNTextBox.Text;
            app.UpdateDemographics(PHN);
            if (app.PatientExists(PHN))
            {
                MRNTextBox.Text = app.GetMRN();
                FirstNameTextBox.Text = app.GetFirstName();
                LastNameTextBox.Text = app.GetLastName();
                PostalCodeTextBox.Text = app.GetZIP();
                DateBox.Text = app.GetDOB(DOBPicker.MinDate).ToString("yyyy/MM/dd");
                gender = app.GetGender();
                if (gender != null)
                {
                    GenderComboBox.SelectedIndex = GenderComboBox.FindStringExact(gender);
                }

            }else
            {
                MRNTextBox.Text = "";
                FirstNameTextBox.Text = "";
                LastNameTextBox.Text = "";
                PostalCodeTextBox.Text = "";
                DateBox.Text = DateTime.Now.ToString("yyyy/MM/dd");
                gender = "";
                if (gender != null)
                {
                    GenderComboBox.SelectedIndex = GenderComboBox.FindStringExact(gender);
                }
            }
        }
   
        private void DOBPicker_ValueChanged(object sender, EventArgs e)
        {
            string date = DOBPicker.Value.ToString();
            ParseTime(date);
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
        private void OrderingPhysicianTextBox_Leave(object sender, EventArgs e)
        {
            orderingPhysician = OrderingPhysicianTextBox.Text;
            if (!orderingPhysician.Contains("("))
            {
                return;
            }
            app.UpdatePhysician(orderingPhysician);
        }
        private void SampleTypeComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string sampleType = "";
            DataRowView drv = SampleTypeComboBox.SelectedItem as DataRowView;
            if (drv != null)
            {
                sampleType = drv.Row["Specimen Type"] as string;
            }
            app.SetSampleID(sampleType);
        }
        private void ClinicalCategoryComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DataRowView drv = ClinicalCategoryComboBox.SelectedItem as DataRowView;
            if (drv != null)
            {
                clinicalSpecialty = drv.Row["Test Type  Description"] as string;
            }
            PTLLTextBox.DataSource = app.GetTestList(clinicalSpecialty);
            drv = PTLLTextBox.SelectedItem as DataRowView;
            if (drv != null)
            {
                PTLLTest = drv.Row["Product Name"] as string;
            }
        }
        private void PTLLTextBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRowView drv = PTLLTextBox.SelectedItem as DataRowView;
            if (drv != null)
            {
                PTLLTest = drv.Row["Product Name"] as string;
            }

            PreferredLabTextBox.DisplayMember = "Company";
            PreferredLabTextBox.DataSource = app.UpdateLabMethodList(1, clinicalSpecialty, PTLLTest);
            PreferredMethodTextBox.DisplayMember = "Method  Description";
            PreferredMethodTextBox.DataSource = app.UpdateLabMethodList(2, clinicalSpecialty, PTLLTest);
        }
        private void PreferredLabTextBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string labName = "";
            DataRowView drv = PreferredLabTextBox.SelectedItem as DataRowView;
            if (drv != null)
            {
                labName = drv.Row["Company"] as string;
            }
            app.SetTestID(PTLLTest, labName);
        }
        #endregion

        #region CLICK EVENTS

        private void FinalizeButton_Click(object sender, EventArgs e)
        {
            CaptureInformation();
            
            if (app.DemographicFieldsCorrect(PHN, noPHN, alternateID, alternateExplanation, firstName, lastName, postalCode)
                && !app.OrderPhysicianFieldEmpty(orderingPhysician)
                && app.TestInfoCorrect(isUrgent, otherReason, urgentExpl, otherReasonExpl, diagnosis, clinicalSpecialty, PTLLTest, gene, newTest, otherLab, otherLabDetail, urgentSelection))
            {
                canFinalize = true;

                if (newTest && !app.OtherTestInfoCorrect(newTestReq, familyHistory, ethnicityRisk, otherTesting, otherRationale, famHistExpl, ethRiskExpl, otherTstExpl, otherRationaleExpl))
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
                    app.CreateNewPatient(PHN, firstName, lastName, genderID, DOB, postalCode, MRN, alternateID, alternateExplanation);
                    MessageBox.Show("Patient " + firstName + " " + lastName + " has been created!");
                }

                if (MessageBox.Show("Once an application has been finalized, it will be sent to the GRC for review. You will not be able to make any further edits or upload documents to this application. Please confirm that you would like to proceed.",
                        "Finalize application",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Information) == DialogResult.No){
                        return;
                }
                app.UpdatePhysician(orderingPhysician);
                finalized = true;
                app.SetTestID(PTLLTest, labName);
                app.CreateNewApplication(PHN, primaryContact, secondaryContact, isUrgent, urgentExpl, reasonCheckboxes, 
                    otherReason, otherReasonExpl, diagnosis, gene, comments, 
                    urgentSelection, newTest, finalized, 2, geneticsID, subtype, sendOutLab, otherLab, otherLabDetail,
                    newTestReq, newPrefMethod, newPrefLab, famHistExpl, ethRiskExpl, otherTstExpl, otherRationaleExpl, familyHistory, ethnicityRisk, otherTesting, otherRationale, additional, rationaleCheckboxes);
                MessageBox.Show("Application created!");
                DataTable dt = dsbClass.UpdateAppTable(true);
                dashBoard.ApplicationListTableView.DataSource = dt;
                dashBoard.UpdateMetricLabels();
                this.Close();   
            }

        }
        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (finalized)
            {
                if (MessageBox.Show("Once an application has been submitted, a GRC number will be created and your application will be sent to the GRC for review. You will not be able to make any further edits or upload documents to this application. Please confirm that you would like to proceed.",
                         "Submit application",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Information) == DialogResult.No)
                {
                    return;
                }
                CaptureInformation();
                app.SetTestID(PTLLTest, labName);
                MessageBox.Show("Application Submitted!");
                app.SubmitApplication(currentAppID, employee_ID);
                DataTable dt = dsbClass.UpdateAppTable(true);
                dashBoard.ApplicationListTableView.DataSource = dt;
                dashBoard.UpdateMetricLabels();
                this.Close();
            }
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            CaptureInformation();
            if (!app.DemographicFieldsCorrect(PHN, noPHN, alternateID, alternateExplanation, firstName, lastName, postalCode))
            {
                return;
            }
            else if(!orderingPhysician.Contains("(") || (orderingPhysician == ""))
            {
                MessageBox.Show("Please Enter a valid Physician. (Search by last name)");
                return;
            }
            app.UpdatePhysician(orderingPhysician);
            deleted = false;
            saved = true;
            finalized = false;
            CheckPatient();
            if (createNewPatient) {
                app.CreateNewPatient(PHN, firstName, lastName, genderID, DOB, postalCode, MRN, alternateID, alternateExplanation);
                MessageBox.Show("Patient " + firstName + " " + lastName + " has been created!");
            }
 
            app.CreateNewApplication(PHN, primaryContact, secondaryContact, isUrgent, urgentExpl, 
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

        private void NewTestReqLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            newTest = !newTest; //make second page appear
            ChangeSecondPageVisibility(newTest);
        }
        private void OtherLabCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(OtherLabCheckBox.Checked)
            {
                OtherLabWarningLabel.Visible = true;
            }else
            {
                OtherLabWarningLabel.Visible = false;
            }
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

        private void FamilyHistoryCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (FamilyHistoryCheckBox.CheckState == CheckState.Checked)
            {
                FamilyDetailsLabel.Show();
                FamilyHistoryTextBox.Show();
            }
            else if (FamilyHistoryCheckBox.CheckState == CheckState.Unchecked)
            {
                FamilyDetailsLabel.Hide();
                FamilyHistoryTextBox.Hide();
            }
        }

        private void EthnicityCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EthnicityCheckBox.CheckState == CheckState.Checked)
            {
                EthnicityRiskLabel.Show();
                EthnicityRiskTextBox.Show();
            }
            else if (EthnicityCheckBox.CheckState == CheckState.Unchecked)
            {
                EthnicityRiskLabel.Hide();
                EthnicityRiskTextBox.Hide();
            }
        }

        private void OtherTestingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (OtherTestingCheckBox.CheckState == CheckState.Checked)
            {
                OtherTestingLabel.Show();
                OtherTestingTextBox.Show();
            }
            else if (OtherTestingCheckBox.CheckState == CheckState.Unchecked)
            {
                OtherTestingLabel.Hide();
                OtherTestingTextBox.Hide();
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {           
            DataRowView drv = DocumentTypeComboBox.SelectedItem as DataRowView;
            string documentType =  (drv != null) ? drv.Row["Document Type Name"] as string : "";
            documentType = documentType.Trim();

            OpenDocumentDialog.InitialDirectory = app.GetDirectory(0, documentType); //Initial Directory
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
            string path = app.GetDirectory(1, documentType);
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
                    currentAppID + ", '" + filename + ext + "', '" + documentType + "', '" + ext +"', '" + employeeName + "')", formConnection.GRC_Connection);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("select top 1 [DocumentID], [Document Type] from [GRC].[dbo].[Application Documents] where [ApplicationID] = '" + 
                    currentAppID + "' order by DocumentID desc", formConnection.GRC_Connection);
                sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    documentID = Convert.ToInt32(sdr[0].ToString());
                }
                path = path + documentType + "_" + currentAppID + "_" + documentID + "_" + PHN + ext;
               
                File.Copy(OpenDocumentDialog.FileName, path);
                formConnection.GRC_Connection.Close();
                app.UpdateDocumentDestination(documentID, path);
                MessageBox.Show("Document uploaded.");
               
            }
            
        }
        private void ViewDocumentsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DocumentViewer appDocs = new DocumentViewer(currentAppID, savedFileName);
            appDocs.Show();
        }

        #endregion

        #region OTHER FUNCTIONS
        private void CheckPatient()
        {
            if (app.DemographicFieldsCorrect(PHN, noPHN, alternateID, alternateExplanation, firstName, lastName, postalCode))
            {
                if (!app.PatientExists(PHN, firstName, lastName, DOB, genderID) && !noPHN)
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
        private void CheckRadioButtons(string radioButton)
        {
            switch (radioButton) {
                case ("Calgary MDL"):
                        CalgaryMDLCheckBox.Checked = true;
                        break;
                case ("Calgary BGL"):
                    CalgaryBGLCheckBox.Checked = true;
                    break;
                case ("Edmonton MDL"):
                    EdmontonMDLCheckBox.Checked = true;
                    break;
                case (""):
                    break;
                default:
                    OtherLabCheckBox.Checked = true;
                    OtherLabTextBox.Text = radioButton;
                    break;
            }
            
        }
        private void ChangeSecondPageVisibility(bool newTest)
        {
            if (newTest == true)
            {
                TestContinuedBackGround.Visible = true;
                TestContLabel.Visible = true;
                NewTestReqLabel.Visible = true;
                NewTestReqTextBox.Visible = true;
                NewPrefMethodLabel.Visible = true;
                NewTestMethodTextBox.Visible = true;
                NewPrefLabLabel.Visible = true;
                NewPrefLabTextBox.Visible = true;
                FamilyHistoryCheckBox.Visible = true;
                EthnicityCheckBox.Visible = true;
                OtherTestingCheckBox.Visible = true;
                RationaleLabel.Visible = true;
                TherapyCheckBox.Visible = true;
                ReduceCheckBox.Visible = true;
                ImpactCheckBox.Visible = true;
                PlanningCheckBox.Visible = true;
                OtherRationaleCheckBox.Visible = true;
                OtherRationaleTextBox.Visible = true;
                AdditionalDetailsLabel.Visible = true;
                AdditionalDetailsTextBox.Visible = true;
                SecondSubmitButton.Visible = (finalized && app.GetStatusID() != 3) ? true : false;
                SecondFinalizeButton.Visible = !finalized && newTest;
                SecondDeleteButton.Visible = !finalized && newTest;
                SecondSaveButton.Visible = !finalized && newTest;
            }
            else if (newTest == false)
            {
                TestContinuedBackGround.Visible = false;
                TestContLabel.Visible = false;
                NewTestReqLabel.Visible = false;
                NewTestReqTextBox.Visible = false;
                NewPrefMethodLabel.Visible = false;
                NewTestMethodTextBox.Visible = false;
                NewPrefLabLabel.Visible = false;
                NewPrefLabTextBox.Visible = false;
                FamilyHistoryCheckBox.Visible = false;
                EthnicityCheckBox.Visible = false;
                OtherTestingCheckBox.Visible = false;
                RationaleLabel.Visible = false;
                TherapyCheckBox.Visible = false;
                ReduceCheckBox.Visible = false;
                ImpactCheckBox.Visible = false;
                PlanningCheckBox.Visible = false;
                OtherRationaleCheckBox.Visible = false;
                OtherRationaleTextBox.Visible = false;
                AdditionalDetailsLabel.Visible = false;
                AdditionalDetailsTextBox.Visible = false;
                SecondSubmitButton.Visible = (finalized && app.GetStatusID() != 3 && newTest) ? true : false;
                SecondFinalizeButton.Visible = !finalized && newTest;
                SecondDeleteButton.Visible = !finalized && newTest;
                SecondSaveButton.Visible = !finalized && newTest;
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
            DOB = DateBox.Text;
            DataRowView drv = GenderComboBox.SelectedItem as DataRowView;
            gender = (drv != null) ? drv.Row["Gender"] as string : "";
            genderID = app.GetGenderID(gender);
            geneticsID = ReferenceNumberTextBox.Text;

            orderingPhysician = OrderingPhysicianTextBox.Text;
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

            diagnosis = DiagnosisTextBox.Text;
            drv = ClinicalCategoryComboBox.SelectedItem as DataRowView;
            clinicalSpecialty = (drv != null) ? drv.Row["Test Type  Description"] as string : "";
            drv = ClinicSubTypeComboBox.SelectedItem as DataRowView;
            subtype = (drv != null) ? drv.Row["SubType Name"] as string : "";
            drv = PTLLTextBox.SelectedItem as DataRowView;
            PTLLTest = (drv != null) ? drv.Row["Product Name"] as string : "";

            reasonCheckboxes[0] = (ClinicallyAffectedCheckBox.CheckState == CheckState.Checked) ? true : false;
            reasonCheckboxes[1] = (FamilyMutationCheckBox.CheckState == CheckState.Checked) ? true : false;
            reasonCheckboxes[2] = (PrenatalTestCheckBox.CheckState == CheckState.Checked) ? true : false;
            reasonCheckboxes[3] = (PostmortemTestCheckBox.CheckState == CheckState.Checked) ? true : false;

            gene = GeneTextBox.Text;
            otherLab = (OtherLabCheckBox.Checked) ? true : false;
            otherLabDetail = OtherLabTextBox.Text;
            comments = AdditionalCommentsTextBox.Text;

            drv = SampleTypeComboBox.SelectedItem as DataRowView;
            sampleType = (drv != null) ? drv.Row["Specimen Type"] as string : "";
            app.SetSampleID(sampleType);

            drv = PreferredLabTextBox.SelectedItem as DataRowView;
            labName = (drv != null) ? drv.Row["Company"] as string : "";
            var checkedButton = this.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
            sendOutLab = (checkedButton != null) ? checkedButton.Text : "";

            newTestReq = NewTestReqTextBox.Text;
            newPrefMethod = NewTestMethodTextBox.Text;
            newPrefLab = NewPrefLabTextBox.Text;

            famHistExpl = FamilyHistoryTextBox.Text;
            ethRiskExpl = EthnicityRiskTextBox.Text;
            otherTstExpl = OtherTestingTextBox.Text;
            otherRationaleExpl = OtherRationaleTextBox.Text;

            familyHistory = (FamilyHistoryCheckBox.CheckState == CheckState.Checked) ? true : false;
            ethnicityRisk = (EthnicityCheckBox.CheckState == CheckState.Checked) ? true : false;
            otherTesting = (OtherTestingCheckBox.CheckState == CheckState.Checked) ? true : false;
            otherRationale = (OtherRationaleCheckBox.CheckState == CheckState.Checked) ? true : false;

            rationaleCheckboxes[0] = (TherapyCheckBox.CheckState == CheckState.Checked) ? true : false;
            rationaleCheckboxes[1] = (ReduceCheckBox.CheckState == CheckState.Checked) ? true : false;
            rationaleCheckboxes[2] = (ImpactCheckBox.CheckState == CheckState.Checked) ? true : false;
            rationaleCheckboxes[3] = (PlanningCheckBox.CheckState == CheckState.Checked) ? true : false;

            additional = AdditionalDetailsTextBox.Text; 
            app.SetTestID(PTLLTest, labName);
        }
        private void FillApplication()
        {
            ApplicationNumberLabel.Text = ApplicationNumberLabel.Text + existApp;
            newTest = app.IsNewTest();
            ChangeSecondPageVisibility(newTest);

            PHNTextBox.Text = app.GetPHN();
            ReferenceNumberTextBox.Text = app.GetGeneticsID();
            OrderingPhysicianTextBox.Text = app.GetOrderingPhysician();
            PrimaryClinicalContactComboBox.SelectedIndex = PrimaryClinicalContactComboBox.FindString(app.GetPrimaryContact());
            AltClinicalContactComboBox.SelectedIndex = AltClinicalContactComboBox.FindString(app.GetSecondaryContact());
            ClinicSubTypeComboBox.SelectedIndex = ClinicSubTypeComboBox.FindString(app.GetClinicSubtype());

            bool[] check = app.GetCheckBoxes();
            UrgentCheckBox.CheckState = (check[0] == true) ? CheckState.Checked : CheckState.Unchecked;
            ClinicallyAffectedCheckBox.CheckState = (check[1] == true) ? CheckState.Checked : CheckState.Unchecked;
            FamilyMutationCheckBox.CheckState = (check[2] == true) ? CheckState.Checked : CheckState.Unchecked;
            PrenatalTestCheckBox.CheckState = (check[3] == true) ? CheckState.Checked : CheckState.Unchecked;
            PostmortemTestCheckBox.CheckState = (check[4] == true) ? CheckState.Checked : CheckState.Unchecked;
            OtherReasonCheckBox.CheckState = (check[5] == true) ? CheckState.Checked : CheckState.Unchecked;

            UrgentExplTextBox.Text = app.GetFreeTextboxes(12);
            DiagnosisTextBox.Text = app.GetFreeTextboxes(13);
            GeneTextBox.Text = app.GetFreeTextboxes(17);
            AdditionalCommentsTextBox.Text = app.GetFreeTextboxes(16);

            string[] combo = app.GetTestComboBoxes();
            UrgentComboBox.SelectedIndex = UrgentComboBox.FindString(combo[6]);
            OtherReasonTextBox.SelectedIndex = OtherReasonTextBox.FindString(combo[5]);
            ClinicalCategoryComboBox.SelectedIndex = ClinicalCategoryComboBox.FindString(combo[1]);
            PTLLTextBox.DisplayMember = "Product Name";
            PTLLTextBox.DataSource = app.GetTestList(combo[1]);
            PTLLTextBox.SelectedIndex = PTLLTextBox.FindString(combo[0]);
            SampleTypeComboBox.SelectedIndex = SampleTypeComboBox.FindString(combo[2]);

            PreferredLabTextBox.DisplayMember = "Company";
            PreferredLabTextBox.DataSource = app.UpdateLabMethodList(1, combo[1], combo[0]);
            PreferredMethodTextBox.DisplayMember = "Method  Description";
            PreferredMethodTextBox.DataSource = app.UpdateLabMethodList(2, combo[1], combo[0]);

            PreferredLabTextBox.SelectedIndex = PreferredLabTextBox.FindString(combo[3]);
            PreferredMethodTextBox.SelectedIndex = PreferredMethodTextBox.FindString(combo[4]);
            CheckRadioButtons(app.GetSendoutLab());

            FamilyHistoryCheckBox.CheckState = (check[6] == true) ? CheckState.Checked : CheckState.Unchecked;
            EthnicityCheckBox.CheckState = (check[7] == true) ? CheckState.Checked : CheckState.Unchecked;
            OtherTestingCheckBox.CheckState = (check[8] == true) ? CheckState.Checked : CheckState.Unchecked;
            TherapyCheckBox.CheckState = (check[9] == true) ? CheckState.Checked : CheckState.Unchecked;
            ReduceCheckBox.CheckState = (check[10] == true) ? CheckState.Checked : CheckState.Unchecked;
            ImpactCheckBox.CheckState = (check[11] == true) ? CheckState.Checked : CheckState.Unchecked;
            PlanningCheckBox.CheckState = (check[12] == true) ? CheckState.Checked : CheckState.Unchecked;
            OtherRationaleCheckBox.CheckState = (check[13] == true) ? CheckState.Checked : CheckState.Unchecked;

            NewTestReqTextBox.Text = app.GetFreeTextboxes(26);
            NewTestMethodTextBox.Text = app.GetFreeTextboxes(27);
            NewPrefLabTextBox.Text = app.GetFreeTextboxes(28);
            FamilyHistoryTextBox.Text = app.GetFreeTextboxes(30);
            EthnicityRiskTextBox.Text = app.GetFreeTextboxes(32);
            OtherTestingTextBox.Text = app.GetFreeTextboxes(34);
            OtherRationaleTextBox.Text = app.GetFreeTextboxes(40);
            AdditionalDetailsTextBox.Text = app.GetFreeTextboxes(41);
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
                    app.ClearApplication();
                }
            }else if (saved)
            {
                MessageBox.Show("Application Saved!");
                DataTable dt = dsbClass.UpdateAppTable(true);
                dashBoard.ApplicationListTableView.DataSource = dt;
                dashBoard.UpdateMetricLabels();
            }
            else if (!loadExisting && !saved && !deleted && !finalized)
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
                    app.ClearApplication();
                }

            }

        }

    }
}

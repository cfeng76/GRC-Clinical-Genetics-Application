using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GRC_Clinical_Genetics_Application
{
    class ApplicationFormClass
    {
        #region VARIABLES
        private string personalHealthNumber;
        private string firstName;
        private string lastName;
        private string postalCode;
        private string dob;
        private string gender;
        private int patientID;
        private int physicianID;
       
        private int applicationID;
        private int newPatientID;
        private int testID;
        private int labID;
        private int sampleID;
        private int methodID;
        private int clinicalSpecialtyID;
        private int primaryContactID;
        private int secondaryContactID;
        private int otherReasonID;
        
        private string orderingPhysician;
        private string primaryContact;
        private string secondaryContact;
        private bool[] checkboxes = new bool[14];
        private string[] combobox = new string[7];
        private int urgentID;
        private bool isReadOnly;
        private int statusID;
        private string geneticsID;
        private bool isNewTest;
        private string medRecNum;
        private int subtypeID;
        private string sendOut;
        private string clinicalSubtype;
        #endregion
        Connections AppCon = new Connections();
        public ApplicationFormClass(){ /*empty constructor*/ }

        public AutoCompleteStringCollection Search(int typeOfSearch, int col = 0)
        {
            AutoCompleteStringCollection MyCollection = new AutoCompleteStringCollection();
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader sdr;
            if (typeOfSearch == 1)
            {
                cmd = AppCon.PatientSearchCommand();
                sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    MyCollection.Add(sdr[col].ToString());
                }
            }
            else if(typeOfSearch == 2)
            {
                cmd = AppCon.PhysicianSearchCommand();
                sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    MyCollection.Add(sdr[0].ToString() + " " + sdr[1].ToString() + " (" + sdr[2].ToString() + ")");
                }
            }
           
            AppCon.GRC_Connection.Close();
            return MyCollection;
        }
        internal DataTable UpdateLabMethodList(int category, string clinicalSpecialty, string ptllTest)
        {
            DataTable dt = new DataTable();
            AppCon.GRC_Connection.Open();
            SqlDataAdapter adapt = AppCon.UpdateLabMethodList(category, clinicalSpecialty, ptllTest);
            adapt.Fill(dt);
            AppCon.GRC_Connection.Close();
            return dt;
        }
        internal void SetTestID(string ptllTest, string labName)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetTestID(ptllTest, labName);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                testID = Convert.ToInt32(sdr[2].ToString());
                labID = Convert.ToInt32(sdr[0].ToString());
                methodID = Convert.ToInt32(sdr[3].ToString());
                clinicalSpecialtyID = Convert.ToInt32(sdr[4].ToString());
            }
            AppCon.GRC_Connection.Close();

        }
        internal void SetSampleID(string sampleType)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetSampleID(sampleType);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                sampleID = Convert.ToInt32(sdr[0].ToString());
            }
            AppCon.GRC_Connection.Close();
        }
        public void UpdateDemographics(string PHN)
        {
            personalHealthNumber = PHN;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.DemographicsCommand(personalHealthNumber);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                firstName = sdr[1].ToString();
                lastName = sdr[2].ToString();
                postalCode = sdr[3].ToString();
                dob = sdr[4].ToString();
                gender = sdr[5].ToString();
                patientID = Convert.ToInt32(sdr[6].ToString());
                medRecNum = sdr[7].ToString();
                //alt ID
            }
            AppCon.GRC_Connection.Close();
        }

        internal int GenerateApplicationID(int employeeID)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GenerateAppIDCommand(employeeID);
            cmd.ExecuteNonQuery();
            cmd = AppCon.GetCurrentAppIDCommand();
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                applicationID = Convert.ToInt32(sdr[0]);
            }
            AppCon.GRC_Connection.Close();
            return applicationID;
        }


        #region CORRECT INFORMATION
        public bool PatientExists(string PHN, string fName = "", string lName = "", string DOB = "", int genderID = 0)
        {
            bool isPatient = false;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.NewPatient(PHN, fName, lName, DOB, genderID);
            SqlDataReader sdr = cmd.ExecuteReader();

            while (sdr.Read())
            {
                if (Convert.ToInt32(sdr[0]) == 1)
                {
                    isPatient = true;
                }else{
                    isPatient = false;
                }
            }
            AppCon.GRC_Connection.Close();
            return isPatient;
        }

        public bool DemographicFieldsCorrect(string PHN, bool noPHN, string alternateID, string alternateExplanation, string fName, string lName, string post)
        {
            bool isComplete = false;
            if ((PHN != "" && PHN.Length >= 9 && !noPHN) ||
               (PHN == "" && noPHN && alternateID != "" && alternateExplanation != "")){
                isComplete = true;
            }
            else{
                isComplete = false;
                MessageBox.Show("Please enter a valid Personal Health Number or Alternate ID with details before saving/finalizing.");
                return isComplete;
            }

            if (fName != "" && lName != ""){
                isComplete = true;
            }
            else{
                isComplete = false;
                MessageBox.Show("Please enter a full name before saving/finalizing.");
                return isComplete;
            }

            if(post != "" && post.Length >= 6){
                isComplete = true;
            }else{
                isComplete = false;
                MessageBox.Show("Please enter a valid Postal Code/ZIP before saving/finalizing.");
                return isComplete;
            }

            return isComplete;
        }

        internal bool OrderPhysicianFieldEmpty(string orderPhys)
        {
            if (!orderPhys.Contains("("))
            {
                MessageBox.Show("Please Enter a valid Physician. (Search by last name)");
                return true;
            }
            if (orderPhys != "")
            {
                return false;
            }else
            {
                MessageBox.Show("Please enter a physician name.");
                return true;
            }
        }

        internal bool TestInfoCorrect(bool isUrgent, bool otherReason, string urgentExpl, string otherExpl, string diagnosis, string specialty, string testReq, string gene, bool newTest, bool otherLab, string otherLabDetail, string urgentSelection)
        {

            if (isUrgent && ((urgentSelection == "Other" && urgentExpl == "") || urgentSelection == ""))
            {
                MessageBox.Show("Please provide explanation to patient's urgency.");
                return false;
            } else if (otherReason && (otherExpl == "" || otherExpl == null)) {

                MessageBox.Show("Please provide other testing reason(s).");
                return false;
            } else if (diagnosis == "" || (specialty == null || specialty == "")) {
                MessageBox.Show("Please provide a diagnosis and specialty.");
                return false;
            } else if((testReq == null || testReq == "") && !newTest){
                MessageBox.Show("Please provide a test request.");
                return false;
            } else if (otherLab && otherLabDetail == "") {
                MessageBox.Show("Please provide other lab details.");
                return false;
            } else{
                return true;
            }
        }

        internal bool OtherTestInfoCorrect(string newTestReq, bool familyHistory, bool ethnicityRisk, bool otherTesting, bool otherRationale, string famHistExpl, string ethRiskExpl, string otherTstExpl, string otherRationaleExpl)
        {
            if(newTestReq == "")
            {
                MessageBox.Show("Please provide a new test request.");
                return false;
            }else if(familyHistory && famHistExpl == "")
            {
                MessageBox.Show("Please provide a family history of this condition.");
                return false;
            }else if(ethnicityRisk && ethRiskExpl == "")
            {
                MessageBox.Show("Please specify risk because of ethnicity/ancestry.");
                return false;
            }else if(otherTesting && otherTstExpl == ""){
                MessageBox.Show("Please specify other testing.");
                return false;
            }else if(otherRationale && otherRationaleExpl == "")
            {
                MessageBox.Show("Please specify other rationale for testing.");
                return false;
            }else
            {
                return true;
            }
           
        }
        #endregion

        #region RETURN INFORMATION
        internal string GetSendoutLab()
        {
            return sendOut;
        }
        internal string GetClinicSubtype()
        {
            return clinicalSubtype;
        }
        internal string GetMRN()
        {
            return medRecNum;
        }
        internal bool IsNewTest()
        {
            return isNewTest;
        }
        internal string GetGeneticsID()
        {
            return geneticsID;
        }
        internal int GetStatusID()
        {
            return statusID;
        }
        internal bool IsReadOnly()
        {
            return isReadOnly;
        }
        internal string[] GetTestComboBoxes()
        {
            return combobox;
        }
        internal bool[] GetCheckBoxes()
        {
            return checkboxes;
        }
        internal string GetPrimaryContact()
        {
            return primaryContact;
        }
        internal string GetSecondaryContact()
        {
            return secondaryContact;
        }
        internal string GetOrderingPhysician()
        {
            return orderingPhysician;
        }
        internal string GetPHN()
        {
            return personalHealthNumber;
        }
        public string GetFirstName(){
            return firstName;
        }

        public string GetLastName()
        {
            return lastName;
        }

        public string GetZIP()
        {
            return postalCode;
        }
        public string GetGender()
        {
            return gender;
        }

        public DateTime GetDOB(DateTime minDate)
        {
            DateTime date = Convert.ToDateTime(dob);
            
            if(date.Year < minDate.Year)
            {
                date = minDate;
            }
            return date;
        }
        private int GetContactID(string contactName)
        {
            int id = 0;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetContactIDs(contactName);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                id = Convert.ToInt32(sdr[0]);
            }
            AppCon.GRC_Connection.Close();
            return id;
        }
        internal int GetGenderID(string gender)
        {
            int genderID = 0;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GenderIDCommand(gender);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                genderID = Convert.ToInt32(sdr[0]);
            }
            AppCon.GRC_Connection.Close();
            return genderID;
        }
        private int GetSubTypeID(string sub)
        {
            int id = 0;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetSubtypeID(sub);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                id = Convert.ToInt32(sdr[0]);
            }
            AppCon.GRC_Connection.Close();
            return id;
        }

        private int GetReasonID(string otherReasonExpl, int group)
        {
            int id = 0;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.ReasonIDCommand(otherReasonExpl, group);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                id = Convert.ToInt32(sdr[0]);
            }
            AppCon.GRC_Connection.Close();
            return id;
        }
        internal string GetDirectory(int location, string documentType)
        {
            string path = "";
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetDirectory(documentType);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                path = sdr[location].ToString();
            }
            AppCon.GRC_Connection.Close();
            return path;
        }
        internal DataTable GetList(int dataType)
        {
            DataTable list = new DataTable();
            AppCon.GRC_Connection.Open();
            SqlDataAdapter adapt = new SqlDataAdapter();

            if (dataType == 0)
            {
                adapt = AppCon.GetClinicalSpecialtyList();
            }
            else if (dataType == 1)
            {
                adapt = AppCon.GetDocumentType();
            }
            else if (dataType == 2)
            {
                adapt = AppCon.GetGenderList();
            }
            else if (dataType == 3)
            {
                adapt = AppCon.GetSampleTypeList();
            }else if(dataType == 4)
            {
                adapt = AppCon.GetReasonList(1);
            }else if(dataType == 5)
            {
                adapt = AppCon.GetReasonList(3);
            }else if (dataType == 6)
            {
                adapt = AppCon.GetSubTypeList();
            }
            adapt.Fill(list);
            AppCon.GRC_Connection.Close();
            return list;
        }
        internal DataTable GetTestList(string clinicalSpecialty)
        {
            DataTable dt = new DataTable();
            AppCon.GRC_Connection.Open();
            SqlDataAdapter adapt = AppCon.GetPTLL(clinicalSpecialty);
            adapt.Fill(dt);
            AppCon.GRC_Connection.Close();
            return dt;
        }
        #endregion

        #region GET DB INFORMATION 
        internal void GetApplication(int appID)
        {
            applicationID = appID;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetExistingApplication(applicationID);
            SqlDataReader sdr = cmd.ExecuteReader();
            int j = 5;
            int k = 29;
            while (sdr.Read())
            {
                physicianID = Convert.ToInt32(sdr[0]);
                primaryContactID = Convert.ToInt32(sdr[1]);
                secondaryContactID = Convert.ToInt32(sdr[2]);
                patientID = Convert.ToInt32(sdr[3]);
                clinicalSpecialtyID = Convert.ToInt32(sdr[14]);
                testID = Convert.ToInt32(sdr[15]);
                sampleID = Convert.ToInt32(sdr[4]);
                otherReasonID = Convert.ToInt32(sdr[11]);
                urgentID = Convert.ToInt32(sdr[20]);
                isReadOnly = Convert.ToBoolean(sdr[21]);
                statusID = Convert.ToInt32(sdr[18]);
                geneticsID = sdr[22].ToString();
                isNewTest = Convert.ToBoolean(sdr[23]);
                subtypeID = Convert.ToInt32(sdr[24]);
                sendOut = sdr[25].ToString();
                for (int i = 0; i < 6; i++)
                {
                    checkboxes[i] = Convert.ToBoolean(sdr[j]);
                    j++;
                }
                j = 5;

                for(int i = 6; i < 14; i++)
                {
                    checkboxes[i] = Convert.ToBoolean(sdr[k]);
                    if (k <= 33)
                    {
                        k += 2;
                    }else
                    {
                        k++;
                    }
                }
                k = 29;

            }
            AppCon.GRC_Connection.Close();
            FillPatientDemographics(patientID);
            FillClinicianInformation(physicianID, primaryContactID, secondaryContactID, subtypeID);
            FillTestInformation(clinicalSpecialtyID, testID, sampleID);
            FillReasons(otherReasonID, urgentID);
        }

        private void FillReasons(int otherReasonID, int urgentID)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetReasons(otherReasonID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                combobox[5] = sdr[0].ToString();
            }
            AppCon.GRC_Connection.Close();

            AppCon.GRC_Connection.Open();
            cmd = AppCon.GetReasons(urgentID);
            sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                combobox[6] = sdr[0].ToString();
            }
            AppCon.GRC_Connection.Close();
        }
        private void FillTestInformation(int clinicalSpecialtyID, int testID, int sampleID)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetTestInformation(testID, clinicalSpecialtyID, sampleID);
            SqlDataReader sdr = cmd.ExecuteReader();
            if (clinicalSpecialtyID == 0 && testID == 0)
            {
                while (sdr.Read())
                {
                    combobox[2] = sdr[0].ToString();
                }
            }
            else
            {
                while (sdr.Read())
                {
                    for (int i = 0; i < 5; i++)
                    {
                        combobox[i] = sdr[i].ToString();
                    }
                }
            }
            AppCon.GRC_Connection.Close();

        }

        internal string GetFreeTextboxes(int column)
        {
            string free = "";
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetExistingApplication(applicationID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                free = sdr[column].ToString();
            }
            AppCon.GRC_Connection.Close();
            return free;
        }

        private void FillClinicianInformation(int physicianID, int primaryContactID, int secondaryContactID, int subtypeID)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.PhysicianSearchCommand(physicianID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                orderingPhysician = sdr[0].ToString() + " " + sdr[1].ToString() + " (" + sdr[2].ToString() + ")";
            }
            AppCon.GRC_Connection.Close();

            AppCon.GRC_Connection.Open();
            cmd = AppCon.NameCommand(primaryContactID);
            sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                primaryContact = sdr[1].ToString() + " " + sdr[2].ToString();
            }
            AppCon.GRC_Connection.Close();

            AppCon.GRC_Connection.Open();
            cmd = AppCon.NameCommand(secondaryContactID);
            sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                secondaryContact = sdr[1].ToString() + " " + sdr[2].ToString();
            }
            AppCon.GRC_Connection.Close();

            AppCon.GRC_Connection.Open();
            cmd = AppCon.GetSubtypeID(id: subtypeID);
            sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                clinicalSubtype = sdr[1].ToString();
            }
            AppCon.GRC_Connection.Close();
        }

        private void FillPatientDemographics(int patientID)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.DemographicsCommand(ID: patientID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                personalHealthNumber = sdr[0].ToString();
                firstName = sdr[1].ToString();
                lastName = sdr[2].ToString();
                postalCode = sdr[3].ToString();
                dob = sdr[4].ToString();
                gender = sdr[5].ToString();
               
            }
            AppCon.GRC_Connection.Close();

        }

        internal bool CanSubmit(int employee_ID)
        {
            bool canSubmit = false;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.CanSubmit(employee_ID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                canSubmit = Convert.ToBoolean(sdr[0]);
            }

            AppCon.GRC_Connection.Close();

            return canSubmit;
        }

        public DataTable UpdateClinicalContacts(int employeeID)
        {
            DataTable list = new DataTable();
            AppCon.GRC_Connection.Open();
            SqlDataAdapter adapt = AppCon.GetContactList(employeeID);
            adapt.Fill(list);
            AppCon.GRC_Connection.Close();
            return list;
        }
        #endregion

        internal void UpdatePhysician(string orderPhys)
        {
            if (orderPhys != "")
            {
                string physicianLastName = orderPhys.Split(' ')[0];
                string companyName = orderPhys.Split('(', ')')[1];
                SetPhysID(physicianLastName, companyName);
            }else
            {
                physicianID = 0;
            }
        }
       
        public void SetPhysID(string physLast, string companyName)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetPhysicianIDCommand(physLast, companyName);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                physicianID = Convert.ToInt32(sdr[0].ToString());
            }
            AppCon.GRC_Connection.Close();
        }

        #region APPLICATION
        internal void UpdateDocumentDestination(int docID, string path)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.UpdateDocDestCommand(docID, path);
            cmd.ExecuteNonQuery();
            AppCon.GRC_Connection.Close();
        }

        internal void ClearApplication()
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.DeleteApp(applicationID, 0);
            cmd.ExecuteNonQuery();
            cmd = AppCon.DeleteApp(applicationID, 1);
            cmd.ExecuteNonQuery();
            AppCon.GRC_Connection.Close();
            
        }
        private int CGappTestID()
        {
            int id = 0;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd;
            if (isNewTest)
            {
                cmd = AppCon.CGapptestID("CG Applicaction NewTest Request");
            }else
            {
                cmd = AppCon.CGapptestID("CG Applicaction Pre-Approved Test");
            }
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                id = Convert.ToInt32(sdr[0]);
            }
            return id;
        }

        internal void CreateNewPatient(string pHN, string firstN, string lastN, int genID, string dOB, string postCode, string mRN, string altID, string altExpl)
        {
            string placeCode = postCode.Substring(0, 3);
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.InsertNewPatient(pHN, firstN, lastN, genID, dOB, postCode, mRN, altID, altExpl);
            cmd.ExecuteNonQuery();
            cmd = AppCon.UpdateNewPatientGeographics(placeCode, pHN, lastN);
            cmd.ExecuteNonQuery();
            cmd = AppCon.DemographicsCommand(pHN);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                newPatientID = Convert.ToInt32(sdr[6].ToString());
            }
            AppCon.GRC_Connection.Close();
            patientID = newPatientID;
        }

        internal void CreateNewApplication(string PHNumber, string primaryContact, string secondaryContact, bool isUrgent, string urgentExpl, bool[] reasonCheckboxes,
            bool otherReason, string otherReasonExpl, string diagnosis, string gene, string comments, string urgentSelection, bool newTest, bool isFinalized, int statusID, 
            string geneticsID, string subtype, string sendOutLab, bool otherLab, string otherLabDetail,
            string newTestReq, string newPrefMethod, string newPrefLab, string famHistExpl, string ethRiskExpl, string otherTstExpl, string otherRationaleExpl, 
            bool familyHistory, bool ethnicityRisk, bool otherTesting, bool otherRationale, string additional, bool[] rationaleCheckboxes)
        {
            UpdateDemographics(PHNumber);
            primaryContactID = GetContactID(primaryContact);
            secondaryContactID = GetContactID(secondaryContact);
            otherReasonID = GetReasonID(otherReasonExpl, 1);
            urgentID = GetReasonID(urgentSelection, 3);
            subtypeID = GetSubTypeID(subtype);
            if (otherLab)
            {
                sendOut = otherLabDetail;
            }else
            {
                sendOut = sendOutLab;
            }
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.CreateNewApplication(applicationID, physicianID, primaryContactID, secondaryContactID, patientID, sampleID, isUrgent, 
                urgentExpl, reasonCheckboxes, otherReason, otherReasonID, diagnosis, clinicalSpecialtyID, testID, comments, gene, urgentID, newTest, isFinalized, statusID, 
                geneticsID, subtypeID, sendOut, newTestReq, newPrefMethod, newPrefLab, famHistExpl, ethRiskExpl, otherTstExpl, otherRationaleExpl, familyHistory, ethnicityRisk, otherTesting, otherRationale, additional, rationaleCheckboxes);
            cmd.ExecuteNonQuery();
            AppCon.GRC_Connection.Close();

        }

        internal void SubmitApplication(int currentAppID, int employeeID, bool v, string gene, string sample)
        {
            isNewTest = v;
            string GRC_ID = "";
            int GRCNum = 0;
            int CGapptestID = CGappTestID(); //add to CreateOrder(); instead of actual testID
            GetApplication(currentAppID);
            string urgent = GetFreeTextboxes(12);
            //Get next GRC ID
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GenerateGRCID();
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                GRC_ID = DateTime.Now.Year.ToString() + "-" + DateTime.Now.ToString("MM") + "-" + sdr[0].ToString();
                GRCNum = Convert.ToInt32(sdr[0]);
            }
            AppCon.GRC_Connection.Close();
            int nextGRC = GRCNum + 1;
            //Update GRC ID
            AppCon.GRC_Connection.Open();
            cmd = AppCon.UpdateGRCID(nextGRC);
            cmd.ExecuteNonQuery();
            //create new order
            cmd = AppCon.CreateOrder(GRC_ID, employeeID, labID, physicianID, primaryContactID, patientID, sampleID, checkboxes[0], urgentID, urgent, checkboxes[1], checkboxes[3], checkboxes[4], checkboxes[2], checkboxes[5], otherReasonID, testID, 0, currentAppID);
            cmd.ExecuteNonQuery();
            //Create Order Details if PTLL
            if (!isNewTest)
            {
                cmd = AppCon.CreateOrderDetails(GRC_ID, testID, gene, sample);
                cmd.ExecuteNonQuery(); 
            }
            //update app status
            cmd = AppCon.SubmittedAppUpdate(currentAppID, GRC_ID);
            cmd.ExecuteNonQuery();
            AppCon.GRC_Connection.Close();
            
        }
        #endregion

    }
}

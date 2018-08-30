using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GRC_Clinical_Genetics_Application
{
    class GRCFormClass
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
       
        private int CurrentOrderID;
        private int newPatientID;
        private int testID;
        private int labID;
        private int sampleID;
        private int methodID;
        private int clinicalSpecialtyID;
        private int primaryContactID;
        private int secondaryContactID;
        private int otherReasonID = 0;
        
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
        private string AdditionalNotes;
        //Order Details
        private int OD_ID;
        private int O_LABID;
        ArrayList OD_ProductIDs = new ArrayList();
        private int OD_OrderID;
        private int OD_LABID;
        private Decimal OD_Quantity;
        private Decimal OD_UnitPrice;
        private Decimal OD_Discount;
        private int OD_StatusID;
        private DateTime OD_DateAllocated;
        private int OD_PurchaseOrderID;
        private int OD_InventoryID;
        private string OD_RequiredSampleType;
        private string OD_SendSampleType;
        private bool OD_TestIsPreApproved;
        private string OD_Genes;



        #endregion
        Connections AppCon = new Connections();
        private string GRCID;
        private string GRCStatusCd = "";
        private int ApplicationsID = 0;
        private int ShippingViaID;
        private string ShippingRef;
        private bool IsSampleShip;
        private string ShippingDate; //DateTime
        private string DOB_Date;
        private string ShippingCompanyName;
       

        public GRCFormClass(){ /*empty constructor*/ }

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
                CurrentOrderID = Convert.ToInt32(sdr[0]);
            }
            AppCon.GRC_Connection.Close();
            return CurrentOrderID;
        }


        #region CORRECT INFORMATION
        public bool PatientExists(string PHN, string fName, string lName, string DOB, int genderID)
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
                MessageBox.Show("Please enter a valid Personal Health Number or Alternate ID with details.");
                return isComplete;
            }

            if (fName != "" && lName != ""){
                isComplete = true;
            }
            else{
                isComplete = false;
                MessageBox.Show("Please enter a full name.");
                return isComplete;
            }

            if(post != "" && post.Length >= 6){
                isComplete = true;
            }else{
                isComplete = false;
                MessageBox.Show("Please enter a valid Postal Code/ZIP");
                return isComplete;
            }

            return isComplete;
        }

        internal bool OrderPhysicianFieldEmpty(string orderPhys)
        {
            if(orderPhys != "")
            {
                return false;
            }else{
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
            } else if(testReq.Contains("familial") && gene == "") {
                MessageBox.Show("Please specify the gene type");
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

        internal string GetGRCId()
        {
            return GRCID;
        }

        internal string GetShippingRef()
        {
            return ShippingRef;
        }

        internal Boolean GetIsSampleShip()
        {
            return IsSampleShip;
        }


        //internal DateTime GetShippingDate()
       internal string GetShippingDate()

        {
            return ShippingDate;
        }

        
        internal int GetApplicationId()
        {
            return ApplicationsID;
        }
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
        internal string GetShippingName()
        {
            return ShippingCompanyName;
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

            

            if (date.Year < minDate.Year)
            {
                date = minDate;
            }
            return date;
        }

        public String GetDOBDate()
        {
            String DOB_date = Convert.ToDateTime(dob).ToString("yyyy-MM-dd"); //Convert.ToDateTime(dob);
            //if (DOB_date.Year < minDate.Year)
            //{
            //    date = minDate;
            //}
            return DOB_date;
        }

        //public string GetDOBDate();
        //String date_DOB = Convert.ToDateTime(dob).ToString("yyyy-MM-dd");
        //{
        //    return date_DOB;
        //}



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
            else if (dataType == 7)
            {
                adapt = AppCon.GetShippingViaList();
            
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
        
        internal bool IsGRCStatusOpen ()
        {

            bool IsOpen = false;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetGRCStatus(CurrentOrderID);
            SqlDataReader sdr = cmd.ExecuteReader();

            while (sdr.Read())

            {
                if (sdr[0].ToString().ToUpper()  == "OPEN")
                {
                    IsOpen = false; // sdr[0].ToString();
                }
                
 
            }
            AppCon.GRC_Connection.Close();
            return IsOpen;
        }
        internal String GRCStatusName(int OrderID)
        {
            GRCStatusCd = "";
             AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetGRCStatus(CurrentOrderID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())

            {
                 GRCStatusCd = Convert.ToString(sdr[0]);

            }
            AppCon.GRC_Connection.Close();
            return GRCStatusCd;
        }

        internal void GetGRCApplication(int OrderID)
        {
            CurrentOrderID = OrderID;
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetExistingGRC(CurrentOrderID);
            SqlDataReader sdr = cmd.ExecuteReader();
            int j = 5;
            int k = 29;
            while (sdr.Read())
            {
                GRCID = Convert.ToString(sdr[3]);
                statusID = Convert.ToInt32(sdr[106]); //GRC Status ID
                labID = Convert.ToInt32(sdr[7]); //GRC Lab ID

                if (!sdr.IsDBNull(115))
                {
                    ApplicationsID = Convert.ToInt32(sdr[115]);
                }

                // 
                if (!sdr.IsDBNull(8))
                {
                    physicianID = Convert.ToInt32(sdr[8]);
                }
                // 
                if (!sdr.IsDBNull(9))
                {
                    primaryContactID = Convert.ToInt32(sdr[9]);
                }
                //secondaryContactID = Convert.ToInt32(sdr[2]);
                patientID = Convert.ToInt32(sdr[10]);
                //clinicalSpecialtyID = Convert.ToInt32(sdr[14]);

                //SHIPPING SAMPLES DATA

                if (!sdr.IsDBNull(45))
                {
                    ShippingViaID = Convert.ToInt32(sdr[45]);
                }

                ShippingRef = Convert.ToString(sdr[114]);
                IsSampleShip = Convert.ToBoolean(sdr[111]);

                // Shipping Date
                if (!sdr.IsDBNull(46))
                {
                    ShippingDate = (Convert.ToDateTime(sdr[46])).ToString("yyyy-MM-dd");
                }
                else {
                    ShippingDate = "";
                }


                if (!sdr.IsDBNull(95))
                {
                    testID = Convert.ToInt32(sdr[95]);
                }

                // this is in the order detail table                  
                sampleID = Convert.ToInt32(sdr[4]); // this is in the order detail table
               
                urgentID = Convert.ToInt32(sdr[62]);
                //isReadOnly = Convert.ToBoolean(sdr[21]);

                
                


                //geneticsID = sdr[22].ToString();
                //isNewTest = Convert.ToBoolean(sdr[23]);
                //subtypeID = Convert.ToInt32(sdr[24]);
                //sendOut = sdr[25].ToString();
                checkboxes[0] = Convert.ToBoolean(sdr[65]);
                checkboxes[1] = Convert.ToBoolean(sdr[67]);
                checkboxes[2] = Convert.ToBoolean(sdr[66]);
                checkboxes[3] = Convert.ToBoolean(sdr[68]);
                checkboxes[4] = Convert.ToBoolean(sdr[69]);
                if (checkboxes[4])
                {
                    otherReasonID = Convert.ToInt32(sdr[70]);
                }
                AdditionalNotes = sdr[105].ToString();

                /*
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
              */
            }

            AppCon.GRC_Connection.Close();

            FillPatientDemographics(patientID);
            FillClinicianInformation(physicianID, primaryContactID, secondaryContactID, subtypeID);
            FillTestInformation(clinicalSpecialtyID, testID, sampleID);
            FillReasons(otherReasonID, urgentID);

            FillShippingName(ShippingViaID);
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
                        Console.WriteLine(combobox[i]);
                    }
                }
            }
            AppCon.GRC_Connection.Close();

        }

        internal string GetFreeTextboxes(int column)
        {
            string free = "";
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetExistingGRC(CurrentOrderID);
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
        // vm
        private void FillShippingName(int ShippingViaID)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetShippingViaName(ShippingViaID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                ShippingCompanyName = sdr[0].ToString();
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

        internal void FillOrderDetails(int OrderID)
        {
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GetOrderDetails(OrderID).SelectCommand;
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                //Order Details
                //OD_LABID = Convert.ToInt32(sdr[2]);
                OD_ProductIDs.Add(sdr[0]);
                //OD_Quantity = Convert.ToDecimal(sdr[3]);
                //OD_UnitPrice = Convert.ToDecimal(sdr[4]);
                //OD_Discount = Convert.ToDecimal(sdr[5]);
                //OD_StatusID = Convert.ToInt32(sdr[6]);
                //OD_DateAllocated = Convert.ToDateTime(sdr[7]);
                //OD_PurchaseOrderID = Convert.ToInt32(sdr[8]);
                //OD_InventoryID = Convert.ToInt32(sdr[9]);
                //OD_RequiredSampleType = Convert.ToString(sdr[10]);
                //OD_SendSampleType = Convert.ToString(sdr[11]);
                //OD_TestIsPreApproved = Convert.ToBoolean(sdr[12]);
                //OD_Genes = Convert.ToString(sdr[14]);

            }
           AppCon.GRC_Connection.Close();

        }
        internal DataTable OrderDetailDataTable(int OrderID)
        {

            DataTable list = new DataTable();
            AppCon.GRC_Connection.Open();
            SqlDataAdapter adapt = AppCon.GetOrderDetails(OrderID);
            adapt.Fill(list);
            AppCon.GRC_Connection.Close();
                return list;

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
            SqlCommand cmd = AppCon.DeleteApp(CurrentOrderID, 0);
            cmd.ExecuteNonQuery();
            cmd = AppCon.DeleteApp(CurrentOrderID, 1);
            cmd.ExecuteNonQuery();
            AppCon.GRC_Connection.Close();
            
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
            SqlCommand cmd = AppCon.CreateNewApplication(CurrentOrderID, physicianID, primaryContactID, secondaryContactID, patientID, sampleID, isUrgent, 
                urgentExpl, reasonCheckboxes, otherReason, otherReasonID, diagnosis, clinicalSpecialtyID, testID, comments, gene, urgentID, newTest, isFinalized, statusID, 
                geneticsID, subtypeID, sendOut, newTestReq, newPrefMethod, newPrefLab, famHistExpl, ethRiskExpl, otherTstExpl, otherRationaleExpl, familyHistory, ethnicityRisk, otherTesting, otherRationale, additional, rationaleCheckboxes);
            cmd.ExecuteNonQuery();
            AppCon.GRC_Connection.Close();

        }

        internal void SubmitApplication(int currentAppID, int employeeID)
        {
            string GRC_ID = "";
            int GRCNum = 0;
            GetGRCApplication(currentAppID);
            string urgent = GetFreeTextboxes(12);
            //Get next GRC ID
            AppCon.GRC_Connection.Open();
            SqlCommand cmd = AppCon.GenerateGRCID();
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                GRC_ID = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + sdr[0].ToString();
                GRCNum = Convert.ToInt32(sdr[0]);
            }
            AppCon.GRC_Connection.Close();
            int nextGRC = GRCNum + 1;
            //Update GRC ID
            AppCon.GRC_Connection.Open();
            cmd = AppCon.UpdateGRCID(nextGRC);
            cmd.ExecuteNonQuery();
            //create order
            cmd = AppCon.CreateOrder(GRC_ID, employeeID, labID, physicianID, primaryContactID, patientID, sampleID, checkboxes[0], urgentID, urgent, checkboxes[1], checkboxes[3], checkboxes[4], checkboxes[2], checkboxes[5], otherReasonID, testID, 0, currentAppID);
            cmd.ExecuteNonQuery();
            //update app status
            cmd = AppCon.SubmittedAppUpdate(currentAppID, GRC_ID);
            cmd.ExecuteNonQuery();
            AppCon.GRC_Connection.Close();
            
        }
        #endregion

    }
}

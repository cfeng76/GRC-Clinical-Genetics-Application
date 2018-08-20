using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;
using System.Configuration;

namespace GRC_Clinical_Genetics_Application
{
    class Connections : IDisposable
    {
        public SqlConnection GRC_Connection = new SqlConnection("Server=WSSQLC011N02\\TSTINST01;database=GRC;integrated security=true;");
        private string sConnection = Properties.Settings.Default.GRCConnectionString;
        public Connections()
        {
            //establish connection
        }
        public void Dispose()
        {

        }
        public SqlCommand LoginCommand(string user)
        {
            SqlCommand loginCommand = new SqlCommand("Select [login], [password], [PwdReset], [ID] from [GRC].[dbo].[Employees] where [login]='" + user + "' ", GRC_Connection);
            return loginCommand;
        }

        public SqlCommand NameCommand(int id)
        {
            SqlCommand cmd = new SqlCommand("Select [ID], [First Name], [Last Name] from [GRC].[dbo].[Employees] where id =" + id , GRC_Connection);
            return cmd;
        }

        internal SqlDataAdapter GetDocumentList(int applicationID)
        {
            SqlDataAdapter doc = new SqlDataAdapter("SELECT [Document Name] FROM [GRC].[dbo].[Application Documents] where [ApplicationID] = '" + applicationID + "' " , GRC_Connection);
            return doc;
        }
        internal SqlDataAdapter GetGenderList()
        {
            SqlDataAdapter gender = new SqlDataAdapter("SELECT [Gender] FROM [GRC].[dbo].[CBO Gender] order by [Gender ID] desc", GRC_Connection);
            return gender;
        }

        internal SqlCommand GetDocPath(int appID, string docName)
        {
            SqlCommand docPath = new SqlCommand("SELECT [Document Destination] FROM [GRC].[dbo].[Application Documents] where ApplicationID = '" + appID + "' and [Document Name] = '" + docName + "' ", GRC_Connection);
            return docPath;
        }

        internal SqlCommand PhysicianSearchCommand(int ID = 0)
        {
            SqlCommand physCmd = new SqlCommand();
            if (ID == 0)
            {
                return physCmd = new SqlCommand("SELECT [Physician Last name], [Physician First Name], [Company], [id] FROM [GRC].[dbo].[Patient Care Provider] where [Clinic Name] IS NOT NULL order by [Physician Last name]", GRC_Connection);
            }else
            {
                return physCmd = new SqlCommand("SELECT [Physician Last name], [Physician First Name], [Company], [id] FROM [GRC].[dbo].[Patient Care Provider] where [Clinic Name] IS NOT NULL and ID = '" + ID + "' order by [Physician Last name]", GRC_Connection);
            }
        }

        internal SqlDataAdapter GetPTLL(string clinicalSpecialty)
        {   
            SqlDataAdapter ptll = new SqlDataAdapter("SELECT distinct [Product Name] FROM [GRC].[dbo].[Products], [GRC].[dbo].[Suppliers], [GRC].[dbo].[CBO Test Types] where Products.[Supplier IDs] = Suppliers.ID and Products.TypeID = [CBO Test Types].ID and Discontinued = 0 and isPreapproved = 1 and [Test Type  Description] = '" + clinicalSpecialty + "' order by [Product Name]", GRC_Connection);
            return ptll;
        }
        internal SqlDataAdapter UpdateLabMethodList(int category, string clinicalSpecialty, string ptllTest)
        {
            SqlDataAdapter labMethod = new SqlDataAdapter();
            if (category == 1)
            {
                labMethod = new SqlDataAdapter("SELECT Suppliers.Company FROM [GRC].[dbo].[Products], [GRC].[dbo].[Suppliers], [GRC].[dbo].[CBO Test Types], [GRC].[dbo].[CBO Test Method] where Products.[Supplier IDs] = Suppliers.ID and Products.MethodID = [CBO Test Method].ID and Products.TypeID = [CBO Test Types].ID and Discontinued = 0 and [Test Type  Description] = '" + clinicalSpecialty + "' and [Product Name] =  '" + ptllTest + "' ", GRC_Connection);
                return labMethod;
            }else if(category == 2)
            {
                labMethod = new SqlDataAdapter("SELECT distinct [CBO Test Method].[Method  Description] FROM [GRC].[dbo].[Products], [GRC].[dbo].[Suppliers], [GRC].[dbo].[CBO Test Types], [GRC].[dbo].[CBO Test Method] where Products.[Supplier IDs] = Suppliers.ID and Products.MethodID = [CBO Test Method].ID and Products.TypeID = [CBO Test Types].ID and Discontinued = 0 and [Test Type  Description] = '" + clinicalSpecialty + "' and [Product Name] =  '" + ptllTest + "' ", GRC_Connection);
                return labMethod;
            }
            return labMethod;
        }

        internal SqlCommand GetSampleID(string sampleName)
        {
            SqlCommand sam = new SqlCommand("SELECT [ID], [Specimen Type] FROM [GRC].[dbo].[CBO Specimen Types] where [Specimen Type] = '" + sampleName + "' ", GRC_Connection);
            return sam;
        }

        internal SqlCommand GetTestID(string testName, string labName)
        {
            SqlCommand tst = new SqlCommand("SELECT Suppliers.ID, [Product Name], Products.ID , [CBO Test Method].ID, Products.[TypeID] FROM [GRC].[dbo].[Products], [GRC].[dbo].[Suppliers], [GRC].[dbo].[CBO Test Types],  [GRC].[dbo].[CBO Test Method] where Products.[Supplier IDs] = Suppliers.ID and Products.TypeID = [CBO Test Types].ID and Products.MethodID = [CBO Test Method].ID and Discontinued = 0 and [Product Name] = '" + testName + "' and Company = '" + labName + "' ", GRC_Connection);
            return tst;
        }

        internal SqlCommand GetTestInformation(int testID, int typeID, int sampleID)
        {
            SqlCommand cmd;
            if (testID == 0 && typeID == 0)
            {
                cmd = new SqlCommand("select [Specimen Type] from [GRC].[dbo].[CBO Specimen Types] where ID =" + sampleID, GRC_Connection);
            }
            else
            {
                cmd = new SqlCommand("SELECT distinct [Product Name], [CBO Test Types].[Test Type  Description], [Specimen Type], Company, [Method  Description] FROM [GRC].[dbo].[Products], [GRC].[dbo].[CBO Test Types], [GRC].[dbo].[CBO Specimen Types], [GRC].[dbo].[Suppliers], [GRC].[dbo].[CBO Test Method] where Products.MethodID = [CBO Test Method].ID and Products.[Supplier IDs] = Suppliers.ID and Products.TypeID = [CBO Test Types].ID and [CBO Test Types].ID = " + typeID + " and Products.ID = " + testID + " and [CBO Specimen Types].ID =" + sampleID, GRC_Connection);
            }
            return cmd;
        }
        internal SqlCommand GetPhysicianIDCommand(string physLastName, string companyName)
        {
            SqlCommand physID = new SqlCommand("SELECT [id] FROM [GRC].[dbo].[Patient Care Provider] where [Physician Last Name] = '" + physLastName + "' and Company = '" + companyName + "'", GRC_Connection);
            return physID;
        }

        public SqlCommand UpdateCommand(string newPass, string user)
        {
            SqlCommand updateCommand = new SqlCommand("Update [GRC].[dbo].[Employees] SET [password] = '" + newPass + "', [PwdReset] = 0 where [login] = '" + user + "' ", GRC_Connection);
            return updateCommand;
        }
        internal SqlCommand UpdateDocDestCommand(int docID, string path)
        {
            SqlCommand docCmd = new SqlCommand("Update [GRC].[dbo].[Application Documents] set [Document Destination] = '" + path + "' where [DocumentID] = '" + docID + "' ", GRC_Connection);
            return docCmd;
        }
        internal SqlCommand UpdateNewPatientGeographics(string placeCode, string pHN, string lastN)
        {
            SqlCommand updGeo = new SqlCommand("Update [GRC].[dbo].[Patients] Set City = GEO.[Place Name], [State/Province] = GEO.[Provinces/States Code] from [GRC].[dbo].[Patients] PAT join [GRC].[dbo].[Geo_Cities] GEO on GEO.[Place Code] = '" + placeCode + "' and PAT.[Last Name] = '" + lastN + "' and PAT.[Personal Health Number] = '" + pHN + "'", GRC_Connection);
            return updGeo;
        }

        internal SqlCommand InsertNewPatient(string pHN, string firstN, string lastN, int genID, string dOB, string pCode, string mRN, string altID, string altExpl)
        {
            SqlCommand cmd = new SqlCommand("insert into [GRC].[dbo].[Patients] ([Personal Health Number],[Last Name],[First Name],[Gender],[DOB],[IsDeceased],[ZIP/Postal Code],[Country/Region], [Medical Record Number (MRN)], [Alternate Identify Number], [Alternate Identify Number Notes]) values ('" + pHN + "','" + lastN + "','" + firstN + "','" + genID + "','" + dOB + "', 0, '" + pCode + "','CA', '"+ mRN + "', '" + altID + "', '" + altExpl + "' )", GRC_Connection);
            return cmd;
        }

        internal SqlCommand DeleteApp(int appID, int table)
        {
            SqlCommand del = new SqlCommand();
            if (table == 0)
            {
                del = new SqlCommand("Delete from [GRC].[dbo].[Application Documents] where ApplicationID = '" + appID + "' ", GRC_Connection);
                return del;
            }else if(table == 1)
            {
                del = new SqlCommand("delete from [GRC].[dbo].[Applications] where [Applications ID] = '" + appID + "' ", GRC_Connection);
                return del;
            }
            return del;
        }

        public SqlCommand MetricsCommand(int userID, int metricsID)
        {
            if (metricsID == 1){
                SqlCommand openCmd = new SqlCommand("SELECT COUNT([Status Name]) FROM [GRC].[dbo].[Orders], [GRC].[dbo].Patients, [GRC].[dbo].[Orders Status], [GRC].[dbo].employees where [Patient ID] = [GRC].[dbo].Patients.ID  and [GRC].[dbo].[Orders].[Status ID] = [GRC].[dbo].[Orders Status].[Status ID] and [Employee ID] = [GRC].[dbo].employees.ID and [Status Name] like '%open%' and [Employee ID] = '" + userID + "' ", GRC_Connection);
                return openCmd;
            }else
            {
                SqlCommand urgentCmd = new SqlCommand("SELECT COUNT([isUrgent]) FROM [GRC].[dbo].Applications where IsUrgent = 1 and [Employee ID] = '" + userID + "' ", GRC_Connection);
                return urgentCmd;
            }
        }

        internal SqlCommand GenerateAppIDCommand(int employeeID)
        {
            SqlCommand appIDCmd = new SqlCommand("insert into [GRC].dbo.Applications ([Applications Date], [Employee ID]) values (Convert(VARCHAR(10), GETDATE(), 126), " + employeeID + ")", GRC_Connection);
            return appIDCmd;
        }

        internal SqlCommand GetCurrentAppIDCommand()
        {
            SqlCommand currentAppIDCmd = new SqlCommand("SELECT Max([Applications ID]) FROM[GRC].[dbo].[Applications]", GRC_Connection);
            return currentAppIDCmd;
        }

        public SqlCommand PatientSearchCommand()
        {
            SqlCommand schCmd = new SqlCommand("SELECT [Personal Health Number], [First Name], [Last Name] FROM [GRC].[dbo].[Patients]", GRC_Connection);
            return schCmd;
        }

        public SqlCommand NewPatient(string PHN, string firstName, string lastName, string DOB, int genderID)
        {
            SqlCommand nPat = new SqlCommand("SELECT count(*) FROM [GRC].[dbo].[Patients] where [Personal Health Number] = '" + PHN + "' " , GRC_Connection);
            return nPat;
        }

        public SqlCommand DemographicsCommand(string phn = "", int ID = 0)
        {
            SqlCommand demCmd = new SqlCommand("SELECT [Personal Health Number], [First Name], [Last Name], [ZIP/Postal Code], [DOB], [CBO Gender].Gender, Patients.ID, Patients.[Medical Record Number (MRN)], Patients.[Alternate Identify Number], Patients.[Alternate Identify Number Notes] FROM [GRC].[dbo].[Patients], [GRC].[dbo].[CBO Gender] where Patients.Gender = [CBO Gender].[Gender ID] and ([Personal Health Number] = '" + phn + "' or ID = '" + ID + "')", GRC_Connection);
            return demCmd;
        }

        internal SqlDataAdapter GetContactList(int empID)
        {
            SqlDataAdapter contactList = new SqlDataAdapter("SELECT [First Name] + ' ' + [Last Name] as 'Clinical Contact' FROM [GRC].[dbo].[Employees], [GRC].[dbo].[Employee Privileges] EP, [GRC].[dbo].[Privileges] P where Employees.ID = EP.[Employee ID] and EP.[Privilege ID] = P.[Privilege ID] and P.[CGA Access] = 1 order by case when [Employee ID] = '" + empID + "' then 1 else 2 end, [First Name]", GRC_Connection);
            return contactList;
        }
        internal SqlDataAdapter GetClinicalSpecialtyList()
        {
            SqlDataAdapter specialtyList = new SqlDataAdapter("SELECT [Test Type  Description] FROM [GRC].[dbo].[CBO Test Types] where not ID = 16", GRC_Connection);
            return specialtyList;
        }
        internal SqlDataAdapter GetDocumentType()
        {
            SqlDataAdapter documentTypeList = new SqlDataAdapter("SELECT [Document Type Name] FROM [GRC].[dbo].[CBO App Document Type]", GRC_Connection);
            return documentTypeList;
        }

        internal SqlCommand GetSubtypeID(string sub = "", int id = 0)
        {
            return new SqlCommand("SELECT [id], [SubType Name] FROM [GRC].[dbo].[CBO App Clinic SubType] where [SubType Name] = '" + sub + "' or [ID] = " + id, GRC_Connection);
        }

        internal SqlCommand ReasonIDCommand(string reason, int group)
        {
            SqlCommand cmd = new SqlCommand("Select [ID], [Reason] From [GRC].[dbo].[CBO Reasons] where [Reason] = '" + reason + "' and [Reason Group ID] =" + group, GRC_Connection);
            return cmd;
        }

        public SqlDataAdapter getDefaultDatatable(int id)
        {   
            SqlDataAdapter dataTable = new SqlDataAdapter("SELECT [Applications ID], APP.[GRC ID], APP.[Genetics ID], ASTAT.[Status Name] as 'App Status', Case when APP.[GRC ID] IS NULL then '' else OSTAT.[Status Name] end as 'GRC Status', PAT.[Last Name] + ', ' + PAT.[First Name] as 'Patient', PAT.[Personal Health Number] as 'PHN', CASE when APP.[IsUrgent] = 1 then 'Yes' else 'No' end as 'Is Urgent?', CONVERT(VARCHAR(10), [Finalized Date], 126) as 'Application Finalized Date', CASE O.[Status ID] when 0 then Convert(VARCHAR(10), O.[Received Date], 126) when 17 then Convert(VARCHAR(10), O.[Appeal Notification Date], 126) when 14 then Convert(VARCHAR(10), O.[Approved Notification Date], 126) when 15 then Convert(VARCHAR(10), O.[Declined Notification Date], 126) when 18 then Convert(VARCHAR(10), O.[ReSubmission Notification Date], 126) when 2 then Convert(VARCHAR(10), O.[Shipped Date], 126) when 16 then Convert(VARCHAR(10), O.[Withdraw Notification Date], 126) end as 'GRC Status Date' , DATEDIFF(DAY, O.[Approved Notification Date], GETDATE()) as 'Days after approved' , EMP.[First Name] + ' ' + EMP.[Last Name] as 'Created by' FROM [GRC].[dbo].Applications APP left join [GRC].[dbo].[Orders] O on O.[Order ID] = APP.[Order ID] left join [GRC].[dbo].employees EMP on EMP.ID = APP.[Employee ID] left join [GRC].dbo.[CBO Application Status] ASTAT on ASTAT.ID = APP.[Application Status ID] left join [GRC].[dbo].[Orders Status] OSTAT on OSTAT.[Status ID] = APP.[Order Status ID] left join [GRC].[dbo].Patients PAT on PAT.ID = APP.[Patient ID] where APP.[Patient ID] is not null and APP.[Employee ID] = '" + id + "' order by [Applications ID]", GRC_Connection);
            return dataTable;
        }

        internal SqlDataAdapter GetSubTypeList()
        {
            return new SqlDataAdapter("SELECT [SubType Name] FROM [GRC].[dbo].[CBO App Clinic SubType] where [isActive] = 1", GRC_Connection);
        }

        internal SqlDataAdapter GetReasonList(int group)
        {
            SqlDataAdapter sda = new SqlDataAdapter("Select [Reason] From [GRC].[dbo].[CBO Reasons] where [Reason Group ID] =" + group, GRC_Connection);
            return sda;
        }

        internal SqlCommand GetExistingApplication(int appID)
        {
            SqlCommand cmd = new SqlCommand("SELECT [Patient Care Provider ID], [PCP Contact ID Primary],[PCP Contact ID Alternate],[Patient ID],[Patient Specimen Type ID],[IsUrgent],[IsPatientClinicallyAffected],[IsFamilyMutation],[IsPrenatalTesting],[IsFetalPostmortemTest],[IsOtherReasonForTesting],[OtherReasonForTestingID],[Urgent Other Reasons],[Diagnosis],[Test Type],[Test Requested ID],[Comments],[Gene],[Application Status ID],[Additional Notes], [Urgent Reason ID], [IsSubmitted], [Genetics ID], [IsNewTestRequest], [Clinic Subtype ID], [Send out Lab], [New Test Request],[New Preferred Method],[New Preferred Laboratory],[IsFamilyHistory],[Family History Reasons],[IsEthnicityRisk],[Ethnicity Risk Reasons],[IsOtherTesting],[Other Testing Reasons],[IsChangeInTherapy],[IsReduceInvestigation],[IsImpactOnRelatives],[IsPrenatalDiagnosis],[IsOtherRationale],[Other Rationale],[Additional Notes] FROM [GRC].[dbo].[Applications] where [Applications ID] = '" + appID + "' ", GRC_Connection);
            return cmd;
        }

        internal SqlDataAdapter GetSampleTypeList()
        {
            SqlDataAdapter sample = new SqlDataAdapter("SELECT [Specimen Type] FROM [GRC].[dbo].[CBO Specimen Types]", GRC_Connection);
            return sample;
        }

        internal SqlCommand GenderIDCommand(string gender)
        {
            SqlCommand genderID = new SqlCommand("SELECT [Gender ID], [Gender] FROM [GRC].[dbo].[CBO Gender] where Gender = '" + gender + "' ", GRC_Connection);
            return genderID;
        }

        public SqlDataAdapter getCustomDatatable(string GRCnum, string status, string patientFirstName, string patientLastName, int PHN, bool isUrgent, bool showAll, int id, string AppStat)
        {
            string cmdString = "SELECT [Applications ID], APP.[GRC ID], APP.[Genetics ID], ASTAT.[Status Name] as 'App Status', Case when APP.[GRC ID] IS NULL then '' else OSTAT.[Status Name] end as 'GRC Status', PAT.[Last Name] + ', ' + PAT.[First Name] as 'Patient', PAT.[Personal Health Number] as 'PHN', CASE when APP.[IsUrgent] = 1 then 'Yes' else 'No' end as 'Is Urgent?', CONVERT(VARCHAR(10), [Applications Date], 126) as 'Application Submission Date', CASE O.[Status ID] when 0 then Convert(VARCHAR(10), O.[Received Date], 126) when 17 then Convert(VARCHAR(10), O.[Appeal Notification Date], 126) when 14 then Convert(VARCHAR(10), O.[Approved Notification Date], 126) when 15 then Convert(VARCHAR(10), O.[Declined Notification Date], 126) when 18 then Convert(VARCHAR(10), O.[ReSubmission Notification Date], 126) when 2 then Convert(VARCHAR(10), O.[Shipped Date], 126) when 16 then Convert(VARCHAR(10), O.[Withdraw Notification Date], 126) end as 'GRC Status Date' , DATEDIFF(DAY, O.[Approved Notification Date], GETDATE()) as 'Days after approved' , EMP.[First Name] + ' ' + EMP.[Last Name] as 'Submitted by' FROM [GRC].[dbo].Applications APP left join [GRC].[dbo].[Orders] O on O.[Order ID] = APP.[Order ID] left join [GRC].[dbo].employees EMP on EMP.ID = APP.[Employee ID] left join [GRC].dbo.[CBO Application Status] ASTAT on ASTAT.ID = APP.[Application Status ID] left join [GRC].[dbo].[Orders Status] OSTAT on OSTAT.[Status ID] = APP.[Order Status ID] left join [GRC].[dbo].Patients PAT on PAT.ID = APP.[Patient ID] where APP.[Patient ID] IS not null ";

            if (GRCnum != "")
            {
                cmdString = cmdString + " and APP.[GRC ID] LIKE '%" + GRCnum + "%' ";
            }
            if(AppStat != "Any")
            {
                cmdString = cmdString + " and ASTAT.[Status Name] LIKE '%" + AppStat + "%' ";
            }
            if(status != "Any")
            {
                cmdString = cmdString + " and OSTAT.[Status Name] LIKE '%" + status + "%' and APP.[GRC ID] is not null "; 
            }
            if(patientFirstName != "")
            {
                cmdString = cmdString + " and PAT.[First Name] LIKE '%" + patientFirstName + "%' ";
            }
            if (patientLastName != "")
            {
                cmdString = cmdString + " and PAT.[Last Name] LIKE '%" + patientLastName + "%' ";
            }
            if(PHN != 0)
            {
                cmdString = cmdString + " and PAT.[Personal Health Number] LIKE '%" + PHN + "%' ";
            }
            if(isUrgent == true)
            {
                cmdString = cmdString + " and APP.[isUrgent] = 1 ";
            }
            if(showAll == false)
            {
                cmdString = cmdString + " and APP.[Employee ID] = " + id.ToString();
            }

            SqlDataAdapter dataTable = new SqlDataAdapter(cmdString, GRC_Connection);
            return dataTable;
        }

        internal SqlCommand GetReasons(int ReasonID)
        {
            SqlCommand cmd = new SqlCommand("Select [Reason] From [GRC].[dbo].[CBO Reasons] where [ID] = '" + ReasonID + "'", GRC_Connection);
            return cmd;
        }

        internal SqlCommand CreateNewApplication(int applicationID, int physicianID, int primaryContactID, int secondaryContactID, int patientID, int sampleID, bool isUrgent, 
            string urgentExpl, bool[] v1, bool otherReason, int otherReasonID, string diagnosis, int clinicalSpecialtyID, int testID, string comments, string gene, 
            int urgentID, bool newTest, bool isFinalized, int statusID, string geneticsID, int subtypeID, string sendoutLab,
            string newTestReq, string newPrefMethod, string newPrefLab, string famHistExpl, string ethRiskExpl, string otherTstExpl, string otherRationaleExpl,
            bool familyHistory, bool ethnicityRisk, bool otherTesting, bool otherRationale, string additional, bool[] rationale)
        {
            string applicationDate = "Convert(VARCHAR(10), GETDATE(), 126)";
            string finalizeDate = (statusID == 2) ? applicationDate : "null";
            //if (statusID == 1)
            //{
            //    date = "null";
            //}
            SqlCommand app = new SqlCommand("update [GRC].[dbo].[Applications] set [Finalized Date] = " + finalizeDate +", [Application Status Date] = " + applicationDate + ", [Patient Care Provider ID] = " + 
                physicianID + ", [PCP Contact ID Primary] = '" + primaryContactID + "', [PCP Contact ID Alternate] = '" + secondaryContactID + "', [Patient ID] = " + 
                patientID + ", [Patient Specimen Type ID] = '" + sampleID + "', [IsUrgent] = '" + isUrgent + "', [Urgent Other Reasons] = '" + urgentExpl + "', [IsPatientClinicallyAffected] = '" + 
                v1[0] + "', [IsFamilyMutation] = '" + v1[1] + "', [IsPrenatalTesting] = '" + v1[2] + "', [IsFetalPostmortemTest] = '" + v1[3] + "', [IsOtherReasonForTesting] = '" + 
                otherReason + "', [OtherReasonForTestingID] = '" + otherReasonID + "', [Diagnosis] = '" + diagnosis + "', [Test Type] = '" + clinicalSpecialtyID + "', [Test Requested ID] = '" + 
                testID + "', [Comments] = '" + comments + "', [Gene] = '" + gene + "', [Urgent Reason ID] = '" + urgentID + "', [Application Status ID] = '" + 
                statusID + "', [IsNewTestRequest] = '" + newTest + "', [isSubmitted] = '" + isFinalized + "', [Genetics ID] = '" + geneticsID + "', [Clinic Subtype ID] = " + 
                subtypeID + ", [Send out Lab] = '" + sendoutLab + "', [New Test Request] ='" + newTestReq + "', [New Preferred Method] = '" + newPrefMethod + "', [New Preferred Laboratory] = '" +
                newPrefLab + "', [IsFamilyHistory] = '" + familyHistory + "', [Family History Reasons] = '" + famHistExpl + "', [IsEthnicityRisk] = '" + 
                ethnicityRisk + "', [Ethnicity Risk Reasons] ='" + ethRiskExpl + "', [IsOtherTesting] = '" + otherTesting + "', [Other Testing Reasons] = '" + otherTstExpl 
                + "', [IsChangeInTherapy] = '" + rationale[0] + "', [IsReduceInvestigation] = '" +  rationale[1] + "', [IsImpactOnRelatives] = '" + rationale[2] + "', [IsPrenatalDiagnosis] = '"+
                rationale[3] + "', [IsOtherRationale] = '" + otherRationale + "', [Other Rationale] = '" + otherRationaleExpl + "', [Additional Notes] = '" + additional 
                + "' where [Applications ID] = '" + applicationID + "' ", GRC_Connection);
            return app;
        }

        internal SqlCommand CanSubmit(int employee_ID)
        {
            return new SqlCommand("SELECT [CGA Modify] FROM [GRC].[dbo].[Employees], [GRC].[dbo].[Employee Privileges] EP, [GRC].[dbo].[Privileges] P where Employees.ID = EP.[Employee ID] and EP.[Privilege ID] = P.[Privilege ID] and [Employee ID] =" + employee_ID, GRC_Connection);
        }

        internal SqlCommand GetContactIDs(string contactName)
        {
            SqlCommand contact = new SqlCommand("SELECT [ID], [First Name] + ' ' + [Last Name] as 'Name' FROM [GRC].[dbo].[Employees] where [First Name] + ' ' + [Last Name] LIKE '%" + contactName + "%' ", GRC_Connection);
            return contact;
        }

        internal SqlCommand GenerateGRCID()
        {
            SqlCommand cmd = new SqlCommand("Select [Next GRC Number] from [GRC].[dbo].[GRC Admin Information]", GRC_Connection);
            return cmd;
        }
        internal SqlCommand UpdateGRCID(int nextGRCID)
        {
            SqlCommand cmd = new SqlCommand("Update [GRC].[dbo].[GRC Admin Information] set [Next GRC Number] = " + nextGRCID, GRC_Connection);
            return cmd;
        }

        internal SqlCommand CreateOrder(string GRC_ID, int employeeID, int labID, int physicianID, int primaryContactID, int patientID, int sampleID, bool v1, int urgentID, string urgentReasons, bool clinicallyAffected, bool prenatal, bool postmortem, bool familyMutation, bool otherReason, int otherReasonID, int testID, int statusID, int appID)
        {
            return new SqlCommand("insert into [GRC].[dbo].[Orders] ([Received Date], [GRC ID], [GRC Coordinator ID], [Employee ID], [Lab ID],[Patient Care Provider ID], [Patient Care Provider Contact ID], [Patient ID], [Patient Specimen Type ID], [IsUrgent], [Urgent Reason], [Urgent Other Reasons],[IsPatientClinicallyAffected], [IsFetalTesting], [IsFetalPostmortemTest],[IsFamilialMutation],[IsOtherReasonForTesting],[Reason For Testing ID],[Test Requested ID],[Status ID], [Applications_ID]) values (Convert(VARCHAR(10), GETDATE(), 126), '" + GRC_ID + "', 22, " + employeeID + ", " + labID + ", " + physicianID + ", " + primaryContactID + ", " + patientID + ", " + sampleID + ", '" + v1 + "', " + urgentID + ", '" + urgentReasons + "', '" + clinicallyAffected + "', '" + prenatal + "', '" + postmortem + "', '" + familyMutation + "', '" + otherReason + "', " + otherReasonID + ", " + testID + ", " + statusID + ", " + appID + ")", GRC_Connection);
            // Convert(VARCHAR(10), GETDATE(), 126), " + GRC_ID + ", 22, " + employeeID + "," + labID + "," + physicianID + "," + primaryContactID + "," + patientID + "," + sampleID + "," + v1 + "," + urgentID + ", '" + urgentReasons + "'," + clinicallyAffected + "," + prenatal + "," + postmortem + "," + familyMutation + "," + otherReason + "," + otherReasonID + ", 1, " + testID + "," + statusID
        }
        internal SqlCommand SubmittedAppUpdate(int appID, string GRC_ID)
        {
            return new SqlCommand("update [GRC].[dbo].[Applications] set [GRC ID] = '" + GRC_ID + "' , [GRC Coordinator ID] = 22, [Order ID] = ORDERS.[Order ID], [Application Status ID] = 3 from [GRC].[dbo].[Applications] APPS join [GRC].[dbo].[Orders] ORDERS on APPS.[Applications ID] = " + appID + " and ORDERS.[GRC ID] = '" + GRC_ID + "' ", GRC_Connection);
        }

        internal SqlCommand GetDirectory(string documentType)
        {
            return new SqlCommand("SELECT [Source Default Location],[Destination Default Location] FROM [GRC].[dbo].[CBO App Document Type] where [Document Type Name] = '" + documentType + "' ", GRC_Connection);
        }

        internal SqlCommand CreateOrderDetails(string gRC_ID, int testID, string gene, string sample)
        {
            return new SqlCommand("insert into [GRC].[dbo].[Order Details] ([Order ID], [Product ID], [Quantity], [Unit Price], [Discount], [Status ID], [Required Sample Type], [Send Sample Type], TestIsPreApproved, Genes) Select O.[Order ID], " + testID + ", 1, P.[List Price], 0, 0, '" + sample + "', 'DNA', 1, '" + gene + "' from [GRC].[dbo].[Orders] O, [GRC].[dbo].[Products] P where O.[GRC ID] = '" + gRC_ID + "' and P.ID =" + testID, GRC_Connection);
        }
        internal SqlCommand CGapptestID(string test)
        {
            return new SqlCommand("SELECT [Test Requested ID],[Test Requested] FROM [GRC].[dbo].[CBO Test Requested] where [Test Requested] Like '" + test + "'", GRC_Connection);
        }
    }
}



////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////* SQL COMMANDS *//////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////
//
// OPEN CONNECTION FIRST AND ALWAYS CLOSE CONNECTION AFTER USE
//
// READ: SqlCommand cmd = connection._________Command(obj);
//       SqlDataReader sdr = cmd.ExecuteReader();
//       while (sdr.Read()){
//           sdr[] is the array with the columns of data
//       }
//
// UPDATE: SqlCommand cmd = connection.________Command(obj); 
//         cmd.ExecuteNonQuery();
//
// DATATABLE:   Datatable dt = new Datatable();
//              SqlDataAdapter adapt = SqlDataAdapter("", con);
//              adapt.Fill(dt);
//              .datasource = dt;
// SqlCommand cmd = new SqlCommand("", GRC_Connection);
//  return cmd;
//FOR GRC DASHBOARD : SELECT[GRC ID], [Status Name] as 'Status', [Patients].[Last Name] + ', ' + [Patients].[First Name] as 'Patient', [Patients].[Personal Health Number] as 'PHN',  CONVERT(VARCHAR(10), Patients.DOB , 126) as 'Date of Birth', CASE when[IsUrgent] = 1 then 'Yes' else 'No' end as 'Is Urgent?', CASE when Orders.[Paperwork Received Date] IS NULL then 'No' else 'Yes' end as 'Paperwork Received?', [Received Date] as 'Application Submission Date' , [Employees].[First Name] + ' ' + Employees.[Last Name] as 'Submitted by' FROM [GRC].[dbo].[Orders], [GRC].[dbo].Patients, [GRC].[dbo].[Orders Status], [GRC].[dbo].employees where [Patient ID] = [GRC].[dbo].Patients.ID and[GRC].[dbo].[Orders].[Status ID] = [GRC].[dbo].[Orders Status].[Status ID] and [Employee ID] = [GRC].[dbo].employees.ID
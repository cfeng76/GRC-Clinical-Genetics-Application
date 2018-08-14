using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace GRC_Clinical_Genetics_Application
{
    class DashboardClass
    {
        private string name;
        Connections dashCon = new Connections();
        private int userID;
        private int result;

        public DashboardClass(int user) {
            userID = user;
        }

        public string UpdateGreeting()
        {
            dashCon.GRC_Connection.Open();
            SqlCommand cmd = dashCon.NameCommand(userID);
            SqlDataReader sdr = cmd.ExecuteReader();
            while (sdr.Read())
            {
                name = sdr[1].ToString() + " " + sdr[2].ToString();
            }
            dashCon.GRC_Connection.Close();
            return name;
        }

        public int UpdateMetrics(int categoryID)
        {
            switch (categoryID)
            {
                case (1):
                    dashCon.GRC_Connection.Open();
                    SqlCommand cmd1 = dashCon.MetricsCommand(userID, categoryID);
                    SqlDataReader sdr1 = cmd1.ExecuteReader();
                    while (sdr1.Read())
                    {
                        result = Convert.ToInt32(sdr1[0].ToString());
                    }
                    dashCon.GRC_Connection.Close();
                    return result;
                case (2):
                    dashCon.GRC_Connection.Open();
                    SqlCommand cmd2 = dashCon.MetricsCommand(userID, categoryID);
                    SqlDataReader sdr2 = cmd2.ExecuteReader();
                    while (sdr2.Read())
                    {
                        result = Convert.ToInt32(sdr2[0].ToString());
                    }
                    dashCon.GRC_Connection.Close();
                    return result;
            }
            return 0;
        }

        public DataTable UpdateAppTable(bool def, string GRCnum = "", string status = "", string patientFirstName = "", string patientLastName = "", int PHN = 0, bool isUrgent = false, bool showAll = false, string AppStat = "")
        {
            DataTable data = new DataTable();
            if (def)//default table
            {
                dashCon.GRC_Connection.Open();
                SqlDataAdapter adapt = dashCon.getDefaultDatatable(userID);
                adapt.Fill(data);
                dashCon.GRC_Connection.Close();
                return data;
            }else
            {
                //create table with optional search parameters 
                dashCon.GRC_Connection.Open();
                SqlDataAdapter adt = dashCon.getCustomDatatable(GRCnum, status, patientFirstName, patientLastName, PHN, isUrgent, showAll, userID, AppStat);
                adt.Fill(data);
                dashCon.GRC_Connection.Close();
                return data;
            }
        }

        public DataTable UpdateGRCTable(bool def, string GRCnum = "", string status = "", string patientFirstName = "", string patientLastName = "", int PHN = 0, bool isUrgent = false, bool showAll = false, string AppStat = "")
        {
            DataTable data = new DataTable();
            if (def)//default table
            {
                dashCon.GRC_Connection.Open();
                SqlDataAdapter adapt = dashCon.getDefaultOrdersDatatable(userID); //(userID)
                adapt.Fill(data);
                dashCon.GRC_Connection.Close();
                return data;
            }
            else
            {

                //create table with optional search parameters 
                dashCon.GRC_Connection.Open();
                SqlDataAdapter adt = dashCon.getCustomOrdersDatatable(GRCnum, status, patientFirstName, patientLastName, PHN, isUrgent, showAll, userID, AppStat);
                adt.Fill(data);
                dashCon.GRC_Connection.Close();
                return data;
            }
        }


    }
}

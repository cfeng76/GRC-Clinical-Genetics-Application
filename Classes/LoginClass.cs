using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Common;
using System.Data.SqlClient;

namespace GRC_Clinical_Genetics_Application
{
    public class LoginClass
    {
        private string username;
        private string password;
        private int logID;
        private int invalidCounter = 0;
        Connections logs = new Connections();
        private int pwdReset;

        public LoginClass() { }//empty constructor

        public bool Verify(string user, string pass)
        {
            this.username = user;
            this.password = pass;

            if (username == "")
            {
                MessageBox.Show("Please enter username");
                return false;
            }
            else if (password == "")
            {
                MessageBox.Show("Please enter your password");
                return false;
            }

            logs.GRC_Connection.Open();
            SqlCommand loginCommand = logs.LoginCommand(username);
            SqlDataReader sdr = loginCommand.ExecuteReader();

            while (sdr.Read()) //credentials[0] is username credentials[1] is password credentials[2] is password reset flag
            {
                if ((username == sdr[0].ToString()) && (password == sdr[1].ToString()))
                {
                    logID = Convert.ToInt32(sdr[3]);//save ID
                    logs.GRC_Connection.Close();
                    invalidCounter = 0;
                    return true;
                }
                else 
                {
                    logs.GRC_Connection.Close();
                    MessageBox.Show("Invalid username or password");
                    if(invalidCounter == 5)
                    {
                        Application.Exit(); //OPTION: Have user reset password
                    }
                    invalidCounter++;
                    return false;
                }
            }
            //No login detected
            logs.GRC_Connection.Close();
            MessageBox.Show("Invalid username or password");
            if (invalidCounter == 5)
            {
                Application.Exit(); //OPTION: Have user reset password
            }
            invalidCounter++;
            return false;
        }

        public string GetOldPassword()
        {
            return password;
        }

        public int GetUserID()
        {
            return logID;
        }

        public bool IspwdReset()
        {
            logs.GRC_Connection.Open();
            SqlCommand loginCommand = logs.LoginCommand(username);
            SqlDataReader sdr = loginCommand.ExecuteReader();
            //Console.WriteLine(Convert.ToInt32(credentials[2]));
            while (sdr.Read())
            {
                pwdReset = Convert.ToInt32(sdr[2]);
            }
            logs.GRC_Connection.Close();
            return (pwdReset == 1) ? true : false; //if 1, password needs to be reset
        }

        public void UpdatePassword(string nPass)
        {
            logs.GRC_Connection.Open();
            SqlCommand updatePasswordCommand = logs.UpdateCommand(nPass, username); //can use username or ID
            updatePasswordCommand.ExecuteNonQuery();
            logs.GRC_Connection.Close();
        }


    }
}

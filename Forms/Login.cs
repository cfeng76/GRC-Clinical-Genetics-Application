using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GRC_Clinical_Genetics_Application
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }
        LoginClass newlogin = new LoginClass();

        private void Login_Load(object sender, EventArgs e)
        {
            this.AcceptButton = this.LoginButton;
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                VersionLabel.Text = string.Format("GRC Clinical Genetics Application - v{0}", ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4));
            }
          
        }
        private void LoginButton_Click(object sender, EventArgs e)
        {
            string user = UsernameTextBox.Text;
            string pass = PasswordTextBox.Text;
            
            if (newlogin.Verify(user, pass)){
                //login to dashboard
                if (newlogin.IspwdReset())
                {
                    //show new/confirm password textboxes and update password
                    PasswordLabel.Hide();
                    NewPasswordLabel.Show();

                    ConfirmPasswordLabel.Show();
                    ConfirmPasswordTextBox.Show();
                    PasswordTextBox.Text = "";
                    MessageBox.Show("Please update your password.");

                    LoginButton.Hide();
                    ConfirmLoginButton.Show();
                }
                else
                {
                    //MessageBox.Show("Login successfull!");
                    this.Hide();
                    Dashboard db = new Dashboard(newlogin.GetUserID());
                    db.Show();
                }
            }
        }

        private void ConfirmLoginButton_Click(object sender, EventArgs e)
        {
            string newPass = PasswordTextBox.Text;
            string confirmPass = ConfirmPasswordTextBox.Text;
            if (newPass != confirmPass)
            {
                MessageBox.Show("Passwords must match!");
            }else if(newPass == newlogin.GetOldPassword()){
                MessageBox.Show("New password must be different from your old password!");
            }
            else
            {
                newlogin.UpdatePassword(newPass);
                MessageBox.Show("Password Updated");
                this.Hide();
                Dashboard db = new Dashboard(newlogin.GetUserID());
                db.Show();
            }
        }

    }
}

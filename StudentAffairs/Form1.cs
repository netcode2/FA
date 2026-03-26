using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace StudentAffairs
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
          
            // ثابت المركز على الشاشة
            this.StartPosition = FormStartPosition.CenterScreen;

            // منع تغيير الحجم
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

        }
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string username = guna2TextBox1.Text.Trim();
            string password = guna2TextBox2.Text.Trim();

            if (username == "" || password == "")
            {
                MessageBox.Show("يرجى إدخال اسم المستخدم وكلمة السر", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hashed = HashPassword(password);

            try
            {
                using (SqlConnection con = DB.GetConnection())
                {
                    con.Open();
                    string query = @"
SELECT 
    u.UserID,
    u.FullName,
    r.RoleName
FROM Users u
JOIN Roles r ON u.RoleID = r.RoleID
WHERE u.Username = @username
  AND u.PasswordHash = @password;";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 200).Value = username;
                    cmd.Parameters.Add("@password", SqlDbType.NVarChar, 400).Value = hashed;

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        int userId = Convert.ToInt32(dr["UserID"]);
                        string fullName = dr["FullName"].ToString();
                        string role = dr["RoleName"].ToString();

                        this.Hide();

                        if (role == "Admin")
                        {
                            FormMain f = new FormMain(userId, fullName);
                            f.Show();
                        }
                        else
                        {
                            MessageBox.Show("اسم المستخدم أو كلمة السر غير صحيحة", "خطأ",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ بالاتصال: " + ex.Message);
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                // إظهار كلمة السر
                guna2TextBox2.PasswordChar = '\0';
            }
            else
            {
                // إخفاء كلمة السر
                guna2TextBox2.PasswordChar = '*'; // أو أي رمز تريده
            }
        }
    }


}

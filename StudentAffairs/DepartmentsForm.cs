using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace StudentAffairs
{
    public partial class DepartmentsForm : Form
    {
        private int _userId;
        private string _fullName;
        public DepartmentsForm(int userId, string fullName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            _userId = userId;
            _fullName = fullName;
            label1.Text = $"Welcome {fullName}";
            LoadSpecializations();
            StyleDataGridView(guna2DataGridView1);
        }
        private void StyleDataGridView(Guna.UI2.WinForms.Guna2DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;
            // إعدادات عامة للداتا كريد فيو
            dgv.EnableHeadersVisualStyles = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;
            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;

            // العناوين
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(94, 148, 255);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Calibri", 16, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 80;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            // الصفوف
            dgv.RowsDefaultCellStyle.Font = new Font("Calibri", 14, FontStyle.Regular);
            dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowTemplate.Height = 80;

            // ألوان الصفوف بالتناوب
            dgv.RowsDefaultCellStyle.BackColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // الفواصل 
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgv.GridColor = Color.Gray;

            // نمط الخلايا
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(94, 148, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        }
        private void DepartmentsForm_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
        }
        private int _selectedSpecId = 0;

        private void LoadSpecializations()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
    s.SpecializationID,
    s.SpecializationName,
    COUNT(DISTINCT st.StageID) AS Stages,
    COUNT(DISTINCT sub.SubjectID) AS Subjects
FROM Specializations s
LEFT JOIN Stages st 
    ON s.SpecializationID = st.SpecializationID
LEFT JOIN Subjects sub 
    ON s.SpecializationID = sub.SpecializationID
GROUP BY s.SpecializationID, s.SpecializationName
ORDER BY s.SpecializationName";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2DataGridView1.DataSource = dt;

                guna2DataGridView1.Columns["SpecializationID"].Visible = false;
                guna2DataGridView1.Columns["SpecializationName"].HeaderText = "الاختصاص";
                guna2DataGridView1.Columns["Stages"].HeaderText = "عدد المراحل";
                guna2DataGridView1.Columns["Subjects"].HeaderText = "عدد المواد";
            }
        }
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            // حركة Fade Out قبل الإغلاق
            Timer fadeOutTimer = new Timer();
            fadeOutTimer.Interval = 10;
            fadeOutTimer.Tick += (s, args) =>
            {
                if (this.Opacity > 0)
                    this.Opacity -= 0.05;
                else
                {
                    fadeOutTimer.Stop();
                    this.Close();
                }
            };
            fadeOutTimer.Start();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string name = guna2TextBox1.Text.Trim();

            if (name == "")
            {
                MessageBox.Show("ادخل اسم الاختصاص");
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                SqlCommand check = new SqlCommand(
                    "SELECT COUNT(*) FROM Specializations WHERE SpecializationName=@n",
                    con);

                check.Parameters.AddWithValue("@n", name);

                if ((int)check.ExecuteScalar() > 0)
                {
                    MessageBox.Show("الاختصاص موجود مسبقاً");
                    return;
                }

                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Specializations VALUES(@n)", con);

                cmd.Parameters.AddWithValue("@n", name);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تمت الإضافة ✅");

            guna2TextBox1.Clear();
            LoadSpecializations();
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = guna2DataGridView1.Rows[e.RowIndex];

            _selectedSpecId = Convert.ToInt32(row.Cells["SpecializationID"].Value);
            guna2TextBox1.Text = row.Cells["SpecializationName"].Value.ToString();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (_selectedSpecId == 0)
            {
                MessageBox.Show("اختر اختصاص للتعديل");
                return;
            }

            string name = guna2TextBox1.Text.Trim();

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Specializations 
              SET SpecializationName=@n 
              WHERE SpecializationID=@id", con);

                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@id", _selectedSpecId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تم التعديل ✅");

            guna2TextBox1.Clear();
            _selectedSpecId = 0;
            LoadSpecializations();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (_selectedSpecId == 0)
            {
                MessageBox.Show("اختر اختصاص للحذف");
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                string checkSql = @"
SELECT 
 (SELECT COUNT(*) FROM Stages WHERE SpecializationID=@id) +
 (SELECT COUNT(*) FROM Subjects WHERE SpecializationID=@id) +
 (SELECT COUNT(*) FROM Students_Iraq WHERE SpecializationID=@id) +
 (SELECT COUNT(*) FROM Students_Foreign WHERE SpecializationID=@id) +
 (SELECT COUNT(*) FROM InstructorSpecializations WHERE SpecializationID=@id)";

                SqlCommand check = new SqlCommand(checkSql, con);
                check.Parameters.AddWithValue("@id", _selectedSpecId);

                int linked = (int)check.ExecuteScalar();

                if (linked > 0)
                {
                    MessageBox.Show(
                        "لا يمكن حذف الاختصاص — مرتبط ببيانات أخرى ❌");
                    return;
                }

                DialogResult r = MessageBox.Show(
                    "هل تريد حذف الاختصاص؟",
                    "تأكيد",
                    MessageBoxButtons.YesNo);

                if (r != DialogResult.Yes) return;

                SqlCommand del = new SqlCommand(
                    "DELETE FROM Specializations WHERE SpecializationID=@id", con);

                del.Parameters.AddWithValue("@id", _selectedSpecId);
                del.ExecuteNonQuery();
            }

            MessageBox.Show("تم الحذف ✅");

            guna2TextBox1.Clear();
            _selectedSpecId = 0;
            LoadSpecializations();
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            (guna2DataGridView1.DataSource as DataTable)
    .DefaultView.RowFilter =
    $"SpecializationName LIKE '%{guna2TextBox2.Text}%'";
        }
    }
}

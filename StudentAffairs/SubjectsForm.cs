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

namespace StudentAffairs
{
    public partial class SubjectsForm : Form
    {
        private int _userId;
        private string _fullName;
        public SubjectsForm(int userId, string fullName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            _userId = userId;
            _fullName = fullName;
            label1.Text = $"Welcome {fullName}";
            LoadSpecializations();
            LoadStagesBySpec();
            LoadSubjects();
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
        private void LoadStagesBySpec()
        {
            if (guna2ComboBox1.SelectedValue == null)
                return;

            // ✅ تحقق من النوع
            if (guna2ComboBox1.SelectedValue is DataRowView)
                return;

            int specId;

            if (!int.TryParse(guna2ComboBox1.SelectedValue.ToString(), out specId))
                return;

            using (SqlConnection con = DB.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT StageID, StageName FROM Stages WHERE SpecializationID=@id", con);

                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = specId;

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox2.DataSource = dt;
                guna2ComboBox2.DisplayMember = "StageName";
                guna2ComboBox2.ValueMember = "StageID";
            }
        }
        private void LoadSpecializations()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT SpecializationID, SpecializationName FROM Specializations", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox1.DataSource = dt;
                guna2ComboBox1.DisplayMember = "SpecializationName";
                guna2ComboBox1.ValueMember = "SpecializationID";
            }
        }
        private int _selectedSubjectId = 0;

        private void LoadSubjects()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
    sb.SubjectID,
    sb.SubjectName,
    sp.SpecializationName,
    st.StageName
FROM Subjects sb
JOIN Specializations sp 
    ON sb.SpecializationID = sp.SpecializationID
JOIN Stages st 
    ON sb.StageID = st.StageID
WHERE 1=1";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                // فلترة الاختصاص
                if (guna2ComboBox1.SelectedIndex > -1 && guna2ComboBox1.SelectedValue is int)
                {
                    sql += " AND sb.SpecializationID = @spec";
                    cmd.Parameters.AddWithValue("@spec", (int)guna2ComboBox1.SelectedValue);
                }

                // فلترة المرحلة
                if (guna2ComboBox2.SelectedIndex > -1 && guna2ComboBox2.SelectedValue is int)
                {
                    sql += " AND sb.StageID = @stage";
                    cmd.Parameters.AddWithValue("@stage", (int)guna2ComboBox2.SelectedValue);
                }

                sql += " ORDER BY sp.SpecializationName, st.StageName";

                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2DataGridView1.DataSource = dt;

                guna2DataGridView1.Columns["SubjectID"].Visible = false;
                guna2DataGridView1.Columns["SubjectName"].HeaderText = "المادة";
                guna2DataGridView1.Columns["SpecializationName"].HeaderText = "الاختصاص";
                guna2DataGridView1.Columns["StageName"].HeaderText = "المرحلة";
            }
        }
        private void SubjectsForm_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStagesBySpec();
            LoadSubjects();
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
                MessageBox.Show("ادخل اسم المادة");
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                // منع تكرار
                SqlCommand check = new SqlCommand(
                    @"SELECT COUNT(*) FROM Subjects 
              WHERE SubjectName=@n 
              AND StageID=@st", con);

                check.Parameters.AddWithValue("@n", name);
                check.Parameters.AddWithValue("@st", guna2ComboBox2.SelectedValue);

                if ((int)check.ExecuteScalar() > 0)
                {
                    MessageBox.Show("المادة موجودة مسبقاً");
                    return;
                }

                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Subjects
              VALUES(@n,@st,@sp)", con);

                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@st", guna2ComboBox2.SelectedValue);
                cmd.Parameters.AddWithValue("@sp", guna2ComboBox1.SelectedValue);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تمت الإضافة ✅");

            guna2TextBox1.Clear();
            LoadSubjects();
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = guna2DataGridView1.Rows[e.RowIndex];

            _selectedSubjectId = Convert.ToInt32(row.Cells["SubjectID"].Value);

            guna2TextBox1.Text = row.Cells["SubjectName"].Value.ToString();
            guna2ComboBox1.Text = row.Cells["SpecializationName"].Value.ToString();
            guna2ComboBox2.Text = row.Cells["StageName"].Value.ToString();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (_selectedSubjectId == 0)
            {
                MessageBox.Show("اختر مادة للتعديل");
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Subjects SET
              SubjectName=@n,
              StageID=@st,
              SpecializationID=@sp
              WHERE SubjectID=@id", con);

                cmd.Parameters.AddWithValue("@n", guna2TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@st", guna2ComboBox2.SelectedValue);
                cmd.Parameters.AddWithValue("@sp", guna2ComboBox1.SelectedValue);
                cmd.Parameters.AddWithValue("@id", _selectedSubjectId);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تم التعديل ✅");

            guna2TextBox1.Clear();
            _selectedSubjectId = 0;

            LoadSubjects();
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            (guna2DataGridView1.DataSource as DataTable)
        .DefaultView.RowFilter =
        $"SubjectName LIKE '%{guna2TextBox2.Text}%'";
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (_selectedSubjectId == 0)
            {
                MessageBox.Show("اختر مادة للحذف");
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                SqlCommand check = new SqlCommand(
                    @"SELECT COUNT(*) FROM StudentGrades 
              WHERE SubjectID=@id", con);

                check.Parameters.AddWithValue("@id", _selectedSubjectId);

                if ((int)check.ExecuteScalar() > 0)
                {
                    MessageBox.Show("المادة مرتبطة بدرجات ❌");
                    return;
                }

                if (MessageBox.Show("تأكيد الحذف؟", "تحذير",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

                SqlCommand del = new SqlCommand(
                    "DELETE FROM Subjects WHERE SubjectID=@id", con);

                del.Parameters.AddWithValue("@id", _selectedSubjectId);
                del.ExecuteNonQuery();
            }

            MessageBox.Show("تم الحذف ✅");

            LoadSubjects();
        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
             LoadSubjects();
        }
    }
    
}

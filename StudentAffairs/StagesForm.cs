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
    public partial class StagesForm : Form
    {
        private int _userId;
        private string _fullName;
        public StagesForm(int userId, string fullName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            _userId = userId;
            _fullName = fullName;
            label1.Text = $"Welcome {fullName}";
            LoadSpecializationsCombo();
            LoadStages();
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
        private void LoadSpecializationsCombo()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = "SELECT SpecializationID, SpecializationName FROM Specializations";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox1.DataSource = dt;
                guna2ComboBox1.DisplayMember = "SpecializationName";
                guna2ComboBox1.ValueMember = "SpecializationID";
            }
        }
        private int _selectedStageId = 0;

        private void LoadStages()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                // استعلام المراحل المرتبطة بالاختصاص المحدد
                string sql = @"
SELECT 
    st.StageID,
    st.StageName,
    sp.SpecializationName
FROM Stages st
JOIN Specializations sp 
    ON st.SpecializationID = sp.SpecializationID
WHERE st.SpecializationID = @spec
ORDER BY st.StageName"; // ترتيب المراحل أبجديًا

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    // فلترة الاختصاص حسب ComboBox
                    if (guna2ComboBox1.SelectedIndex > -1 && guna2ComboBox1.SelectedValue is int)
                    {
                        cmd.Parameters.AddWithValue("@spec", (int)guna2ComboBox1.SelectedValue);
                    }
                    else
                    {
                        // لو ما تم اختيار أي اختصاص، يمكن إرجاع جميع المراحل أو التعامل حسب رغبتك
                        cmd.Parameters.AddWithValue("@spec", DBNull.Value);
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    guna2DataGridView1.DataSource = dt;

                    // إعدادات الأعمدة
                    guna2DataGridView1.Columns["StageID"].Visible = false;
                    guna2DataGridView1.Columns["StageName"].HeaderText = "المرحلة";
                    guna2DataGridView1.Columns["SpecializationName"].HeaderText = "الاختصاص";
                }
            }
        }
        private void StagesForm_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
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
            int specId = (int)guna2ComboBox1.SelectedValue;

            if (name == "")
            {
                MessageBox.Show("ادخل اسم المرحلة");
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                // منع التكرار داخل نفس الاختصاص
                SqlCommand check = new SqlCommand(
                    @"SELECT COUNT(*) FROM Stages 
              WHERE StageName=@n AND SpecializationID=@s", con);

                check.Parameters.AddWithValue("@n", name);
                check.Parameters.AddWithValue("@s", specId);

                if ((int)check.ExecuteScalar() > 0)
                {
                    MessageBox.Show("المرحلة موجودة مسبقاً");
                    return;
                }

                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Stages VALUES(@n,@s)", con);

                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@s", specId);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تمت الإضافة ✅");

            guna2TextBox1.Clear();
            LoadStages();
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = guna2DataGridView1.Rows[e.RowIndex];

            _selectedStageId = Convert.ToInt32(row.Cells["StageID"].Value);

            guna2TextBox1.Text = row.Cells["StageName"].Value.ToString();
            guna2ComboBox1.Text = row.Cells["SpecializationName"].Value.ToString();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (_selectedStageId == 0)
            {
                MessageBox.Show("اختر مرحلة للتعديل");
                return;
            }

            string name = guna2TextBox1.Text.Trim();
            int specId = (int)guna2ComboBox1.SelectedValue;

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Stages 
              SET StageName=@n, SpecializationID=@s
              WHERE StageID=@id", con);

                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@s", specId);
                cmd.Parameters.AddWithValue("@id", _selectedStageId);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تم التعديل ✅");

            guna2TextBox1.Clear();
            _selectedStageId = 0;
            LoadStages();
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            (guna2DataGridView1.DataSource as DataTable)
       .DefaultView.RowFilter =
       $"StageName LIKE '%{guna2TextBox2.Text}%'";
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (_selectedStageId == 0)
            {
                MessageBox.Show("اختر مرحلة للحذف");
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                string checkSql = @"
SELECT 
 (SELECT COUNT(*) FROM Subjects WHERE StageID=@id) +
 (SELECT COUNT(*) FROM Students_Iraq WHERE StageID=@id) +
 (SELECT COUNT(*) FROM Students_Foreign WHERE StageID=@id)";

                SqlCommand check = new SqlCommand(checkSql, con);
                check.Parameters.AddWithValue("@id", _selectedStageId);

                if ((int)check.ExecuteScalar() > 0)
                {
                    MessageBox.Show("لا يمكن حذف المرحلة — مرتبطة ببيانات ❌");
                    return;
                }

                if (MessageBox.Show("تأكيد الحذف؟", "تحذير",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

                SqlCommand del = new SqlCommand(
                    "DELETE FROM Stages WHERE StageID=@id", con);

                del.Parameters.AddWithValue("@id", _selectedStageId);
                del.ExecuteNonQuery();
            }

            MessageBox.Show("تم الحذف ✅");

            guna2TextBox1.Clear();
            _selectedStageId = 0;
            LoadStages();
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStages();
        }
    }
}

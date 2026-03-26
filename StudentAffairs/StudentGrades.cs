using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace StudentAffairs
{
    public partial class StudentGrades : Form
    {
        private int _userId;
        private string _fullName;

        public StudentGrades(int userId, string fullName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;

            _userId = userId;
            _fullName = fullName;

            label1.Text = $"Welcome {fullName}";
            StyleDataGridView(guna2DataGridView1);
        }

        private void StudentGrades_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
            LoadSpecializations();
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            guna2ComboBox4.SelectionChangeCommitted += (s, e) => LoadStages();
            guna2ComboBox2.SelectionChangeCommitted += (s, e) => LoadSubjects();
            guna2ComboBox3.SelectionChangeCommitted += (s, e) => LoadStudents();
        }


        private void LoadSpecializations()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT SpecializationID, SpecializationName FROM Specializations", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox4.DataSource = dt;
                guna2ComboBox4.DisplayMember = "SpecializationName";
                guna2ComboBox4.ValueMember = "SpecializationID";
            }
        }

        private void LoadStages()
        {
            if (guna2ComboBox4.SelectedValue == null) return;

            using (SqlConnection con = DB.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT StageID, StageName FROM Stages WHERE SpecializationID=@spec", con);

                da.SelectCommand.Parameters.AddWithValue("@spec", guna2ComboBox4.SelectedValue);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox2.DataSource = dt;
                guna2ComboBox2.DisplayMember = "StageName";
                guna2ComboBox2.ValueMember = "StageID";
            }
        }

        private void LoadSubjects()
        {
            if (guna2ComboBox2.SelectedValue == null) return;

            using (SqlConnection con = DB.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT SubjectID, SubjectName 
                      FROM Subjects 
                      WHERE StageID=@stage", con);

                da.SelectCommand.Parameters.AddWithValue("@stage", guna2ComboBox2.SelectedValue);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox3.DataSource = dt;
                guna2ComboBox3.DisplayMember = "SubjectName";
                guna2ComboBox3.ValueMember = "SubjectID";
            }
        }
        DataTable dtStudents = new DataTable();
        private void LoadStudents()
        {
            if (guna2ComboBox2.SelectedValue == null ||
                guna2ComboBox3.SelectedValue == null)
                return;

            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
    s.StudentID,
    s.FullName AS [الاسم],
    s.StudentType AS [النوع],
    g.Midterm AS [السعي],
    g.Final AS [النهائي],

    ISNULL(g.Midterm,0) + ISNULL(g.Final,0) AS [المجموع],

   CASE 
    WHEN (g.Midterm IS NULL OR g.Final IS NULL 
          OR (g.Midterm = 0 AND g.Final = 0)) THEN N'لم يمتحن'

    WHEN (ISNULL(g.Midterm,0) + ISNULL(g.Final,0)) < 50 THEN N'ضعيف'

    WHEN (ISNULL(g.Midterm,0) + ISNULL(g.Final,0)) BETWEEN 50 AND 59 THEN N'مقبول'
    WHEN (ISNULL(g.Midterm,0) + ISNULL(g.Final,0)) BETWEEN 60 AND 69 THEN N'متوسط'
    WHEN (ISNULL(g.Midterm,0) + ISNULL(g.Final,0)) BETWEEN 70 AND 79 THEN N'جيد'
    WHEN (ISNULL(g.Midterm,0) + ISNULL(g.Final,0)) BETWEEN 80 AND 89 THEN N'جيد جداً'
    ELSE N'امتياز'
END AS [التقدير]

FROM
(
    SELECT IraqStudentID AS StudentID, FullName, StageID, N'عراقي' AS StudentType
    FROM Students_Iraq

    UNION ALL

    SELECT ForeignStudentID AS StudentID, FullName, StageID, N'أجنبي' AS StudentType
    FROM Students_Foreign
) s

LEFT JOIN StudentGrades g
ON g.StudentID = s.StudentID
AND g.SubjectID = @sub
AND g.StudentType = s.StudentType

WHERE s.StageID = @stage
ORDER BY s.FullName";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);

                da.SelectCommand.Parameters.AddWithValue("@stage", guna2ComboBox2.SelectedValue);
                da.SelectCommand.Parameters.AddWithValue("@sub", guna2ComboBox3.SelectedValue);

                dtStudents.Clear();
                da.Fill(dtStudents);
                guna2DataGridView1.DataSource = dtStudents;

                // 🔹 اخفاء الايدي
                guna2DataGridView1.Columns["StudentID"].Visible = false;

                // 🔹 قراءة فقط
                guna2DataGridView1.Columns["الاسم"].ReadOnly = true;
                guna2DataGridView1.Columns["النوع"].ReadOnly = true;
                guna2DataGridView1.Columns["المجموع"].ReadOnly = true;
                guna2DataGridView1.Columns["التقدير"].ReadOnly = true;

                // 🔥 التلوين الاحترافي
                foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    decimal mid = 0;
                    decimal fin = 0;

                    if (row.Cells["السعي"].Value != DBNull.Value)
                        mid = Convert.ToDecimal(row.Cells["السعي"].Value);

                    if (row.Cells["النهائي"].Value != DBNull.Value)
                        fin = Convert.ToDecimal(row.Cells["النهائي"].Value);

                    decimal total = mid + fin;

                    // 🟡 لم يمتحن
                    if (mid == 0 && fin == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        continue;
                    }
                    if (mid == 0 && fin >= 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        continue;
                    }
                    if (mid >= 0 && fin == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        continue;
                    }
                    if (total < 50)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral; // 🔴 ضعيف
                    }
                    else if (total <= 59)
                    {
                        row.DefaultCellStyle.BackColor = Color.Orange; // 🟠 مقبول
                    }
                    else if (total <= 69)
                    {
                        row.DefaultCellStyle.BackColor = Color.Khaki; // 🟡 متوسط
                    }
                    else if (total <= 79)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightBlue; // 🔵 جيد
                    }
                    else if (total <= 89)
                    {
                        row.DefaultCellStyle.BackColor = Color.MediumPurple; // 🟣 جيد جداً
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen; // 🟢 امتياز
                    }
                }
            }
        }
        private void guna2DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "Midterm" ||
                guna2DataGridView1.Columns[e.ColumnIndex].Name == "Final")
            {
                var row = guna2DataGridView1.Rows[e.RowIndex];

                decimal mid = 0;
                decimal fin = 0;

                decimal.TryParse(row.Cells["السعي"].Value?.ToString(), out mid);
                decimal.TryParse(row.Cells["النهائي"].Value?.ToString(), out fin);

                row.Cells["Total"].Value = mid + fin;
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (guna2ComboBox3.SelectedValue == null) return;

            int subjectId = (int)guna2ComboBox3.SelectedValue;

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    int studentId = Convert.ToInt32(row.Cells["StudentID"].Value);

                    // ✅ نجيب نوع الطالب من الجدول نفسه
                    string type = row.Cells["النوع"].Value.ToString().Trim();
                    decimal mid = 0;
                    decimal fin = 0;

                    // ✅ حماية من القيم الفارغة
                    if (row.Cells["السعي"].Value != DBNull.Value)
                        mid = Convert.ToDecimal(row.Cells["السعي"].Value);

                    if (row.Cells["النهائي"].Value != DBNull.Value)
                        fin = Convert.ToDecimal(row.Cells["النهائي"].Value);

                    SqlCommand cmd = new SqlCommand(@"
IF EXISTS (SELECT 1 FROM StudentGrades 
WHERE StudentID=@st AND SubjectID=@sub AND StudentType=@type)

UPDATE StudentGrades
SET Midterm=@mid, Final=@fin
WHERE StudentID=@st AND SubjectID=@sub AND StudentType=@type

ELSE

INSERT INTO StudentGrades
(StudentType,StudentID,SubjectID,Midterm,Final,GradeDate)
VALUES(@type,@st,@sub,@mid,@fin,GETDATE())", con);

                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@st", studentId);
                    cmd.Parameters.AddWithValue("@sub", subjectId);
                    cmd.Parameters.AddWithValue("@mid", mid);
                    cmd.Parameters.AddWithValue("@fin", fin);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("✅ تم حفظ الدرجات بنجاح");
            LoadStudents();
        }
        private void StyleDataGridView(Guna.UI2.WinForms.Guna2DataGridView dgv)
        {
            dgv.AutoGenerateColumns = true;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ReadOnly = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;
            dgv.RowHeadersVisible = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(94, 148, 255);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 50;
            // الصفوف
            dgv.RowsDefaultCellStyle.Font = new Font("Calibri", 14, FontStyle.Regular);
            dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowTemplate.Height = 60;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            if (dtStudents == null) return;

            string txt = guna2TextBox2.Text.Trim().Replace("'", "''");

            if (txt == "")
            {
                dtStudents.DefaultView.RowFilter = "";
            }
            else
            {
                dtStudents.DefaultView.RowFilter =
                    $"[الاسم] LIKE '%{txt}%' OR [النوع] LIKE '%{txt}%'";
            }
               // 🔥 التلوين الاحترافي
                foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    decimal mid = 0;
                    decimal fin = 0;

                    if (row.Cells["السعي"].Value != DBNull.Value)
                        mid = Convert.ToDecimal(row.Cells["السعي"].Value);

                    if (row.Cells["النهائي"].Value != DBNull.Value)
                        fin = Convert.ToDecimal(row.Cells["النهائي"].Value);

                    decimal total = mid + fin;

                    // 🟡 لم يمتحن
                    if (mid == 0 && fin == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        continue;
                    }
                    if (mid == 0 && fin >= 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        continue;
                    }
                    if (mid >= 0 && fin == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        continue;
                    }
                    if (total < 50)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral; // 🔴 ضعيف
                    }
                    else if (total <= 59)
                    {
                        row.DefaultCellStyle.BackColor = Color.Orange; // 🟠 مقبول
                    }
                    else if (total <= 69)
                    {
                        row.DefaultCellStyle.BackColor = Color.Khaki; // 🟡 متوسط
                    }
                    else if (total <= 79)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightBlue; // 🔵 جيد
                    }
                    else if (total <= 89)
                    {
                        row.DefaultCellStyle.BackColor = Color.MediumPurple; // 🟣 جيد جداً
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen; // 🟢 امتياز
                    }
                }
        }

        private void guna2ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
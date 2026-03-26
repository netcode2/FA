using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace StudentAffairs
{
    public partial class LectureScheduleForm : Form
    {
        private int _userId;
        private string _fullName;
        public LectureScheduleForm(int userId, string fullName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            _userId = userId;
            _fullName = fullName;
            label1.Text = $"Welcome {fullName}";
            LoadSpecializations();
            LoadInstructors();
            LoadScheduleGrid();
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
        private void LoadStages()
        {
            if (!(guna2ComboBox1.SelectedValue is int specId))
                return;

            using (SqlConnection con = DB.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT StageID, StageName FROM Stages WHERE SpecializationID=@id", con);

                da.SelectCommand.Parameters.AddWithValue("@id", specId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox2.DataSource = dt;
                guna2ComboBox2.DisplayMember = "StageName";
                guna2ComboBox2.ValueMember = "StageID";
            }
        }
        private void LectureScheduleForm_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
            dateTimePicker2.Value = DateTime.Now;
            dateTimePicker1.Value = DateTime.Now.AddHours(1);
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

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadInstructors();
            LoadStages();
            
        }
        private void LoadSubjects()
        {
            if (!(guna2ComboBox2.SelectedValue is int stageId))
                return;

            using (SqlConnection con = DB.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT SubjectID, SubjectName FROM Subjects WHERE StageID=@id", con);

                da.SelectCommand.Parameters.AddWithValue("@id", stageId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox3.DataSource = dt;
                guna2ComboBox3.DisplayMember = "SubjectName";
                guna2ComboBox3.ValueMember = "SubjectID";
            }
        }
        private void LoadInstructors()
        {
            // تحقق أن هناك اختيار صالح في ComboBox1
            if (guna2ComboBox1.SelectedIndex == -1 || guna2ComboBox1.SelectedValue == null)
            {
                // نفضي ComboBox4 إذا لم يتم اختيار اختصاص
                guna2ComboBox4.DataSource = null;
                return;
            }

            int specializationId;
            if (!int.TryParse(guna2ComboBox1.SelectedValue.ToString(), out specializationId))
                return; // إذا لم يتم تحويل القيمة بنجاح نتوقف

            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT DISTINCT i.InstructorID, i.FullName
FROM Instructors i
JOIN InstructorSpecializations ispec ON i.InstructorID = ispec.InstructorID
WHERE ispec.SpecializationID = @spec
ORDER BY i.FullName";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@spec", specializationId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox4.DataSource = dt;
                guna2ComboBox4.DisplayMember = "FullName";
                guna2ComboBox4.ValueMember = "InstructorID";
            }
        }
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // التحقق من الإدخالات
            if (guna2ComboBox3.SelectedIndex == -1)
            {
                MessageBox.Show("❌ الرجاء اختيار المادة", "خطأ الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2ComboBox4.SelectedIndex == -1)
            {
                MessageBox.Show("❌ الرجاء اختيار الأستاذ", "خطأ الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2ComboBox5.SelectedIndex == -1 || string.IsNullOrWhiteSpace(guna2ComboBox5.Text))
            {
                MessageBox.Show("❌ الرجاء اختيار اليوم", "خطأ الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
            {
                MessageBox.Show("❌ الرجاء إدخال اسم القاعة", "خطأ الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int subjectId = (int)guna2ComboBox3.SelectedValue;
            int instructorId = (int)guna2ComboBox4.SelectedValue;
            string hall = guna2TextBox1.Text.Trim();
            string day = guna2ComboBox5.Text;

            TimeSpan start = dateTimePicker2.Value.TimeOfDay;
            TimeSpan end = dateTimePicker1.Value.TimeOfDay;

            if (start >= end)
            {
                MessageBox.Show("❌ وقت البداية يجب أن يكون قبل وقت النهاية", "خطأ الوقت", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // التحقق من التعارض
            string conflictMessage = HasScheduleConflict(instructorId, day, start, end);
            if (!string.IsNullOrEmpty(conflictMessage))
            {
                MessageBox.Show(conflictMessage, "تعارض جدول الأستاذ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();
                SqlTransaction tr = con.BeginTransaction();

                try
                {
                    // إدخال المحاضرة
                    SqlCommand cmdLecture = new SqlCommand(@"
INSERT INTO Lectures (SubjectID, InstructorID, Hall)
VALUES (@sub, @inst, @hall);
SELECT SCOPE_IDENTITY();", con, tr);

                    cmdLecture.Parameters.Add("@sub", SqlDbType.Int).Value = subjectId;
                    cmdLecture.Parameters.Add("@inst", SqlDbType.Int).Value = instructorId;
                    cmdLecture.Parameters.Add("@hall", SqlDbType.NVarChar).Value = hall;

                    int lectureId = Convert.ToInt32(cmdLecture.ExecuteScalar());

                    // إدخال جدول المحاضرة
                    SqlCommand cmdSchedule = new SqlCommand(@"
INSERT INTO LectureSchedule
(LectureID, DayOfWeek, StartTime, EndTime)
VALUES (@lec, @day, @start, @end)", con, tr);

                    cmdSchedule.Parameters.Add("@lec", SqlDbType.Int).Value = lectureId;
                    cmdSchedule.Parameters.Add("@day", SqlDbType.NVarChar).Value = day;
                    cmdSchedule.Parameters.Add("@start", SqlDbType.Time).Value = start;
                    cmdSchedule.Parameters.Add("@end", SqlDbType.Time).Value = end;

                    cmdSchedule.ExecuteNonQuery();

                    tr.Commit();

                    MessageBox.Show("✅ تم حفظ المحاضرة والجدول بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    MessageBox.Show("❌ حدث خطأ أثناء الحفظ: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            LoadScheduleGrid();
        }

        // دالة التحقق من التعارض بشكل احترافي
        private string HasScheduleConflict(int instructorId, string day, TimeSpan newStart, TimeSpan newEnd)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                string sql = @"
SELECT s.StartTime, s.EndTime, sub.SubjectName
FROM LectureSchedule s
JOIN Lectures l ON s.LectureID = l.LectureID
JOIN Subjects sub ON l.SubjectID = sub.SubjectID
WHERE l.InstructorID = @inst
AND s.DayOfWeek = @day
AND @newStart < s.EndTime
AND @newEnd > s.StartTime";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@inst", instructorId);
                cmd.Parameters.AddWithValue("@day", day);
                cmd.Parameters.AddWithValue("@newStart", newStart);
                cmd.Parameters.AddWithValue("@newEnd", newEnd);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    // تجميع كل المحاضرات المتعارضة في رسالة واحدة
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine("❌ يوجد تعارض مع المحاضرات التالية للأستاذ في نفس اليوم:");

                    while (reader.Read())
                    {
                        TimeSpan sTime = (TimeSpan)reader["StartTime"];
                        TimeSpan eTime = (TimeSpan)reader["EndTime"];
                        string subj = reader["SubjectName"].ToString();

                        sb.AppendLine($"- {subj}: {sTime:h\\:mm} - {eTime:h\\:mm}");
                    }

                    return sb.ToString();
                }

                return string.Empty; // لا يوجد تعارض
            }
        }
        private void LoadScheduleGrid()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
sc.ScheduleID,
sc.DayOfWeek,
s.SubjectName,
FORMAT(CAST(sc.StartTime AS datetime), 'h:mm tt') AS StartTime,
FORMAT(CAST(sc.EndTime AS datetime), 'h:mm tt') AS EndTime,
i.FullName,
l.Hall
FROM LectureSchedule sc
JOIN Lectures l ON sc.LectureID = l.LectureID
JOIN Subjects s ON l.SubjectID = s.SubjectID
JOIN Instructors i ON l.InstructorID = i.InstructorID
WHERE 1=1";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                // فلترة الاختصاص
                if (guna2ComboBox1.SelectedIndex > -1 && guna2ComboBox1.SelectedValue is int)
                {
                    sql += " AND s.SpecializationID = @spec";
                    cmd.Parameters.AddWithValue("@spec", (int)guna2ComboBox1.SelectedValue);
                }

                // فلترة المرحلة
                if (guna2ComboBox2.SelectedIndex > -1 && guna2ComboBox2.SelectedValue is int)
                {
                    sql += " AND s.StageID = @stage";
                    cmd.Parameters.AddWithValue("@stage", (int)guna2ComboBox2.SelectedValue);
                }

                // فلترة اليوم
                if (guna2ComboBox6.SelectedIndex > -1)
                {
                    sql += " AND sc.DayOfWeek = @day";
                    cmd.Parameters.AddWithValue("@day", guna2ComboBox6.Text); // أو SelectedValue حسب ComboBox3
                }

                sql += " ORDER BY sc.DayOfWeek, sc.StartTime";

                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2DataGridView1.DataSource = dt;

                // إعداد الأعمدة بالعربي
                guna2DataGridView1.Columns["ScheduleID"].Visible = false;
                guna2DataGridView1.Columns["SubjectName"].HeaderText = "المادة";
                guna2DataGridView1.Columns["FullName"].HeaderText = "المحاضر";
                guna2DataGridView1.Columns["DayOfWeek"].HeaderText = "اليوم";
                guna2DataGridView1.Columns["StartTime"].HeaderText = "وقت البداية";
                guna2DataGridView1.Columns["EndTime"].HeaderText = "وقت النهاية";
                guna2DataGridView1.Columns["Hall"].HeaderText = "القاعة";
            }
        }
        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadScheduleGrid();
            LoadSubjects();
        }

    
        int _selectedLectureId;
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (_selectedLectureId == 0)
            {
                MessageBox.Show("اختر محاضرة للتعديل");
                return;
            }

            int instructorId = (int)guna2ComboBox4.SelectedValue;
            int subjectId = (int)guna2ComboBox3.SelectedValue;
            string hall = guna2TextBox1.Text.Trim();
            string day = guna2ComboBox5.Text;

            TimeSpan start = dateTimePicker2.Value.TimeOfDay;
            TimeSpan end = dateTimePicker1.Value.TimeOfDay;

            if (start >= end)
            {
                MessageBox.Show("وقت غير صحيح");
                return;
            }

            if (HasConflictOnUpdate(
                _selectedLectureId,
                instructorId,
                day,
                start,
                end))
            {
                MessageBox.Show("❌ يوجد تعارض جدول");
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();
                SqlTransaction tr = con.BeginTransaction();

                try
                {
                    // تحديث المحاضرة
                    string lecSql = @"
UPDATE Lectures
SET SubjectID=@sub,
InstructorID=@inst,
Hall=@hall
WHERE LectureID=@id";

                    SqlCommand cmd1 =
                        new SqlCommand(lecSql, con, tr);

                    cmd1.Parameters.AddWithValue("@sub", subjectId);
                    cmd1.Parameters.AddWithValue("@inst", instructorId);
                    cmd1.Parameters.AddWithValue("@hall", hall);
                    cmd1.Parameters.AddWithValue("@id", _selectedLectureId);

                    cmd1.ExecuteNonQuery();

                    // تحديث الجدول
                    string schSql = @"
UPDATE LectureSchedule
SET DayOfWeek=@day,
StartTime=@st,
EndTime=@en
WHERE LectureID=@id";

                    SqlCommand cmd2 =
                        new SqlCommand(schSql, con, tr);

                    cmd2.Parameters.AddWithValue("@day", day);
                    cmd2.Parameters.AddWithValue("@st", start);
                    cmd2.Parameters.AddWithValue("@en", end);
                    cmd2.Parameters.AddWithValue("@id", _selectedLectureId);

                    cmd2.ExecuteNonQuery();

                    tr.Commit();

                    MessageBox.Show("✅ تم التعديل");

                    LoadScheduleGrid();
                }
                catch
                {
                    tr.Rollback();
                    throw;
                }
            }
        }
        private bool HasConflictOnUpdate(
 int lectureId,
 int instructorId,
 string day,
 TimeSpan newStart,
 TimeSpan newEnd)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                string sql = @"
SELECT COUNT(*)
FROM LectureSchedule sc
JOIN Lectures l ON sc.LectureID = l.LectureID
WHERE l.InstructorID = @inst
AND sc.DayOfWeek = @day
AND sc.LectureID <> @lecId
AND @newStart < sc.EndTime
AND @newEnd > sc.StartTime";

                SqlCommand cmd = new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@inst", instructorId);
                cmd.Parameters.AddWithValue("@day", day);
                cmd.Parameters.AddWithValue("@lecId", lectureId);
                cmd.Parameters.AddWithValue("@newStart", newStart);
                cmd.Parameters.AddWithValue("@newEnd", newEnd);

                return (int)cmd.ExecuteScalar() > 0;
            }
        }
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (_selectedLectureId == 0)
            {
                MessageBox.Show("اختر محاضرة للحذف");
                return;
            }

            if (MessageBox.Show(
                "هل تريد حذف المحاضرة؟",
                "تأكيد",
                MessageBoxButtons.YesNo)
                != DialogResult.Yes)
                return;

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();
                SqlTransaction tr = con.BeginTransaction();

                try
                {
                    SqlCommand cmd1 = new SqlCommand(
                        "DELETE FROM LectureSchedule WHERE LectureID=@id",
                        con, tr);

                    cmd1.Parameters.AddWithValue("@id", _selectedLectureId);
                    cmd1.ExecuteNonQuery();

                    SqlCommand cmd2 = new SqlCommand(
                        "DELETE FROM Lectures WHERE LectureID=@id",
                        con, tr);

                    cmd2.Parameters.AddWithValue("@id", _selectedLectureId);
                    cmd2.ExecuteNonQuery();

                    tr.Commit();

                    MessageBox.Show("🗑 تم الحذف");

                    LoadScheduleGrid();
                }
                catch
                {
                    tr.Rollback();
                    throw;
                }
            }
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // تجاهل رأس الجدول

            var row = guna2DataGridView1.Rows[e.RowIndex];

            // حفظ ScheduleID للمعالجة لاحقًا (تحديث أو حذف)
            _selectedLectureId = Convert.ToInt32(row.Cells["ScheduleID"].Value);

            // تعبئة الأدوات بالقيم
            // المادة
            if (row.Cells["SubjectName"].Value != null)
            {
                string subjName = row.Cells["SubjectName"].Value.ToString();
                guna2ComboBox3.SelectedIndex = guna2ComboBox3.FindStringExact(subjName);
            }

            // الأستاذ
            if (row.Cells["FullName"].Value != null)
            {
                string instName = row.Cells["FullName"].Value.ToString();
                guna2ComboBox4.SelectedIndex = guna2ComboBox4.FindStringExact(instName);
            }

            // اليوم
            if (row.Cells["DayOfWeek"].Value != null)
            {
                string day = row.Cells["DayOfWeek"].Value.ToString();
                guna2ComboBox5.SelectedIndex = guna2ComboBox5.FindStringExact(day);
            }


            if (guna2DataGridView1.Columns.Contains("Hall") && row.Cells["Hall"].Value != null)
            {
                guna2TextBox1.Text = row.Cells["Hall"].Value.ToString();
            }
            else
            {
                guna2TextBox1.Text = "";
            }

            // وقت البداية
            if (row.Cells["StartTime"].Value != null)
            {
                DateTime dtStart;
                if (DateTime.TryParse(row.Cells["StartTime"].Value.ToString(), out dtStart))
                {
                    dateTimePicker2.Value = DateTime.Today.Add(dtStart.TimeOfDay); // الحفاظ على الوقت فقط
                }
            }

            // وقت النهاية
            if (row.Cells["EndTime"].Value != null)
            {
                DateTime dtEnd;
                if (DateTime.TryParse(row.Cells["EndTime"].Value.ToString(), out dtEnd))
                {
                    dateTimePicker1.Value = DateTime.Today.Add(dtEnd.TimeOfDay);
                }
            }
        }
        private void guna2ComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBox6.SelectedIndex > -1)
            {
                // ننسخ القيمة مباشرة إلى ComboBox6
                guna2ComboBox5.Text = guna2ComboBox6.Text;
                if (guna2ComboBox1.SelectedIndex > -1 && guna2ComboBox2.SelectedIndex > -1)
                {
                    LoadScheduleGrid();
                }
            }
        }

        private void guna2ComboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBox5.SelectedIndex > -1)
            {
                // ننسخ القيمة مباشرة إلى ComboBox6
                guna2ComboBox6.Text = guna2ComboBox5.Text;

                // إذا تريد تحديث الجدول بعد النسخ
                if (guna2ComboBox1.SelectedIndex > -1 && guna2ComboBox2.SelectedIndex > -1)
                {
                    LoadScheduleGrid();
                }
            }
        }
    }
}

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
using System.Drawing.Printing;

namespace StudentAffairs
{

    public partial class GoToStage : Form
    {
        private int _userId;
        private string _fullName;
        public GoToStage(int userId, string fullName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            _userId = userId;
            _fullName = fullName;
            label1.Text = $"Welcome {fullName}";
            LoadSpecializations();
            RegisterEvents();
            StyleDataGridView(guna2DataGridView1);
            StyleDataGridView(guna2DataGridView2);
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
        private void RegisterEvents()
        {
            guna2ComboBox4.SelectionChangeCommitted += (s, e) => LoadStages();
            guna2ComboBox2.SelectionChangeCommitted += (s, e) => LoadStudentsForPromotion();



        }
        private void LoadStudentsForPromotion()
        {
            if (guna2ComboBox2.SelectedValue == null ||
                guna2ComboBox4.SelectedValue == null)
                return;

            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
    s.StudentID,
    s.FullName AS [الاسم],
    s.StudentType AS [النوع],
    s.StageID,
    s.SpecializationID,

    -- ✅ المعدل الحقيقي
    AVG(ISNULL(g.Midterm,0) + ISNULL(g.Final,0)) AS [المعدل],

    -- ✅ التحقق من النجاح او المكمل
    CASE 
        WHEN COUNT(g.SubjectID) = 0 THEN N'لم يمتحن'

        WHEN SUM(
            CASE 
                WHEN (ISNULL(g.Midterm,0) + ISNULL(g.Final,0)) < 50 THEN 1 
                ELSE 0 
            END
        ) > 0 THEN N'مكمل'

        ELSE N'ناجح'
    END AS [الحالة]

FROM
(
    SELECT IraqStudentID AS StudentID, FullName, StageID, SpecializationID, N'عراقي' AS StudentType
    FROM Students_Iraq

    UNION ALL

    SELECT ForeignStudentID AS StudentID, FullName, StageID, SpecializationID, N'أجنبي' AS StudentType
    FROM Students_Foreign
) s

LEFT JOIN StudentGrades g
ON g.StudentID = s.StudentID
AND g.StudentType = s.StudentType

WHERE s.StageID = @stage
AND s.SpecializationID = @spec

GROUP BY 
    s.StudentID, 
    s.FullName, 
    s.StudentType, 
    s.StageID, 
    s.SpecializationID

ORDER BY s.FullName";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                da.SelectCommand.Parameters.AddWithValue("@stage", guna2ComboBox2.SelectedValue);
                da.SelectCommand.Parameters.AddWithValue("@spec", guna2ComboBox4.SelectedValue);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2DataGridView1.DataSource = dt;

                // إخفاء الأعمدة
                guna2DataGridView1.Columns["StudentID"].Visible = false;
                guna2DataGridView1.Columns["StageID"].Visible = false;
                guna2DataGridView1.Columns["SpecializationID"].Visible = false;

                // تعريب
                guna2DataGridView1.Columns["المعدل"].HeaderText = "المعدل";
                guna2DataGridView1.Columns["الحالة"].HeaderText = "النتيجة";

                // 🎨 تلوين
                foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    string status = row.Cells["الحالة"].Value.ToString();

                    if (status == "مكمل")
                        row.DefaultCellStyle.BackColor = Color.LightCoral; // 🔴
                    else if (status == "ناجح")
                        row.DefaultCellStyle.BackColor = Color.LightGreen; // 🟢
                    else
                        row.DefaultCellStyle.BackColor = Color.LightYellow; // 🟡
                }
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
        private void GoToStage_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
            pd.PrintPage += Pd_PrintPage;
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
        private void LoadStudentSubjects(int studentId, string type)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
    sb.SubjectName AS [المادة],
    g.Midterm AS [السعي],
    g.Final AS [النهائي],

    ISNULL(g.Midterm,0) + ISNULL(g.Final,0) AS [المجموع],

    CASE 
        WHEN g.Midterm IS NULL OR g.Final IS NULL THEN N'غير مكتمل'
        WHEN (g.Midterm + g.Final) < 50 THEN N'ضعيف'
        WHEN (g.Midterm + g.Final) BETWEEN 50 AND 59 THEN N'مقبول'
        WHEN (g.Midterm + g.Final) BETWEEN 60 AND 69 THEN N'متوسط'
        WHEN (g.Midterm + g.Final) BETWEEN 70 AND 79 THEN N'جيد'
        WHEN (g.Midterm + g.Final) BETWEEN 80 AND 89 THEN N'جيد جداً'
        ELSE N'امتياز'
    END AS [التقدير]

FROM StudentGrades g
JOIN Subjects sb ON g.SubjectID = sb.SubjectID

WHERE g.StudentID = @id
AND g.StudentType = @type

ORDER BY sb.SubjectName";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                da.SelectCommand.Parameters.AddWithValue("@id", studentId);
                da.SelectCommand.Parameters.AddWithValue("@type", type);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2DataGridView2.DataSource = dt;

                // 🔥 تنسيق
                guna2DataGridView2.Columns["المادة"].HeaderText = "المادة";
                guna2DataGridView2.Columns["السعي"].HeaderText = "السعي";
                guna2DataGridView2.Columns["النهائي"].HeaderText = "النهائي";
                guna2DataGridView2.Columns["المجموع"].HeaderText = "المجموع";
                guna2DataGridView2.Columns["التقدير"].HeaderText = "التقدير";

                // 🎨 تلوين
                foreach (DataGridViewRow row in guna2DataGridView2.Rows)
                {
                    if (row.IsNewRow) continue;

                    if (row.Cells["السعي"].Value == DBNull.Value ||
                        row.Cells["النهائي"].Value == DBNull.Value)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        continue;
                    }

                    decimal total = Convert.ToDecimal(row.Cells["المجموع"].Value);

                    if (total < 50)
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                    else if (total < 60)
                        row.DefaultCellStyle.BackColor = Color.Orange;
                    else if (total < 70)
                        row.DefaultCellStyle.BackColor = Color.Khaki;
                    else if (total < 80)
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                    else if (total < 90)
                        row.DefaultCellStyle.BackColor = Color.MediumPurple;
                    else
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                }
            }
        }
        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2DataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int studentId = Convert.ToInt32(guna2DataGridView1.Rows[e.RowIndex].Cells["StudentID"].Value);
            string type = guna2DataGridView1.Rows[e.RowIndex].Cells["النوع"].Value.ToString();
            guna2TextBox1.Text = guna2DataGridView1.Rows[e.RowIndex].Cells["الاسم"].Value.ToString();
            guna2TextBox3.Text = guna2ComboBox4.Text;
            guna2TextBox4.Text = guna2ComboBox2.Text;
            string DC = guna2DataGridView1.Rows[e.RowIndex].Cells["الحالة"].Value.ToString();
            if (DC == "ناجح")
            {
                guna2TextBox5.Text = "ناجح";
                guna2TextBox5.ForeColor = Color.Green;
            }
            else
            {
                guna2TextBox5.Text = "مكمل";
                guna2TextBox5.ForeColor = Color.Red;
            }
            LoadStudentSubjects(studentId, type);
        }
        private bool IsStudentPassedAllSubjects(int studentId, string type)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT COUNT(*) 
FROM StudentGrades
WHERE StudentID = @id 
AND StudentType = @type
AND (Midterm IS NULL OR Final IS NULL OR (Midterm + Final) < 50)";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@type", type);

                con.Open();
                int count = (int)cmd.ExecuteScalar();

                return count == 0; // ✅ يعني ناجح بكل المواد
            }
        }

        private void RemoveOldSubjects(int studentId, string type)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
DELETE FROM StudentGrades
WHERE StudentID=@id AND StudentType=@type";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@type", type);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private void AssignSubjectsToStudent(int studentId, string type, int stageId)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                // 🔹 نجيب مواد المرحلة
                SqlCommand cmdSubjects = new SqlCommand(@"
SELECT SubjectID 
FROM Subjects
WHERE StageID = @stage", con);

                cmdSubjects.Parameters.AddWithValue("@stage", stageId);

                SqlDataReader dr = cmdSubjects.ExecuteReader();

                List<int> subjects = new List<int>();

                while (dr.Read())
                {
                    subjects.Add((int)dr["SubjectID"]);
                }

                dr.Close();

                // 🔹 نضيف المواد للطالب
                foreach (int subId in subjects)
                {
                    SqlCommand cmdInsert = new SqlCommand(@"
IF NOT EXISTS (
    SELECT 1 FROM StudentGrades 
    WHERE StudentID=@st AND SubjectID=@sub AND StudentType=@type
)
INSERT INTO StudentGrades
(StudentID, StudentType, SubjectID)
VALUES(@st,@type,@sub)", con);

                    cmdInsert.Parameters.AddWithValue("@st", studentId);
                    cmdInsert.Parameters.AddWithValue("@type", type);
                    cmdInsert.Parameters.AddWithValue("@sub", subId);

                    cmdInsert.ExecuteNonQuery();
                }
            }
        }
        private int GetNextStage(int currentStage, int specId)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT TOP 1 StageID
FROM Stages
WHERE SpecializationID = @spec
AND StageID > @current
ORDER BY StageID";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@spec", specId);
                cmd.Parameters.AddWithValue("@current", currentStage);

                con.Open();

                var result = cmd.ExecuteScalar();

                if (result == null)
                    return currentStage; // ماكو مرحلة بعدها

                return Convert.ToInt32(result);
            }
        }
        private int GetSpecializationByAverage(decimal avg)
        {
            if (avg >= 90) return 7; // CY
            if (avg >= 70) return 5; // CS
            if (avg >= 50) return 6; // IT

            return 6; // احتياط (لو اقل من 50 ما ينقبل اصلا)
        }
        private string BackupStudentData(int studentId, string type)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT SubjectID, Midterm, Final
FROM StudentGrades
WHERE StudentID=@id AND StudentType=@type";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@type", type);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                string data = "";

                while (dr.Read())
                {
                    data += dr["SubjectID"] + "," +
                            (dr["Midterm"] == DBNull.Value ? "0" : dr["Midterm"].ToString()) + "," +
                            (dr["Final"] == DBNull.Value ? "0" : dr["Final"].ToString()) + ";";
                }

                return data;
            }
        }
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    int studentId = Convert.ToInt32(row.Cells["StudentID"].Value);
                    string type = row.Cells["النوع"].Value.ToString();

                    int oldStage = Convert.ToInt32(row.Cells["StageID"].Value);
                    int oldSpec = Convert.ToInt32(row.Cells["SpecializationID"].Value);

                    decimal avg = Convert.ToDecimal(row.Cells["المعدل"].Value);

                    // ❌ شرط النجاح
                    if (!IsStudentPassedAllSubjects(studentId, type))
                        continue;

                    // 🔹 نسخ احتياطي
                    string backup = BackupStudentData(studentId, type);

                    int newStage = GetNextStage(oldStage, oldSpec);
                    int newSpec = oldSpec;

                    // 🔥 إذا كان General مرحلة ثانية → توزيع
                    if (oldSpec == 9 && oldStage == 77)
                    {
                        newSpec = GetSpecializationByAverage(avg);
                        newStage = GetNextStage(2, newSpec); // يبدي مرحلة 3
                    }

                    string table = type == "عراقي" ? "Students_Iraq" : "Students_Foreign";
                    string idCol = type == "عراقي" ? "IraqStudentID" : "ForeignStudentID";

                    // 🔹 تحديث الطالب
                    SqlCommand update = new SqlCommand($@"
UPDATE {table}
SET StageID=@newStage, SpecializationID=@newSpec
WHERE {idCol}=@id", con);

                    update.Parameters.AddWithValue("@newStage", newStage);
                    update.Parameters.AddWithValue("@newSpec", newSpec);
                    update.Parameters.AddWithValue("@id", studentId);

                    update.ExecuteNonQuery();

                    // ❌ حذف القديم
                    RemoveOldSubjects(studentId, type);

                    // ✅ ربط الجديد
                    AssignSubjectsToStudent(studentId, type, newStage);

                    // 🔥 تسجيل العملية
                    SqlCommand log = new SqlCommand(@"
INSERT INTO PromotionLog
(StudentID, StudentType, OldStageID, OldSpecID, NewStageID, NewSpecID, BackupData, ActionDate)
VALUES (@id,@type,@oldStage,@oldSpec,@newStage,@newSpec,@backup,GETDATE())", con);

                    log.Parameters.AddWithValue("@id", studentId);
                    log.Parameters.AddWithValue("@type", type);
                    log.Parameters.AddWithValue("@oldStage", oldStage);
                    log.Parameters.AddWithValue("@oldSpec", oldSpec);
                    log.Parameters.AddWithValue("@newStage", newStage);
                    log.Parameters.AddWithValue("@newSpec", newSpec);
                    log.Parameters.AddWithValue("@backup", backup);

                    log.ExecuteNonQuery();
                }
            }

            MessageBox.Show("✅ تم الترحيل بنجاح");
            LoadStudentsForPromotion();
        }
    
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();

                foreach (DataGridViewRow row in guna2DataGridView1.SelectedRows)
                {
                    int studentId = Convert.ToInt32(row.Cells["StudentID"].Value);
                    string type = row.Cells["النوع"].Value.ToString();

                    // 🔹 نجيب اخر ترحيل
                    SqlCommand getLog = new SqlCommand(@"
SELECT TOP 1 *
FROM PromotionLog
WHERE StudentID=@id AND StudentType=@type
ORDER BY LogID DESC", con);

                    getLog.Parameters.AddWithValue("@id", studentId);
                    getLog.Parameters.AddWithValue("@type", type);

                    SqlDataReader dr = getLog.ExecuteReader();

                    if (!dr.Read())
                    {
                        dr.Close();
                        continue;
                    }

                    int oldStage = (int)dr["OldStageID"];
                    int oldSpec = (int)dr["OldSpecID"];
                    string backup = dr["BackupData"].ToString();

                    dr.Close();

                    string table = type == "عراقي" ? "Students_Iraq" : "Students_Foreign";
                    string idCol = type == "عراقي" ? "IraqStudentID" : "ForeignStudentID";

                    // 🔹 رجوع المرحلة
                    SqlCommand update = new SqlCommand($@"
UPDATE {table}
SET StageID=@stage, SpecializationID=@spec
WHERE {idCol}=@id", con);

                    update.Parameters.AddWithValue("@stage", oldStage);
                    update.Parameters.AddWithValue("@spec", oldSpec);
                    update.Parameters.AddWithValue("@id", studentId);

                    update.ExecuteNonQuery();

                    // ❌ حذف الحالي
                    RemoveOldSubjects(studentId, type);

                    // 🔥 استرجاع الدرجات
                    string[] records = backup.Split(';');

                    foreach (string rec in records)
                    {
                        if (string.IsNullOrWhiteSpace(rec)) continue;

                        var parts = rec.Split(',');

                        int subId = int.Parse(parts[0]);
                        decimal mid = decimal.Parse(parts[1]);
                        decimal fin = decimal.Parse(parts[2]);

                        SqlCommand insert = new SqlCommand(@"
INSERT INTO StudentGrades
(StudentID, StudentType, SubjectID, Midterm, Final)
VALUES(@id,@type,@sub,@mid,@fin)", con);

                        insert.Parameters.AddWithValue("@id", studentId);
                        insert.Parameters.AddWithValue("@type", type);
                        insert.Parameters.AddWithValue("@sub", subId);
                        insert.Parameters.AddWithValue("@mid", mid);
                        insert.Parameters.AddWithValue("@fin", fin);

                        insert.ExecuteNonQuery();
                    }

                    // 🔹 حذف السجل
                    SqlCommand deleteLog = new SqlCommand(@"
DELETE FROM PromotionLog
WHERE LogID = (
    SELECT TOP 1 LogID 
    FROM PromotionLog 
    WHERE StudentID=@id AND StudentType=@type 
    ORDER BY LogID DESC)", con);

                    deleteLog.Parameters.AddWithValue("@id", studentId);
                    deleteLog.Parameters.AddWithValue("@type", type);

                    deleteLog.ExecuteNonQuery();
                }
            }

            MessageBox.Show("↩️ تم التراجع عن الترحيل");
            LoadStudentsForPromotion();
        }
        PrintDocument pd = new PrintDocument();
        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            // 🔥 خطوط احتراف
            Font titleFont = new Font("Segoe UI", 20, FontStyle.Bold);
            Font subTitleFont = new Font("Segoe UI", 12, FontStyle.Regular);
            Font normalFont = new Font("Segoe UI", 10);
            Font boldFont = new Font("Segoe UI", 10, FontStyle.Bold);

            int pageWidth = e.MarginBounds.Width;
            int startX = e.MarginBounds.Left;
            int y = 40;

            // RTL
            StringFormat right = new StringFormat()
            {
                FormatFlags = StringFormatFlags.DirectionRightToLeft,
                Alignment = StringAlignment.Near
            };

            StringFormat center = new StringFormat()
            {
                Alignment = StringAlignment.Center
            };

            // 🎨 ألوان ناعمة
            Brush primary = new SolidBrush(Color.FromArgb(30, 30, 30));
            Brush lightGray = new SolidBrush(Color.FromArgb(245, 245, 245));
            Brush headerColor = new SolidBrush(Color.FromArgb(220, 230, 241));

            // ================== HEADER ==================
            g.DrawString("الجامعة المستنصرية", titleFont, primary, startX + pageWidth / 2, y, center);
            y += 30;

            g.DrawString("كلية العلوم - قسم علوم الحاسوب", subTitleFont, Brushes.Gray, startX + pageWidth / 2, y, center);
            y += 25;

            g.DrawLine(new Pen(Color.Gray, 1), startX + 50, y, startX + pageWidth - 50, y);
            y += 25;

            // ================== CARD (معلومات الطالب) ==================
            int cardPadding = 15;
            int rowHeight = 35;

            Rectangle card = new Rectangle(startX, y, pageWidth, 120);
            g.FillRectangle(lightGray, card);
            g.DrawRectangle(new Pen(Color.LightGray, 1), card);

            int insideY = y + cardPadding;

            g.DrawString("الاسم: " + guna2TextBox1.Text, boldFont, primary,
                startX + pageWidth - 15, insideY, right);
            insideY += 25;

            g.DrawString("الاختصاص: " + guna2TextBox3.Text, normalFont, primary,
                startX + pageWidth - 15, insideY, right);
            insideY += 25;

            g.DrawString("المرحلة: " + guna2TextBox4.Text, normalFont, primary,
                startX + pageWidth - 15, insideY, right);
            insideY += 25;

            Brush resultColor = guna2TextBox5.Text.Contains("ناجح")
                ? new SolidBrush(Color.Green)
                : new SolidBrush(Color.Red);

            g.DrawString("النتيجة: " + guna2TextBox5.Text, boldFont, resultColor,
                startX + pageWidth - 15, insideY, right);

            y += 140;

            // ================== TABLE ==================
            int colSubject = (int)(pageWidth * 0.7);
            int colGrade = (int)(pageWidth * 0.3);

            // 🔹 HEADER
            g.DrawRectangle(Pens.LightGray, startX + colGrade, y, colSubject, 35);

            // التقدير (يسار)
            g.DrawRectangle(Pens.LightGray, startX, y, colGrade, 35);

            g.DrawString("المادة", boldFont, primary,
                startX + pageWidth - 10, y + 8, right);

            g.DrawString("التقدير", boldFont, primary,
                startX + colGrade - 10, y + 8, right);

            y += 35;

            bool alternate = false;

            // 🔹 ROWS
            foreach (DataGridViewRow row in guna2DataGridView2.Rows)
            {
                if (row.IsNewRow) continue;

                string subject = row.Cells["المادة"].Value?.ToString();
                string grade = row.Cells["التقدير"].Value?.ToString();

                // 🔥 alternating rows (ستايل CSS)
                Brush rowColor = alternate ? Brushes.White : lightGray;
                g.FillRectangle(rowColor, startX, y, pageWidth, 30);

                g.DrawRectangle(Pens.LightGray, startX + colGrade, y, colSubject, 30);

                // التقدير (يسار)
                g.DrawRectangle(Pens.LightGray, startX, y, colGrade, 30);

                g.DrawString(subject, normalFont, primary,
                    startX + pageWidth - 10, y + 7, right);

                g.DrawString(grade, normalFont, primary,
                    startX + colGrade - 10, y + 7, right);

                alternate = !alternate;
                y += 30;
            }

            y += 30;

            // ================== FOOTER ==================
            g.DrawLine(Pens.Gray, startX + 50, y, startX + pageWidth - 50, y);
            y += 10;

            g.DrawString("تاريخ الطباعة: " + DateTime.Now.ToString("yyyy/MM/dd"),
                normalFont, Brushes.Gray,
                startX + pageWidth - 10, y, right);
        }
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = pd;

            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169); // A4
            pd.DefaultPageSettings.Landscape = false; // عمودي
            preview.WindowState = FormWindowState.Maximized;
            preview.ShowDialog();
        }
    }
}

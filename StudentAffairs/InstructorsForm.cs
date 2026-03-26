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
    public partial class InstructorsForm : Form
    {
        private int _userId;
        private string _fullName;
        public InstructorsForm(int userId, string fullName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            _userId = userId;
            _fullName = fullName;
            label1.Text = $"Welcome {fullName}";
           
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
            try
            {
                checkedListBox1.Items.Clear();

                using (SqlConnection con = DB.GetConnection())
                {
                    con.Open();
                    string sql = "SELECT SpecializationID, SpecializationName FROM Specializations ORDER BY SpecializationName";

                    SqlCommand cmd = new SqlCommand(sql, con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        // نخزن ID داخل العنصر نفسه
                        checkedListBox1.Items.Add(
                            new ComboBoxItem(
                                reader["SpecializationName"].ToString(),
                                Convert.ToInt32(reader["SpecializationID"])
                            )
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ بجلب الاختصاصات:\n" + ex.Message);
            }
        }
        private void LoadInstructors()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
    i.InstructorID,
    i.FullName,
    i.Phone,
    i.Email,
i.StartDate,
i.Gender,
    STUFF((
        SELECT ' , ' + s.SpecializationName
        FROM InstructorSpecializations is2
        INNER JOIN Specializations s 
            ON is2.SpecializationID = s.SpecializationID
        WHERE is2.InstructorID = i.InstructorID
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 3, '') AS Specializations
FROM Instructors i
ORDER BY i.FullName";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2DataGridView1.DataSource = dt;

                guna2DataGridView1.Columns["InstructorID"].Visible = false;

                guna2DataGridView1.Columns["FullName"].HeaderText = "اسم الأستاذ";
                guna2DataGridView1.Columns["Phone"].HeaderText = "رقم الهاتف";
                guna2DataGridView1.Columns["Email"].HeaderText = "البريد الإلكتروني";
                guna2DataGridView1.Columns["Specializations"].HeaderText = "الاختصاصات";
                guna2DataGridView1.Columns["Gender"].HeaderText = "الجنس";
                guna2DataGridView1.Columns["StartDate"].HeaderText = "تاريخ المباشرة";
            }
        }

        private void InstructorsForm_Load(object sender, EventArgs e)
        {
            LoadSpecializations();
            LoadInstructors();
            StyleDataGridView(guna2DataGridView1);
            guna2DateTimePicker1.Format = DateTimePickerFormat.Custom;
            guna2DateTimePicker1.CustomFormat = "yyyy/MM/dd";
            guna2DateTimePicker1.Value = DateTime.Now;

            guna2ComboBox2.Items.Clear();
            guna2ComboBox2.Items.Add("قم بتحديد الجنس");
            guna2ComboBox2.Items.AddRange(new string[] { "ذكر", "انثى" });
            guna2ComboBox2.SelectedIndex = 0; // افتراضي فارغ
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private void guna2ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
        private int _selectedInstructorId = 0;
        private void guna2DataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {

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
            // ======================
            // 1️⃣ تحقق من الإدخال
            // ======================
            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
            {
                MessageBox.Show("⚠ الرجاء إدخال اسم الأستاذ", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2TextBox1.Focus();
                return;
            }
            if (guna2ComboBox2.SelectedIndex==0)
            {
                MessageBox.Show("⚠ الرجاء تحديد الجنس  ", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2ComboBox2.Focus();
                return;
            }
            if (checkedListBox1.CheckedItems.Count == 0)
            {
                MessageBox.Show("⚠ الرجاء اختيار اختصاص واحد على الأقل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = DB.GetConnection())
                {
                    con.Open();

                    // ======================
                    // 2️⃣ بدء معاملة (Transaction)
                    // ======================
                    SqlTransaction tran = con.BeginTransaction();

                    try
                    {
                        // ======================
                        // 3️⃣ منع التكرار
                        // ======================
                        string checkSql = "SELECT COUNT(*) FROM Instructors WHERE FullName = @Name";
                        using (SqlCommand checkCmd = new SqlCommand(checkSql, con, tran))
                        {
                            checkCmd.Parameters.AddWithValue("@Name", guna2TextBox1.Text.Trim());
                            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (exists > 0)
                            {
                                MessageBox.Show("⚠ هذا الأستاذ مسجل مسبقاً", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                tran.Rollback();
                                return;
                            }
                        }

                        // ======================
                        // 4️⃣ إدخال الأستاذ
                        // ======================
                        string insertInstructorSql = @"
INSERT INTO Instructors (FullName, Phone, Email,Gender,StartDate, UserID)
VALUES (@FullName, @Phone, @Email,@Gender,@StartDate, @UserID);
SELECT SCOPE_IDENTITY();";

                        int newInstructorId;
                        using (SqlCommand cmd = new SqlCommand(insertInstructorSql, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@FullName", guna2TextBox1.Text.Trim());
                            cmd.Parameters.AddWithValue("@Phone", guna2TextBox2.Text.Trim());
                            cmd.Parameters.AddWithValue("@Email", guna2TextBox3.Text.Trim());
                            cmd.Parameters.AddWithValue("@UserID", _userId);
                            cmd.Parameters.AddWithValue("@Gender", guna2ComboBox2.Text.Trim());
                            cmd.Parameters.AddWithValue("@StartDate", guna2DateTimePicker1.Value);
                            newInstructorId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // ======================
                        // 5️⃣ إدخال الاختصاصات
                        // ======================
                        foreach (var obj in checkedListBox1.CheckedItems)
                        {
                            ComboBoxItem item = obj as ComboBoxItem;
                            if (item == null) continue;

                            string insertSpecSql = @"
INSERT INTO InstructorSpecializations (InstructorID, SpecializationID)
VALUES (@InstructorID, @SpecID)";

                            using (SqlCommand cmdSpec = new SqlCommand(insertSpecSql, con, tran))
                            {
                                cmdSpec.Parameters.AddWithValue("@InstructorID", newInstructorId);
                                cmdSpec.Parameters.AddWithValue("@SpecID", item.Value);
                                cmdSpec.ExecuteNonQuery();
                            }
                        }

                        // ======================
                        // 6️⃣ تأكيد الحفظ
                        // ======================
                        tran.Commit();

                        MessageBox.Show(
                            $"✅ تم حفظ الأستاذ بنجاح!\n\n" +
                            $"الاسم: {guna2TextBox1.Text}\n" +
                            $"عدد الاختصاصات: {checkedListBox1.CheckedItems.Count}",
                            "نجاح",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        // ======================
                        // 7️⃣ تحديث الواجهة
                        // ======================
                        LoadInstructors();
                        ClearForm();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ حدث خطأ أثناء الحفظ:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void ClearForm()
        {
            guna2TextBox1.Clear();
            guna2TextBox2.Clear();
            guna2TextBox3.Clear();
            guna2ComboBox2.SelectedIndex = 0;
            guna2DateTimePicker1.Value = DateTime.Now;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }

            guna2TextBox1.Focus();
        }
        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            _selectedInstructorId = Convert.ToInt32(
                guna2DataGridView1.Rows[e.RowIndex].Cells["InstructorID"].Value
            );

            LoadInstructorToForm(_selectedInstructorId);
        }
        private void LoadInstructorToForm(int instructorId)
        {
            try
            {
                using (SqlConnection con = DB.GetConnection())
                {
                    con.Open();

                    // ==========================
                    // 1️⃣ جلب بيانات الأستاذ
                    // ==========================
                    string sqlInstructor = @"
SELECT FullName, Phone, Email,Gender,StartDate
FROM Instructors
WHERE InstructorID = @ID";

                    using (SqlCommand cmd = new SqlCommand(sqlInstructor, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", instructorId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                               guna2TextBox1.Text = dr["FullName"].ToString();
                                guna2TextBox2.Text = dr["Phone"].ToString();
                                guna2TextBox3.Text = dr["Email"].ToString();
                                guna2ComboBox2.Text = dr["Gender"].ToString();
                                guna2DateTimePicker1.Text = dr["StartDate"].ToString();

                            }
                        }
                    }

                    // ==========================
                    // 2️⃣ تنظيف الاختصاصات
                    // ==========================
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                        checkedListBox1.SetItemChecked(i, false);
                    }

                    // ==========================
                    // 3️⃣ جلب اختصاصات الأستاذ
                    // ==========================
                    string sqlSpecs = @"
SELECT SpecializationID
FROM InstructorSpecializations
WHERE InstructorID = @ID";

                    using (SqlCommand cmd = new SqlCommand(sqlSpecs, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", instructorId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            List<int> specIds = new List<int>();

                            while (dr.Read())
                            {
                                specIds.Add(Convert.ToInt32(dr["SpecializationID"]));
                            }

                            // ==========================
                            // 4️⃣ تعليم الاختصاصات في الليست
                            // ==========================
                            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                            {
                                ComboBoxItem item = checkedListBox1.Items[i] as ComboBoxItem;
                                if (item != null && specIds.Contains(item.Value))
                                {
                                    checkedListBox1.SetItemChecked(i, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء تحميل بيانات الأستاذ:\n" + ex.Message, "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2DateTimePicker2_ValueChanged(object sender, EventArgs e)
        {

        }
    
        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
            // 1️⃣ تحقق من اختيار أستاذ
            if (_selectedInstructorId == 0)
            {
                MessageBox.Show("⚠ الرجاء اختيار أستاذ من الجدول أولاً", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (guna2ComboBox2.SelectedIndex == 0)
            {
                MessageBox.Show("⚠ الرجاء تحديد الجنس  ", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2ComboBox2.Focus();
                return;
            }
            // 2️⃣ تحقق من الاسم
            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
            {
                MessageBox.Show("⚠ الرجاء إدخال اسم الأستاذ", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2TextBox1.Focus();
                return;
            }

            // 3️⃣ تحقق من اختيار اختصاص واحد على الأقل
            if (checkedListBox1.CheckedItems.Count == 0)
            {
                MessageBox.Show("⚠ الرجاء اختيار اختصاص واحد على الأقل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4️⃣ تأكيد
            if (MessageBox.Show("هل أنت متأكد من تعديل بيانات الأستاذ؟",
                "تأكيد التعديل",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection con = DB.GetConnection())
                {
                    con.Open();
                    SqlTransaction trans = con.BeginTransaction();

                    try
                    {
                        // ===============================
                        // 5️⃣ تحديث بيانات الأستاذ
                        // ===============================
                        string sqlUpdate = @"
UPDATE Instructors
SET FullName = @FullName,
    Phone = @Phone,
    Email = @Email,
StartDate=@StartDate,
Gender=@Gender
WHERE InstructorID = @ID";

                        using (SqlCommand cmd = new SqlCommand(sqlUpdate, con, trans))
                        {
                            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = guna2TextBox1.Text.Trim();
                            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = guna2TextBox2.Text.Trim();
                            cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = guna2TextBox3.Text.Trim();
                            cmd.Parameters.Add("@StartDate", SqlDbType.NVarChar).Value = guna2DateTimePicker1.Value;
                            cmd.Parameters.Add("@Gender", SqlDbType.NVarChar).Value = guna2ComboBox2.Text.Trim();
                            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = _selectedInstructorId;
                            cmd.ExecuteNonQuery();
                        }

                        // ===============================
                        // 6️⃣ حذف الاختصاصات القديمة
                        // ===============================
                        string sqlDelete = @"
DELETE FROM InstructorSpecializations
WHERE InstructorID = @ID";

                        using (SqlCommand cmd = new SqlCommand(sqlDelete, con, trans))
                        {
                            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = _selectedInstructorId;
                            cmd.ExecuteNonQuery();
                        }

                        // ===============================
                        // 7️⃣ إضافة الاختصاصات الجديدة
                        // ===============================
                        foreach (var item in checkedListBox1.CheckedItems)
                        {
                            ComboBoxItem spec = item as ComboBoxItem;
                            if (spec == null)
                                continue;

                            string sqlInsert = @"
INSERT INTO InstructorSpecializations (InstructorID, SpecializationID)
VALUES (@InstructorID, @SpecID)";

                            using (SqlCommand cmd = new SqlCommand(sqlInsert, con, trans))
                            {
                                cmd.Parameters.Add("@InstructorID", SqlDbType.Int).Value = _selectedInstructorId;
                                cmd.Parameters.Add("@SpecID", SqlDbType.Int).Value = spec.Value;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // ===============================
                        // 8️⃣ تأكيد العملية
                        // ===============================
                        trans.Commit();

                        MessageBox.Show("✅ تم تعديل بيانات الأستاذ والاختصاصات بنجاح",
                            "نجاح",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        LoadInstructors();   // تحديث الجدول
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء التعديل:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void SearchInstructors(string searchText)
        {
            try
            {
                using (SqlConnection con = DB.GetConnection())
                {
                    string sql = @"
SELECT 
    i.InstructorID,
    i.FullName,
    i.Phone,
    i.Email,
    i.StartDate,
    i.Gender,
    STUFF((
        SELECT ' , ' + s.SpecializationName
        FROM InstructorSpecializations is2
        INNER JOIN Specializations s 
            ON is2.SpecializationID = s.SpecializationID
        WHERE is2.InstructorID = i.InstructorID
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 3, '') AS Specializations
FROM Instructors i
WHERE i.FullName LIKE '%' + @searchText + '%'
   OR EXISTS (
       SELECT 1
       FROM InstructorSpecializations is2
       INNER JOIN Specializations s 
           ON is2.SpecializationID = s.SpecializationID
       WHERE is2.InstructorID = i.InstructorID
         AND s.SpecializationName LIKE '%' + @searchText + '%'
   )
ORDER BY i.FullName";

                    using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@searchText", "%" + searchText.Trim() + "%");

                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        guna2DataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء البحث:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void guna2TextBox8_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(guna2TextBox8.Text))
            {
                LoadInstructors(); // يرجع كل البيانات
            }
            else
            {
                SearchInstructors(guna2TextBox8.Text);
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {

            // 1️⃣ تحقق من اختيار أستاذ
            if (_selectedInstructorId == 0)
            {
                MessageBox.Show(
                    "⚠ الرجاء اختيار أستاذ من الجدول أولاً",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // 2️⃣ تأكيد الحذف
            DialogResult result = MessageBox.Show(
                "⚠ هل أنت متأكد من حذف هذا الأستاذ؟\n\n" +
                "سيتم حذف جميع البيانات المرتبطة به (الاختصاصات، المحاضرات، الجداول).",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection con = DB.GetConnection())
                {
                    con.Open();
                    SqlTransaction trans = con.BeginTransaction();

                    try
                    {
                        // ==========================
                        // 3️⃣ حذف جداول المحاضرات
                        // ==========================
                        string deleteSchedule = @"
DELETE FROM LectureSchedule
WHERE LectureID IN (
    SELECT LectureID FROM Lectures WHERE InstructorID = @ID
)";

                        using (SqlCommand cmd = new SqlCommand(deleteSchedule, con, trans))
                        {
                            cmd.Parameters.AddWithValue("@ID", _selectedInstructorId);
                            cmd.ExecuteNonQuery();
                        }

                        // ==========================
                        // 4️⃣ حذف المحاضرات
                        // ==========================
                        string deleteLectures = @"
DELETE FROM Lectures
WHERE InstructorID = @ID";

                        using (SqlCommand cmd = new SqlCommand(deleteLectures, con, trans))
                        {
                            cmd.Parameters.AddWithValue("@ID", _selectedInstructorId);
                            cmd.ExecuteNonQuery();
                        }

                        // ==========================
                        // 5️⃣ حذف اختصاصات الأستاذ
                        // ==========================
                        string deleteSpecs = @"
DELETE FROM InstructorSpecializations
WHERE InstructorID = @ID";

                        using (SqlCommand cmd = new SqlCommand(deleteSpecs, con, trans))
                        {
                            cmd.Parameters.AddWithValue("@ID", _selectedInstructorId);
                            cmd.ExecuteNonQuery();
                        }

                        // ==========================
                        // 6️⃣ حذف الأستاذ نفسه
                        // ==========================
                        string deleteInstructor = @"
DELETE FROM Instructors
WHERE InstructorID = @ID";

                        using (SqlCommand cmd = new SqlCommand(deleteInstructor, con, trans))
                        {
                            cmd.Parameters.AddWithValue("@ID", _selectedInstructorId);
                            cmd.ExecuteNonQuery();
                        }

                        // 7️⃣ تأكيد العملية
                        trans.Commit();

                        MessageBox.Show(
                            "✅ تم حذف الأستاذ وجميع بياناته بنجاح",
                            "نجاح",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        // 8️⃣ تحديث الواجهة
                        _selectedInstructorId = 0;
                        LoadInstructors();
                        ClearForm();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ حدث خطأ أثناء الحذف:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    } 
}

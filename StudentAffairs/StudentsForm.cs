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
using static System.Collections.Specialized.BitVector32;

namespace StudentAffairs
{
    public partial class StudentsForm : Form
    {
        private int _userId;
        private string _fullName;

        public StudentsForm(int userId, string fullName)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            _userId = userId;
            _fullName = fullName;
            label1.Text = $"Welcome {fullName}";
            LoadComboBoxes();
           
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
        private void ShowIraqiFields()
        {
            guna2Panel1.Visible = true;
            guna2Panel2.Visible = false;
        }
        private void ShowForeignFields()
        {
            guna2Panel1.Visible = false;
            guna2Panel2.Visible = true;
        }
        private void ToggleGrids()
        {
            bool isForeign = guna2ComboBox1.SelectedItem?.ToString() == "طلاب أجانب";

            guna2DataGridView1.Visible = !isForeign;
            guna2DataGridView2.Visible = isForeign;
        }
        
        bool IsPhoneValid(string phone)
        {
            return phone.All(char.IsDigit) && phone.Length == 11;
        }
        bool IsEmailValid(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch { return false; }
        }

        public DataTable GetDepartments()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = DB.GetConnection())
                {
                    con.Open();
                    string query = "SELECT [SpecializationID], [SpecializationName] FROM Specializations ORDER BY [SpecializationName]";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.Fill(dt);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ عند جلب الأقسام: " + ex.Message);
            }
            return dt;
        }

        public DataTable GetStagesBySpecialization(int specializationId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = DB.GetConnection())
                {
                    con.Open();

                    string query = @"
SELECT 
    StageID, 
    StageName
FROM Stages
WHERE SpecializationID = @specId
ORDER BY StageName";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@specId", specializationId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ عند جلب المراحل حسب الاختصاص: " + ex.Message);
            }
            return dt;
        }
        private void LoadComboBoxes()
        {
            // ===== الجنس =====
            guna2ComboBox2.Items.Clear();
            guna2ComboBox2.Items.Add("قم بتحديد الجنس");
            guna2ComboBox2.Items.AddRange(new string[] { "ذكر", "انثى" });
            guna2ComboBox2.SelectedIndex = 0; // افتراضي فارغ
            guna2ComboBox5.Items.Clear();
            guna2ComboBox5.Items.Add("قم بتحديد الجنس");
            guna2ComboBox5.Items.AddRange(new string[] { "ذكر", "انثى" });
            guna2ComboBox5.SelectedIndex = 0; // افتراضي فارغ

            // ===== الأقسام =====
            DataTable dtDepartments = GetDepartments(); // دالة ترجع DataTable من جدول Departments
            if (dtDepartments.Rows.Count > 0)
            {
              
                guna2ComboBox3.DataSource = dtDepartments;
                guna2ComboBox3.DisplayMember = "SpecializationName";
                guna2ComboBox3.ValueMember = "SpecializationID";
                guna2ComboBox3.SelectedIndex = 0;
                guna2ComboBox6.Items.Add("قم بتحديد الاختصاص");
                guna2ComboBox6.DataSource = dtDepartments;
                guna2ComboBox6.DisplayMember = "SpecializationName";
                guna2ComboBox6.ValueMember = "SpecializationID";
                guna2ComboBox6.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("لا توجد أقسام حالياً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2ComboBox3.DataSource = null;
                MessageBox.Show("لا توجد أقسام حالياً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2ComboBox6.DataSource = null;
            }

          
        }

        private void LoadIraqiStudents()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
    s.IraqStudentID,
    s.FullName,
    s.Gender,
    s.BirthDate,
    s.Phone,
    s.Email,
    s.Address,
    s.RegistrationDate,

    s.StageID,
    st.StageName,

    s.SpecializationID,
    sp.SpecializationName

FROM Students_Iraq s
JOIN Stages st 
    ON s.StageID = st.StageID
JOIN Specializations sp 
    ON s.SpecializationID = sp.SpecializationID

ORDER BY s.FullName";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2DataGridView1.DataSource = dt;

                // ❌ إخفاء الأعمدة التقنية
                guna2DataGridView1.Columns["IraqStudentID"].Visible = false;
                guna2DataGridView1.Columns["StageID"].Visible = false;
                guna2DataGridView1.Columns["SpecializationID"].Visible = false;

                // ✅ تعريب العناوين
                guna2DataGridView1.Columns["FullName"].HeaderText = "اسم الطالب";
                guna2DataGridView1.Columns["Gender"].HeaderText = "الجنس";
                guna2DataGridView1.Columns["BirthDate"].HeaderText = "تاريخ الولادة";
                guna2DataGridView1.Columns["Phone"].HeaderText = "رقم الهاتف";
                guna2DataGridView1.Columns["Email"].HeaderText = "البريد الإلكتروني";
                guna2DataGridView1.Columns["Address"].HeaderText = "العنوان";
                guna2DataGridView1.Columns["RegistrationDate"].HeaderText = "تاريخ التسجيل";
                guna2DataGridView1.Columns["StageName"].HeaderText = "المرحلة";
                guna2DataGridView1.Columns["SpecializationName"].HeaderText = "الاختصاص";
            }
        }
        private void LoadForeignStudents()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string sql = @"
SELECT 
    f.ForeignStudentID,
    f.FullName,
    f.Gender,
    f.BirthDate,
    f.Phone,
    f.Email,
    f.Country,
    f.PassportNo,
    f.VisaExpireDate,
    f.RegistrationDate,

    f.StageID,
    st.StageName,

    f.SpecializationID,
    sp.SpecializationName

FROM Students_Foreign f
JOIN Stages st 
    ON f.StageID = st.StageID
JOIN Specializations sp 
    ON f.SpecializationID = sp.SpecializationID

ORDER BY f.FullName";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2DataGridView2.DataSource = dt;

                // ❌ إخفاء الأعمدة التقنية
                guna2DataGridView2.Columns["ForeignStudentID"].Visible = false;
                guna2DataGridView2.Columns["StageID"].Visible = false;
                guna2DataGridView2.Columns["SpecializationID"].Visible = false;

                // ✅ تعريب العناوين
                guna2DataGridView2.Columns["FullName"].HeaderText = "اسم الطالب";
                guna2DataGridView2.Columns["Gender"].HeaderText = "الجنس";
                guna2DataGridView2.Columns["BirthDate"].HeaderText = "تاريخ الولادة";
                guna2DataGridView2.Columns["Phone"].HeaderText = "رقم الهاتف";
                guna2DataGridView2.Columns["Email"].HeaderText = "البريد الإلكتروني";
                guna2DataGridView2.Columns["Country"].HeaderText = "الدولة";
                guna2DataGridView2.Columns["PassportNo"].HeaderText = "رقم الجواز";
                guna2DataGridView2.Columns["VisaExpireDate"].HeaderText = "انتهاء الفيزا";
                guna2DataGridView2.Columns["RegistrationDate"].HeaderText = "تاريخ التسجيل";
                guna2DataGridView2.Columns["StageName"].HeaderText = "المرحلة";
                guna2DataGridView2.Columns["SpecializationName"].HeaderText = "الاختصاص";
            }
        }
        private void StudentsForm_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
            guna2ComboBox1.Items.AddRange(new string[]
   {
        "طلاب عراقيين",
        "طلاب أجانب"
   });

            guna2ComboBox1.SelectedIndex = 0; // افتراضي عراقي
            columnMap = new Dictionary<string, string>()
    {
        { "اسم الطالب", "FullName" },
        { "الجنس", "Gender" },
        { "تاريخ الولادة", "BirthDate" },
        { "رقم الهاتف", "Phone" },
        { "البريد الإلكتروني", "Email" },
        { "العنوان", "Address" },
        { "الاختصاص", "SpecializationName" },
        { "المرحلة", "StageName" },
        { "تاريخ التسجيل", "RegistrationDate" }
    };

            checkedListBox1.Items.Clear();

            foreach (var item in columnMap.Keys)
                checkedListBox1.Items.Add(item, true); // true = ظاهر افتراضياً

            guna2DateTimePicker1.Format = DateTimePickerFormat.Custom;
            guna2DateTimePicker1.CustomFormat = "yyyy/MM/dd";
            guna2DateTimePicker1.Value = DateTime.Now;
            guna2DateTimePicker2.Format = DateTimePickerFormat.Custom;
            guna2DateTimePicker2.CustomFormat = "yyyy/MM/dd";
            guna2DateTimePicker2.Value = DateTime.Now;
            guna2DateTimePicker3.Format = DateTimePickerFormat.Custom;
            guna2DateTimePicker3.CustomFormat = "yyyy/MM/dd";
            guna2DateTimePicker3.Value = DateTime.Now;
        }
        private void SearchStudents()
        {
            using (SqlConnection con = DB.GetConnection())
            {
                string table = guna2ComboBox1.SelectedIndex == 0 ? "Students_Iraq" : "Students_Foreign";
                string idColumn = table == "Students_Iraq" ? "IraqStudentID" : "ForeignStudentID";

                string sql = $@"
SELECT 
    s.{idColumn},
    s.FullName,
    s.Gender,
    s.BirthDate,
    s.Phone,
    s.Email,
    {(table == "Students_Iraq" ? "s.Address" : "s.Country")} AS Address,
    s.RegistrationDate,
    s.StageID,
    st.StageName,
    s.SpecializationID,
    sp.SpecializationName

FROM {table} s
JOIN Stages st ON s.StageID = st.StageID
JOIN Specializations sp ON s.SpecializationID = sp.SpecializationID
WHERE 1=1
";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                // الاسم
                if (!string.IsNullOrWhiteSpace(guna2TextBox8.Text))
                {
                    sql += " AND s.FullName LIKE @name";
                    cmd.Parameters.AddWithValue("@name", "%" + guna2TextBox8.Text.Trim() + "%");
                }

                // الهاتف او الايميل
                if (!string.IsNullOrWhiteSpace(guna2TextBox9.Text))
                {
                    sql += " AND (s.Phone LIKE @contact OR s.Email LIKE @contact)";
                    cmd.Parameters.AddWithValue("@contact", "%" + guna2TextBox9.Text.Trim() + "%");
                }

                // العنوان
                if (table == "Students_Iraq" && !string.IsNullOrWhiteSpace(guna2TextBox7.Text))
                {
                    sql += " AND s.Address LIKE @address";
                    cmd.Parameters.AddWithValue("@address", "%" + guna2TextBox7.Text.Trim() + "%");
                }

                if (guna2ComboBox5.SelectedIndex > 0) // مو اول عنصر
                {
                    sql += " AND s.Gender = @gender";
                    cmd.Parameters.AddWithValue("@gender", guna2ComboBox5.Text);
                }

                // الاختصاص
                if (guna2ComboBox6.SelectedIndex > -1 && guna2ComboBox6.SelectedValue is int)
                {
                    sql += " AND s.SpecializationID = @spec";
                    cmd.Parameters.AddWithValue("@spec", (int)guna2ComboBox6.SelectedValue);
                }

                // المرحلة
                if (guna2ComboBox7.SelectedIndex > -1 && guna2ComboBox7.SelectedValue is int)
                {
                    sql += " AND s.StageID = @stage";
                    cmd.Parameters.AddWithValue("@stage", (int)guna2ComboBox7.SelectedValue);
                }

                sql += " ORDER BY s.FullName";

                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (table == "Students_Iraq")
                    guna2DataGridView1.DataSource = dt;
                else
                    guna2DataGridView2.DataSource = dt;
            }
        }
        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBox1.SelectedIndex == 0)
            {
                ShowIraqiFields();
                LoadIraqiStudents();
               StyleDataGridView(guna2DataGridView1);
            }
            else
            {
                ShowForeignFields();
                LoadForeignStudents();
                StyleDataGridView(guna2DataGridView2);
            }
            ToggleGrids();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string name = guna2TextBox1.Text.Trim();
            string phone = guna2TextBox2.Text.Trim();
            string email = guna2TextBox3.Text.Trim();
            string gender = guna2ComboBox2.Text;
            DateTime birthDate = guna2DateTimePicker1.Value.Date;
            int specializationId = (int)guna2ComboBox3.SelectedValue;
            int stageId = (int)guna2ComboBox4.SelectedValue;
            DateTime registerDate = guna2DateTimePicker2.Value.Date;

            bool isForeign = guna2ComboBox1.SelectedItem?.ToString() == "طلاب أجانب";

            // التحقق من الحقول الفارغة
            if (string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(gender) ||
                specializationId <= 0 || stageId <= 0)
            {
                MessageBox.Show("جميع الحقول العامة مطلوبة!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // التحقق من الطلاب المكرر
            if (isForeign)
            {
                if (IsStudentExistsInForeign(name))
                {
                    MessageBox.Show("الطالب الأجنبي موجود مسبقاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                if (IsStudentExistsInIraq(name))
                {
                    MessageBox.Show("الطالب العراقي موجود مسبقاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            if (guna2ComboBox2.SelectedIndex == 0)
            {
                MessageBox.Show("⚠ الرجاء تحديد الجنس  ", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2ComboBox2.Focus();
                return;
            }
            // التحقق من الهاتف والبريد
            if (!IsPhoneValid(phone))
            {
                MessageBox.Show("رقم الهاتف غير صحيح، يجب أن يكون 11 رقماً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsEmailValid(email))
            {
                MessageBox.Show("البريد الإلكتروني غير صالح.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                if (isForeign)
                {
                    string country = guna2TextBox5.Text.Trim();
                    string passport = guna2TextBox6.Text.Trim();
                    DateTime visaExpire = guna2DateTimePicker3.Value.Date;

                    if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(passport))
                    {
                        MessageBox.Show("الرجاء تعبئة جميع الحقول الخاصة بالطلاب الأجانب.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    cmd.CommandText = @"
INSERT INTO Students_Foreign
(FullName, UserID, Gender, BirthDate, Phone, Email,
 SpecializationID, StageID,
 Country, PassportNo, VisaExpireDate, RegistrationDate)
VALUES
(@name, @UserID, @gender, @birth, @phone, @email,
 @spec, @stage,
 @country, @passport, @visa, @regDate)";

                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@UserID", _userId);
                    cmd.Parameters.AddWithValue("@gender", gender);
                    cmd.Parameters.AddWithValue("@birth", birthDate);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@spec", specializationId);
                    cmd.Parameters.AddWithValue("@stage", stageId);
                    cmd.Parameters.AddWithValue("@country", country);
                    cmd.Parameters.AddWithValue("@passport", passport);
                    cmd.Parameters.AddWithValue("@visa", visaExpire);
                    cmd.Parameters.AddWithValue("@regDate", registerDate);
                }
                else
                {
                    string address = guna2TextBox4.Text.Trim();
                    if (string.IsNullOrEmpty(address))
                    {
                        MessageBox.Show("الرجاء تعبئة عنوان الطالب العراقي.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    cmd.CommandText = @"
INSERT INTO Students_Iraq
(FullName, UserID, Gender, BirthDate, Phone, Email,
 SpecializationID, StageID,
 Address, RegistrationDate)
VALUES
(@name, @UserID, @gender, @birth, @phone, @email,
 @spec, @stage,
 @address, @regDate)";

                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@UserID", _userId);
                    cmd.Parameters.AddWithValue("@gender", gender);
                    cmd.Parameters.AddWithValue("@birth", birthDate);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@spec", specializationId);
                    cmd.Parameters.AddWithValue("@stage", stageId);
                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.Parameters.AddWithValue("@regDate", registerDate);
                }

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تم إضافة الطالب بنجاح ✅", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadForeignStudents();
            LoadIraqiStudents();
        }

        // ======== الدالتين للتحقق من التكرار ========
        private bool IsStudentExistsInIraq(string fullName)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM Students_Iraq WHERE FullName=@name";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", fullName);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        private bool IsStudentExistsInForeign(string fullName)
        {
            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM Students_Foreign WHERE FullName=@name";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", fullName);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
        int _selectedStudentId;
        bool _selectedIsForeign;
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // 1️⃣ تحقق من اختيار طالب
            if (_selectedStudentId <= 0)
            {
                MessageBox.Show("يرجى اختيار طالب للتعديل.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2️⃣ قراءة القيم
            string name = guna2TextBox1.Text.Trim();
            string phone = guna2TextBox2.Text.Trim();
            string email = guna2TextBox3.Text.Trim();
            string gender = guna2ComboBox2.Text;
            DateTime birthDate = guna2DateTimePicker1.Value.Date;
            int specializationId = Convert.ToInt32(guna2ComboBox3.SelectedValue);
            int stageId = Convert.ToInt32(guna2ComboBox4.SelectedValue);
            DateTime registerDate = guna2DateTimePicker2.Value.Date;

            bool isForeign = guna2ComboBox1.SelectedItem?.ToString() == "أجنبي";

            // 3️⃣ تحقق من الحقول العامة
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(gender) ||
                specializationId <= 0 ||
                stageId <= 0)
            {
                MessageBox.Show("جميع الحقول مطلوبة.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (guna2ComboBox2.SelectedIndex == 0)
            {
                MessageBox.Show("⚠ الرجاء تحديد الجنس  ", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2ComboBox2.Focus();
                return;
            }
            if (!IsPhoneValid(phone))
            {
                MessageBox.Show("رقم الهاتف يجب أن يكون 11 رقم.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsEmailValid(email))
            {
                MessageBox.Show("البريد الإلكتروني غير صحيح.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection con = DB.GetConnection())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                // ================== 🌍 طالب أجنبي ==================
                if (isForeign)
                {
                    string country = guna2TextBox5.Text.Trim();
                    string passport = guna2TextBox6.Text.Trim();
                    DateTime visaExpire = guna2DateTimePicker3.Value.Date;

                    if (string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(passport))
                    {
                        MessageBox.Show("يرجى تعبئة جميع بيانات الطالب الأجنبي.", "تنبيه",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    cmd.CommandText = @"
UPDATE Students_Foreign SET
    FullName = @name,
    Gender = @gender,
    BirthDate = @birth,
    Phone = @phone,
    Email = @email,
    SpecializationID = @spec,
    StageID = @stage,
    Country = @country,
    PassportNo = @passport,
    VisaExpireDate = @visa,
    RegistrationDate = @regDate
WHERE ForeignStudentID = @id";

                    cmd.Parameters.AddWithValue("@country", country);
                    cmd.Parameters.AddWithValue("@passport", passport);
                    cmd.Parameters.AddWithValue("@visa", visaExpire);
                }
                // ================== 🇮🇶 طالب عراقي ==================
                else
                {
                    string address = guna2TextBox4.Text.Trim();

                    if (string.IsNullOrWhiteSpace(address))
                    {
                        MessageBox.Show("يرجى إدخال عنوان الطالب العراقي.", "تنبيه",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    cmd.CommandText = @"
UPDATE Students_Iraq SET
    FullName = @name,
    Gender = @gender,
    BirthDate = @birth,
    Phone = @phone,
    Email = @email,
    SpecializationID = @spec,
    StageID = @stage,
    Address = @address,
    RegistrationDate = @regDate
WHERE IraqStudentID = @id";

                    cmd.Parameters.AddWithValue("@address", address);
                }

                // 4️⃣ باراميترات مشتركة
                cmd.Parameters.AddWithValue("@id", _selectedStudentId);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@gender", gender);
                cmd.Parameters.AddWithValue("@birth", birthDate);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@spec", specializationId);
                cmd.Parameters.AddWithValue("@stage", stageId);
                cmd.Parameters.AddWithValue("@regDate", registerDate);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("✅ تم تعديل بيانات الطالب بنجاح", "نجاح",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadForeignStudents();
            LoadIraqiStudents();
        }
        private void TogglePanels()
        {
            bool isForeign = guna2ComboBox1.SelectedItem?.ToString() == "أجنبي";
            guna2Panel2.Visible = isForeign;
            guna2Panel1.Visible = !isForeign;
        }
        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];

            // 🆔 تحديد الطالب
            _selectedStudentId = Convert.ToInt32(row.Cells["IraqStudentID"].Value);
            _selectedIsForeign = false;

            // 🇮🇶 عراقي
            guna2ComboBox1.SelectedItem = "عراقي";
            TogglePanels();

            // 🧾 تعبئة الأدوات
            guna2TextBox1.Text = row.Cells["FullName"].Value?.ToString();
            guna2ComboBox2.Text = row.Cells["Gender"].Value?.ToString();
            guna2DateTimePicker1.Value = Convert.ToDateTime(row.Cells["BirthDate"].Value);

            guna2TextBox2.Text = row.Cells["Phone"].Value?.ToString();
            guna2TextBox3.Text = row.Cells["Email"].Value?.ToString();

            // 🎓 الاختصاص + المرحلة (بدل Department)
            guna2ComboBox3.SelectedValue = row.Cells["SpecializationID"].Value;
            guna2ComboBox4.SelectedValue = row.Cells["StageID"].Value;

            guna2TextBox4.Text = row.Cells["Address"].Value?.ToString();
            guna2DateTimePicker2.Value = Convert.ToDateTime(row.Cells["RegistrationDate"].Value);
        }
        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2GroupBox2_Click(object sender, EventArgs e)
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
        private void ToggleColumn(string columnName, bool visible)
        {
            if (guna2DataGridView1.Columns.Contains(columnName))
                guna2DataGridView1.Columns[columnName].Visible = visible;

            if (guna2DataGridView2.Columns.Contains(columnName))
                guna2DataGridView2.Columns[columnName].Visible = visible;
        }
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            if(checkedListBox1.Visible==true)
            {
                checkedListBox1.Visible = false;
            }
            else
            checkedListBox1.Visible = true;
        }
        private Dictionary<string, string> columnMap;
      
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
         
            string displayName = checkedListBox1.Items[e.Index].ToString();

            if (!columnMap.ContainsKey(displayName))
                return;

            string columnName = columnMap[displayName];

            // Checked = ظاهر / Unchecked = مخفي
            bool visible = e.NewValue == CheckState.Checked;

            ToggleColumn(columnName, visible);
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("هل تريد تسجيل الخروج؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                
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
        }

        private void guna2DataGridView2_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = guna2DataGridView2.Rows[e.RowIndex];

            // 🆔 تحديد الطالب
            _selectedStudentId = Convert.ToInt32(row.Cells["ForeignStudentID"].Value);
            _selectedIsForeign = true;

            // 🌍 أجنبي
            guna2ComboBox1.SelectedItem = "أجنبي";
            ToggleGrids();

            // 🧾 تعبئة الأدوات
            guna2TextBox1.Text = row.Cells["FullName"].Value?.ToString();
            guna2ComboBox2.Text = row.Cells["Gender"].Value?.ToString();
            guna2DateTimePicker1.Value = Convert.ToDateTime(row.Cells["BirthDate"].Value);

            guna2TextBox2.Text = row.Cells["Phone"].Value?.ToString();
            guna2TextBox3.Text = row.Cells["Email"].Value?.ToString();

            // 🎓 الاختصاص + المرحلة
            guna2ComboBox3.SelectedValue = row.Cells["SpecializationID"].Value;
            guna2ComboBox4.SelectedValue = row.Cells["StageID"].Value;

            guna2TextBox5.Text = row.Cells["Country"].Value?.ToString();
            guna2TextBox6.Text = row.Cells["PassportNo"].Value?.ToString();
            guna2DateTimePicker3.Value = Convert.ToDateTime(row.Cells["VisaExpireDate"].Value);

            guna2DateTimePicker2.Value = Convert.ToDateTime(row.Cells["RegistrationDate"].Value);

        }
        private void LoadStages()
        {
            if (!(guna2ComboBox3.SelectedValue is int specId))
                return;

            using (SqlConnection con = DB.GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT StageID, StageName FROM Stages WHERE SpecializationID=@id", con);

                da.SelectCommand.Parameters.AddWithValue("@id", specId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                guna2ComboBox4.DataSource = dt;
                guna2ComboBox4.DisplayMember = "StageName";
                guna2ComboBox4.ValueMember = "StageID";
            }
        }
        private void guna2ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStages();
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            // 1️⃣ تحقق من تحديد طالب
            if (_selectedStudentId <= 0)
            {
                MessageBox.Show("⚠ يرجى اختيار طالب من الجدول أولاً", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2️⃣ تأكيد الحذف
            DialogResult result = MessageBox.Show(
                "⚠ هل أنت متأكد من حذف هذا الطالب؟\nسيتم حذف جميع بياناته نهائياً.",
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

                    string sql = "";

                    // ==========================
                    // 3️⃣ تحديد نوع الطالب
                    // ==========================
                    if (_selectedIsForeign)
                    {
                        sql = "DELETE FROM Students_Foreign WHERE ForeignStudentID = @ID";
                    }
                    else
                    {
                        sql = "DELETE FROM Students_Iraq WHERE IraqStudentID = @ID";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = _selectedStudentId;
                        cmd.ExecuteNonQuery();
                    }
                }

                // ==========================
                // 4️⃣ تنظيف الواجهة
                // ==========================
                ClearStudentForm();
                _selectedStudentId = 0;

                // ==========================
                // 5️⃣ تحديث الجداول
                // ==========================
                LoadForeignStudents();
                LoadIraqiStudents();

                MessageBox.Show(
                    "✅ تم حذف الطالب بنجاح",
                    "نجاح",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "❌ لا يمكن حذف الطالب لوجود بيانات مرتبطة به.\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ حدث خطأ غير متوقع:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

        }
        private void ClearStudentForm()
        {
            guna2TextBox1.Clear();
            guna2TextBox2.Clear();
            guna2TextBox3.Clear();
            guna2TextBox4.Clear();
            guna2TextBox5.Clear();
            guna2TextBox6.Clear();

            guna2ComboBox2.SelectedIndex = -1;
            guna2ComboBox3.SelectedIndex = -1;
            guna2ComboBox4.SelectedIndex = -1;

            guna2DateTimePicker1.Value = DateTime.Today;
            guna2DateTimePicker2.Value = DateTime.Today;
            guna2DateTimePicker3.Value = DateTime.Today;
        }

        private void guna2TextBox8_TextChanged(object sender, EventArgs e)
        {
            SearchStudents();
        }

        private void guna2TextBox9_TextChanged(object sender, EventArgs e)
        {
            SearchStudents();
        }

        private void guna2TextBox7_TextChanged(object sender, EventArgs e)
        {
            SearchStudents();
        }

        private void guna2ComboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchStudents();
        }

        private void guna2ComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchStudents();
            if (guna2ComboBox6.SelectedValue == null)
                return;

            // نتأكد مو DataRowView
            if (guna2ComboBox6.SelectedValue is DataRowView)
                return;

            int specId = Convert.ToInt32(guna2ComboBox6.SelectedValue);

            DataTable dtStages = GetStagesBySpecialization(specId);

            guna2ComboBox7.DataSource = dtStages;
            guna2ComboBox7.DisplayMember = "StageName";
            guna2ComboBox7.ValueMember = "StageID";
            guna2ComboBox7.SelectedIndex = -1;
        }

        private void guna2ComboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchStudents();
        }
    }
}

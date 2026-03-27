using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace StudentAffairs
{
    public partial class FormMain : Form
    {
        private int _userId;
        private string _fullName;
        public FormMain(int userId, string fullName)
        {
            InitializeComponent();
            _userId = userId;
            _fullName = fullName;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.WindowState = FormWindowState.Maximized;
            label1.Text = $"Welcome {fullName}";

        }
      
        private void FormMain_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // كل ثانية
            timer1.Start();
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd -- hh:mm:ss tt");
        }
     

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<StudentsForm>().Any())
            {
                MessageBox.Show("واجهة إضافة التحاليل مفتوحة بالفعل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // إخفاء الواجهة الحالية وفتح الثانية
            this.Hide();

            StudentsForm FrmManagePatients = new StudentsForm(_userId, _fullName);

            // نضيف حركة Fade In
            FrmManagePatients.Opacity = 0;
            Timer fadeInTimer = new Timer();
            fadeInTimer.Interval = 6;
            fadeInTimer.Tick += (s, args) =>
            {
                if (FrmManagePatients.Opacity < 1)
                    FrmManagePatients.Opacity += 0.05;
                else
                    fadeInTimer.Stop();
            };
            fadeInTimer.Start();

            // لما ينغلق الفورم ترجع فورم المراجعين
            FrmManagePatients.FormClosed += (s, args) =>
            {
                this.Show();
                this.Activate(); // نضمن التركيز يرجع
            };

            FrmManagePatients.Show();
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<StudentsForm>().Any())
            {
                MessageBox.Show("واجهة إضافة التحاليل مفتوحة بالفعل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // إخفاء الواجهة الحالية وفتح الثانية
            this.Hide();

            StagesForm StagesForm = new StagesForm(_userId, _fullName);

            // نضيف حركة Fade In
            StagesForm.Opacity = 0;
            Timer fadeInTimer = new Timer();
            fadeInTimer.Interval = 6;
            fadeInTimer.Tick += (s, args) =>
            {
                if (StagesForm.Opacity < 1)
                    StagesForm.Opacity += 0.05;
                else
                    fadeInTimer.Stop();
            };
            fadeInTimer.Start();

            // لما ينغلق الفورم ترجع فورم المراجعين
            StagesForm.FormClosed += (s, args) =>
            {
                this.Show();
                this.Activate(); // نضمن التركيز يرجع
            };

            StagesForm.Show();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<StudentsForm>().Any())
            {
                MessageBox.Show("واجهة إضافة التحاليل مفتوحة بالفعل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // إخفاء الواجهة الحالية وفتح الثانية
            this.Hide();

            DepartmentsForm DepartmentsForm = new DepartmentsForm(_userId, _fullName);

            // نضيف حركة Fade In
            DepartmentsForm.Opacity = 0;
            Timer fadeInTimer = new Timer();
            fadeInTimer.Interval = 6;
            fadeInTimer.Tick += (s, args) =>
            {
                if (DepartmentsForm.Opacity < 1)
                    DepartmentsForm.Opacity += 0.05;
                else
                    fadeInTimer.Stop();
            };
            fadeInTimer.Start();

            // لما ينغلق الفورم ترجع فورم المراجعين
            DepartmentsForm.FormClosed += (s, args) =>
            {
                this.Show();
                this.Activate(); // نضمن التركيز يرجع
            };

            DepartmentsForm.Show();
        }

     
        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<InstructorsForm>().Any())
            {
                MessageBox.Show("واجهة إضافة التحاليل مفتوحة بالفعل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // إخفاء الواجهة الحالية وفتح الثانية
            this.Hide();

            InstructorsForm FrmManagePatients = new InstructorsForm(_userId, _fullName);

            // نضيف حركة Fade In
            FrmManagePatients.Opacity = 0;
            Timer fadeInTimer = new Timer();
            fadeInTimer.Interval = 6;
            fadeInTimer.Tick += (s, args) =>
            {
                if (FrmManagePatients.Opacity < 1)
                    FrmManagePatients.Opacity += 0.05;
                else
                    fadeInTimer.Stop();
            };
            fadeInTimer.Start();

            // لما ينغلق الفورم ترجع فورم المراجعين
            FrmManagePatients.FormClosed += (s, args) =>
            {
                this.Show();
                this.Activate(); // نضمن التركيز يرجع
            };

            FrmManagePatients.Show();
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<StudentsForm>().Any())
            {
                MessageBox.Show("واجهة إضافة التحاليل مفتوحة بالفعل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // إخفاء الواجهة الحالية وفتح الثانية
            this.Hide();

            SubjectsForm SubjectsForm = new SubjectsForm(_userId, _fullName);

            // نضيف حركة Fade In
            SubjectsForm.Opacity = 0;
            Timer fadeInTimer = new Timer();
            fadeInTimer.Interval = 6;
            fadeInTimer.Tick += (s, args) =>
            {
                if (SubjectsForm.Opacity < 1)
                    SubjectsForm.Opacity += 0.05;
                else
                    fadeInTimer.Stop();
            };
            fadeInTimer.Start();

            // لما ينغلق الفورم ترجع فورم المراجعين
            SubjectsForm.FormClosed += (s, args) =>
            {
                this.Show();
                this.Activate(); // نضمن التركيز يرجع
            };

            SubjectsForm.Show();
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<StudentsForm>().Any())
            {
                MessageBox.Show("واجهة إضافة التحاليل مفتوحة بالفعل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // إخفاء الواجهة الحالية وفتح الثانية
            this.Hide();

            LectureScheduleForm LectureScheduleForm = new LectureScheduleForm(_userId, _fullName);

            // نضيف حركة Fade In
            LectureScheduleForm.Opacity = 0;
            Timer fadeInTimer = new Timer();
            fadeInTimer.Interval = 6;
            fadeInTimer.Tick += (s, args) =>
            {
                if (LectureScheduleForm.Opacity < 1)
                    LectureScheduleForm.Opacity += 0.05;
                else
                    fadeInTimer.Stop();
            };
            fadeInTimer.Start();

            // لما ينغلق الفورم ترجع فورم المراجعين
            LectureScheduleForm.FormClosed += (s, args) =>
            {
                this.Show();
                this.Activate(); // نضمن التركيز يرجع
            };

            LectureScheduleForm.Show();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<StudentsForm>().Any())
            {
                MessageBox.Show("واجهة إضافة التحاليل مفتوحة بالفعل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // إخفاء الواجهة الحالية وفتح الثانية
            this.Hide();

            StudentGrades StudentGrades = new StudentGrades(_userId, _fullName);

            // نضيف حركة Fade In
            StudentGrades.Opacity = 0;
            Timer fadeInTimer = new Timer();
            fadeInTimer.Interval = 6;
            fadeInTimer.Tick += (s, args) =>
            {
                if (StudentGrades.Opacity < 1)
                    StudentGrades.Opacity += 0.05;
                else
                    fadeInTimer.Stop();
            };
            fadeInTimer.Start();

            // لما ينغلق الفورم ترجع فورم المراجعين
            StudentGrades.FormClosed += (s, args) =>
            {
                this.Show();
                this.Activate(); // نضمن التركيز يرجع
            };

            StudentGrades.Show();
        }

        private void guna2Button7_Click_1(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<StudentsForm>().Any())
            {
                MessageBox.Show("واجهة إضافة التحاليل مفتوحة بالفعل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // إخفاء الواجهة الحالية وفتح الثانية
            this.Hide();

            GoToStage GoToStage = new GoToStage(_userId, _fullName);

            // نضيف حركة Fade In
            GoToStage.Opacity = 0;
            Timer fadeInTimer = new Timer();
            fadeInTimer.Interval = 6;
            fadeInTimer.Tick += (s, args) =>
            {
                if (GoToStage.Opacity < 1)
                    GoToStage.Opacity += 0.05;
                else
                    fadeInTimer.Stop();
            };
            fadeInTimer.Start();

            // لما ينغلق الفورم ترجع فورم المراجعين
            GoToStage.FormClosed += (s, args) =>
            {
                this.Show();
                this.Activate(); // نضمن التركيز يرجع
            };

            GoToStage.Show();
        }
    }
}

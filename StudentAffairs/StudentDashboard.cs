using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudentAffairs
{

    public partial class StudentDashboard : Form
    {
        private int _userId;
        private string _fullName;
        public StudentDashboard(int userId, string fullName)
        {
            InitializeComponent();
            _userId = userId;
            _fullName = fullName;
        }

        private void StudentDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}

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
    public partial class InstructorDashboard : Form
    {
        private int _userId;
        private string _fullName;
        public InstructorDashboard(int userId, string fullName)
        {
            InitializeComponent();
            _userId = userId;
            _fullName = fullName;
        }

        private void InstructorDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}

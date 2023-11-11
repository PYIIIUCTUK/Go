using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Go
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Game game = new Game(this, 9, 9);
            game.Show();
            this.Hide();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Game game = new Game(this, 13, 13);
            game.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Game game = new Game(this, 19, 19);
            game.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

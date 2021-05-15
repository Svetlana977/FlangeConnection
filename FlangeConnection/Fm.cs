using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlangeConnection
{
    public partial class Fm : Form
    {
        private Calc calc;

        int index = 1; // индекс для радиобтн материал

        public Fm()
        {
            InitializeComponent();

            calc = new Calc();


            createRadioBtnMaterial("Сталь");
            createRadioBtnMaterial("Чугун");

            buExit.Click += (s, e) => Application.Exit();
            
        }

        // создание материала
        private void createRadioBtnMaterial(string text)
        {
            RadioButton temp = new RadioButton();
            temp.Text = text;
            temp.Location = new Point(10, 20 * index);
            temp.Name = $"radioButton{index}";
            temp.BackColor = Color.FromName("#D7D5D5");

            groupBoxMaterial.Controls.Add(temp);
            index++;
        }
    }
}

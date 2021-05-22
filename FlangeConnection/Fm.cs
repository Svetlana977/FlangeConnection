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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace FlangeConnection
{
    public partial class Fm : Form
    {
        private SqlConnection SqlConnection = null;
        private Calc calc;

        public double PN { get; private set; }
        public int Temperature { get; private set; }

        public Fm()
        {
            InitializeComponent();

            calc = new Calc();

            tbPressure.TextChanged += TbPressure_TextChanged;
            tbTemperature.TextChanged += TbTemperature_TextChanged;
            tbPressure.KeyPress += TbPressure_KeyPress;
            tbTemperature.KeyPress += TbTemperature_KeyPress;
            tbDiametr.KeyPress += TbDiametr_KeyPress;
            lvEnvironment.SelectedIndexChanged += LvEnvironment_SelectedIndexChanged;

            buExit.Click += (s, e) => Application.Exit();
        }

        private void LvEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvEnvironment.SelectedItems.Count > 0)
                changeListOfDesignSeal();
        }



        private void TbDiametr_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры, клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void TbTemperature_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 45) // цифры, клавиша BackSpace и минус
            {
                e.Handled = true;
            }
        }

        private void TbPressure_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 44) // цифры, клавиша BackSpace и запятая
            {
                e.Handled = true;
            }
        }

        private void SetParams()
        {
            if (tbPressure.Text != "")
                PN = findPN(Convert.ToDouble(tbPressure.Text));
            if (tbTemperature.Text != "")
                Temperature = Convert.ToInt32(tbTemperature.Text);
        }

        private double findPN(double v)
        {
            double convertPressureFromMPaToKgSm2 = Math.Round(10.197162 * v, 1);

            double[] arrPN = new double[] {1, 2.5, 6, 10, 16, 25};
            double PN = arrPN[0];

            for (int i = 0; i < arrPN.Length; i++)
            {
                if (convertPressureFromMPaToKgSm2 <= arrPN[i])
                {
                    PN = arrPN[i];
                    break;
                }
            }
            return PN;
        }


        private void TbPressure_TextChanged(object sender, EventArgs e)
        {
            changeListOfMaterials();
            changeListOfEnvironment();
            changeListOfDesignSeal();
        }

        private void TbTemperature_TextChanged(object sender, EventArgs e)
        {
            changeListOfMaterials();       
        }

        private void changeListOfMaterials()
        {
            lvMaterialOfFlange.Items.Clear();

            if (tbTemperature.Text != "" && tbTemperature.Text != "-" && tbPressure.Text != "")
            {
                SetParams();

                SqlDataReader dataReader = null;

                try
                {
                    SqlCommand sqlCommand = new SqlCommand($"SELECT DISTINCT BrandOfMaterial, GroupOfMaterial FROM FlangeMaterial_ WHERE TemperatureTo > {Convert.ToInt32(tbTemperature.Text)} AND TemperatureFrom  < {Convert.ToInt32(tbTemperature.Text)} AND Pressure > {Convert.ToInt32(PN)}", SqlConnection);

                    dataReader = sqlCommand.ExecuteReader();

                    ListViewItem item = null;

                    while (dataReader.Read())
                    {
                        item = new ListViewItem(new string[] {Convert.ToString(dataReader["GroupOfMaterial"]),
                    Convert.ToString(dataReader["BrandOfMaterial"])});

                        lvMaterialOfFlange.Items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (dataReader != null && !dataReader.IsClosed)
                        dataReader.Close();
                }
            }
        }

        private void changeListOfEnvironment()
        {
            lvEnvironment.Items.Clear();

            if (tbPressure.Text != "")
            {
                SetParams();
                SqlDataReader dataReader = null;

                try
                {
                    SqlCommand sqlCommand = new SqlCommand($"SELECT DISTINCT Environment FROM EnvironmentTable WHERE Pressure > {Convert.ToInt32(PN)}", SqlConnection);

                    dataReader = sqlCommand.ExecuteReader();

                    ListViewItem item = null;

                    while (dataReader.Read())
                    {
                        item = new ListViewItem(new string[] { Convert.ToString(dataReader["Environment"]) });

                        lvEnvironment.Items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (dataReader != null && !dataReader.IsClosed)
                        dataReader.Close();
                }
            }
        }

        private void changeListOfDesignSeal()
        {
            lvDesign.Items.Clear();
            if (tbPressure.Text != "" && lvEnvironment.SelectedItems.Count != 0)
            {
                SetParams();

                SqlDataReader dataReader = null;

                try
                {
                    string str = lvEnvironment.SelectedItems[0].Text;

                    SqlCommand sqlCommand = new SqlCommand($"SELECT DISTINCT Execution FROM EnvironmentTable WHERE Pressure > {Convert.ToInt32(PN)} AND Environment = N'{str}'", SqlConnection);

                    dataReader = sqlCommand.ExecuteReader();

                    ListViewItem item = null;

                    while (dataReader.Read())
                    {
                        item = new ListViewItem(new string[] { Convert.ToString(dataReader["Execution"]) });

                        lvDesign.Items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (dataReader != null && !dataReader.IsClosed)
                        dataReader.Close();
                }
            }
        }
        private void Fm_Load(object sender, EventArgs e)
        {
            SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Database"].ConnectionString);

            SqlConnection.Open();

            changeListOfMaterials();
            changeListOfEnvironment();
        }
    }
}

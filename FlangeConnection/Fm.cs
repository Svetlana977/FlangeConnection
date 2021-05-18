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


        public Fm()
        {
            InitializeComponent();

            calc = new Calc();

            tbPressure.TextChanged += TbPressure_TextChanged;
            tbTemperature.TextChanged += TbTemperature_TextChanged;
            buExit.Click += (s, e) => Application.Exit();
            
        }

        private void TbPressure_TextChanged(object sender, EventArgs e)
        {
            changeListOfMaterials();
        }

        private void TbTemperature_TextChanged(object sender, EventArgs e)
        {
            changeListOfMaterials();       
        }

        private void changeListOfMaterials()
        {
            lvMaterial.Items.Clear();


            if (tbTemperature.Text != "" && tbTemperature.Text != "-" && tbPressure.Text != "")
            {
                SqlDataReader dataReader = null;

                try
                {
                    SqlCommand sqlCommand = new SqlCommand($"SELECT DISTINCT BrandOfMaterial, GroupOfMaterial FROM FlangeMaterial WHERE TemperatureTo > {Convert.ToInt32(tbTemperature.Text)} AND TemperatureFrom  < {Convert.ToInt32(tbTemperature.Text)} AND Pressure > {Convert.ToInt32(tbPressure.Text)}", SqlConnection);

                    dataReader = sqlCommand.ExecuteReader();

                    ListViewItem item = null;

                    while (dataReader.Read())
                    {
                        item = new ListViewItem(new string[] {Convert.ToString(dataReader["GroupOfMaterial"]),
                    Convert.ToString(dataReader["BrandOfMaterial"])});

                        lvMaterial.Items.Add(item);
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

        }
    }
}

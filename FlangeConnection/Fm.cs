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
using System.Diagnostics;

namespace FlangeConnection
{
    public partial class Fm : Form
    {
        private SqlConnection SqlConnection = null;
        private Calc calc;

        private float b_p;
        private float b_0;

        public int D_cp { get; private set; }
        public float P_obj { get; private set; }
        public float R_p { get; private set; }
        public double PN { get; private set; }
        public int Temperature { get; private set; }
        public int Diametr { get; private set; }
        public int S { get; private set; }

        public Fm()
        {
            InitializeComponent();

            calc = new Calc();


            buExit.Click += (s, e) => Application.Exit();
        }

        // чтение данных введенных пользователем
        private void SetParams()
        {
            if (tbPressure.Text != "" && tbPressure.Text != ",")
                PN = findPN(Convert.ToDouble(tbPressure.Text));
            if (tbTemperature.Text != "" && tbTemperature.Text != "-")
                Temperature = Convert.ToInt32(tbTemperature.Text);
            if (tbDiametr.Text != "")
                Diametr = Convert.ToInt32(tbDiametr.Text);
            if (tbS.Text != "")
                S = Convert.ToInt32(tbS.Text);
        }

        // функция нахождения номинального диаметра из внутреннего давления среды
        private double findPN(double v)
        {
            if (v != 0)
            {
                double convertPressureFromMPaToKgSm2 = Math.Round(10.197162 * v, 1);

                double[] arrPN = new double[] { 1, 2.5, 6, 10, 16, 25 };
                PN = 5e4;

                for (int i = 0; i < arrPN.Length; i++)
                {
                    if (convertPressureFromMPaToKgSm2 <= arrPN[i])
                    {
                        PN = arrPN[i];
                        break;
                    }
                }
            }
            else
                PN = 0;
            return PN;
        }

        // функция для обновления списка материалов фланца
        private void changeListOfMaterialsOfFlange()
        {
            lvMaterialOfFlange.Items.Clear();

            if (tbTemperature.Text != "" && tbTemperature.Text != "-" && tbPressure.Text != "")
            {
                SetParams();

                if (Temperature <= 300 && Temperature >= -70 && PN >= 1 && PN <= 25)
                {
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
        }

        // функция для обновления списка сред
        private void changeListOfEnvironment()
        {
            lvEnvironment.Items.Clear();

            if (tbPressure.Text != "" && tbTemperature.Text != "" && tbTemperature.Text != "-")
            {
                SetParams();

                if (PN >= 1 && PN <= 25 && Temperature >=-70 && Temperature <=300)
                {
                    SqlDataReader dataReader = null;

                    try
                    {
                        SqlCommand sqlCommand = new SqlCommand($"SELECT DISTINCT EnvironmentOfSeal FROM EnvironmentForMaterialOfSeal WHERE Pressure > {Convert.ToInt32(PN)}", SqlConnection);

                        dataReader = sqlCommand.ExecuteReader();

                        ListViewItem item = null;

                        while (dataReader.Read())
                        {
                            item = new ListViewItem(new string[] { Convert.ToString(dataReader["EnvironmentOfSeal"]) });

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
        }

        // функция для обновления списка исполнения уплотнительных поверхностей
        private void changeListOfDesignSeal()
        {
            lvDesign.Items.Clear();
            if (tbPressure.Text != "" && tbTemperature.Text != "" && tbDiametr.Text != "" && lvEnvironment.SelectedItems.Count != 0)
            {
                SetParams();

                if (PN >= 1 && PN <= 25 && Temperature >= -70 && Temperature <= 300 && Diametr >= 15 && Diametr <= 2420)
                {
                    SqlDataReader dataReader = null;

                    try
                    {
                        string str = lvMaterialOfSeal.SelectedItems[0].SubItems[1].Text;
                        SqlCommand sqlCommand = new SqlCommand($"SELECT DISTINCT DesignOfSeal, MaterialCut FROM DesignOfPlateSeal, TypeAndMaterialOfSeal WHERE PressurePNFrom <= {PN.ToString().Replace(',', '.')} AND PressurePNTo >= {PN.ToString().Replace(',', '.')} AND DiametrDNFrom <= {Diametr} AND DiametrDNTo >= {Diametr} AND MaterialOFSeal LIKE '%' + MaterialCut + '%' AND Material = N'{str}'", SqlConnection);

                        dataReader = sqlCommand.ExecuteReader();

                        ListViewItem item = null;

                        while (dataReader.Read())
                        {
                            item = new ListViewItem(new string[] { Convert.ToString(dataReader["DesignOfSeal"]) });

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
        }

        // функция для обновления списка материалов прокладок
        private void changeListOfMaterialsOfSeal()
        {
            lvMaterialOfSeal.Items.Clear();
            if (tbPressure.Text != "" && tbTemperature.Text != "" && tbDiametr.Text != "" && lvEnvironment.SelectedItems.Count != 0)
            {
                SetParams();

                if (PN >= 1 && PN <= 25 && Temperature >= -70 && Temperature <= 300 && Diametr >= 15 && Diametr <= 2420)
                {
                    SqlDataReader dataReader = null;

                    try
                    {
                        string str = lvEnvironment.SelectedItems[0].Text;

                        SqlCommand sqlCommand = new SqlCommand($"SELECT DISTINCT tmp.Material, tmp.Type_seal FROM(SELECT DISTINCT MaterialOfSealCut, MaterialOfSeal, EnvironmentOfSeal, TemperatureFrom, TemperatureTo, PressurePNTo, Material, Type_seal, DiametrDNFrom, DiametrDNTo FROM EnvironmentForMaterialOfSeal, TypeAndMaterialOfSeal, DesignOfPlateSeal WHERE EnvironmentOfSeal = N'{str}' AND Material LIKE '%' + MaterialOfSealCut + '%' AND (TemperatureFrom <= {Temperature} OR TemperatureFrom is null) AND (TemperatureTo >= {Temperature} OR TemperatureTo is null) AND PressurePNTo >= {PN.ToString().Replace(',', '.')} AND MaterialOfSeal LIKE '%' + MaterialOfSealCut + '%' AND DiametrDNFrom <= {Diametr} AND DiametrDNTo >= {Diametr} ) tmp", SqlConnection);

                        dataReader = sqlCommand.ExecuteReader();

                        ListViewItem item = null;

                        while (dataReader.Read())
                        {
                            item = new ListViewItem(new string[] {Convert.ToString(dataReader["Type_seal"]),
                    Convert.ToString(dataReader["Material"])});

                            lvMaterialOfSeal.Items.Add(item);
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

            // очистка списка исполнений, так как не выбран материал прокладки
            lvDesign.Items.Clear();
        }
        private void Fm_Load(object sender, EventArgs e)
        {
            // соединение с базой данных при загрузке
            SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Database"].ConnectionString);

            SqlConnection.Open();
        }

        private void tbPressure_TextChanged_1(object sender, EventArgs e)
        {
            // при изменении давления обновить список материалов, среды, исполнения
            changeListOfMaterialsOfFlange();
            changeListOfEnvironment();
            changeListOfMaterialsOfSeal();
            changeListOfDesignSeal();
        }

        private void tbTemperature_TextChanged_1(object sender, EventArgs e)
        {
            // при изменении температуры обновить список материалов, среды, исполнения
            changeListOfMaterialsOfFlange();
            changeListOfEnvironment();
            changeListOfMaterialsOfSeal();
            changeListOfDesignSeal();
        }

        private void tbTemperature_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            // ввод только цифр, клавишы BackSpace и минус
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 45)
            {
                e.Handled = true;
            }

            // минус только первым символом
            var tb = (Guna2TextBox)sender;
            if (number.ToString().Equals("-"))
            {
                e.Handled = tb.SelectionStart != 0 || tb.Text.IndexOf("-") != -1;
                if (!e.Handled)
                    return;
            }
        }

        private void tbPressure_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            // ввод только цифр, клавишы BackSpace и запятой
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 44)
            {
                e.Handled = true;
            }

            // запятая только вторым символом
            var tb = (Guna2TextBox)sender;
            if (number.ToString().Equals(","))
            {
                e.Handled = tb.SelectionStart != 1 || tb.Text.IndexOf("-") != -1;
                if (!e.Handled)
                    return;
            }

        }

        private void tbDiametr_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            // ввод только цифр и клавишы BackSpace
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) 
            {
                e.Handled = true;
            }            
        }

        private void lvEnvironment_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // обновить список материалов прокладки, если выбрана среда
            if (lvEnvironment.SelectedItems.Count > 0)
                changeListOfMaterialsOfSeal();
            // обновить список исполнений прокладки, если выбран ее материал
            if (lvMaterialOfSeal.SelectedItems.Count > 0)
                changeListOfDesignSeal();
        }

        

        private void tbS_KeyPress(object sender, KeyPressEventArgs e)
        {
            // ввод только цифр, клавишы BackSpace
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8)
            {
                e.Handled = true;
            }
        }

        private void lvMaterialOfSeal_SelectedIndexChanged(object sender, EventArgs e)
        {
            // обновить список исполнений прокладки, если выбран ее материал
            if (lvMaterialOfSeal.SelectedItems.Count > 0)
                changeListOfDesignSeal();
        }

        private void tbDiametr_TextChanged(object sender, EventArgs e)
        {           
            // обновить список материалов прокладки, если выбрана среда
            if (lvEnvironment.SelectedItems.Count > 0)
                changeListOfMaterialsOfSeal();
        }

        private void buCalc_Click(object sender, EventArgs e)
        {
            calcPoint5();
        }

        // 5 пункт по ГОСТ 34233.4-2017 по расчету фланцевого соединения на прочность и герметичность
        private void calcPoint5()
        {
            SetParams();
            richTextBox1.Clear();
            // вычисление ширины прокладки в зависимости от выбранного исполнения и материала прокладки
            b_p = calc.findWidthOfSeal(SqlConnection, Convert.ToInt32(tbDiametr.Text), Convert.ToDouble(tbPressure.Text), lvMaterialOfSeal.SelectedItems[0].SubItems[1].Text, lvDesign.SelectedItems[0].Text);
            // вычисление эффективной ширины прокладки
            b_0 = calc.findEffectWidthOfSeal(b_p);
            // вычисление расчетного диаметра плоских прокладок
            D_cp = calc.findCalculatedDiametr(SqlConnection, Convert.ToInt32(tbDiametr.Text), Convert.ToDouble(tbPressure.Text), lvMaterialOfSeal.SelectedItems[0].SubItems[1].Text, lvDesign.SelectedItems[0].Text, b_0);
            // усилие необходимое для смятия прокладки при затяжке
            P_obj = calc.findTighteningForce(D_cp, b_0, SqlConnection, lvMaterialOfSeal.SelectedItems[0].SubItems[1].Text);
            // усилие на прокладке в рабочих условиях,
            // необходимое для обеспечения герметичности фланцевого соединения
            R_p = calc.findForceUnderOperatingConditions(D_cp, b_0, Convert.ToDouble(tbPressure.Text), SqlConnection, lvMaterialOfSeal.SelectedItems[0].SubItems[1].Text);

            richTextBox1.AppendText($"Ширина прокладки = {b_p} мм\n" +
                $"Эффективная ширина = {b_0} мм\n" +
                $"Расчетный диаметр = {D_cp} мм\n" +
                $"Усилие необходимое для смятия прокладки при затяжке = {P_obj} Н\n" +
                $"Усилие на прокладке в рабочих условиях, необходимое для обеспечения герметичности фланцевого соединения = {R_p} Н\n");
        }

    }
}

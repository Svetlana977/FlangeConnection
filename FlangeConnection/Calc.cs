using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlangeConnection
{
    class Calc
    {        
        // вычисление ширины прокладки по данным из базы
        internal float findWidthOfSeal(SqlConnection sqlConnection, int diametr, double pressure, string materialOfSeal, string DesignOfSeal)
        {
            float widthOfSeal = -1;
            SqlCommand sqlCommand;
            SqlDataReader dataReader = null;

            // проверка на материал прокладки
            if (materialOfSeal.Contains("паронит") || materialOfSeal.Contains("картон асбестовый") || materialOfSeal.Contains("резина") || materialOfSeal.Contains("фторопласт 4"))
            {
                try
                {
                    // разные запросы в зависимости от исполнения прокладки
                    switch (DesignOfSeal)
                    {
                        case "плоскость":
                            sqlCommand = requestForPloskostSeal(sqlConnection, diametr, pressure);
                            dataReader = sqlCommand.ExecuteReader();
                            while (dataReader.Read())
                            {
                                widthOfSeal = (Convert.ToInt32(dataReader["Type01_D"]) - Convert.ToInt32(dataReader["d"])) / 2;
                            }
                            break;
                        case "соединительный выступ - соединительный выступ":
                        case "соединительный выступ - паз":
                        case "выступ - впадина":
                            sqlCommand = requestForNOTftoroplasrSeal(sqlConnection, diametr, pressure, DesignOfSeal);
                            dataReader = readData(ref widthOfSeal, sqlCommand);
                            break;
                        case "шип - паз":
                            if (materialOfSeal.Contains("фторопласт 4"))
                                sqlCommand = requestForFtoroplastSeal(sqlConnection, diametr, pressure);
                            else
                                sqlCommand = requestForNOTftoroplasrSeal(sqlConnection, diametr, pressure, DesignOfSeal);
                            dataReader = readData(ref widthOfSeal, sqlCommand);
                            break;
                    }               
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    if (dataReader != null && !dataReader.IsClosed)
                        dataReader.Close();
                }
            }
            return widthOfSeal;
        }

        private SqlCommand requestForPloskostSeal(SqlConnection sqlConnection, int diametr, double pressure)
        {
            return new SqlCommand($"SELECT Type01_D, d FROM(SELECT Type01_DN, Type01_PN, Type01_D, D_, d, RANK() OVER(ORDER BY Type01_PN ASC) rnk1  FROM(SELECT Type01_D, Type01_DN, Type01_PN, DN, D_, d, Ftoroplast, Design, RANK() OVER(ORDER BY DN DESC) rnk FROM SizeOfPlateSeal, FlangeSizeType01 WHERE Ftoroplast IS NULL  AND Design = N'соединительный выступ - соединительный выступ' AND DN <= {diametr} AND PNfrom <= {convertPressureToPN(pressure).ToString().Replace(',', '.')} AND PNto >= {convertPressureToPN(pressure).ToString().Replace(',', '.')} AND Type01_DN LIKE DN AND Type01_PN >= {convertPressureToPN(pressure).ToString().Replace(',', '.')}) cte1 WHERE rnk = 1) cte2 WHERE rnk1 = 1; ", sqlConnection);
        }

        // запрос для прокладки из фторопласта шип - паз
        private SqlCommand requestForFtoroplastSeal(SqlConnection sqlConnection, int diametr, double pressure)
        {            
            return new SqlCommand($"SELECT D_, d FROM(SELECT DN, D_, d, Ftoroplast, RANK() OVER(ORDER BY DN DESC) rnk FROM SizeOfPlateSeal WHERE Ftoroplast = 'true' AND DN <= {diametr} AND PNfrom <= {convertPressureToPN(pressure).ToString().Replace(',', '.')} AND PNto >= {convertPressureToPN(pressure).ToString().Replace(',', '.')}) cte1 WHERE rnk = 1; ", sqlConnection);
        }

        // запрос для остальных плоских прокладок
        private SqlCommand requestForNOTftoroplasrSeal(SqlConnection sqlConnection, int diametr, double pressure, string DesignOfSeal)
        {
            return new SqlCommand($"SELECT D_, d FROM(SELECT DN, D_, d, Ftoroplast, Design, RANK() OVER(ORDER BY DN DESC) rnk FROM SizeOfPlateSeal WHERE Ftoroplast IS NULL  AND Design = N'{DesignOfSeal}' AND DN <= {diametr} AND PNfrom <= {convertPressureToPN(pressure).ToString().Replace(',', '.')} AND PNto >= {convertPressureToPN(pressure).ToString().Replace(',', '.')}) cte1 WHERE rnk = 1; ", sqlConnection);
        }

        // вычисление ширины прокладки
        private static SqlDataReader readData(ref float widthOfSeal, SqlCommand sqlCommand)
        {
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            while (dataReader.Read())
            {
                widthOfSeal = (Convert.ToInt32(dataReader["D_"]) - Convert.ToInt32(dataReader["d"])) / 2;
            }
            
            return dataReader;
        }

        // перевод давления p в PN
        private double convertPressureToPN(double pressure)
        {
            return Math.Round(10.197162 * pressure, 1);
        }

        // поиск эффективной ширины плоской прокладки
        internal float findEffectWidthOfSeal(float b_p)
        {
            return (float)Math.Round(b_p <= 15 ? b_p : 3.8 * Math.Sqrt(b_p));
        }

        // вычисление расчетного диаметра прокладки по данным из базы
        internal int findCalculatedDiametr(SqlConnection sqlConnection, int diametr, double pressure, string materialOfSeal, string DesignOfSeal, float b_0)
        {
            int Diametr = -1;
            SqlCommand sqlCommand;
            SqlDataReader dataReader = null;

            // проверка на материал прокладки
            if (materialOfSeal.Contains("паронит") || materialOfSeal.Contains("картон асбестовый") || materialOfSeal.Contains("резина") || materialOfSeal.Contains("фторопласт 4"))
            {        
                try
                {
                    // разные запросы в зависимости от исполнения прокладки
                    switch (DesignOfSeal)
                    {
                        case "плоскость":
                            sqlCommand = requestForPloskostSeal(sqlConnection, diametr, pressure);
                            dataReader = sqlCommand.ExecuteReader();
                            while (dataReader.Read())
                            {
                                Diametr = ((int)(Convert.ToInt32(dataReader["Type01_D"]) - b_0));
                            }
                            break;
                        case "соединительный выступ - соединительный выступ":
                        case "соединительный выступ - паз":
                        case "выступ - впадина":
                            sqlCommand = requestForNOTftoroplasrSeal(sqlConnection, diametr, pressure, DesignOfSeal);
                            dataReader = readData1(ref Diametr, sqlCommand, b_0);
                            break;
                        case "шип - паз":
                            if (materialOfSeal.Contains("фторопласт 4"))
                                sqlCommand = requestForFtoroplastSeal(sqlConnection, diametr, pressure);
                            else
                                sqlCommand = requestForNOTftoroplasrSeal(sqlConnection, diametr, pressure, DesignOfSeal);
                            dataReader = readData1(ref Diametr, sqlCommand, b_0);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    if (dataReader != null && !dataReader.IsClosed)
                        dataReader.Close();
                }
            }
            return Diametr;
        }

        // вычисление расчетного диаметра прокладки
        private static SqlDataReader readData1(ref int Diametr, SqlCommand sqlCommand, float b_0)
        {
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            while (dataReader.Read())
            {
                Diametr = ((int)(Convert.ToInt32(dataReader["D_"]) - b_0));
            }
            return dataReader;
        }


        // функция для вычисления усилия, необходимого для смятия прокладки при затяжке
        internal float findTighteningForce(int d_cp, float b_0, SqlConnection sqlConnection, string material)
        {
            float P = -1;
            int q_obzh_;
            SqlDataReader dataReader = null;

            try
            {
                SqlCommand sqlCommand = new SqlCommand($"SELECT q_obzh FROM TypeAndMaterialOfSeal WHERE Material LIKE N'{material}'", sqlConnection);

                dataReader = sqlCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    q_obzh_ = Convert.ToInt32(dataReader["q_obzh"]);
                    P = (float)(0.5 * Math.PI * d_cp * b_0 * q_obzh_);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                    dataReader.Close();
            }
            return P;
        }

        // усилие на прокладке в рабочих условиях,
        // необходимое для обеспечения герметичности фланцевого соединения
        internal float findForceUnderOperatingConditions(int d_cp, float b_0, double pressure, SqlConnection sqlConnection, string material)
        {
            float R = -1;
            double coef_m;
            SqlDataReader dataReader = null;

            try
            {
                SqlCommand sqlCommand = new SqlCommand($"SELECT Coef_m FROM TypeAndMaterialOfSeal WHERE Material LIKE N'{material}'", sqlConnection);

                dataReader = sqlCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    coef_m = Convert.ToDouble(dataReader["Coef_m"]);
                    R = (float)(Math.PI * d_cp * b_0 * coef_m * pressure);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                    dataReader.Close();
            }
            return R;
        }
    }
}

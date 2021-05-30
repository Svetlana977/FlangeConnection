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

            // проверка на материал прокладки
            if (materialOfSeal.Contains("паронит") || materialOfSeal.Contains("картон асбестовый") || materialOfSeal.Contains("резина") || materialOfSeal.Contains("фторопласт 4"))
            {
                SqlCommand sqlCommand;
                SqlDataReader dataReader = null;

                try
                {
                    // разные запросы в зависимости от исполнения прокладки
                    switch (DesignOfSeal)
                    {
                        case "плоскость":
                            widthOfSeal = 14; // исправить!!!!
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
                    return -1;
                }
                finally
                {
                    if (dataReader != null && !dataReader.IsClosed)
                        dataReader.Close();
                }
            }
            return widthOfSeal;
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

            // проверка на материал прокладки
            if (materialOfSeal.Contains("паронит") || materialOfSeal.Contains("картон асбестовый") || materialOfSeal.Contains("резина") || materialOfSeal.Contains("фторопласт 4"))
            {
                SqlCommand sqlCommand;
                SqlDataReader dataReader = null;

                try
                {
                    // разные запросы в зависимости от исполнения прокладки
                    switch (DesignOfSeal)
                    {
                        case "плоскость":
                            Diametr = 140; // исправить!!!!
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
                    return -1;
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
    }
}

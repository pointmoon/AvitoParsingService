using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Net;
using System.IO;
using System.Data.SqlClient;

namespace AvitoParsingService
{
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы Service1.svc или Service1.svc.cs в обозревателе решений и начните отладку.
    public class AvitoParser : IAvitoParser
    {
        private const string ConnectionString = @"Data Source=LAPTOP_TUNS;Initial Catalog=FlatsAvito;Integrated Security=True";
        #region IsActive

        /// <summary>
        /// проверка работоспособности
        /// </summary>
        /// <returns> TRUE </returns>
        public bool IsActive()
        {
            return true;
        }

        #endregion

        #region Working Method

        /// <summary>
        /// непосредственно парсинг
        /// </summary>
        /// <param name="p1"> параметр 1 </param>
        /// <param name="p2"> параметр 2 </param>
        /// <param name="etc"> параметр etc </param>
        /// <returns> TRUE or FALSE </returns>
        public bool Parsing(string parsingString)
        {
            int getParse = 0;
            string next = "/sankt-peterburg/kvartiry/2-k_kvartira_64_m_924_et._1383948453";

            #region MSSQL Connect

            SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();

            #endregion

            while (getParse != 10)
            {
                Flat newFlat = new Flat();
                WebRequest req = WebRequest.Create("https://www.avito.ru" + next);
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                string Out = sr.ReadToEnd();
                sr.Close();

                HtmlAgilityPack.HtmlDocument answers = new HtmlAgilityPack.HtmlDocument();
                answers.LoadHtml(Out);
                HtmlAgilityPack.HtmlNodeCollection nods = answers.DocumentNode.SelectNodes("//a");

                foreach (var node in nods)
                {
                    try
                    {
                        if (node.InnerText == "Следующее &rarr;")
                        {
                            next = "";
                            next = node.Attributes[1].Value;

                            //Start get info
                            HtmlAgilityPack.HtmlNodeCollection _params = answers.DocumentNode.SelectNodes("//li");
                            foreach (var param in _params)
                            {
                                try
                                {
                                    if (param.Attributes[0].Value == "item-params-list-item")
                                    {
                                        switch (param.ChildNodes[1].InnerText)
                                        {
                                            case "Этаж: ":
                                                newFlat.floor = param.ChildNodes[2].InnerText;
                                                break;
                                            case "Этажей в доме: ":
                                                newFlat.floorInHome = param.ChildNodes[2].InnerText;
                                                break;
                                            case "Тип дома: ":
                                                newFlat.homeType = param.ChildNodes[2].InnerText;
                                                break;
                                            case "Количество комнат: ":
                                                newFlat.countRoom = param.ChildNodes[2].InnerText;
                                                break;
                                            case "Общая площадь: ":
                                                newFlat.totalArea = param.ChildNodes[2].InnerText;
                                                break;
                                            case "Жилая площадь: ":
                                                newFlat.lifeArea = param.ChildNodes[2].InnerText;
                                                break;
                                            default:
                                                //Console.Write(param.ChildNodes[2].InnerText);
                                                break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string sqlExp = "INSERT INTO [log] ([logMessage], [timeDate]) VALUES (@logMessage, @timeDate)";
                                    SqlCommand querry = new SqlCommand(sqlExp, sqlConnection);
                                    querry.Parameters.AddWithValue("@logMessage", ex.Message);
                                    querry.Parameters.AddWithValue("@timeDate", DateTime.Now.ToString());
                                    querry.ExecuteNonQuery();
                                }
                            }

                            HtmlAgilityPack.HtmlNodeCollection _price = answers.DocumentNode.SelectNodes("//meta");
                            foreach (var param in _price)
                            {
                                try
                                {
                                    if (param.Attributes[0].Value == "product:price:amount")
                                    {
                                        newFlat.price = param.Attributes[1].Value;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string sqlExp = "INSERT INTO [log] ([logMessage], [timeDate]) VALUES (@logMessage, @timeDate)";
                                    SqlCommand querry = new SqlCommand(sqlExp, sqlConnection);
                                    querry.Parameters.AddWithValue("@logMessage", ex.Message);
                                    querry.Parameters.AddWithValue("@timeDate", DateTime.Now.ToString());
                                    querry.ExecuteNonQuery();
                                }
                            }

                            HtmlAgilityPack.HtmlNodeCollection _links = answers.DocumentNode.SelectNodes("//div");
                            foreach (var param in _links)
                            {
                                try
                                {
                                    if (param.Attributes[0].Value == "js-social-share social-share")
                                    {
                                        newFlat.link = param.Attributes[4].Value;
                                        newFlat.summary = param.Attributes[3].Value;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string sqlExp = "INSERT INTO [log] ([logMessage], [timeDate]) VALUES (@logMessage, @timeDate)";
                                    SqlCommand querry = new SqlCommand(sqlExp, sqlConnection);
                                    querry.Parameters.AddWithValue("@logMessage", ex.Message);
                                    querry.Parameters.AddWithValue("@timeDate", DateTime.Now.ToString());
                                    querry.ExecuteNonQuery();
                                }
                            }
                            getParse++;
                            System.Threading.Thread.Sleep(2000);
                            if (getParse % 3 == 0)
                            {
                                System.Threading.Thread.Sleep(15000);
                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        string sqlExp = "INSERT INTO [log] ([logMessage], [timeDate]) VALUES (@logMessage, @timeDate)";
                        SqlCommand querry = new SqlCommand(sqlExp, sqlConnection);
                        querry.Parameters.AddWithValue("@logMessage", ex.Message);
                        querry.Parameters.AddWithValue("@timeDate", DateTime.Now.ToString());
                        querry.ExecuteNonQuery();
                    }
                }
                string sqlExpression = "INSERT INTO [Flats] ([summary], [floor], [floorInHome], [homeType], [countRoom], [totalArea], [lifeArea], [price], [link]) VALUES (@summary, @floor, @floorInHome, @homeType, @countRoom, @totalArea, @lifeArea, @price, @link)";
                SqlCommand command = new SqlCommand(sqlExpression, sqlConnection);
                command.Parameters.AddWithValue("@summary", newFlat.summary);
                command.Parameters.AddWithValue("@floor", newFlat.floor);
                command.Parameters.AddWithValue("@floorInHome", newFlat.floorInHome);
                command.Parameters.AddWithValue("@homeType", newFlat.homeType);
                command.Parameters.AddWithValue("@countRoom", newFlat.countRoom);
                command.Parameters.AddWithValue("@totalArea", newFlat.totalArea);
                command.Parameters.AddWithValue("@lifeArea", newFlat.lifeArea);
                command.Parameters.AddWithValue("@price", newFlat.price);
                command.Parameters.AddWithValue("@link", newFlat.link);
                command.ExecuteNonQuery();
            }
            sqlConnection.Close();
            return true;
        }

        #endregion
    }
}

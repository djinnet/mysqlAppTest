using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace mySqlTransAction
{
    public class Program
    {
        static void Main(string[] args)
        {
            string connString = ConfigurationManager.ConnectionStrings["ConnectionMysql"].ConnectionString;
            RunTransaction(connString);
        }

        private static void RunTransaction(string myConnString)
        {
            MySqlConnection sqlConnection = new MySqlConnection(myConnString);
            sqlConnection.Open();

            MySqlCommand sqlCommand = sqlConnection.CreateCommand();
            MySqlTransaction sqlTransaction;

            sqlTransaction = sqlConnection.BeginTransaction();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.Transaction = sqlTransaction;

            try
            {
                Console.WriteLine("  1) READ UNCOMMITTED");
                Console.WriteLine("  2) READ COMMITTED");
                Console.WriteLine("  3) SERIALIZABLE");

                Console.WriteLine("Vælg Dit Isolation Lvl: ");

                string input = Console.ReadLine();

                var VælgLevel = Convert.ToInt32(input);

                string s;
                string selectSQL;

                MySqlDataReader dataReader;

                switch (VælgLevel)
                {
                    case 1:
                        s = "SET GLOBAL TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
                        break;
                    case 2:
                        s = "SET GLOBAL TRANSACTION ISOLATION LEVEL READ COMMITTED";
                        break;
                    case 3:
                        s = "SET GLOBAL TRANSACTION ISOLATION LEVEL SERIALIZABLE";
                        break;

                    default:
                        s = "";
                        break;
                }

                
                //sqlCommand.CommandText = s;
                sqlCommand.CommandText = s;
                sqlCommand.ExecuteNonQuery();
                //Console.ReadLine();
                Console.WriteLine($" {s}");

                Console.WriteLine("Indtast ID eller ");
                Console.WriteLine("Indtast -1 for alle");

                string indput2 = Console.ReadLine();
                int PersonID = Convert.ToInt32(indput2);
                

                switch (PersonID)
                {
                    case -1:
                        {
                            selectSQL = "SELECT * FROM Kunde";

                            sqlCommand = new MySqlCommand(selectSQL, sqlConnection);
                            dataReader = sqlCommand.ExecuteReader(); // NB new method used here
                            Console.WriteLine(selectSQL);

                            while (dataReader.Read())
                            {
                                Console.WriteLine("  ID: " + dataReader["ID"] + " Navn: " + dataReader["Navn"] +
                                    " Saldo: " + dataReader["Saldo"]);
                            }
                            dataReader.Close();
                            break;
                        }
                    default:
                        {
                            selectSQL = "SELECT Saldo FROM Kunde WHERE ID = " + PersonID + "; ";
                            sqlCommand = new MySqlCommand(selectSQL, sqlConnection);
                            dataReader = sqlCommand.ExecuteReader(); // NB new method used here
                            Console.WriteLine("  " + selectSQL);
                            dataReader.Read(); // first advance the curser to the next tuple.
                            int hentetSaldoInt = dataReader.GetInt32(0);

                            Console.WriteLine("  Saldo = " + hentetSaldoInt);
                            dataReader.Close(); // close nicely the ResultSet

                            Console.Write("Læg følgende beløb til Saldo: ");
                            //Break 3
                            var saldoLægTil = Console.ReadLine();
                            int saldoLægTilInt = Convert.ToInt32(saldoLægTil);
                            int nySaldo = saldoLægTilInt + hentetSaldoInt;

                            selectSQL = "UPDATE Kunde SET Saldo = '" + nySaldo + "' WHERE ID = " +
                                        PersonID + " ; ";
                            sqlCommand = new MySqlCommand(selectSQL, sqlConnection);
                            sqlCommand.ExecuteNonQuery();
                            Console.WriteLine("  " + selectSQL);

                            selectSQL = "SELECT Saldo FROM Kunde WHERE ID = " + PersonID + "; ";
                            sqlCommand = new MySqlCommand(selectSQL, sqlConnection);
                            dataReader = sqlCommand.ExecuteReader(); // NB new method used here
                            Console.WriteLine("  " + selectSQL);
                            dataReader.Read(); // first advance the curser to the next tuple.
                            hentetSaldoInt = dataReader.GetInt32(0);
                            Console.WriteLine("  FirstName is: " + hentetSaldoInt);
                            dataReader.Close(); // close nicely the ResultSet
                            break;
                        }
                }

                Console.WriteLine("1 for COMMIT");
                Console.WriteLine("2 for ROLLBACK");

                //Break 4
                string svar = Console.ReadLine(); // halt, department not released yet
                int svarID = Convert.ToInt32(svar);

                if (svarID == 2)
                {
                    s = "ROLLBACK";
                }
                else if (svarID == 1)
                {
                    s = "commit";
                }
                sqlCommand = new MySqlCommand(s, sqlConnection);
                sqlCommand.ExecuteNonQuery();
                Console.WriteLine(s);

                Console.WriteLine("Tryk en tast for at lukke ned.");
                //Break 5
                s = Console.ReadLine();

            }
            catch (Exception e)
            {

                try
                {
                    sqlTransaction.Rollback();
                }
                catch (MySqlException ex)
                {
                    if (sqlTransaction.Connection != null)
                    {
                        Console.WriteLine($"An exception of type { ex.GetType() } was encountered while attempting to roll back the transaction.");
                    }
                }
                Console.WriteLine($"An exception of type { e.GetType() } was encountered while inserting the data.");
                Console.WriteLine("Neither record was written to database.");
            }
            finally
            {
                sqlConnection.Close();
            }
        }
    }

    internal class Person
    {
        public string firstname { get; set; }
    }
}

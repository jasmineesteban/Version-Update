using System;
using System.Timers;
using MySql.Data.MySqlClient;

namespace Price_Checker.Services
{
    internal class DataRetrievalService
    {
        private Timer timer;
        private int interval = 5000; // Default interval is 5 seconds (5000 milliseconds)
        private string connectionString;
        private DatabaseConfig _config;

        public enum TimeUnit
        {
            Seconds,
            Minutes,
            Hours
        }

        public DataRetrievalService(DatabaseConfig databaseConfig)
        {
            _config = databaseConfig;
            connectionString = $"server={_config.Server};port={_config.Port};uid={_config.Uid};pwd={_config.Pwd};database={_config.Database}";
            // Initialize the timer
            timer = new Timer(interval);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Call the stored procedure
            CallStoredProcedure();
        }

        private void CallStoredProcedure()
        {
            string storedProcedure = "call bgc_prod_verifier.insert_price()";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(storedProcedure, connection);
                command.CommandType = System.Data.CommandType.Text;

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        public void SetInterval(int value, TimeUnit unit)
        {
            // Stop the timer
            timer.Stop();

            // Update the interval based on the time unit
            switch (unit)
            {
                case TimeUnit.Seconds:
                    interval = value * 1000; // Convert seconds to milliseconds
                    break;
                case TimeUnit.Minutes:
                    interval = value * 60 * 1000; // Convert minutes to milliseconds
                    break;
                case TimeUnit.Hours:
                    interval = value * 60 * 60 * 1000; // Convert hours to milliseconds
                    break;
            }

            // Restart the timer with the new interval
            timer.Interval = interval;
            timer.Start();
        }
    }
}
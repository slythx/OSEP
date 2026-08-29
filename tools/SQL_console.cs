using System;
using System.Data.SqlClient;

namespace SQL_console
{
    class Program
    {
        static void show_help()
        {
            Console.WriteLine("[+] List of available commands: ");
            Console.WriteLine("- help\t\t:\tTo list available commands.");
            Console.WriteLine("- reconnect\t:\tEnter new SQL server and database name to establish new connection.");
            Console.WriteLine("- status\t:\tCheck current connection and user context.");
            Console.WriteLine("- exit\t\t:\tExit console application.");
            Console.WriteLine();
        }
        static SqlConnection server_connect() 
        { 
            Console.Write("Enter Server Name: ");
            String sqlServer = Console.ReadLine();
            Console.Write("Enter Database Name: ");
            String database = Console.ReadLine();
            String conString = "Server = " + sqlServer + "; Database = " + database + "; Integrated Security = True;";
            try
            {
                SqlConnection conn = new SqlConnection(conString);
                conn.Open();
                check_status(conn);
                return conn;
            }
            catch (Exception ex){
                Console.WriteLine("error: " + ex.Message);
                Environment.Exit(1);
                return null; 
            }
        }

        static void check_status(SqlConnection conn)
        {
            String querylogin = @"
                                SELECT SYSTEM_USER AS sys_user,
                                IS_SRVROLEMEMBER('sysadmin') AS sysadmin, 
                                USER_NAME() as DatabaseUser, 
                                IS_MEMBER('db_owner') AS IsDbOwner;";
            SqlCommand command = new SqlCommand(querylogin, conn);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("[+] Connected to '" +  conn.DataSource + "' SQL Server");
            Console.WriteLine("[+] Database name '" + conn.Database + "' ");
            Console.WriteLine("[+] Executing in the context of: ");
            Console.WriteLine("- System User: " + reader["sys_user"]);
            Console.WriteLine("- System Admin: " + reader["sysadmin"]);
            Console.WriteLine("- DB User: " + reader["DatabaseUser"]);
            Console.WriteLine("- DB Owner: " + reader["IsDbOwner"]); 
            reader.Close();
            Console.WriteLine();
        }

        static void perform_sql_query(SqlConnection conn, String query, String trimmed)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                 
                if (trimmed.StartsWith("SELECT") ||
                    trimmed.StartsWith("INSERT") ||
                    trimmed.StartsWith("UPDATE") ||
                    trimmed.StartsWith("DELETE") || 
                    trimmed.StartsWith("WITH") ||
                    trimmed.StartsWith("EXEC") ||
                    trimmed.StartsWith("USE") ||
                    trimmed.StartsWith("SP_"))
                {
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        Console.WriteLine("\n(No rows returned)");
                    }
                    else
                    {
                        // Print column names
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader.GetName(i),-30}");
                        }
                        Console.WriteLine();

                        Console.WriteLine(new string('-', reader.FieldCount * 30));

                        // Print rows
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader[i],-30}");
                            }
                            Console.WriteLine();
                        }
                        Console.WriteLine();
                    }
                    reader.Close();
                }
                else
                {
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"\nRows affected: {rows}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[!] SQL Error: {ex.Message}");
            }
        }
         
        static void Main(string[] args)
        {  
            try
            {
                SqlConnection conn = server_connect(); 

                while (true)
                {
                    Console.Write("[+] Enter Command or SQL query:\n> ");
                    string query = Console.ReadLine();
                    string trimmed = query.TrimStart().ToUpperInvariant();

                    if (string.IsNullOrWhiteSpace(query))
                        continue;

                    // Check if Command or SQL query Entered
                    if (trimmed.StartsWith("HELP"))
                    {
                        show_help();
                    }
                    else if (trimmed.StartsWith("RECONNECT"))
                    {
                        if (conn != null)
                        {
                            Console.WriteLine("Closing current connection to establish new connection..");
                            conn.Close();
                            conn = server_connect(); 
                        } 
                    }
                    else if (trimmed.StartsWith("STATUS"))
                    {
                        check_status(conn);
                    } 
                    else if (trimmed.StartsWith("EXIT"))
                    {
                        Console.WriteLine("\nExiting console application..");
                        conn.Close();
                        Console.WriteLine("Goodbye!");
                        Environment.Exit(0);
                    } 
                    else
                    {
                        perform_sql_query(conn, query, trimmed);
                    } 
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
             
        }
         
    }
}
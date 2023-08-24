using SqlImport;
using System.Configuration;
using SqlImport.DataReader.Csv;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace PNRLiabilityReportTool
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Stopwatch totalDurStopwatch = Stopwatch.StartNew();
			try
			{
				#region properties
				// Load defaults for all parameters.
				string
					server = ConfigurationManager.AppSettings["Server"],
					destinationDatabase = ConfigurationManager.AppSettings["DestinationDatabase"],
					connectionCredentials = ConfigurationManager.AppSettings["ConnectionCredentials"],
					dropbox = ConfigurationManager.AppSettings["Dropbox"],
					outputBox = ConfigurationManager.AppSettings["OutputBox"],
					outpreviousMasterKey = ConfigurationManager.AppSettings["PreviousMasterKeyPassword"],
					newMasterKey = ConfigurationManager.AppSettings["NewMasterKeyPassword"],
				    connectionString = "Server=localhost;Database=master;Trusted_Connection=True;Timeout=300";

				bool
					//killConnections = false,
					fixOrphanedUsers = bool.Parse(ConfigurationManager.AppSettings["FixOrphanedUsers"]),
					regenerateMasterKey = bool.Parse(ConfigurationManager.AppSettings["RegenerateMasterKey"]);


				int databaseTimeout = int.Parse(ConfigurationManager.AppSettings["DatabaseTimeOut"]);

				#endregion properties
				try
				{
					using (SqlConnection connection = new SqlConnection(connectionString))
					{
						connection.Open();

						// Run the import CSV command. 
						// TODO: create the bulk shit something method
						string importCommand = getImportCSVCommand(connectionString);
						using (SqlCommand command = new SqlCommand(importCommand, connection))
						{
							command.CommandTimeout = databaseTimeout;
							Console.WriteLine("Importing {0} to {1} {2}", "BlackFile.csv", server, destinationDatabase);
							command.ExecuteNonQuery();
						}

						//TODO: insert method to bang bang
						string bangbangCommand = getBangBangCommand();
						using (SqlCommand command = new SqlCommand(bangbangCommand, connection))
						{
							command.CommandTimeout = databaseTimeout;
							Console.WriteLine("Updating golden file table");
							command.ExecuteNonQuery();
						}

						//TODO: code to extract sql log files and save into logs.txt
						string logCaptureCommand = getCaptureLogsCommand();
						using (SqlCommand command = new SqlCommand(logCaptureCommand, connection))
						{
							command.CommandTimeout = databaseTimeout;
							Console.WriteLine("Generating logs.txt in {0}.", outputBox);
							command.ExecuteNonQuery();
						}

						//TODO: code to export result table into NGF.csv
						string exportCommand = getExportCommand();
						using (SqlCommand command = new SqlCommand(exportCommand, connection))
						{
							command.CommandTimeout = databaseTimeout;
							Console.WriteLine("Generating updated golden .csv to {0}.", outputBox);
							command.ExecuteNonQuery();
						}

					}
				}
				finally
				{
					//insert code to idk... whatever
				}
				Console.WriteLine($"Duration: {(int)totalDurStopwatch.Elapsed.TotalMinutes} minutes {totalDurStopwatch.Elapsed.Seconds} seconds (Done)");
				Environment.Exit(0);
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception occurred");
				Console.WriteLine(e.ToString());
				Console.WriteLine($"Duration: {(int)totalDurStopwatch.Elapsed.TotalMinutes} minutes {totalDurStopwatch.Elapsed.Seconds} seconds");
				Environment.Exit(-1);
			}
			
		}

		private static string getCaptureLogsCommand()
		{
			throw new NotImplementedException();
		}

		#region Methods

		private static string getBackupPath(string server, string database, string filePath, out string backupPathFromServer)
		{
			string backupRootFromLocalServer = null;
			if (server.ToLower() == Environment.MachineName.ToLower())
			{
				backupRootFromLocalServer = filePath;
				if (!Directory.Exists(backupRootFromLocalServer))
				{
					throw new ApplicationException("Directory \"" + backupRootFromLocalServer + "\" does not exist");
				}
			}
			else
			{
				backupRootFromLocalServer = @"\\" + server + @"\" + filePath.Substring(0, 1) + "$" + filePath.Substring(2);
				if (!Directory.Exists(backupRootFromLocalServer))
				{
					throw new ApplicationException("Directory \"" + backupRootFromLocalServer + "\" does not exist");
				}
			}

			string sanitizedDestinationDatabase =
				new string
				(
					(
						from c in database
						where !Path.GetInvalidFileNameChars().Contains<char>(c)
						select c
					).ToArray<char>()
				);

			string backupFileNamePrefix = "bu_" + sanitizedDestinationDatabase;
			string backupPathFromLocalServer;

				backupPathFromLocalServer = Path.Combine(backupRootFromLocalServer, backupFileNamePrefix + ".bak");
				backupPathFromServer = Path.Combine(filePath, backupFileNamePrefix + ".bak");
				int j = 2;
				while (File.Exists(backupPathFromLocalServer))
				{
					backupPathFromLocalServer = backupPathFromLocalServer.Substring(0, backupPathFromLocalServer.Length - 4) + "_" + j.ToString() + ".bak";
					backupPathFromServer = backupPathFromServer.Substring(0, backupPathFromServer.Length - 4) + "_" + j.ToString() + ".bak";
					j++;
				}

			return backupPathFromLocalServer;
		}
		private static string getExportCommand()
		{
			throw new NotImplementedException();
		}
		
		private static string getBangBangCommand()
		{
			throw new NotImplementedException();
		}

		private static string getRegenerateMasterKeyCommand(string previousMasterKeyPassword, string newMasterKeyPassword)
		{
			throw new NotImplementedException();
		}

		private static string getImportCSVCommand(string connectionString)
		{
			#region bulk copy

			BulkCopyUtility bulkCopyUtility = new BulkCopyUtility(connectionString);

			string blackFilePath = "";
			//Instantiate the reader, providing the list of columns which matches 1 to 1 the data table structure.
			var dataReader = new CsvDataReader(blackFilePath,
							new List<TypeCode>(5)
							{
					TypeCode.String,
					TypeCode.Decimal,
					TypeCode.String,
					TypeCode.Boolean,
					TypeCode.DateTime
							});

			bulkCopyUtility.BulkCopy("TableName", dataReader);

			#endregion
			throw new NotImplementedException();
		}

		private class OrphanedUser
		{
			public string UserName = "";
			public bool LoginExists;
		}

		#endregion Methods
	}
}
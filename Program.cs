using System.Data;
using System.Diagnostics;

namespace PTL
{
    class Program
    {
        private static List<ProgramLimiter> programsToLimit = [];

        private static DateTime date = DateTime.Now.Date;

        private static string GetNameOfTempFile() => $"/tmp/ptl-date-{date.Day}-{date.Month}-{date.Year}";
       
        public static void Main(string[] args)
        {
            // Check if ~/.ptl exists
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".ptl");
            if(File.Exists(path))
            {
                Console.WriteLine($"File exists at {path}! Parsing...");
                // Read the contents of the file into a string[]
                string[] lines = File.ReadAllLines(path);
                // Split each line at =
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    programsToLimit.Add(new(parts[0], float.Parse(parts[1])));
                    Console.WriteLine($"Added {parts[0]} with time {parts[1]} seconds");
                }

                // Check if GetNameOfTempFile() exists
                if (File.Exists(GetNameOfTempFile()))
                {
                    // Update programsToLimit with the contents of the file
                    lines = File.ReadAllLines(GetNameOfTempFile());
                    // Split each line at =
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');
                        // Check if the proc name exists in programsToLimit struct
                        for (int i = 0; i < programsToLimit.Count; i++)
                        {
                            if (programsToLimit[i].procName == parts[0])
                            {
                                programsToLimit[i] = new(programsToLimit[i].procName, float.Parse(parts[1]));
                                Console.WriteLine($"Updated {parts[0]} with time {parts[1]} seconds");
                                break;
                            }
                        }
                    }

                    // Delete the file
                    File.Delete(GetNameOfTempFile());
                }
            }
            else
            {
                Console.WriteLine($"File {path}/.ptl does not exist! Error!");
                Environment.Exit(1);
            }

            // Get a list of programs currently running on the system
            while (true)
            {
                // Update
                GetProcesses();

                // Update date
                date = DateTime.Now.Date;

                // Write file
                if (File.Exists(GetNameOfTempFile())) File.Delete(GetNameOfTempFile());
                using (StreamWriter writer = new StreamWriter(GetNameOfTempFile()))
                {
                    foreach (var program in programsToLimit)
                    {
                        writer.WriteLine($"{program.procName}={program.time}");
                    }
                }

                // Thread sleep 1 sec
                Thread.Sleep(1000);
            }
        }

        // Get processes
        private static void GetProcesses()
        {
            var procs = Process.GetProcesses();

            // If program is in restricted list, remove time
            foreach (var proc in procs)
            {
                for (int i = 0; i < programsToLimit.Count; i++)
                {
                    if (programsToLimit[i].procName == proc.ProcessName)
                    {
                        programsToLimit[i] = new(programsToLimit[i].procName, programsToLimit[i].time - 1);
                        Console.WriteLine($"Application {programsToLimit[i].procName} set to {programsToLimit[i].time} secs left");
                        if (programsToLimit[i].time <= 0)
                        {
                            // Kill the process
                            Process[] processes = Process.GetProcessesByName(programsToLimit[i].procName);
                            foreach (Process process in processes)
                                process.Kill();
                        }
                    }
                }
            }
        }
    }
}
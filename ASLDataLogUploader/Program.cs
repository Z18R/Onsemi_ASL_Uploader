using ASLDataLogUploader.Class;
using Renci.SshNet.Messages;
using System;
using System.Data;
using System.IO;


class Program
{
    static void Main()
    {

        string sourceDirectory = @"F:\Onsemi\ASL1K_DATALOG";
        string backupDirectory = @"F:\Onsemi\BACKUP";
        string uploadedDirectory = @"F:\Onsemi\UPLOADED";
        string backupDestination = @"F:\Onsemi\FSC TEST DATALOG BACKUP";
        EmailHandler em = new EmailHandler();
        DataSet dsEmail = em.GetMailRecipients(141);


        try
        {

            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine("Source directory does not exist.");
                return;
            }


            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            if (!Directory.Exists(uploadedDirectory))
            {
                Directory.CreateDirectory(uploadedDirectory);
            }


            CopyAllFilesAndFolders(sourceDirectory, backupDestination);

            string[] subdirectories = Directory.GetDirectories(sourceDirectory);

            foreach (string subdirectory in subdirectories)
            {
                string[] zipFiles = Directory.GetFiles(subdirectory, "*.zip");
                foreach (string zipFile in zipFiles)
                {
                    string fileName = Path.GetFileName(zipFile);
                    string destinationBackup = Path.Combine(backupDirectory, fileName);
                    string destinationUploaded = Path.Combine(uploadedDirectory, fileName);
                    File.Copy(zipFile, destinationBackup, true);
                    Console.WriteLine($"Copied {fileName} to backup directory.");
                    File.Move(zipFile, destinationUploaded);
                    Console.WriteLine($"Moved {fileName} to uploaded directory.");
                }


                Directory.Delete(subdirectory, true);
                Console.WriteLine($"Removed directory: {subdirectory}");
            }

            Console.WriteLine("All files copied, moved, and directories removed successfully.");


            // Call SendEmail method here
            string EmailSubject = "";
            string message = "this is just test";
            string filePath = ""; // Provide the path to the file you want to attach
            em.SendEmail(EmailSubject, message, filePath, dsEmail);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void CopyAllFilesAndFolders(string sourceDir, string targetDir)
    {
        foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));
        }

        // Copy all files
        foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
        {
            string destFile = Path.Combine(targetDir, filePath.Substring(sourceDir.Length + 1));
            File.Copy(filePath, destFile, true);
        }
    }
}

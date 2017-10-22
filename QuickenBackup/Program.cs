// Copyright (c) Victor Hurdugaci (https://victorhurdugaci.com). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.File;

namespace QuickenBackup
{
    class Program
    {
        private class Config
        {
            public string QuickenExecutablePath { get; set; }
            public string BackupDirectory { get; set; }
            public string FileShareSasUrl { get; set; }
        }

        static int Main(string[] args)
        {
            Console.Title = "Quicken Backup";

            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("config.json", optional: true)
                    .AddCommandLine(args)
                    .Build()
                    .Get<Config>();

                MainAsync(config).Wait();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                // In case of error make sure the window stays open
                Console.ReadLine();
                return 1;
            }
        }

        private static async Task MainAsync(Config config)
        {
            var backupDirectory = EnsureValidBackupFolder(config.BackupDirectory);

            await (WaitForQuickenToExitAsync(FindExistingQuicken() ?? StartNewQuicken(config.QuickenExecutablePath)));

            var latestBackupFile = FindLatestBackupFile(backupDirectory);
            await (UploadFileAsync(config.FileShareSasUrl, latestBackupFile));
        }

        private static string EnsureValidBackupFolder(string backupDirectory) => 
            Directory.Exists(backupDirectory) ?
                backupDirectory :
                throw new InvalidOperationException($"Directory {backupDirectory} doesn't exist");


        private static Task WaitForQuickenToExitAsync(Process quickenProcess)
        {
            Console.WriteLine($"Waiting for {quickenProcess.ProcessName}({quickenProcess.Id}) to exit...");
            return Task.Run(() => quickenProcess.WaitForExit());
        }

        private static Process FindExistingQuicken() => Process.GetProcessesByName("qw").FirstOrDefault();

        private static Process StartNewQuicken(string path)
        {
            Console.WriteLine("Starting new Quicken instance...");
            return Process.Start(path);
        }

        private static string FindLatestBackupFile(string backupDir) => 
            Directory.GetFiles(backupDir, "*.QDF-backup")
                .OrderByDescending(f => File.GetCreationTime(f))
                .First();

        private static async Task UploadFileAsync(string fileShareSasUrl, string localFilePath)
        {
            var share = new CloudFileShare(new Uri(fileShareSasUrl));

            var fileName = Path.GetFileName(localFilePath);

            var rootDir = share.GetRootDirectoryReference();
            var remoteFile = rootDir.GetFileReference(fileName);

            if (await remoteFile.ExistsAsync())
            {
                throw new InvalidOperationException($"File {fileName} already exists");
            }

            Console.WriteLine($"Uploading file '{localFilePath}'...");
            await remoteFile.UploadFromFileAsync(localFilePath);
            Console.WriteLine("File uploaded");
        }
    }
}

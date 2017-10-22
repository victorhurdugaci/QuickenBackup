# Quicken Backup

Quicken Backup is a small utility to backup Quicken files into Azure File Storage. It only works on Windows.

## How it works

1. Quicken creates backup files when you exit the program
1. Quicken Backup will wait for Quicken to exit and then it will upload the latest backup to Azure File Storage

## How to build it

1. Install Visual Studio 2017 or the .NET Core 2.0 SDK
1. Compile the project

## Is there a precompiled version?

No, sorry.

## How to use

1. Once you have the `exe` file, create a file named `config.json` next to it, with the following content:

        {
            "FileShareSasUrl": "<URL to file share> + <SAS token> (see below)",
            "BackupDirectory": "<Path to Quicken Backup directory; probably the BACKUP folder in same folder as the qdf file>",
            "QuickenExecutablePath": "<Path to qw.exe; probably C:\\Program Files (x86)\\Quicken\\qw.exe>"
        }

1. (optional) Create a shortcut to the exe
1. In Quicken, enable backups after every save Edit -> Preferences -> Setup
    - Check "Automatic Backup"
    - Back up after running Quicken: 1
    - Maximum number of backup copies: 10

1. Tip: in the backup preferences window you can click "Open backup directory" if you don't know where backups are stored

## How to generate the File Share SAS URL

1. Create an Azure File Storage account. E.g. `myfilestorage`
1. Create a share inside the storage account. E.g. `backup`
1. The path to the share will be `https://<account name>.file.core.windows.net/<share>`. E.g. `https://myfilestorage.file.core.windows.net/backup`
1. Create a SAS token with:
    - Allowed services: File
    - Allowed resource types: Object
    - Allowed permissions: Read, Write, List, Create
    - Allowed protocols: HTTPS only
1. The SAS token will be something like `?sv=2017-04-17&ss=f&srt=o&sp=rwlc&se=....`
1. The File Share SAS url is the share url followed by the SAS token. E.g. `https://myfilestorage.file.core.windows.net/backup?sv=2017-04-17&ss=f&srt=o&sp=rwlc&se=...`

Keep the token in a safe place. Anyone that has it can access the File Storage service!

## License

See `LICENSE` in the root of the repository.
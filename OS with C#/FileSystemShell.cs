using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OS_with_C_
{
    public class FileSystemShell
    {
        private static Directory currentDirectory = null;
        private static string currentPath = "C:\\"; // Starting path
        private static string diskPath = "Virtual.txt"; // Path to the virtual disk file          

        public static void Main(string[] args)
        {
            Console.WriteLine("VirtualDisk PowerShell\nCopyright (C) KIbrahim. All rights reserved.\n\n");


            // Initialize the virtual disk
            MiniFAT.InitializeOrOpenFileSystem(diskPath);
            VirtualDisk.Intialize(diskPath);
            currentDirectory = VirtualDisk.Root;

            // Main shell loop
            while (true)
            {
                Console.Write($"{currentPath}> ");
                string input = Console.ReadLine();

                // Parse the input into tokens, handling quoted paths
                string[] tokens = ParseCommandArguments(input);

                if (tokens.Length == 0) continue; //Skip empty lines

                string command = tokens[0].ToLower();

                try
                {
                    switch (command)
                    {
                        case "help": HandleHelp(tokens); break;
                        case "cd": HandleCd(tokens); break;
                        case "cls": HandleCls(tokens); break;
                        case "dir": HandleDir(tokens); break;
                        case "quit": if (tokens.Length > 1) { Console.WriteLine("Error: The syntax of the command is quit."); break; } MiniFAT.CloseFileSystem(); return;
                        case "copy": HandleCopy(tokens); break;
                        case "rename": HandleRename(tokens); break;
                        case "del": HandleDel(tokens); break;
                        case "md": HandleMd(tokens); break;
                        case "rd": HandleRd(tokens); break;
                        case "type": HandleType(tokens); break;
                        case "import": HandleImport(tokens); break;
                        case "export": HandleExport(tokens); break;
                        default: Console.WriteLine($"Error: Command '{command}' not found."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static string[] ParseCommandArguments(string input)
        {
            List<string> tokens = new List<string>();
            int index = 0;

            while (index < input.Length)
            {
                // Skip leading whitespace
                while (index < input.Length && char.IsWhiteSpace(input[index]))
                {
                    index++;
                }

                if (index >= input.Length)
                {
                    break;
                }

                // Check if the current character is a quote
                if (input[index] == '"' || input[index] == '\'')
                {
                    // Parse quoted path
                    string path = ParseQuotedPath(input, ref index);
                    tokens.Add(path);
                }
                else
                {
                    // Parse unquoted argument
                    StringBuilder arg = new StringBuilder();
                    while (index < input.Length && !char.IsWhiteSpace(input[index]))
                    {
                        arg.Append(input[index]);
                        index++;
                    }
                    tokens.Add(arg.ToString());
                }
            }

            return tokens.ToArray();
        }
        private static string ParseQuotedPath(string input, ref int index)
        {
            char quoteChar = input[index];
            index++; // Move past the opening quote

            StringBuilder path = new StringBuilder();
            while (index < input.Length && input[index] != quoteChar)
            {
                path.Append(input[index]);
                index++;
            }

            if (index < input.Length && input[index] == quoteChar)
            {
                index++; // Move past the closing quote
            }

            return path.ToString();
        }

        private static void HandleHelp(string[] tokens)
        {
            if (tokens.Length == 1)
            {
                // Display general help
                Console.WriteLine("Available commands:");
                Console.WriteLine("cd, cls, dir, quit, copy, rename, del, md, rd, type, import, export, help"); // Added help itself
                Console.WriteLine("For more information on a specific command, type 'help [command]'");
            }
            else
            {
                string commandToHelp = tokens[1].ToLower();

                switch (commandToHelp)  // Using a switch for clarity
                {
                    case "cd":
                        Console.WriteLine("Changes the current directory.");
                        Console.WriteLine("Syntax: cd [directory]");
                        Console.WriteLine("[directory] can be a directory name or a full path.");
                        break;

                    case "cls":
                        Console.WriteLine("Clears the console screen.");
                        Console.WriteLine("Syntax: cls");
                        break;

                    case "dir":
                        Console.WriteLine("Lists the contents of a directory.");
                        Console.WriteLine("Syntax: dir [directory/file]");
                        Console.WriteLine("[directory/file] can be a directory/file name or a full path. If omitted, lists the current directory.");
                        break;

                    case "quit":
                        Console.WriteLine("Exits the file system shell.");
                        Console.WriteLine("Syntax: quit");
                        break;

                    case "copy":
                        Console.WriteLine("Copies a file or directory.");
                        Console.WriteLine("Syntax: copy [source] [destination]");
                        Console.WriteLine("[source] and [destination] can be file/directory names or full paths.");
                        break;

                    case "rename":
                        Console.WriteLine("Renames a file.");
                        Console.WriteLine("Syntax: rename [filename] [newfilename]");
                        Console.WriteLine("[filename] can be a file name or a full path. [newfilename] should be a file name only.");
                        break;

                    case "del":
                        Console.WriteLine("Deletes a file or directory (all files in it) and prompts for confirmation..");
                        Console.WriteLine("Syntax: del [file/directory]+ "); // + indicates one or more arguments
                        Console.WriteLine("[file/directory] can be a file/directory name or a full path.");
                        break;

                    case "md":
                        Console.WriteLine("Creates a new directory.");
                        Console.WriteLine("Syntax: md [directory]");
                        Console.WriteLine("[directory] can be a directory name or a full path.");
                        break;


                    case "rd":
                        Console.WriteLine("Removes an empty directory and prompts for confirmation..");
                        Console.WriteLine("Syntax: rd [directory]+"); // + indicates one or more
                        Console.WriteLine("[directory] can be a directory name or a full path.");
                        break;

                    case "type":
                        Console.WriteLine("Displays the contents of a text file.");
                        Console.WriteLine("Syntax: type [file]+");
                        Console.WriteLine("[file] can be a file name or a full path.");
                        break;

                    case "import":
                        Console.WriteLine("Imports a file or directory from the host file system.");
                        Console.WriteLine("Syntax: import [source] [destination]");
                        Console.WriteLine("[source] is a path on the host system. [destination] is a path in the virtual disk.");
                        break;

                    case "export":
                        Console.WriteLine("Exports a file or directory to the host file system.");
                        Console.WriteLine("Syntax: export [source] [destination]");
                        Console.WriteLine("[source] is a path in the virtual disk. [destination] is a path on the host system.");
                        break;
                    case "help":
                        Console.WriteLine("Provides help information for commands.");
                        Console.WriteLine("Syntax: help [command]");
                        Console.WriteLine("If [command] is omitted, displays a list of available commands.");
                        break;

                    default:
                        Console.WriteLine($"Error: Command '{commandToHelp}' not found.");
                        break;

                }
            }
        }

        private static void HandleCls(string[] tokens)
        {
            if (tokens.Length > 1)  // Check for extra arguments
            {
                Console.WriteLine("Error: The syntax of the command is just cls\nFunction: Clear the screen.");
                return;
            }
            Console.Clear();
        }

        private static void HandleCd(string[] tokens)
        {
            if (tokens.Length > 2)
            {
                Console.WriteLine("Error: Invalid 'cd' command syntax.");
                return;
            }

            if (tokens.Length == 1) // Display current directory
            {
                Console.WriteLine(currentPath);
                return;
            }

            string targetPath = tokens[1];

            if (targetPath.Contains(".."))
            {
                HandleCdWithParentTraversal(targetPath);    //Case (8)(Bonus): type cd then any number of “..” separated by \.
            }
            else
            {
                ChangeDirectory(targetPath);
            }
        }

        private static void ChangeDirectory(string targetPath)
        {
            Directory newCurrent = null;

            if (targetPath == ".")
            {
                return; // Do nothing
            }
            else if (targetPath == "..")
            {
                newCurrent = currentDirectory.parent;
                if (newCurrent == null)
                {
                    Console.WriteLine("Cannot go above root directory.");
                    return;
                }
            }
            else if (targetPath.Contains('\\'))
            {
                // Handle full paths
                newCurrent = MoveToDir(targetPath); // MoveToDir handles error messages. No need to check 'newCurrent' again
            }
            else
            {
                // Handle relative paths
                int dirIndex = currentDirectory.SearchDirectory(targetPath);
                if (dirIndex == -1)
                {
                    Console.WriteLine($"Error: Directory '{targetPath}' not found.");
                    return;
                }

                DirectoryEntry entry = currentDirectory.DirFiles[dirIndex];
                if ((entry.DIR_Attr & 0x10) == 0x10)
                {
                    newCurrent = new Directory(new string(entry.DIR_Name).Trim('\0'), entry.DIR_Attr, entry.DIR_FirstCluster, entry.DIR_FileSize, currentDirectory);
                    newCurrent.ReadDirectory();
                }
                else
                {
                    Console.WriteLine($"'{targetPath}' is not a directory.");
                    return;
                }
            }


            if (newCurrent != null)
            {
                currentDirectory = newCurrent;
                currentPath = GetFullPath(currentDirectory); // Call GetFullPath ONLY if newCurrent is not null
            }
        }


        private static string GetFullPath(Directory dir)
        {
            if (dir == null)  // Handle the case where dir is null (e.g., MoveToDir failed)
            {
                return currentPath; // Return the current path if dir is null (don't change the path)
            }

            if (dir.parent == null) { return "C:\\"; } // Root directory

            Stack<string> pathParts = new Stack<string>(); // Using a stack for efficient path building

            while (dir != null)
            {
                pathParts.Push(dir.CleanTheName()); // Push directory names onto the stack
                dir = dir.parent;
            }
            pathParts.Pop();//pop "C:"

            return "C:\\" + string.Join("\\", pathParts); // builds path, starting with C:\
        }

        private static void HandleCdWithParentTraversal(string targetPath)
        {
            string[] parts = targetPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            Directory tempCurrent = currentDirectory;

            foreach (string part in parts)
            {
                if (part == "..")
                {
                    if (tempCurrent.parent != null)
                    {
                        tempCurrent = tempCurrent.parent;
                    }
                    else
                    {
                        Console.WriteLine("Cannot go above root directory.");
                        return; // Stop traversal if we reach the root
                    }
                }
                else
                {
                    Console.WriteLine("Error: Invalid path with parent traversal. Only \"..\" is allowed.");
                    return;
                }
            }
            // If all ".." were valid, update the current directory and path
            currentDirectory = tempCurrent;
            currentPath = GetFullPath(currentDirectory);
        }
        private static Directory MoveToDir(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Error: Path cannot be empty.");
                return null;
            }

            string[] pathParts = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length == 0)
            {
                Console.WriteLine("Error: Invalid path.");
                return null;
            }

            Directory current = VirtualDisk.Root;  // Start at the root

            // Check if the first part of the path is the root directory
            if (pathParts[0] != "C:") //assuming C: is root
            {
                Console.WriteLine("Error: Invalid root directory. Must start with 'C:\\'.");
                return null;
            }
            if (pathParts.Length == 1 && pathParts[0] == "C:") return current; //we are at Root directory

            for (int i = 1; i < pathParts.Length; i++) // Start from 1 to skip the root
            {
                string dirName = pathParts[i];


                int entryIndex = current.SearchDirectory(dirName); //case-insensitive search
                if (entryIndex == -1)
                {
                    Console.WriteLine($"Error: Directory '{dirName}' not found."); // use {path} for full path
                    return null;
                }

                DirectoryEntry entry = current.DirFiles[entryIndex];
                if ((entry.DIR_Attr & 0x10) != 0x10) // Check if it's a directory.
                {
                    Console.WriteLine($"Error: '{dirName}' is not a directory.");
                    return null;

                }

                // Instantiate a Directory object (don't reuse entry)
                Directory subDirectory = new Directory(new string(entry.DIR_Name).Trim('\0'), entry.DIR_Attr, entry.DIR_FirstCluster, entry.DIR_FileSize, current); // Don't forget to set the parent!
                subDirectory.ReadDirectory();//very important 

                current = subDirectory;
            }
            return current;
        }

        private static FileEntry MoveToFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Error: Path cannot be empty.");
                return null;
            }

            string fileName;
            string directoryPath;
            Directory parentDir;

            // Extract file name and directory path
            int lastBackslash = path.LastIndexOf('\\');
            if (lastBackslash == -1) // No backslash, assume file is in the current directory
            {

                fileName = path;
                parentDir = currentDirectory;
            }
            else
            {
                fileName = path.Substring(lastBackslash + 1);
                directoryPath = path.Substring(0, lastBackslash);



                parentDir = MoveToDir(directoryPath);

                if (parentDir == null)
                {
                    // MoveToDir handles error messages

                    return null;
                }
            }

            // Ensure the parentDir is read before searching
            if (parentDir != currentDirectory) { parentDir.ReadDirectory(); }

            // Search for the file in the parent directory
            int entryIndex = parentDir.SearchDirectory(fileName); //case-insensitive search

            if (entryIndex == -1)

            {
                Console.WriteLine($"Error: File '{fileName}' not found in the specified path."); // use {path} for full path
                return null;
            }

            DirectoryEntry entry = parentDir.DirFiles[entryIndex];

            if ((entry.DIR_Attr & 0x10) == 0x10) // Check if it's a directory (not a file).

            {
                Console.WriteLine($"Error: '{fileName}' is a directory, not a file.");
                return null;
            }

            // Instantiate a new FileEntry (important)
            FileEntry file = new FileEntry(new string(entry.DIR_Name).Trim('\0'), entry.DIR_Attr, entry.DIR_FirstCluster, entry.DIR_FileSize, parentDir, null); //content will be read by readFileContent if needed.
            return file;
        }



        private static void HandleDir(string[] tokens)
        {
            if (tokens.Length > 2)
            {
                Console.WriteLine("Error: Invalid 'dir' command syntax."); //check if the user inputs more than one argument
                return;
            }

            Directory directoryToList;
            FileEntry fileToList = null;
            string directoryNameOrPath = ""; // To store the directory name or path for display

            // If no argument or a "." -> list the current directory
            if (tokens.Length == 1 || tokens[1] == ".")  // List current directory
            {
                // Force a re-read to ensure changes are reflected
                currentDirectory.ReadDirectory();
                directoryToList = currentDirectory;
                directoryNameOrPath = currentPath; // Display current path
            }
            else
            {
                string dirFile = tokens[1];

                // Check if it's a full path or just a name
                if (dirFile.Contains('\\'))
                {
                    directoryNameOrPath = dirFile; // Full path provided

                    // Full path
                    if (IsFilePath(dirFile)) // Helper function to determine if a path is a file path or directory path by checking if it has an extension and if the last directory in the path exists.
                    {
                        fileToList = MoveToFile(dirFile);

                        if (fileToList == null) return;

                        directoryToList = fileToList.Parent;

                    }
                    else
                    {
                        directoryToList = MoveToDir(dirFile);
                        if (directoryToList == null) return;

                    }

                }
                else if (dirFile == "..")
                {
                    if (currentDirectory.parent != null)
                    {
                        directoryToList = currentDirectory.parent;
                        directoryNameOrPath = GetFullPath(currentDirectory.parent); // Display parent's path
                    }

                    else
                    {
                        Console.WriteLine("Cannot go above Root.");
                        return;
                    }
                }
                else //directory or file name
                {
                    int index = currentDirectory.SearchDirectory(dirFile);
                    if (index == -1)
                    {
                        Console.WriteLine($"Error: '{dirFile}' not found.");
                        return;
                    }

                    DirectoryEntry dirEntry = currentDirectory.DirFiles[index];
                    if ((dirEntry.DIR_Attr & 0x10) == 0x10) // Check if it's a directory
                    {

                        directoryToList = new Directory(new string(dirEntry.DIR_Name).Trim('\0'), dirEntry.DIR_Attr, dirEntry.DIR_FirstCluster, dirEntry.DIR_FileSize, currentDirectory);
                        directoryToList.ReadDirectory();//Very Important
                        directoryNameOrPath = Path.Combine(currentPath, dirEntry.CleanTheName()); // Combine for correct display

                    }
                    else // File
                    {
                        fileToList = new FileEntry(new string(dirEntry.DIR_Name).Trim('\0'), dirEntry.DIR_Attr, dirEntry.DIR_FirstCluster, dirEntry.DIR_FileSize, currentDirectory, "");

                        directoryToList = currentDirectory; // fileToList will be listed alone, and we set directoryToList to the currentDirectory for later calculations.
                        directoryNameOrPath = currentPath; // File is listed within current directory
                    }
                }
            }
            // List the contents
            if (directoryToList != null)
            {
                Console.WriteLine($"\nDirectory of {directoryNameOrPath}  :\n");

                int fileCount = 0;
                int dirCount = 0;
                long totalFileSize = 0;


                if (directoryToList != VirtualDisk.Root)

                {
                    Console.WriteLine($"<DIR>\t.");
                    Console.WriteLine($"<DIR>\t..");
                    dirCount += 2;

                }

                if (fileToList != null) //fileToList has a value which means list only this File

                {

                    Console.WriteLine($"{fileToList.DIR_FileSize}\t{new string(fileToList.DIR_Name).Trim('\0')}");
                    fileCount++;
                    totalFileSize = fileToList.DIR_FileSize;
                    //free space calculation

                }
                else
                {
                    foreach (DirectoryEntry entry in directoryToList.DirFiles)

                    {
                        if ((entry.DIR_Attr & 0x10) == 0x10)

                        {
                            Console.WriteLine($"<DIR>\t{new string(entry.DIR_Name).Trim('\0')}");
                            dirCount++;

                        }
                        else
                        {
                            Console.WriteLine($"{entry.DIR_FileSize}\t{new string(entry.DIR_Name).Trim('\0')}");

                            fileCount++;
                            totalFileSize += entry.DIR_FileSize;

                        }
                    }


                }
                Console.WriteLine($"\t\t{fileCount} File(s)\t{totalFileSize} bytes");
                Console.WriteLine($"\t\t{dirCount} Dir(s)\t{MiniFAT.GetFreeSize()} bytes free");

            }

        }
        //helper function 
        private static bool IsFilePath(string path)
        {
            if (!path.Contains('.')) return false;

            try
            {
                string directoryPart = Path.GetDirectoryName(path); //This may throw exception if path is invalid


                if (string.IsNullOrEmpty(directoryPart))
                {
                    // If directory part is null/empty, treat as a file name in current directory.
                    return true;  // Treat as file

                }

                if (MoveToDir(directoryPart) == null)
                {
                    //Not existing directory return false
                    return false;

                }
                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private static void HandleCopy(string[] tokens)
        {
            if (tokens.Length < 2 || tokens.Length > 3)
            {
                Console.WriteLine("Error: Invalid 'copy' command syntax.\nUsage: copy [source]\nor\ncopy [source] [destination]");
                return;
            }

            string sourcePath = tokens[1];
            string destinationPath = (tokens.Length == 3) ? tokens[2] : null;

            // Resolve source
            DirectoryEntry sourceEntry = GetDirectoryEntryFromPath(sourcePath);
            Directory parentSource = GetParentDirectoryFromPath(sourcePath);

            if (sourceEntry == null)
            {
                Console.WriteLine($"Error: Source '{sourcePath}' not found on your disk."); // Case (2) and Case (5)
                return;
            }

            // Resolve destination
            DirectoryEntry destinationEntry = null;
            Directory parentDest = null;

            if (destinationPath != null)
            {
                destinationEntry = GetDirectoryEntryFromPath(destinationPath);
                parentDest = GetParentDirectoryFromPath(destinationPath);
            }

            // Case (3) and Case (4): Source and destination are the same
            if (sourcePath == destinationPath)
            {
                Console.WriteLine("Error: The file cannot be copied onto itself.");
                return;
            }

            // Perform the copy operation
            CopyItem(sourceEntry, parentSource, destinationEntry, parentDest, destinationPath);
        }


        private static void CopyItem(DirectoryEntry sourceEntry, Directory parentSource, DirectoryEntry destinationEntry, Directory parentDest, string destinationPath)
        {
            int filesCopied = 0; // Counter for the number of files copied

            if (sourceEntry.DIR_Attr == 0x10) // Source is a directory
            {
                Directory sourceDir = new Directory(new string(sourceEntry.DIR_Name).Trim('\0'), sourceEntry.DIR_Attr, sourceEntry.DIR_FirstCluster, sourceEntry.DIR_FileSize, parentSource);
                sourceDir.ReadDirectory();

                if (destinationEntry == null) // Copy directory to current directory
                {
                    // Case (7) and Case (8): Copy directory to current directory
                    foreach (DirectoryEntry entry in sourceDir.DirFiles)
                    {
                        if (entry.DIR_Attr != 0x10) // Copy only files, not subdirectories
                        {
                            FileEntry fileEntry = new FileEntry(new string(entry.DIR_Name).Trim('\0'), entry.DIR_Attr, entry.DIR_FirstCluster, entry.DIR_FileSize, sourceDir, null);
                            fileEntry.ReadFileContent();
                            CreateFile(currentDirectory, new string(entry.DIR_Name).Trim('\0'), fileEntry.FileContent);
                            filesCopied++;
                        }
                    }
                }
                else if (destinationEntry.DIR_Attr == 0x10) // Copy directory to another directory
                {
                    // Case (10): Copy directory to directory
                    Directory destDir = new Directory(new string(destinationEntry.DIR_Name).Trim('\0'), destinationEntry.DIR_Attr, destinationEntry.DIR_FirstCluster, destinationEntry.DIR_FileSize, parentDest);
                    destDir.ReadDirectory();

                    foreach (DirectoryEntry entry in sourceDir.DirFiles)
                    {
                        if (entry.DIR_Attr != 0x10) // Copy only files, not subdirectories
                        {
                            FileEntry fileEntry = new FileEntry(new string(entry.DIR_Name).Trim('\0'), entry.DIR_Attr, entry.DIR_FirstCluster, entry.DIR_FileSize, sourceDir, null);
                            fileEntry.ReadFileContent();

                            if (destDir.SearchDirectory(fileEntry.CleanTheName()) != -1)
                            {
                                // Case (14): Prompt for overwrite
                                Console.Write($"File '{fileEntry.CleanTheName()}' already exists in destination directory. Overwrite? (y/n): ");
                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    OverwriteFile(destDir, fileEntry.CleanTheName(), fileEntry.FileContent);
                                    filesCopied++;
                                }
                            }
                            else
                            {
                                CreateFile(destDir, fileEntry.CleanTheName(), fileEntry.FileContent);
                                filesCopied++;
                            }
                        }
                    }
                }
                else // Copy directory to file (concatenate contents)
                {
                    // Case (11): Copy directory to file (concatenate contents)
                    string combinedContent = "";

                    foreach (DirectoryEntry entry in sourceDir.DirFiles)
                    {
                        if (entry.DIR_Attr != 0x10) // Copy only files, not subdirectories
                        {
                            FileEntry fileEntry = new FileEntry(new string(entry.DIR_Name).Trim('\0'), entry.DIR_Attr, entry.DIR_FirstCluster, entry.DIR_FileSize, sourceDir, null);
                            fileEntry.ReadFileContent();
                            combinedContent += fileEntry.FileContent + Environment.NewLine; // Add newline between files
                            filesCopied++;
                        }
                    }

                    // Extract the file name from the destination path
                    string newFileName = destinationPath.Substring(destinationPath.LastIndexOf('\\') + 1);

                    // Check if the destination file already exists
                    if (parentDest.SearchDirectory(newFileName) != -1)
                    {
                        // Case (14): Prompt for overwrite
                        Console.Write($"File '{newFileName}' already exists. Overwrite? (y/n): ");
                        if (Console.ReadLine().ToLower() == "y")
                        {
                            OverwriteFile(parentDest, newFileName, combinedContent);
                            filesCopied = 1; // Only one file is created/overwritten
                        }
                        else
                        {
                            filesCopied = 0; // No files copied if user declines
                        }
                    }
                    else
                    {
                        // Create a new file and write the combined content
                        CreateFile(parentDest, newFileName, combinedContent);
                        filesCopied = 1; // Only one file is created
                    }
                }
            }
            else // Source is a file
            {
                FileEntry sourceFile = new FileEntry(new string(sourceEntry.DIR_Name).Trim('\0'), sourceEntry.DIR_Attr, sourceEntry.DIR_FirstCluster, sourceEntry.DIR_FileSize, parentSource, null);
                sourceFile.ReadFileContent();

                if (destinationEntry == null) // Copy file to current directory
                {
                    // Case (6): Copy file to current directory
                    CreateFile(currentDirectory, new string(sourceEntry.DIR_Name).Trim('\0'), sourceFile.FileContent);
                    filesCopied = 1;
                }
                else if (destinationEntry.DIR_Attr != 0x10) // Copy file to file
                {
                    // Case (12) and Case (14): Copy file to file
                    Console.Write($"File '{new string(destinationEntry.DIR_Name).Trim('\0')}' already exists. Overwrite? (y/n): ");
                    if (Console.ReadLine().ToLower() == "y")
                    {
                        OverwriteFile(parentDest, new string(destinationEntry.DIR_Name).Trim('\0'), sourceFile.FileContent);
                        filesCopied = 1;
                    }
                    else
                    {
                        filesCopied = 0;
                    }
                }
                else // Copy file to directory
                {
                    // Case (13): Copy file to directory
                    Directory destDir = new Directory(new string(destinationEntry.DIR_Name).Trim('\0'), destinationEntry.DIR_Attr, destinationEntry.DIR_FirstCluster, destinationEntry.DIR_FileSize, parentDest);
                    destDir.ReadDirectory();

                    string newFileName = new string(sourceFile.DIR_Name).Trim('\0');
                    if (destDir.SearchDirectory(newFileName) != -1)
                    {
                        // Case (14): Prompt for overwrite
                        Console.Write($"File '{newFileName}' already exists in destination directory. Overwrite? (y/n): ");
                        if (Console.ReadLine().ToLower() == "y")
                        {
                            OverwriteFile(destDir, newFileName, sourceFile.FileContent);
                            filesCopied = 1;
                        }
                        else
                        {
                            filesCopied = 0;
                        }
                    }
                    else
                    {
                        CreateFile(destDir, newFileName, sourceFile.FileContent);
                        filesCopied = 1;
                    }
                }
            }

            // Display the number of files copied
            Console.WriteLine($"\n        ({filesCopied}) file(s) copied.\n");
        }

        private static DirectoryEntry GetDirectoryEntryFromPath(string path)
        {
            string dirName;
            string parentDirPath;
            int lastSlash = path.LastIndexOf('\\');
            Directory parentDir;
            if (lastSlash == -1)
            {

                dirName = path;
                parentDir = currentDirectory;
            }
            else
            {
                dirName = path.Substring(lastSlash + 1);
                parentDirPath = path.Substring(0, lastSlash);
                parentDir = MoveToDir(parentDirPath);
                if (parentDir == null) return null;
            }
            int dirIndex = parentDir.SearchDirectory(dirName);

            if (dirIndex == -1)
            {

                return null;

            }

            DirectoryEntry entry = parentDir.DirFiles[dirIndex];
            return entry;
        }

        private static Directory GetParentDirectoryFromPath(string path)
        {

            string parentDirPath;

            int lastSlash = path.LastIndexOf('\\');
            Directory parentDir;

            if (lastSlash == -1)

            {
                parentDir = currentDirectory;
            }

            else

            {
                parentDirPath = path.Substring(0, lastSlash);
                parentDir = MoveToDir(parentDirPath);

                if (parentDir == null) return null;
            }

            return parentDir;

        }


        // Function to create a file (Simplified)
        private static void CreateFile(Directory parent, string fileName, string content)
        {
            // Check if file already exists
            if (parent.EntryExists(fileName))
            {
                Console.Write($"File '{fileName}' already exists. Overwrite? (y/n): ");
                if (Console.ReadLine().ToLower() != "y")
                {
                    return; // Don't overwrite
                }

                OverwriteFile(parent, fileName, content);// Overwrite existing file

                return;
            }

            FileEntry newFile = new FileEntry(fileName, 0x00, 0, 0, parent, content);

            if (parent.CanAddEntry(newFile))
            {
                parent.AddEntry(newFile); // AddEntry now handles writing file content and directory updates
            }

            else
            {
                Console.WriteLine("Not enough space to create file.");
            }
        }

        // Function to overwrite a file
        private static void OverwriteFile(Directory parent, string fileName, string newContent)
        {
            if (parent.SearchDirectory(fileName) != -1) //If File Already Exists, overwrite it.
            {
                int fileIndex = parent.SearchDirectory(fileName);

                DirectoryEntry oldEntry = parent.DirFiles[fileIndex];
                FileEntry oldFile = new FileEntry(new string(oldEntry.DIR_Name).Trim('\0'), oldEntry.DIR_Attr, oldEntry.DIR_FirstCluster, oldEntry.DIR_FileSize, parent, null);

                oldFile.ReadFileContent();//get its content
                oldFile.DeleteFile();

                FileEntry newFile = new FileEntry(fileName, 0x00, 0, 0, parent, newContent);
                parent.AddEntry(newFile);
                newFile.WriteFileContent();//Write the new Content into the File after Deleting it.

            }
        }
        private static void HandleRename(string[] tokens)
        {
            // Case (1): If "rename" is typed with too few tokens, show syntax and return
            if (tokens.Length < 3)
            {
                Console.WriteLine("Error: The syntax of the command is: rename [oldName] [newName]");
                return;
            }

            // If there are more than 3 tokens, it’s also invalid.
            if (tokens.Length > 3)
            {
                Console.WriteLine("Error: The syntax of the command is: rename [oldName] [newName]");
                return;
            }

            // Extract old and new from tokens
            string oldPath = tokens[1];
            string newFileName = tokens[2];

            // Case (6): Check if `newFileName` is a full path (has a backslash or colon, for instance)
            if (newFileName.Contains("\\") || newFileName.Contains(":"))
            {
                Console.WriteLine("Error: The new file name should be a file name only; you cannot provide a full path.");
                return;
            }

            // Identify parent directory for old path
            string oldDirectory = Path.GetDirectoryName(oldPath);
            string oldFileName = Path.GetFileName(oldPath);

            Directory parentDirOld;
            if (string.IsNullOrEmpty(oldDirectory))
            {
                // Old is in current directory
                parentDirOld = currentDirectory;
            }
            else
            {
                // Old is in some full path parent
                parentDirOld = MoveToDir(oldDirectory);
                if (parentDirOld == null)
                {
                    // Cases (4) or (5) → The parent directory does not exist
                    //Console.WriteLine("Error: That file or directory does not exist on your disk.");
                    return;
                }
            }

            // Look for old file name in that directory
            parentDirOld.ReadDirectory(); // Make sure we have up-to-date info
            int oldEntryIndex = parentDirOld.SearchDirectory(oldFileName);
            if (oldEntryIndex == -1)
            {
                // Cases (4) and (5): old file name not found
                Console.WriteLine($"Error: This file '{oldFileName}' does not exist on your disk.");
                return;
            }

            // Check if something with the new name already exists
            if (parentDirOld.SearchDirectory(newFileName) != -1)
            {
                // Case (7): the new file name already exists
                Console.WriteLine($"Error: A duplicate file name '{newFileName}' already exists.");
                return;
            }

            // Retrieve the old entry
            DirectoryEntry oldEntry = parentDirOld.DirFiles[oldEntryIndex];

            // Create a new entry that has the new name but the same cluster, attr, etc.
            DirectoryEntry newEntry = new DirectoryEntry(newFileName, oldEntry.DIR_Attr, oldEntry.DIR_FirstCluster)
            {
                DIR_FileSize = oldEntry.DIR_FileSize
            };

            // Rename in place
            parentDirOld.UpdateContent(oldEntry, newEntry);

            // If it’s a file, ensure we refresh and write content so the rename is recognized on disk
            if (oldEntry.DIR_Attr == 0x00) // 0x00 → file
            {
                FileEntry renamedFile = MoveToFile(Path.Combine(GetFullPath(parentDirOld), newFileName));
                if (renamedFile != null)
                {
                    // Ensure FileContent is read before writing, preventing null references
                    renamedFile.ReadFileContent();
                    renamedFile.WriteFileContent();
                }
            }
            else if (oldEntry.DIR_Attr == 0x10) // 0x10 → directory
            {
                // Optionally handle renaming a directory. If I do not want to allow it, I will show error here.
                // Otherwise, it is already renamed in the directory entries.
            }

            // Display a success message
            Console.WriteLine($"File '{oldFileName}' renamed to '{newFileName}'.");
        }
        private static void HandleDel(string[] tokens)
        {
            // Case (1): No arguments provided
            if (tokens.Length < 2)
            {
                Console.WriteLine("Error: The syntax of the command is: del [dirFile]+");
                Console.WriteLine("Usage: del [file/directory]+");
                return;
            }

            // Process each argument
            for (int i = 1; i < tokens.Length; i++)
            {
                string itemPath = tokens[i];
                DirectoryEntry entry = GetDirectoryEntryFromPath(itemPath);
                Directory parentDir = GetParentDirectoryFromPath(itemPath);

                if (entry == null || parentDir == null)
                {
                    Console.WriteLine($"Error: '{itemPath}' not found.");
                    continue;
                }

                // Case (2) and (3): File deletion
                if ((entry.DIR_Attr & 0x10) == 0) // It's a file
                {
                    Console.Write($"Are you sure you want to delete the file '{entry.CleanTheName()}'? (y/n): ");
                    if (Console.ReadLine().ToLower() == "y")
                    {
                        FileEntry fileToDelete = new FileEntry(
                            entry.CleanTheName(),
                            entry.DIR_Attr,
                            entry.DIR_FirstCluster,
                            entry.DIR_FileSize,
                            parentDir,
                            null
                        );

                        // Delete the file
                        fileToDelete.DeleteFile();

                        // Remove the entry from the parent directory
                        parentDir.DirFiles.Remove(entry);

                        // Update the parent directory on disk
                        parentDir.WriteDirectory();

                        // If the parent is the root directory, ensure it is updated
                        if (parentDir == VirtualDisk.Root)
                        {
                            VirtualDisk.Root.WriteDirectory();
                        }

                        // Flush changes to disk
                        VirtualDisk.disk.Flush();

                        Console.WriteLine($"File '{entry.CleanTheName()}' deleted.");
                    }
                    else
                    {
                        Console.WriteLine("Deletion canceled.");
                    }
                }
                // Case (4): Directory deletion (only files, not subdirectories)
                else if ((entry.DIR_Attr & 0x10) == 0x10) // It's a directory
                {
                    Console.Write($"Are you sure you want to delete all files in the directory '{entry.CleanTheName()}'? (y/n): ");
                    if (Console.ReadLine().ToLower() == "y")
                    {
                        Directory dirToDelete = new Directory(
                            entry.CleanTheName(),
                            entry.DIR_Attr,
                            entry.DIR_FirstCluster,
                            entry.DIR_FileSize,
                            parentDir
                        );

                        dirToDelete.ReadDirectory();

                        // Delete all files in the directory (not subdirectories)
                        List<DirectoryEntry> filesToDelete = dirToDelete.DirFiles
                            .Where(e => (e.DIR_Attr & 0x10) == 0) // Only files
                            .ToList();

                        foreach (var fileEntry in filesToDelete)
                        {
                            FileEntry fileToDelete = new FileEntry(
                                fileEntry.CleanTheName(),
                                fileEntry.DIR_Attr,
                                fileEntry.DIR_FirstCluster,
                                fileEntry.DIR_FileSize,
                                dirToDelete,
                                null
                            );

                            fileToDelete.DeleteFile();
                            dirToDelete.DirFiles.Remove(fileEntry);
                        }

                        // Update the directory on disk
                        dirToDelete.WriteDirectory();

                        // If the parent is the root directory, ensure it is updated
                        if (parentDir == VirtualDisk.Root)
                        {
                            VirtualDisk.Root.WriteDirectory();
                        }

                        // Flush changes to disk
                        VirtualDisk.disk.Flush();

                        Console.WriteLine($"All files in directory '{entry.CleanTheName()}' deleted.");
                    }
                    else
                    {
                        Console.WriteLine("Deletion canceled.");
                    }
                }
            }
        }


        private static void HandleMd(string[] tokens)
        {
            if (tokens.Length != 2)
            {
                Console.WriteLine("Error: Invalid 'md' command syntax. Usage: md [directory]");
                return;
            }

            string newDirNameOrPath = tokens[1];

            CreateDirectory(newDirNameOrPath);
        }

        private static void CreateDirectory(string path)
        {
            string newDirName;
            Directory parentDir;

            int lastBackslash = path.LastIndexOf('\\');
            if (lastBackslash == -1) // No backslash, create in current directory
            {
                parentDir = currentDirectory;
                newDirName = path;
            }
            else //It's a path
            {
                newDirName = path.Substring(lastBackslash + 1);
                string parentPath = path.Substring(0, lastBackslash);
                parentDir = MoveToDir(parentPath);
                if (parentDir == null) return;
            }

            if (parentDir.SearchDirectory(newDirName) != -1)
            {
                Console.WriteLine($"Error: Directory '{newDirName}' already exists.");
                return;
            }

            DirectoryEntry newDirEntry = new DirectoryEntry(newDirName, 0x10, 0);

            if (!parentDir.CanAddEntry(newDirEntry))
            {
                Console.WriteLine("Error: Not enough space to create directory.");
                return;
            }

            parentDir.AddEntry(newDirEntry); // The AddEntry method now writes to disk internally
        }

        private static void HandleRd(string[] tokens)
        {
            if (tokens.Length < 2)
            {
                Console.WriteLine("Error: Invalid 'rd' command syntax. Usage: rd [directory]+");
                return;
            }

            for (int i = 1; i < tokens.Length; i++)
            {
                string directoryPath = tokens[i];

                RemoveDirectory(directoryPath);
            }
        }

        private static void RemoveDirectory(string directoryPath)
        {
            Directory dirToRemove = null;

            if (directoryPath.Contains('\\'))
            {
                dirToRemove = MoveToDir(directoryPath);
            }
            else if (directoryPath == "..") // Handle parent directory traversal
            {
                if (currentDirectory.parent != null)
                    dirToRemove = currentDirectory.parent;
                else
                {
                    Console.WriteLine("Error:Cannot go above Root"); // Handle going above root directory
                    return;
                }

            }
            else
            {
                int dirIndex = currentDirectory.SearchDirectory(directoryPath);

                if (dirIndex != -1)
                {
                    DirectoryEntry dirEntry = currentDirectory.DirFiles[dirIndex];

                    dirToRemove = new Directory(dirEntry.CleanTheName(), dirEntry.DIR_Attr, dirEntry.DIR_FirstCluster, dirEntry.DIR_FileSize, currentDirectory);
                }
            }

            if (dirToRemove == null)
            {
                Console.WriteLine($"Error: Directory '{directoryPath}' not found.");

                return;
            }
            if (dirToRemove == VirtualDisk.Root) //Prevent Removing Root
            {
                Console.WriteLine("Error: Cannot remove root directory.");

                return;

            }

            Console.Write($"Are you sure you want to remove directory '{dirToRemove.CleanTheName()}'? (y/n): ");

            if (Console.ReadLine().ToLower() != "y") return;

            dirToRemove.ReadDirectory();

            if (dirToRemove.DirFiles.Count > 0) //Prevent Removing non-empty Directory

            {
                Console.WriteLine($"Error: Directory '{dirToRemove.CleanTheName()}' is not empty.");
                return;

            }
            //Directory is empty so delete it
            dirToRemove.DeleteDirectory();// RemoveDirectory updates the disk and parent directory
        }
        private static void HandleType(string[] tokens)
        {
            if (tokens.Length < 2)
            {
                Console.WriteLine("Error: Invalid 'type' command syntax. Usage: type [file]+");
                return;
            }

            for (int i = 1; i < tokens.Length; i++)
            {
                string filePath = tokens[i];
                DisplayFileContent(filePath);
            }
        }

        private static void DisplayFileContent(string path)
        {
            FileEntry fileToDisplay;

            if (path.Contains('\\'))
            {
                fileToDisplay = MoveToFile(path);
            }
            else
            {
                int fileIndex = currentDirectory.SearchDirectory(path);

                if (fileIndex != -1)
                {
                    DirectoryEntry fileEntry = currentDirectory.DirFiles[fileIndex];

                    fileToDisplay = new FileEntry(new string(fileEntry.DIR_Name).Trim('\0'), fileEntry.DIR_Attr, fileEntry.DIR_FirstCluster, fileEntry.DIR_FileSize, currentDirectory, "");
                }
                else
                {
                    Console.WriteLine($"Error: File '{path}' not found.");
                    return;
                }
            }

            if (fileToDisplay == null)
            {
                Console.WriteLine($"Error: File '{path}' not found.");
                return;
            }

            if (fileToDisplay.DIR_Attr == 0x10)
            {
                Console.WriteLine($"Error: '{path}' is a directory, not a file.");
                return;
            }
            fileToDisplay.ReadFileContent();

            // Display the content with the file name
            Console.WriteLine($"\nFile: {new string(fileToDisplay.DIR_Name).Trim('\0')}\n"); // Added file name display

            if (string.IsNullOrEmpty(fileToDisplay.FileContent))
            {
                Console.WriteLine("<Empty File>");
            }
            else
                Console.WriteLine(fileToDisplay.FileContent);

        }
        private static void HandleImport(string[] tokens)
        { // Case (1): user only typed "import" if (tokens.Length == 1) { Console.WriteLine("Error: The syntax of the import command is: import [source] [destination]"); return; }

            // The import command syntax allows 2 or 3 arguments in total
            if (tokens.Length < 2 || tokens.Length > 3)
            {
                Console.WriteLine("Error: The syntax of the import command is:\n" +
                                  " import [source]\n or\n import [source] [destination]\n" +
                                  "[source] can be file name (or fullpath) or directory name (or fullpath)\n" +
                                  "        on your physical disk.\n" +
                                  "[destination] can be file name (or fullpath) or directory name (or fullpath)\n" +
                                  "             on your virtual disk.");
                return;
            }

            string sourcePath = tokens[1];       // The file/directory path on the host
            string destinationPath = (tokens.Length == 3) ? tokens[2] : null; // The file/directory path in the virtual disk

            // Determine whether sourcePath is a full path or relative to the .exe's current directory
            string fullHostPath;
            if (sourcePath.Contains('\\'))
            {
                // Possibly a full path from user
                fullHostPath = sourcePath;
            }
            else
            {
                // Combine with current directory of the exe if it doesn't look like a full path
                fullHostPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), sourcePath);
            }

            // Check existence on the host
            // Covering Cases (2) and (4) for missing file, (6), (7), (8) for directories
            bool isHostFile = File.Exists(fullHostPath);
            bool isHostDir = System.IO.Directory.Exists(fullHostPath);

            if (!isHostFile && !isHostDir)
            {
                // If neither a file nor a directory, display an appropriate error
                // This covers missing files or directories (Cases 2, 4, 8)
                // The message can be specialized a bit based on the user’s input
                if (tokens.Length == 2)
                {
                    // The user typed: import <something> (no second argument)
                    if (Path.HasExtension(fullHostPath))
                        // Possibly a missing file
                        Console.WriteLine($"Error: File '{sourcePath}' does not exist in the current directory of your computer.");
                    else
                        // Possibly a missing directory
                        Console.WriteLine($"Error: Directory '{sourcePath}' was not found in your computer.");
                }
                else
                {
                    // The user typed: import <something> <something>
                    // Either the first or second is missing in the host, but the second is a destination in the virtual disk, so the missing one is the source
                    if (Path.HasExtension(fullHostPath))
                        Console.WriteLine($"Error: File '{sourcePath}' does not exist in your computer.");
                    else
                        Console.WriteLine($"Error: Directory '{sourcePath}' was not found in your computer.");
                }
                return;
            }

            // If it exists, delegate to the main import logic
            ImportItem(fullHostPath, destinationPath, isHostFile, isHostDir);

        }
        private static void ImportItem(string hostPath, /*file or directory path on the host*/ string destinationPath, /* file or directory in the virtual disk (can be null)*/ bool sourceIsFile, /* based on File.Exists(...)*/ bool sourceIsDirectory /*based on System.IO.Directory.Exists(...)*/ )
        {
            // Cases : // (2), (4), (8): Already handled by existence check in HandleImport. // (3) or (5): file import into the current directory of the virtual disk if no destination is provided (3 for local name, 5 for full path). // (6) or (7): directory import into the current directory of the virtual disk if no destination is provided (6 for full path, 7 for local name). // (9) or (10): directory import into a directory or file in the virtual disk. // (11) or (12): file import into a file or directory in the virtual disk. // (13): Overwrite prompts for existing files in the virtual disk.

            List<string> importedFiles = new List<string>();

            // If no destination is given, import into the “currentDirectory” in the virtual disk
            if (string.IsNullOrEmpty(destinationPath))
            {
                // If the source is a directory, import all .txt into currentDirectory
                if (sourceIsDirectory)
                {
                    // Covers Cases (6) or (7) if it’s local name vs. full path
                    // but we handle them in the same logic: import text files to currentDirectory
                    string[] txtFiles = System.IO.Directory.GetFiles(hostPath, "*.txt");
                    foreach (string txtFile in txtFiles)
                    {
                        string fileNameHost = Path.GetFileName(txtFile);
                        string content = "";
                        try
                        {
                            content = File.ReadAllText(txtFile);
                        }
                        catch (Exception eRead)
                        {
                            Console.WriteLine($"Error: Could not read file '{txtFile}' -> {eRead.Message}");
                            continue;
                        }
                        // If a file with the same name exists, ask for overwrite (Case 13)
                        if (currentDirectory.SearchDirectory(fileNameHost) != -1)
                        {
                            Console.Write($"File '{fileNameHost}' already exists in the virtual disk. Overwrite? (y/n): ");
                            string ans = Console.ReadLine().ToLower();
                            if (ans == "y")
                            {
                                OverwriteFile(currentDirectory, fileNameHost, content);
                                importedFiles.Add(fileNameHost);
                            }
                        }
                        else
                        {
                            CreateFile(currentDirectory, fileNameHost, content);
                            importedFiles.Add(fileNameHost);
                        }
                    }
                    foreach (var f in importedFiles) Console.WriteLine(f);
                    Console.WriteLine($"\t\t{importedFiles.Count} File(s) imported.");
                    return;
                }
                else
                {
                    // The source is a single file
                    // Covers Cases (3) or (5): import file to the current directory of the virtual disk
                    string fileNameHost = Path.GetFileName(hostPath);
                    string content = "";
                    try
                    {
                        content = File.ReadAllText(hostPath);
                    }
                    catch (Exception eRead)
                    {
                        Console.WriteLine($"Error: Could not read file '{hostPath}' -> {eRead.Message}");
                        return;
                    }
                    // Overwrite prompt if file already exists (Case 13)
                    if (currentDirectory.SearchDirectory(fileNameHost) != -1)
                    {
                        Console.Write($"File '{fileNameHost}' already exists in the virtual disk. Overwrite? (y/n): ");
                        string ans = Console.ReadLine().ToLower();
                        if (ans == "y")
                        {
                            OverwriteFile(currentDirectory, fileNameHost, content);
                            importedFiles.Add(fileNameHost);
                        }
                    }
                    else
                    {
                        CreateFile(currentDirectory, fileNameHost, content);
                        importedFiles.Add(fileNameHost);
                    }
                    foreach (var f in importedFiles) Console.WriteLine(f);
                    Console.WriteLine($"\t\t{importedFiles.Count} File(s) imported.");
                    return;
                }
            }

            // If destinationPath is given, see if that item exists in the virtual disk
            DirectoryEntry destEntry = GetDirectoryEntryFromPath(destinationPath);
            Directory destParent = GetParentDirectoryFromPath(destinationPath);

            // If the parent does not exist, we cannot create or import
            if (destParent == null)
            {
                Console.WriteLine($"Error: The destination path '{destinationPath}' not found in the virtual disk.");
                return;
            }

            bool destExists = (destEntry != null);
            bool destIsDirectory = (destExists && ((destEntry.DIR_Attr & 0x10) == 0x10));

            // ────────────────────────────────────────────────
            // Source is a directory => Cases (9) or (10)
            // ────────────────────────────────────────────────
            if (sourceIsDirectory)
            {
                // If the destination is:
                //   (a) an existing directory => Case (9) (All .txt from host directory -> existing directory in virtual disk)
                //   (b) an existing file => Case (10) (concatenate .txt from host directory -> single file in virtual disk)
                //   (c) does not exist => decide if it's a new directory or new file by extension presence

                string[] txtFiles = System.IO.Directory.GetFiles(hostPath, "*.txt");
                if (destExists && destIsDirectory)
                {
                    // Case (9): directory -> directory
                    Directory destDir = new Directory(
                        new string(destEntry.DIR_Name).Trim('\0'),
                        destEntry.DIR_Attr,
                        destEntry.DIR_FirstCluster,
                        destEntry.DIR_FileSize,
                        destParent
                    );
                    destDir.ReadDirectory();

                    foreach (string txtFile in txtFiles)
                    {
                        string fileNameHost = Path.GetFileName(txtFile);
                        string content = "";
                        try
                        {
                            content = File.ReadAllText(txtFile);
                        }
                        catch (Exception eRead)
                        {
                            Console.WriteLine($"Error reading file '{txtFile}': {eRead.Message}");
                            continue;
                        }

                        // Overwrite prompt if file name already exists (Case 13)
                        if (destDir.SearchDirectory(fileNameHost) != -1)
                        {
                            Console.Write($"File '{fileNameHost}' already exists in '{destinationPath}' (virtual disk). Overwrite? (y/n): ");
                            string ans = Console.ReadLine().ToLower();
                            if (ans == "y")
                            {
                                OverwriteFile(destDir, fileNameHost, content);
                                importedFiles.Add(fileNameHost);
                            }
                        }
                        else
                        {
                            CreateFile(destDir, fileNameHost, content);
                            importedFiles.Add(fileNameHost);
                        }
                    }
                    foreach (var file in importedFiles) Console.WriteLine(file);
                    Console.WriteLine($"\t\t{importedFiles.Count} File(s) imported.");
                }
                else if (destExists && !destIsDirectory)
                {
                    // Case (10): directory -> a single file (concatenate .txt into one new or existing file)
                    Console.Write($"File '{destinationPath}' already exists in the virtual disk. Overwrite? (y/n): ");
                    string overwriteAns = Console.ReadLine().ToLower();
                    if (overwriteAns != "y") return;

                    // Concatenate .txt
                    string combined = "";
                    foreach (var txtFile in txtFiles)
                    {
                        try
                        {
                            combined += File.ReadAllText(txtFile);
                        }
                        catch (Exception eRead)
                        {
                            Console.WriteLine($"Error reading '{txtFile}': {eRead.Message}");
                        }
                    }
                    string destFileName = new string(destEntry.DIR_Name).Trim('\0');
                    OverwriteFile(destParent, destFileName, combined);
                    Console.WriteLine($"Concatenated {txtFiles.Length} .txt file(s) into '{destFileName}'.");
                }
                else
                {
                    // Destination does not exist => interpret based on extension if it’s a file or directory
                    bool hasExt = Path.HasExtension(destinationPath);
                    string newName = Path.GetFileName(destinationPath);
                    if (hasExt)
                    {
                        // Creating a new file => still Case (10)
                        // We will combine all .txt
                        Console.WriteLine($"File '{destinationPath}' does not exist in the virtual disk. Creating it.");
                        string combined = "";
                        foreach (string txtFile in txtFiles)
                        {
                            try
                            {
                                combined += File.ReadAllText(txtFile);
                            }
                            catch (Exception eRead)
                            {
                                Console.WriteLine($"Error reading '{txtFile}': {eRead.Message}");
                            }
                        }
                        CreateFile(destParent, newName, combined);
                        Console.WriteLine($"Concatenated {txtFiles.Length} .txt file(s) into '{newName}'.");
                    }
                    else
                    {
                        // Creating a new directory => still a form of Case (9)
                        Console.WriteLine($"Directory '{destinationPath}' not found in the virtual disk. Creating it.");
                        DirectoryEntry newDirEntry = new DirectoryEntry(newName, 0x10, 0);
                        if (!destParent.CanAddEntry(newDirEntry))
                        {
                            Console.WriteLine("Error: Not enough space to create a new directory in the virtual disk.");
                            return;
                        }
                        destParent.AddEntry(newDirEntry);

                        Directory newDestDir = new Directory(
                            newName,
                            0x10,
                            newDirEntry.DIR_FirstCluster,
                            0,
                            destParent
                        );
                        newDestDir.ReadDirectory();

                        foreach (string txtFile in txtFiles)
                        {
                            string fileNameHost = Path.GetFileName(txtFile);
                            string content = "";
                            try
                            {
                                content = File.ReadAllText(txtFile);
                            }
                            catch (Exception eRead)
                            {
                                Console.WriteLine($"Error reading '{txtFile}': {eRead.Message}");
                                continue;
                            }
                            if (newDestDir.SearchDirectory(fileNameHost) != -1)
                            {
                                Console.Write($"File '{fileNameHost}' already exists in '{destinationPath}' (virtual disk). Overwrite? (y/n): ");
                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    OverwriteFile(newDestDir, fileNameHost, content);
                                    importedFiles.Add(fileNameHost);
                                }
                            }
                            else
                            {
                                CreateFile(newDestDir, fileNameHost, content);
                                importedFiles.Add(fileNameHost);
                            }
                        }
                        foreach (var f in importedFiles) Console.WriteLine(f);
                        Console.WriteLine($"\t\t{importedFiles.Count} File(s) imported.");
                    }
                }
                return;
            }

            // ────────────────────────────────────────────────
            // Source is a single file => Cases (11) or (12)
            // ────────────────────────────────────────────────
            if (sourceIsFile)
            {
                string fileContent;
                try
                {
                    fileContent = File.ReadAllText(hostPath);
                }
                catch (Exception eRead)
                {
                    Console.WriteLine($"Error: Could not read file '{hostPath}' -> {eRead.Message}");
                    return;
                }
                string hostFileName = Path.GetFileName(hostPath);

                if (destExists && !destIsDirectory)
                {
                    // Case (11): file -> file, destination already exists => prompt
                    Console.Write($"File '{destinationPath}' already exists in the virtual disk. Overwrite? (y/n): ");
                    if (Console.ReadLine().ToLower() != "y") return;

                    string destFileName = new string(destEntry.DIR_Name).Trim('\0');
                    OverwriteFile(destParent, destFileName, fileContent);
                    Console.WriteLine($"Successfully overwritten '{destFileName}' with content from '{hostFileName}'.");
                }
                else if (destExists && destIsDirectory)
                {
                    // Case (12): file -> directory, if a file with the same name does not exist, create it
                    Directory actualDestination = new Directory(
                        new string(destEntry.DIR_Name).Trim('\0'),
                        destEntry.DIR_Attr,
                        destEntry.DIR_FirstCluster,
                        destEntry.DIR_FileSize,
                        destParent
                    );
                    actualDestination.ReadDirectory();

                    // If a file with the same name is found => ask user to overwrite or do nothing
                    if (actualDestination.SearchDirectory(hostFileName) != -1)
                    {
                        Console.Write($"File '{hostFileName}' already exists in '{destinationPath}' (virtual disk). Overwrite? (y/n): ");
                        if (Console.ReadLine().ToLower() == "y")
                        {
                            OverwriteFile(actualDestination, hostFileName, fileContent);
                            Console.WriteLine($"Overwritten file '{hostFileName}' inside '{destinationPath}'.");
                        }
                    }
                    else
                    {
                        CreateFile(actualDestination, hostFileName, fileContent);
                        Console.WriteLine($"Created '{hostFileName}' inside '{destinationPath}'.");
                    }
                }
                else
                {
                    // Destination does not exist => decide if it’s a file or directory
                    bool hasExt = Path.HasExtension(destinationPath);
                    string newName = Path.GetFileName(destinationPath);
                    if (hasExt)
                    {
                        // Case (11): create new file if not found
                        Console.WriteLine($"File '{destinationPath}' not found in the virtual disk. Creating it.");
                        CreateFile(destParent, newName, fileContent);
                        Console.WriteLine($"Created '{newName}' with the content from '{hostFileName}'.");
                    }
                    else
                    {
                        // Case (12): create a new directory, then the file inside it
                        Console.WriteLine($"Directory '{destinationPath}' not found in the virtual disk. Creating it.");
                        DirectoryEntry newDirEntry = new DirectoryEntry(newName, 0x10, 0);
                        if (!destParent.CanAddEntry(newDirEntry))
                        {
                            Console.WriteLine("Error: Not enough space to create a directory in the virtual disk.");
                            return;
                        }
                        destParent.AddEntry(newDirEntry);

                        Directory newDestDir = new Directory(
                            newName,
                            0x10,
                            newDirEntry.DIR_FirstCluster,
                            0,
                            destParent
                        );
                        newDestDir.ReadDirectory();

                        // Now create or overwrite the file inside that directory
                        if (newDestDir.SearchDirectory(hostFileName) != -1)
                        {
                            Console.Write($"File '{hostFileName}' already exists in newly created directory '{destinationPath}'. Overwrite? (y/n): ");
                            if (Console.ReadLine().ToLower() == "y")
                            {
                                OverwriteFile(newDestDir, hostFileName, fileContent);
                                Console.WriteLine($"Overwritten '{hostFileName}' in '{destinationPath}'.");
                            }
                        }
                        else
                        {
                            CreateFile(newDestDir, hostFileName, fileContent);
                            Console.WriteLine($"Created '{hostFileName}' in directory '{destinationPath}'.");
                        }
                    }
                }
            }

        }
        private static void HandleExport(string[] tokens)
        { // Case (1): user only typed "export" // Display syntax error if (tokens.Length == 1) { Console.WriteLine("Error: The syntax of the export command is: export [source] [destination]"); return; }

            // tokens.Length must be 2 or 3 to be valid
            if (tokens.Length < 2 || tokens.Length > 3)
            {
                Console.WriteLine("Error: The syntax of the export command is: export [source] [destination]");
                return;
            }

            string sourcePath = tokens[1];
            // Destination is optional. If omitted, we assume exporting into our shell's current executable directory.
            string destinationPath = (tokens.Length == 3) ? tokens[2] : null;

            // Delegate the export operation:
            ExportItem(sourcePath, destinationPath);
        }

        private static void ExportItem(string sourcePath, string destinationPath)
        { // 1. Find the source entry in the virtual disk.
            DirectoryEntry sourceEntry = GetDirectoryEntryFromPath(sourcePath);
            if (sourceEntry == null)
            { // Covers: // Case (2) or (4): file not found (by name or full path). // Case (8) if source is a directory and not found.
                Console.WriteLine($"Error: Source '{sourcePath}' not found in the virtual disk."); return;
            }

            // 2. If destination is omitted, export to the current directory of the executable (Cases (3), (5), (6), (7))
            if (string.IsNullOrEmpty(destinationPath))
            {
                destinationPath = System.IO.Directory.GetCurrentDirectory(); //If no destionation path is provided, export to where the exe is running
            }

            // 3. Check if the source entry is a directory or file (custom DirectoryEntry.DIR_Attr = 0x10 means directory)
            bool isDirectory = (sourceEntry.DIR_Attr == 0x10);

            // 3A. If the source is a directory:
            //   Cases (6), (7), (9), (10) when directory is used as source.
            if (isDirectory)
            {
                List<string> exportedFiles = new List<string>(); // To store virtual paths of exported files

                // Load the directory information so we can iterate its files if needed
                Directory parentDir = GetParentDirectoryFromPath(sourcePath);
                Directory sourceDir = new Directory(
                    new string(sourceEntry.DIR_Name).Trim('\0'),
                    sourceEntry.DIR_Attr,
                    sourceEntry.DIR_FirstCluster,
                    sourceEntry.DIR_FileSize,
                    parentDir
                );
                sourceDir.ReadDirectory(); // Make sure we have up-to-date listings

                // If the destination path does not look like a file (no extension), assume it's a directory.
                // Create the directory (on the real host) if it doesn't exist.
                //   Cases (6), (7): no second argument means export all files to local directory
                //   Case (8) if the user typed an invalid directory for destination, but we’ll attempt to create it if it’s meant to be a directory.
                if (!System.IO.Path.HasExtension(destinationPath))
                {
                    // The user wants to export an entire directory to a directory on the host:
                    //   Cases (6), (7), (9)
                    // Or they want to export to a directory that may not exist yet.
                    try
                    {
                        System.IO.Directory.CreateDirectory(destinationPath); // safe if it already exists
                    }
                    catch (Exception exDir)
                    {
                        Console.WriteLine($"Error: Unable to create or access directory '{destinationPath}': {exDir.Message}");
                        return;
                    }

                    // Export all files from the virtual directory to the real file system
                    foreach (FileEntry file in sourceDir.GetFiles())
                    {
                        file.ReadFileContent();
                        string hostFileName = System.IO.Path.Combine(destinationPath, file.CleanTheName());
                        try
                        {
                            // Overwrite or create new
                            System.IO.File.WriteAllText(hostFileName, file.FileContent);

                            exportedFiles.Add(GetFullPath(sourceDir) + "\\" + file.CleanTheName()); //Collect virtual file paths for output message
                        }
                        catch (Exception exWrite)
                        {
                            Console.WriteLine($"Error exporting '{file.CleanTheName()}': {exWrite.Message}");
                        }
                    }
                    foreach (string file in exportedFiles)
                    {
                        Console.WriteLine(file); // Print each exported file path
                    }
                    Console.WriteLine($"\t\t{exportedFiles.Count} File(s) exported.");
                }
                else
                {
                    // The user wants to export a directory into one single file:
                    //3B. Directory -> single file, i.e. combine all files in directory into one file on the host
                    //   Case (10)
                    // (destinationPath has an extension → treat as a file)
                    string dirPart = System.IO.Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(dirPart) && !System.IO.Directory.Exists(dirPart))
                    {
                        // Attempt to create the directory if it doesn't exist
                        try
                        {
                            System.IO.Directory.CreateDirectory(dirPart);
                        }
                        catch (Exception exDir)
                        {
                            Console.WriteLine($"Error: Cannot create directory '{dirPart}': {exDir.Message}");
                            return;
                        }
                    }

                    // Concatenate all file contents in the source directory
                    // and write them to a single host file.
                    //case 10
                    string combinedContent = "";
                    List<string> combinedFiles = new List<string>(); // Keep track of files being combined
                    foreach (FileEntry file in sourceDir.GetFiles())
                    {
                        file.ReadFileContent();
                        combinedContent += file.FileContent + "\n\n";
                        combinedFiles.Add(GetFullPath(sourceDir) + "\\" + file.CleanTheName());//Keep track of files that their contents are combined
                    }

                    // Overwrite or create the file
                    try
                    {
                        System.IO.File.WriteAllText(destinationPath, combinedContent);
                        foreach (string file in combinedFiles)
                        {
                            Console.WriteLine(file); // Print each exported file path
                        }
                        Console.WriteLine($"\t\t{combinedFiles.Count} File(s) exported.");// print total file count
                    }
                    catch (Exception exWrite)
                    {
                        Console.WriteLine($"Error exporting directory '{sourceDir.CleanTheName()}' into a file: {exWrite.Message}");
                    }
                }
            }
            else
            {
                // 4. Source is a file:
                //   Cases (2), (3), (4), (5), (11), (12)
                // We already confirmed it exists in the virtual disk. Now see how we treat the destination.

                // Read file content from our virtual disk.
                Directory parentSource = GetParentDirectoryFromPath(sourcePath);
                FileEntry sourceFile = new FileEntry(
                    new string(sourceEntry.DIR_Name).Trim('\0'),
                    sourceEntry.DIR_Attr,
                    sourceEntry.DIR_FirstCluster,
                    sourceEntry.DIR_FileSize,
                    parentSource,
                    null
                );
                sourceFile.ReadFileContent();

                // If destination has no extension, treat it as a directory export
                if (!System.IO.Path.HasExtension(destinationPath))
                {
                    // 4A. File -> directory
                    //   Case (12): exporting a single file from the virtual disk into a directory on the real system.
                    // Make sure that directory exists or try to create it
                    try
                    {
                        System.IO.Directory.CreateDirectory(destinationPath);
                    }
                    catch (Exception exDir)
                    {
                        Console.WriteLine($"Error: Unable to create or access directory '{destinationPath}': {exDir.Message}");
                        return;
                    }

                    // Construct a new file path in that destination directory using the same name as source
                    string newFilePath = System.IO.Path.Combine(destinationPath, sourceFile.CleanTheName());
                    try
                    {
                        // Overwrite or create new
                        System.IO.File.WriteAllText(newFilePath, sourceFile.FileContent);

                        Console.WriteLine(GetFullPath(sourceFile.Parent) + "\\" + sourceFile.CleanTheName()); // Show virtual file path
                        Console.WriteLine($"\t\t1 File(s) exported.");
                    }
                    catch (Exception exWrite)
                    {
                        Console.WriteLine($"Error exporting file '{sourceFile.CleanTheName()}': {exWrite.Message}");
                    }
                }
                else
                {
                    // 4B. File -> file
                    //   Case (11): exporting a single file from our virtual disk to some file name or full path on the real system
                    // Possibly overwriting. Check if directory part exists; create if needed.
                    string dirPart = System.IO.Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(dirPart) && !System.IO.Directory.Exists(dirPart))
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(dirPart);
                        }
                        catch (Exception exDir)
                        {
                            Console.WriteLine($"Error: Cannot create directory '{dirPart}': {exDir.Message}");
                            return;
                        }
                    }

                    // Overwrite or create the file
                    try
                    {
                        System.IO.File.WriteAllText(destinationPath, sourceFile.FileContent);

                        Console.WriteLine(GetFullPath(sourceFile.Parent) + "\\" + sourceFile.CleanTheName()); // Show virtual file path
                        Console.WriteLine($"\t\t1 File(s) exported.");
                    }
                    catch (Exception exWrite)
                    {
                        Console.WriteLine($"Error exporting file '{sourceFile.CleanTheName()}': {exWrite.Message}");
                    }
                }
            }

        }

    }
}
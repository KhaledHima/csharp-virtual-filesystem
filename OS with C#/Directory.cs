using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_with_C_
{
    public class Directory : DirectoryEntry
    {
        
        /// A list of directory entries (files and subdirectories) within this directory.
        public List<DirectoryEntry> DirFiles;

     
        /// The parent directory of this directory. Null if this is the root directory.
        public Directory parent;

      
        /// Constructor for creating a Directory object with a name, attribute, first cluster, file size, and parent directory.
        ///  name="name">The name of the directory.
        ///  name="dirAttr">The attribute of the directory (0x10 for directories).
        ///  name="dirFirstCluster">The first cluster number of the directory.
        ///  name="dirFileSize">The size of the directory (typically 0 for directories).
        ///  name="parent">The parent directory of this directory.
        public Directory(string name, byte dirAttr, int dirFirstCluster, int dirFileSize, Directory parent)
            : base(name, dirAttr, dirFirstCluster)
        {
            DirFiles = new List<DirectoryEntry>(); // Initialize the list of directory entries
            this.parent = parent; // Set the parent directory
            DIR_FileSize = dirFileSize; // Initialize the file size (typically 0 for directories)
        }

        
        /// Gets a copy of this directory as a DirectoryEntry object.
        /// <returns> A DirectoryEntry object representing this directory.
        public DirectoryEntry GetDirectoryEntry()
        {
            return new DirectoryEntry(new string(DIR_Name).Trim('\0'), DIR_Attr, DIR_FirstCluster)
            {
                DIR_FileSize = DIR_FileSize
            }; // Create a *copy*
        }

        
        /// Calculates the size of this directory on disk based on the number of entries.
        /// <returns>The size of the directory in bytes.
        public int GetMySizeOnDisk()
        {
            return DirFiles.Count * 32; // Each directory entry is 32 bytes
        }


        /// Checks if a new entry can be added to this directory.
        ///  name="d">The directory entry to add.
        /// <returns>True if there is enough space to add the entry, otherwise false.
        public bool CanAddEntry(DirectoryEntry d)
        {
            return MiniFAT.GetFreeSize() >= 32; // Check if there's enough free space for one entry (32 bytes)
        }

      
        /// Frees all clusters associated with this directory, optionally clearing their data.
        /// name="clearData">If true, clears the data in the clusters.
        public void EmptyMyClusters(bool clearData = false)
        {
            if (DIR_FirstCluster != 0)
            {
                int currentCluster = DIR_FirstCluster;
                while (currentCluster != -1)
                {
                    int nextCluster = MiniFAT.GetClusterPointer(currentCluster);
                    if (clearData)
                    {
                        // Clear the cluster data by writing zeros
                        byte[] emptyCluster = new byte[VirtualDisk.ClusterSize];
                        VirtualDisk.WriteCluster(emptyCluster, currentCluster);
                    }
                    // Free the cluster in the FAT
                    MiniFAT.SetClusterPointer(currentCluster, 0);
                    currentCluster = nextCluster;
                }
                DIR_FirstCluster = 0; // Reset the directory's first cluster
                VirtualDisk.disk.Flush(); // Ensure data is written to disk
            }
        }

       
        /// Writes the directory's content to the virtual disk.
        public void WriteDirectory()
        {
            if (DirFiles == null || DirFiles.Count == 0)
            {
                if (DIR_FirstCluster != 0)// if the directory had cluster(s) before, free them.
                {
                    EmptyMyClusters(); // Free clusters

                    MiniFAT.WriteFAT();//Update the FAT table on disk.
                }
                // Update the directory's own entry in the parent directory
                if (parent != null) //Crucial step to update directory entry in parent.
                {
                    // Crucial: Remove this directory's entry from parent BEFORE parent writes to disk
                    parent.RemoveEntry(this); //Remove entry from parent's directory list

                    parent.WriteDirectory(); //This will correctly remove the dir from virtual disk

                    if (parent == VirtualDisk.Root) // if parent is Root, update root directory
                    {


                        VirtualDisk.Root.WriteDirectory(); //Ensure Virtual Disk update

                        VirtualDisk.disk.Flush();//ensure the change written to Virtual Disk
                    }


                }
                else if (this == VirtualDisk.Root) // Root directory handling
                {
                    EmptyMyClusters();// free the Root Clusters if empty.
                    MiniFAT.WriteFAT(); // Update FAT table

                    if (DirFiles.Count == 0) //If Root directory is empty
                    {
                        //Clear its cluster by writing empty entry
                        DIR_FirstCluster = 5; //Set DIR_FirstCluster to 5 as an initial value
                        byte[] emptyRootEntry = new byte[VirtualDisk.ClusterSize]; // Ensures it's the correct size

                        // ... validation ...
                        VirtualDisk.WriteCluster(emptyRootEntry, DIR_FirstCluster);
                    }
                    VirtualDisk.disk.Flush();
                }

                return; // Exit early if directory is empty
            }

            // 1. Prepare directory data
            List<byte> dirData = new List<byte>();
            foreach (DirectoryEntry entry in DirFiles)
            {
                dirData.AddRange(entry.GetBytes());
            }

            // 2. Calculate needed clusters and check for space
            int neededClusters = (int)Math.Ceiling((double)dirData.Count / VirtualDisk.ClusterSize);
            if (MiniFAT.GetAvailableClustersCount() < neededClusters)
            {
                Console.WriteLine("Not enough space to write directory.");
                return;
            }

           
            if (VirtualDisk.GetFreeSpace() < neededClusters * VirtualDisk.ClusterSize) //Check enough free space
            {
                Console.WriteLine("Error: Not enough free space on the disk.");
                return;
            }

            // 3. Free existing clusters and update parent (Important)
            if (DIR_FirstCluster != 0)//&& parent != null)
            {
                EmptyMyClusters();
                MiniFAT.WriteFAT();
            }

            // 4. Allocate new clusters & write directory data
            int currentCluster = MiniFAT.GetAvailableCluster();
            if (currentCluster == -1)
            {
                Console.WriteLine("No available clusters.");
                return;
            }
            DIR_FirstCluster = currentCluster;  // Update DIR_FirstCluster *after* getting a free cluster

            int bytesWritten = 0;

            while (bytesWritten < dirData.Count)

            {
                int nextCluster = MiniFAT.GetAvailableCluster();

                MiniFAT.SetClusterPointer(currentCluster, nextCluster);  // Link the current cluster to the next

                int bytesToWrite = Math.Min(VirtualDisk.ClusterSize, dirData.Count - bytesWritten);

                byte[] dataToWrite = dirData.Skip(bytesWritten).Take(bytesToWrite).ToArray();
                // Pad with zeros if necessary
                if (bytesToWrite < VirtualDisk.ClusterSize)
                {
                    dataToWrite = dataToWrite.Concat(new byte[VirtualDisk.ClusterSize - bytesToWrite]).ToArray();
                }

                VirtualDisk.WriteCluster(dataToWrite, currentCluster);

                bytesWritten += bytesToWrite;

                currentCluster = nextCluster; // Move to the next cluster for the next iteration

            }
            MiniFAT.SetClusterPointer(currentCluster, -1); // Mark the end of the chain.



            // 5. Update parent directory entry (Crucial)
            if (parent != null)
            {
                int indexInParent = parent.DirFiles.FindIndex(e => new string(e.DIR_Name).Trim('\0').Equals(new string(this.DIR_Name).Trim('\0'), StringComparison.OrdinalIgnoreCase));
                if (indexInParent != -1)
                {
                    parent.DirFiles[indexInParent].DIR_FirstCluster = DIR_FirstCluster; // Update FirstCluster for directories

                    parent.WriteDirectory(); //Update Parent's Directory in virtual disk
                }
            }

            MiniFAT.WriteFAT(); // Update FAT after all modifications
            VirtualDisk.disk.Flush(); //<--- Ensure immediate visibility
        }

        
        /// Reads the directory's content from the virtual disk.
        public void ReadDirectory()
        {
            DirFiles.Clear(); // Clear any existing entries
            if (DIR_FirstCluster == 0) // If the directory is empty
                return; // Return early if the directory is empty (nothing to read)

            int currentCluster = DIR_FirstCluster;
            while (currentCluster != -1)
            {
                byte[] clusterData = VirtualDisk.ReadCluster(currentCluster); // Read the cluster
                for (int i = 0; i < clusterData.Length; i += 32) // Each entry is 32 bytes
                {
                    if (clusterData[i] == 0) // Check for empty entry
                        break;

                    byte[] entryBytes = new byte[32];
                    Array.Copy(clusterData, i, entryBytes, 0, 32); // Extract the entry
                    DirFiles.Add(DirectoryEntry.FromBytes(entryBytes)); // Convert bytes to DirectoryEntry
                }
                currentCluster = MiniFAT.GetClusterPointer(currentCluster); // Move to the next cluster
            }
        }

        
        /// Adds a new entry (file or subdirectory) to this directory.
        ///  name="entry">The entry to add.
        public void AddEntry(DirectoryEntry entry)
        {
            if (!CanAddEntry(entry))
            {
                Console.WriteLine("Not enough space to add entry.");
                return;
            }

            if (SearchDirectory(entry.CleanTheName()) != -1) // Prevent duplicate entries
            {
                Console.WriteLine("Error: Entry with this name already exists.");
                return;
            }
            DirFiles.Add(entry); // Add the entry to the directory

            if (entry is FileEntry fileEntry)
            {
                fileEntry.Parent = this; // Set the parent directory for the file
                if (fileEntry.FileContent != null) // Write the file content if it exists
                {
                    fileEntry.WriteFileContent();
                }
            }
            WriteDirectory(); // Write the directory to disk after adding the entry
        }

        /// Removes an entry (file or subdirectory) from this directory.
        /// name="entry">The entry to remove.
        public void RemoveEntry(DirectoryEntry entry)
        {
            int index = DirFiles.FindIndex(e =>
                new string(e.DIR_Name).Trim('\0') == new string(entry.DIR_Name).Trim('\0')); // Find the entry

            if (index != -1)
            {
                DirFiles.RemoveAt(index); //Remove the entry from parent's DirFiles

                if (DirFiles.Count == 0 && this == VirtualDisk.Root) //If Root is Empty after remove, update it explicitly
                {
                    DIR_FirstCluster = 0; // Reset the root directory's first cluster
                }
           
                VirtualDisk.disk.Flush(); // Ensure changes are written to disk
            }
        }

        
        /// Deletes this directory and all its contents.
        public void DeleteDirectory()
        {

            if (this == VirtualDisk.Root)
            {
                Console.WriteLine("Cannot delete the root directory.");
                return;
            }

            // 1. Delete all files  and subdirectories within the directory
            List<DirectoryEntry> entriesToDelete = new List<DirectoryEntry>(DirFiles); // Create a copy to avoid modification during iteration
            foreach (DirectoryEntry entry in entriesToDelete)
            {
                if (entry.DIR_Attr != 0x10) //it's a file
                {

                    FileEntry fileToDelete = new FileEntry(new string(entry.DIR_Name).Trim('\0'), entry.DIR_Attr, entry.DIR_FirstCluster, entry.DIR_FileSize, this, null);

                    fileToDelete.DeleteFile(); // Delete the file
                }
                else // If it's a subdirectory

                {
                    Directory subDir = new Directory(new string(entry.DIR_Name).Trim('\0'), entry.DIR_Attr, entry.DIR_FirstCluster, entry.DIR_FileSize, this);

                    subDir.DeleteDirectory(); // Recursively delete the subdirectory
                }
            }

            // 2.  Clear directory content on disk
            EmptyMyClusters(true); // Pass 'true' to clear the clusters

            // 3. Remove the directory entry from its parent
            if (parent != null)
            {
                parent.RemoveEntry(this); // Remove this directory from the parent

                parent.WriteDirectory();//Very Important

                MiniFAT.WriteFAT();
            }
        }

       
        /// Updates the content of an entry in this directory.
        /// name="oldEntry">The old entry to update.
        /// name="newEntry">The new entry to replace the old one.
        public void UpdateContent(DirectoryEntry oldEntry, DirectoryEntry newEntry)
        {
            int index = DirFiles.FindIndex(e =>
                new string(e.DIR_Name) == new string(oldEntry.DIR_Name)); // Find the old entry

            if (index != -1)
            {
                DirFiles[index] = newEntry; // Replace the old entry with the new one
                WriteDirectory(); // Write the directory to disk
            }
        }

        
        /// Searches for an entry in this directory by name.
        /// name="name">The name of the entry to search for.
        /// <returns>The index of the entry if found, otherwise -1.
        public int SearchDirectory(string name)
        {
            for (int i = 0; i < DirFiles.Count; i++)
            {
                if (string.Equals(DirFiles[i].CleanTheName(), name, StringComparison.OrdinalIgnoreCase))
                {
                    return i; // Return the index of the matching entry
                }
            }
            return -1; // Entry not found
        }

       
        /// Gets a list of all files in this directory.
        /// <returns>A list of FileEntry objects representing the files in this directory.
        
        //Aditional Function For HandleExport  in FileSystemShell Class
        public List<FileEntry> GetFiles()
        {
            List<FileEntry> files = new List<FileEntry>();
            foreach (var entry in DirFiles)
            {
                if ((entry.DIR_Attr & 0x10) != 0x10) // If it's a file
                {
                    FileEntry file = new FileEntry(
                        new string(entry.DIR_Name).Trim('\0'),
                        entry.DIR_Attr,
                        entry.DIR_FirstCluster,
                        entry.DIR_FileSize,
                        this, 
                        null
                    ); //this : current directory
                    files.Add(file); // Add the file to the list
                }
            }
            return files;
        }

      
        /// Checks if an entry with the given name exists in this directory.
        /// name="entryName">The name of the entry to check.
        /// <returns>True if the entry exists, otherwise false.

        //additional function for CreateFile in FileSystemShell Class
        public bool EntryExists(string entryName)
        {
            ReadDirectory(); // Ensure the directory is up-to-date
            return SearchDirectory(entryName) != -1; // Search for the entry
        }
    }
}

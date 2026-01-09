using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_with_C_
{
 
    /// Represents a file entry in the virtual file system.
    /// Inherits from <"DirectoryEntry"> to include file-specific properties and methods.
    public class FileEntry : DirectoryEntry
    {
        
        /// The content of the file as a string.
        public string FileContent;

        
        /// The parent directory of the file.
        public Directory Parent;

        
        /// Initializes a new instance of the <"FileEntry"> class.
        /// name="name">The name of the file.
        ///  name="dirAttr">The attribute of the file (e.g., 0x00 for files).
        ///  name="dirFirstCluster">The first cluster of the file in the FAT.
        ///  name="dirFileSize">The size of the file in bytes.
        ///  name="parent">The parent directory of the file.
        ///  name="content">The content of the file as a string.
        public FileEntry(string name, byte dirAttr, int dirFirstCluster, int dirFileSize, Directory parent, string content)
            : base(name, dirAttr, dirFirstCluster)
        {
            this.FileContent = content;
            this.Parent = parent;
            DIR_FileSize = dirFileSize;
        }

       
        /// Initializes a new instance of the <"FileEntry"> class with default values.
        public FileEntry() : base()
        {
            FileContent = "";
        }

        
        /// Calculates the size of the file on disk in bytes.
        /// <returns>The size of the file on disk in bytes.
        public int GetSizeOnDisk()
        {
            if (DIR_FirstCluster == 0)
            {
                return 0; // No clusters allocated, size is 0.
            }

            int size = 0;
            int cluster = DIR_FirstCluster;

            // Traverse the cluster chain to calculate the total size.
            while (cluster != -1)
            {
                size += VirtualDisk.ClusterSize;
                cluster = MiniFAT.GetClusterPointer(cluster);
            }

            return size;
        }

        
        /// Frees all clusters associated with this file.
        /// name="clearContent">If true, clears the content of the clusters on disk.
        public void EmptyMyClusters(bool clearContent = false)
        {
            if (DIR_FirstCluster != 0)
            {
                int currentCluster = DIR_FirstCluster;

                // Traverse the cluster chain and free each cluster.
                while (currentCluster != -1)
                {
                    int nextCluster = MiniFAT.GetClusterPointer(currentCluster);

                    if (clearContent)
                    {
                        // Clear the cluster data on disk by writing zeros.
                        byte[] emptyCluster = new byte[VirtualDisk.ClusterSize];//Array of zeros.
                        VirtualDisk.WriteCluster(emptyCluster, currentCluster);//Clear disk space
                    }

                    // Free the cluster in the FAT.
                    MiniFAT.SetClusterPointer(currentCluster, 0);
                    currentCluster = nextCluster;
                }

                DIR_FirstCluster = 0; // Reset the first cluster pointer.
                VirtualDisk.disk.Flush(); // Ensure changes are written to disk.
            }
        }

        
        /// Writes the file content to the virtual disk.
        public void WriteFileContent()
        {
            if (FileContent == null)
            {
                FileContent = ""; // Handle null content
            }

            byte[] contentBytes = Encoding.ASCII.GetBytes(FileContent);

            DIR_FileSize = contentBytes.Length;

            // Free any previously occupied clusters
            EmptyMyClusters();

            int neededClusters = (int)Math.Ceiling((double)DIR_FileSize / VirtualDisk.ClusterSize);

            if (MiniFAT.GetAvailableClustersCount() < neededClusters)
            {
                Console.WriteLine("Not enough space to write file content.");
                return;
            }

            // the check here 
            if (VirtualDisk.GetFreeSpace() < neededClusters * VirtualDisk.ClusterSize)
            {
                Console.WriteLine("Error: Not enough free space on the disk.");
                return;
            }

            int currentCluster = MiniFAT.GetAvailableCluster();

            if (currentCluster == -1)
            {
                Console.WriteLine("No available clusters.");
                return;
            }

            DIR_FirstCluster = currentCluster;

            int bytesWritten = 0;
            while (bytesWritten < DIR_FileSize)
            {
                int nextCluster = MiniFAT.GetAvailableCluster();

                MiniFAT.SetClusterPointer(currentCluster, nextCluster);  // Set next cluster BEFORE writing to the current one

                int bytesToWrite = Math.Min(VirtualDisk.ClusterSize, DIR_FileSize - bytesWritten);

                byte[] dataToWrite = new byte[VirtualDisk.ClusterSize];

                Array.Copy(contentBytes, bytesWritten, dataToWrite, 0, bytesToWrite);

                VirtualDisk.WriteCluster(dataToWrite, currentCluster);

                bytesWritten += bytesToWrite;

                currentCluster = nextCluster;
            }

            if (currentCluster != -1)  // Make sure to mark the end of the cluster chain
            {
                MiniFAT.SetClusterPointer(currentCluster, -1);
            }

            //Update Parent after all operations
            if (Parent != null)
            {
                Parent.UpdateContent(this, this);  // Update parent's entry
            }

            MiniFAT.WriteFAT();// important no delete: updated FAT after Modification.
        }

        
        /// Reads the file content from the virtual disk.
        public void ReadFileContent()
        {
            if (DIR_FirstCluster == 0)
            {
                FileContent = "";
                return;
            }

            List<byte> fileBytes = new List<byte>();
            int currentCluster = DIR_FirstCluster;

            // Traverse the cluster chain and read the content.
            while (currentCluster != -1)
            {
                byte[] clusterBytes = VirtualDisk.ReadCluster(currentCluster);
                int bytesToRead = Math.Min(clusterBytes.Length, DIR_FileSize - fileBytes.Count);
                fileBytes.AddRange(clusterBytes.Take(bytesToRead));
                currentCluster = MiniFAT.GetClusterPointer(currentCluster);
            }

            // Convert the bytes to a string.
            FileContent = Encoding.ASCII.GetString(fileBytes.ToArray());
        }

      
        /// Deletes the file from the virtual disk.
        public void DeleteFile() 
        {
            if (Parent != null)
            {
                // Clear the file content on disk and free clusters.
                EmptyMyClusters(true);

                // Remove the file entry from the parent directory.
                Parent.RemoveEntry(this);

                // If the parent is the root directory, update it explicitly.
                if (Parent == VirtualDisk.Root)
                {
                    Parent.WriteDirectory();
                    VirtualDisk.disk.Flush();
                }

                // Update the FAT after cluster changes.
                MiniFAT.WriteFAT();
            }
        }

        
        /// Prints the file content to the console.
        public void PrintContent()
        {
            Console.WriteLine($"<{new string(DIR_Name).Trim('\0')}>|<<{FileContent}>><<end>>");
        }
    }
}

using OS_with_C_;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OS_with_C_
{
    // Represents the virtual disk for the OS. Holds methods to read, write, initialize,
    // and manage clusters on the disk.
    public static class VirtualDisk
    {
        public static FileStream disk;
        public const int ClusterSize = 1024;
        public const int TotalClusters = 1024;
        private const int DiskSize = ClusterSize * TotalClusters;  //1 MB


        public static Directory Root;

        // Opens or creates the virtual disk file 
        public static void CreateOrOpenDisk(string path)
        {
            try
            {
                if (disk != null)
                {
                    disk.Close(); // Close any previous stream
                }

                disk = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite); //Ensure exclusive access
            }
            catch (IOException ex)  //specific about catching IOExceptions
            {
                Console.WriteLine($"Error creating or opening virtual disk: {ex.Message}"); // Provide more informative error messages
            }
            catch (Exception ex)
            {
                //  unexpected error 
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }


        // Write cluster to disk: index validation included
        public static void WriteCluster(byte[] cluster, int clusterIndex)
        {
            if (clusterIndex < 0 || clusterIndex >= TotalClusters)
                throw new ArgumentOutOfRangeException("Cluster index out of range.");

            if (cluster.Length != ClusterSize)  // NEW: Validate cluster size
                throw new ArgumentException($"Cluster size mismatch. Expected: {ClusterSize}, Actual: {cluster.Length}");
            try
            {
                disk.Seek(clusterIndex * ClusterSize, SeekOrigin.Begin);
                disk.Write(cluster, 0, ClusterSize);
                disk.Flush(); // Ensure data is written to disk
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing cluster {clusterIndex}: {ex.Message}"); 
            }
        }

        // Read cluster from disk: index validation included
        public static byte[] ReadCluster(int clusterIndex)
        {
            if (clusterIndex < 0 || clusterIndex >= TotalClusters)
                throw new ArgumentOutOfRangeException("Cluster index out of range.");


            disk.Seek(clusterIndex * ClusterSize, SeekOrigin.Begin);
            byte[] cluster = new byte[ClusterSize];
            disk.Read(cluster, 0, ClusterSize);
            return cluster;
        }

        // Get free space
        public static int GetFreeSpace()
        {
            disk.Seek(0, SeekOrigin.End); // get the length directly
            return DiskSize - (int)disk.Length; 
        }

        // IsNew check
        public static bool IsNew()
        {
            return disk.Length == 0;
        }

        public static void CloseDisk()
        {
            if (disk != null)
            {
                disk.Flush();//write all data from buffer into file.
                disk.Close();//close fileStream.
                disk.Dispose(); // Release the file handle, very Important

                disk = null; // Set to null after closing
            }
        }

        public static void Intialize(string path)
        {
            CreateOrOpenDisk(path);
            if (IsNew())
            {
                // Initialize a new virtual disk

                byte[] superBlock = MiniFAT.CreateSuperBlock();
                WriteCluster(superBlock, 0);
                MiniFAT.InitializeFAT();

                Root = new Directory("C:", 0x10, 5, 0, null); // Use string directly
                Root.WriteDirectory(); // write Root content when initializing  virtual disk

            }
            else
            {
                MiniFAT.ReadFAT();//read fat if exists (existing disk data)

                Root = ReadRootDirectory();
                Root.ReadDirectory();// Read existing root directory from disk

            }

            // Ensure FAT is written to disk at the end of initialization.
            MiniFAT.WriteFAT();
            disk.Flush();
        }
        private static Directory ReadRootDirectory()
        {
            if (Root == null)
            {
                Root = new Directory("C:", 0x10, 5, 0, null); // Initialize Root if it's null
            }

            try
            {
                if (Root.DIR_FirstCluster == 0 || Root.DirFiles.Count == 0)
                {
                    Root.DIR_FirstCluster = 5; //Set it to 5 when Root is empty and is being read from VirtualDisk
                    return Root; // Return the initialized root
                }

                byte[] rootClusterBytes = ReadCluster(Root.DIR_FirstCluster); //Access VirtualDisk.Root after its creation

                List<DirectoryEntry> entries = Converter.BytesToDirectoryEntries(rootClusterBytes);

                Root.DirFiles = entries;//populate Root with its files and folders.

                return Root;
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error reading root directory: {ex.Message}");
                return Root; // To not avoid null
            }
        }
    }
} 
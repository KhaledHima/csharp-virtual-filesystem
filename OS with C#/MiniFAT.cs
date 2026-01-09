using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_with_C_
{
    
    /// Manages the File Allocation Table (FAT) for the virtual disk.
    /// The FAT keeps track of which clusters are free, allocated, or reserved,
    /// and links clusters together to form files and directories.
  
    public static class MiniFAT
    {
        public static int[] FAT = new int[1024];

        // 1. Initialize FAT 
        public static void InitializeFAT()
        {
            for (int i = 0; i < 1024; i++)
            {
                if (i == 0 || i == 4)
                {
                    FAT[i] = -1;
                }
                else if (i > 0 && i <= 3)
                {
                    FAT[i] = i + 1;
                }
                else
                {
                    FAT[i] = 0;
                }
            }
            WriteFAT(); // Write after initialization
        }


        // 2. Print FAT 
        public static void PrintFAT()
        {
            Console.WriteLine("FAT has the following:");
            for (int i = 0; i < 1024; i++)
            {
                Console.WriteLine($"FAT[{i}] = {FAT[i]}");
            }
        }

        // 3. Get Available Cluster (start from cluster 5 as 0-4 are reserved)
        public static int GetAvailableCluster()
        {
            for (int i = 5; i < 1024; i++) // Start from 5
            {
                if (FAT[i] == 0)
                {
                    return i;
                }
            }
            return -1; // Disk is full
        }


        // 4. Get Available Clusters Count (excluding reserved clusters)
        public static int GetAvailableClustersCount()
        {
            int count = 0;
            for (int i = 5; i < 1024; i++) // Exclude 0-4 (reserved for Superblock and FAT)
            {
                if (FAT[i] == 0)
                {
                    count++;
                }
            }
            return count;
        }


        // 5. Get Free Size (in bytes, considering reserved clusters)
        public static int GetFreeSize()
        {

            return GetAvailableClustersCount() * 1024;
        }


        // 6. Get Cluster Status/Pointer (with validation) 
        public static int GetClusterPointer(int clusterIndex)
        {
            if (clusterIndex >= 0 && clusterIndex < 1024)
            {
                return FAT[clusterIndex];
            }
            else
            {
                return -1;
            }
        }


        // 7. Set Cluster Status/Pointer 
        public static void SetClusterPointer(int clusterIndex, int status)
        {
            if (clusterIndex >= 0 && clusterIndex < 1024)
            {
                FAT[clusterIndex] = status;
                WriteFAT(); // Write immediately after modification
            }
        }

        // 8. Write FAT to Disk (using Converter and VirtualDisk)
        public static void WriteFAT()
        {
            byte[] fatBytes = Converter.IntArrayToByteArray(FAT);
            List<byte[]> clusters = Converter.SplitBytes(fatBytes);
            for (int i = 0; i < clusters.Count; i++)
            {
                VirtualDisk.WriteCluster(clusters[i], i + 1);
            }
            VirtualDisk.disk.Flush();// <-- Flush after writing FAT
        }

        // 9. Read FAT from Disk (using Converter and VirtualDisk)
        public static void ReadFAT()
        {
            List<byte> ls = new List<byte>();
            for (int i = 1; i <= 4; i++)
            {
                byte[] b = VirtualDisk.ReadCluster(i);
                ls.AddRange(b);
            }
            FAT = Converter.ByteArrayToIntArray(ls.ToArray());
        }

        // 10. Create Superblock (all zeros)
        public static byte[] CreateSuperBlock()
        {
            return new byte[1024];
        }

        // 11. Initialize or Open File System (using VirtualDisk)
        public static void InitializeOrOpenFileSystem(string name)

        {
            VirtualDisk.CreateOrOpenDisk(name);

            if (VirtualDisk.IsNew())

            {

                byte[] superBlock = MiniFAT.CreateSuperBlock();

                VirtualDisk.WriteCluster(superBlock, 0);

                MiniFAT.InitializeFAT();

                MiniFAT.WriteFAT();

            }

            else

            {
                MiniFAT.ReadFAT();
            }
            VirtualDisk.disk.Flush();//<--Flush here as well
        }

        // 12. Close File System 
        public static void CloseFileSystem()

        {
            MiniFAT.WriteFAT();
            VirtualDisk.CloseDisk();

        }
    }
}

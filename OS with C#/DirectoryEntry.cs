using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_with_C_
{
    public class DirectoryEntry
    {
        
        /// The name of the directory entry (11 characters).
        public char[] DIR_Name = new char[11];

        
        /// The attribute of the directory entry (e.g., file or directory).
        public byte DIR_Attr;

        
        /// Reserved space (12 bytes) for future use.
        public byte[] DIR_empty = new byte[12];

       
        /// The first cluster number of the file or directory.
        public int DIR_FirstCluster;

        
        /// The size of the file in bytes. For directories, this is typically 0.
        public int DIR_FileSize;

        
        /// Default constructor initializes all fields to default values.
        public DirectoryEntry()
        {
            DIR_Attr = 0;
            DIR_FirstCluster = 0;
            DIR_FileSize = 0;
            Array.Clear(DIR_Name, 0, DIR_Name.Length); // Clear the name array
            Array.Clear(DIR_empty, 0, DIR_empty.Length); // Clear the reserved space
        }

       
        /// Constructor for creating a DirectoryEntry with a name, attribute, and first cluster.
        ///  name="name">The name of the entry.
        ///  name="dirAttr">The attribute of the entry (e.g., file or directory).
        ///  name="dirFirstCluster">The first cluster number of the entry.
        public DirectoryEntry(string name, byte dirAttr, int dirFirstCluster) : this() // Call the default constructor first
        {
            DIR_Attr = dirAttr;
            DIR_FirstCluster = dirFirstCluster;

            if (dirAttr == 0x10) // If it's a directory
            {
                AssignDIRName(name); // Assign the directory name
            }
            else // If it's a file
            {
                AssignFileName(name); // Assign the file name
            }
        }

        
        /// Constructor for creating a DirectoryEntry with a name, attribute, first cluster, and file size.
        ///  name="dirName">The name of the entry as a character array.
        /// name="dirAttr">The attribute of the entry (e.g., file or directory).
        ///  name="dirFirstCluster">The first cluster number of the entry.
        ///  name="dirFileSize">The size of the file in bytes.
        public DirectoryEntry(char[] dirName, byte dirAttr, int dirFirstCluster, int dirFileSize) : this()
        {
            Array.Copy(dirName, DIR_Name, Math.Min(11, dirName.Length)); // Copy the name, ensuring it doesn't exceed 11 characters
            DIR_Attr = dirAttr;
            DIR_FirstCluster = dirFirstCluster;
            DIR_FileSize = dirFileSize;
        }

        
        /// Cleans the name by removing invalid characters and trailing nulls.
        /// <returns>The cleaned name as a string.
        public string CleanTheName()
        {
            string name = new string(DIR_Name);
            string invalidChars = "*:/\\?\"<>|"; // List of invalid characters

            // Replace invalid characters with "0"
            foreach (char c in invalidChars)
            {
                if (name.Contains(c))
                {
                    name = name.Replace(c.ToString(), "0"); // Replace invalid characters with "0"
                }
            }

            return name.TrimEnd('\0'); // Remove trailing null characters
        }

       
        /// Assigns a file name to the directory entry, handling truncation and extension separation.
        ///  name="fullName">The full name of the file, including the extension.
        public void AssignFileName(string fullName)
        {
            string name = "";
            string extension = "";

            if (fullName.Contains('.')) // If the name contains an extension
            {
                string[] parts = fullName.Split('.');
                name = parts[0]; // Name part
                extension = parts[1]; // Extension part

                // Truncate name and extension if necessary
                if (name.Length > 7) { name = name.Substring(0, 7); }
                if (extension.Length > 3) { extension = extension.Substring(0, 3); }

                fullName = name + "." + extension; // Recombine truncated parts
                DIR_Attr = 0x00; // File attribute
            }
            else // If it's a directory
            {
                DIR_Attr = 0x10; // Directory attribute
            }

            // Convert the name to a character array and copy it to DIR_Name
            char[] nameChars = fullName.PadRight(11, '\0').ToCharArray();
            Array.Copy(nameChars, DIR_Name, 11);
        }

      
        /// Assigns a directory name to the directory entry.
        ///  name="name">The name of the directory.
        public void AssignDIRName(string name)
        {
            // Convert the name to a character array and copy it to DIR_Name
            char[] nameChars = name.PadRight(11, '\0').ToCharArray();
            Array.Copy(nameChars, DIR_Name, 11);
        }

       
        /// Converts the DirectoryEntry object to a byte array.
        /// <returns>A byte array representing the DirectoryEntry.
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[32];
            Encoding.ASCII.GetBytes(DIR_Name).CopyTo(bytes, 0); // Copy the name
            bytes[11] = DIR_Attr; // Copy the attribute
            BitConverter.GetBytes(DIR_FirstCluster).CopyTo(bytes, 24); // Copy the first cluster
            BitConverter.GetBytes(DIR_FileSize).CopyTo(bytes, 28); // Copy the file size
            return bytes;
        }

       
        /// Creates a DirectoryEntry object from a byte array.
        /// name="bytes">The byte array to convert.
        /// <returns>A DirectoryEntry object representing the byte array.
        public static DirectoryEntry FromBytes(byte[] bytes)
        {
            char[] name = Encoding.ASCII.GetChars(bytes, 0, 11); // Extract the name
            byte attr = bytes[11]; // Extract the attribute
            int firstCluster = BitConverter.ToInt32(bytes, 24); // Extract the first cluster
            int fileSize = BitConverter.ToInt32(bytes, 28); // Extract the file size

            return new DirectoryEntry(new string(name).Trim('\0'), attr, firstCluster) { DIR_FileSize = fileSize };
        }
    }
}

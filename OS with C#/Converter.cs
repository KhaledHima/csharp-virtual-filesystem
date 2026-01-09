using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_with_C_
{
    public class Converter
    {
        
        /// Converts an integer to a byte array (4 bytes).
        ///  name="n">The integer to convert.
        /// <returns>A byte array representing the integer.
        public static byte[] IntToByte(int n)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((n >> 24) & 0xFF); // Extract the most significant byte
            result[1] = (byte)((n >> 16) & 0xFF); // Extract the second most significant byte
            result[2] = (byte)((n >> 8) & 0xFF);  // Extract the third most significant byte
            result[3] = (byte)(n & 0xFF);         // Extract the least significant byte
            return result;
        }

     
        /// Converts a byte array to an integer.
        ///  name="bytes">The byte array to convert.
        /// <returns>The integer representation of the byte array.
        public static int ByteToInt(byte[] bytes)
        {
            int n = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                n = (n << 8) | (bytes[i] & 0xFF); // Shift and combine bytes to form the integer
            }
            return n;
        }

        
        /// Converts an array of integers to a byte array.
        /// name="ints">The array of integers to convert.
        /// <returns>A byte array representing the integers.
        public static byte[] IntArrayToByteArray(int[] ints)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < ints.Length; i++)
            {
                bytes.AddRange(IntToByte(ints[i])); // Convert each integer to bytes and add to the list
            }
            return bytes.ToArray();
        }

      
        /// Converts a byte array to an array of integers.
        ///  name="bytes">The byte array to convert.
        /// <returns>An array of integers representing the byte array.
        public static int[] ByteArrayToIntArray(byte[] bytes)
        {
            int[] ints = new int[bytes.Length / 4]; // Each integer is 4 bytes
            for (int i = 0; i < ints.Length; i++)
            {
                byte[] b = new byte[4];
                Array.Copy(bytes, i * 4, b, 0, 4); // Extract 4 bytes for each integer
                ints[i] = ByteToInt(b); // Convert the 4 bytes to an integer
            }
            return ints;
        }

     
        /// Splits a byte array into smaller chunks of a specified size.
        ///  name="bytes">The byte array to split.
        ///  name="chunkSize">The size of each chunk (default is 1024 bytes).
        /// <returns>A list of byte arrays, each representing a chunk of the original array.
        public static List<byte[]> SplitBytes(byte[] bytes, int chunkSize = 1024)
        {   
            // Initialize a list to hold the chunks of byte arrays
            List<byte[]> ls = new List<byte[]>();
            if (bytes.Length > 0)
            {
                int numberOfArrays = bytes.Length / chunkSize; // Calculate the number of full chunks
                int rem = bytes.Length % chunkSize; // Calculate the remaining bytes

                for (int i = 0; i < numberOfArrays; i++)
                {
                    byte[] b = new byte[chunkSize];
                    Array.Copy(bytes, i * chunkSize, b, 0, chunkSize); // Copy a full chunk
                    ls.Add(b);
                }

                if (rem > 0)
                {
                    byte[] b1 = new byte[chunkSize];
                    Array.Copy(bytes, numberOfArrays * chunkSize, b1, 0, rem); // Copy the remaining bytes
                    ls.Add(b1);
                }
            }
            else
            {
                ls.Add(new byte[chunkSize]); // Add an empty chunk if the input array is empty
            }
            return ls;
        }

        
        /// Converts a DirectoryEntry object to a byte array.
        ///  name="d">The DirectoryEntry object to convert.
        /// <returns>A byte array representing the DirectoryEntry.
        public static byte[] DirectoryEntryToBytes(DirectoryEntry d)
        {
            byte[] bytes = new byte[32];
            Encoding.ASCII.GetBytes(d.DIR_Name).CopyTo(bytes, 0); // Copy the directory name
            bytes[11] = d.DIR_Attr; // Copy the attribute byte
            Array.Copy(Converter.IntToByte(d.DIR_FirstCluster), 0, bytes, 24, 4); // Copy the first cluster
            Array.Copy(Converter.IntToByte(d.DIR_FileSize), 0, bytes, 28, 4); // Copy the file size
            return bytes;
        }

        
        /// Converts a byte array to a DirectoryEntry object.
        ///  name="bytes">The byte array to convert.
        /// <returns>A DirectoryEntry object representing the byte array.
        public static DirectoryEntry BytesToDirectoryEntry(byte[] bytes)
        {
            char[] name = Encoding.ASCII.GetChars(bytes, 0, 11); // Extract the name
            byte attr = bytes[11]; // Extract the attribute
            int firstCluster = BitConverter.ToInt32(bytes, 24); // Extract the first cluster
            int fileSize = BitConverter.ToInt32(bytes, 28); // Extract the file size

            return new DirectoryEntry(name, attr, firstCluster, fileSize);
        }

        
        /// Converts a byte array to a list of DirectoryEntry objects.
        /// name="bytes">The byte array to convert.</param>
        /// <returns>A list of DirectoryEntry objects.
        public static List<DirectoryEntry> BytesToDirectoryEntries(byte[] bytes)
        {
            List<DirectoryEntry> entries = new List<DirectoryEntry>();
            for (int i = 0; i < bytes.Length; i += 32) // Each entry is 32 bytes
            {
                byte[] b = new byte[32];
                Array.Copy(bytes, i, b, 0, 32); // Copy 32 bytes for each entry

                if (b[0] == 0) // Check for empty entry (first byte is 0)
                    break;

                entries.Add(BytesToDirectoryEntry(b)); // Convert the bytes to a DirectoryEntry
            }
            return entries;
        }

        
        /// Converts a list of DirectoryEntry objects to a byte array.
        ///  name="entries">The list of DirectoryEntry objects to convert.
        /// <returns>A byte array representing the list of DirectoryEntry objects.
        public static byte[] DirectoryEntriesToBytes(List<DirectoryEntry> entries)
        {
            List<byte> bytes = new List<byte>();
            foreach (DirectoryEntry entry in entries)
            {
                bytes.AddRange(DirectoryEntryToBytes(entry)); // Convert each entry to bytes
            }
            return bytes.ToArray();
        }


        /// Converts a string to a byte array using ASCII encoding.
        /// name="str">The string to convert.
        /// <returns>A byte array representing the string.
        public static byte[] StringToByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        
        /// Converts a byte array to a string using ASCII encoding.
        /// name="byteArray">The byte array to convert.
        /// <returns>A string representing the byte array.
        public static string ByteArrayToString(byte[] byteArray)
        {
            return Encoding.ASCII.GetString(byteArray);
        }
    }
}

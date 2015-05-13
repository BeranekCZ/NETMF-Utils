using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF
{
    public static class ByteUtils
    {
        /// <summary>
        /// Append arr2 to arr1
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <returns></returns>
        public static byte[] AppendArrays(byte[] arr1, byte[] arr2)
        {
            if (arr1 == null) return arr2;
            byte[] ret = new byte[arr1.Length + arr2.Length];
            arr1.CopyTo(ret, 0);
            arr2.CopyTo(ret, arr1.Length);
            return ret;
        }

        public static byte[] AppendToArray(byte[] arr1, byte b)
        {
            if (arr1 == null) return new byte[] { b };

            byte[] ret = new byte[arr1.Length + 1];
            arr1.CopyTo(ret, 0);
            ret[arr1.Length] = b;
            return ret;
        }

        public static byte[] CopyArrays(byte[] from, int start, int stop)
        {
            if (start >= from.Length) return null;
            byte[] arr = new byte[stop - start + 1];
            for (int i = 0; i <= stop - start; i++)
            {
                arr[i] = from[start + i];
            }
            return arr;
        }

        public static void PrintDataToConsole(byte[] data)
        {
            if (data == null)
            {
                Debug.Print("null");
                return;
            }
            char[] chars = System.Text.Encoding.UTF8.GetChars(data);
            Debug.Print(new String(chars));
        }

        public static int GetIndexOf(byte[] array, byte b,int startIndex=0)
        {
            if (array == null) return -1;
            for (int i = startIndex; i < array.Length; i++)
            {
                if (array[i]==b) return i;
            }
            return -1;
        }

        public static bool Find(byte[] array, string str)
        {
            if (array == null) return false;
            string text = new string(System.Text.Encoding.UTF8.GetChars(CopyNotNull(array)));
            //text.Trim((char)0);
            return text.IndexOf(str) != -1;
        }

        

        public static byte[] CopyNotNull(byte[] array)
        {
            int count=0;
            foreach (byte b in array)
            {
                if (b != 0) count++;
            }

            byte[] ret = new byte[count];
            int index=0;
            foreach (byte b in array)
            {
                if (b != 0)
                {
                    ret[index] = b;
                    index++;
                }
            }
            return ret;
        }
    }
}

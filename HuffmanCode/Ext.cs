using System;
using System.Collections;
using System.Text;

namespace HuffmanCode
{
    public static class Ext
    {
        public static String ToBinaryString(this BitArray array)
        {
            StringBuilder sb = new StringBuilder();
            foreach(bool item in array)
            {
                sb.Append(item ? "1" : "0");
            }
            return sb.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Helpers
{
    public static class ByteConverterHelper
    {
        #region 1. 字节码 (byte[]) 与 Hex 十六进制字符串互转

        /// <summary>
        /// 将字节数组转换为 Hex 十六进制字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="separator">字节之间的分隔符（如 " " 或 "-"，留空则紧凑显示）</param>
        /// <param name="upperCase">是否大写字母（默认 true）</param>
        public static string ToHex(byte[]? bytes, string? separator = " ", bool upperCase = true)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;

            string format = upperCase ? "X2" : "x2";
            if (string.IsNullOrEmpty(separator))
            {
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString(format));
                }
                return sb.ToString();
            }
            else
            {
                var hexArr = new string[bytes.Length];
                for (int i = 0; i < bytes.Length; i++)
                {
                    hexArr[i] = bytes[i].ToString(format);
                }
                return string.Join(separator, hexArr);
            }
        }

        /// <summary>
        /// 将单个字节转换为 Hex 字符串
        /// </summary>
        public static string ToHex(byte b, bool upperCase = true) => b.ToString(upperCase ? "X2" : "x2");

        /// <summary>
        /// 将 Hex 十六进制字符串转换为字节数组（自动忽略空格、换行符和连字符）
        /// </summary>
        public static byte[] FromHex(string? hexStr)
        {
            if (string.IsNullOrWhiteSpace(hexStr)) return Array.Empty<byte>();

            // 清洗字符串，去除常见的空格、横杠、0x 前缀等
            string cleanHex = hexStr
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("0x", "")
                .Replace("0X", "");

            if (cleanHex.Length % 2 != 0)
            {
                cleanHex = "0" + cleanHex; // 补齐奇数位
            }

            var bytes = new byte[cleanHex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(cleanHex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        #endregion

        #region 2. 文本字符串 (String) 与 字节码 (byte[]) 互转

        /// <summary>
        /// 字符串转字节码（默认 UTF-8）
        /// </summary>
        public static byte[] GetBytes(string? text, Encoding? encoding = null)
        {
            if (string.IsNullOrEmpty(text)) return Array.Empty<byte>();
            return (encoding ?? Encoding.UTF8).GetBytes(text);
        }

        /// <summary>
        /// 字节码转字符串（默认 UTF-8）
        /// </summary>
        public static string GetString(byte[]? bytes, Encoding? encoding = null)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;
            return (encoding ?? Encoding.UTF8).GetString(bytes);
        }

        #endregion

        #region 3. 数字类型转字节码 (支持 大端 / 小端 字节序)

        // --- short (Int16) ---
        public static byte[] ToBytes(short value, bool isLittleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            CheckEndian(ref bytes, isLittleEndian);
            return bytes;
        }
        public static byte[] ToBytes(ushort value, bool isLittleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            CheckEndian(ref bytes, isLittleEndian);
            return bytes;
        }

        // --- int (Int32) ---
        public static byte[] ToBytes(int value, bool isLittleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            CheckEndian(ref bytes, isLittleEndian);
            return bytes;
        }
        public static byte[] ToBytes(uint value, bool isLittleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            CheckEndian(ref bytes, isLittleEndian);
            return bytes;
        }

        // --- long (Int64) ---
        public static byte[] ToBytes(long value, bool isLittleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            CheckEndian(ref bytes, isLittleEndian);
            return bytes;
        }
        public static byte[] ToBytes(ulong value, bool isLittleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            CheckEndian(ref bytes, isLittleEndian);
            return bytes;
        }

        // --- float & double ---
        public static byte[] ToBytes(float value, bool isLittleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            CheckEndian(ref bytes, isLittleEndian);
            return bytes;
        }
        public static byte[] ToBytes(double value, bool isLittleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            CheckEndian(ref bytes, isLittleEndian);
            return bytes;
        }

        #endregion

        #region 4. 字节码转数字类型 (支持 大端 / 小端 字节序)

        public static short ToInt16(byte[] bytes, int startIndex = 0, bool isLittleEndian = true)
        {
            EnsureLength(bytes, startIndex, 2);
            var slice = GetSlice(bytes, startIndex, 2, isLittleEndian);
            return BitConverter.ToInt16(slice, 0);
        }

        public static ushort ToUInt16(byte[] bytes, int startIndex = 0, bool isLittleEndian = true)
        {
            EnsureLength(bytes, startIndex, 2);
            var slice = GetSlice(bytes, startIndex, 2, isLittleEndian);
            return BitConverter.ToUInt16(slice, 0);
        }

        public static int ToInt32(byte[] bytes, int startIndex = 0, bool isLittleEndian = true)
        {
            EnsureLength(bytes, startIndex, 4);
            var slice = GetSlice(bytes, startIndex, 4, isLittleEndian);
            return BitConverter.ToInt32(slice, 0);
        }

        public static uint ToUInt32(byte[] bytes, int startIndex = 0, bool isLittleEndian = true)
        {
            EnsureLength(bytes, startIndex, 4);
            var slice = GetSlice(bytes, startIndex, 4, isLittleEndian);
            return BitConverter.ToUInt32(slice, 0);
        }

        public static float ToSingle(byte[] bytes, int startIndex = 0, bool isLittleEndian = true)
        {
            EnsureLength(bytes, startIndex, 4);
            var slice = GetSlice(bytes, startIndex, 4, isLittleEndian);
            return BitConverter.ToSingle(slice, 0);
        }

        public static double ToDouble(byte[] bytes, int startIndex = 0, bool isLittleEndian = true)
        {
            EnsureLength(bytes, startIndex, 8);
            var slice = GetSlice(bytes, startIndex, 8, isLittleEndian);
            return BitConverter.ToDouble(slice, 0);
        }

        #endregion

        #region 内部辅助方法

        private static void CheckEndian(ref byte[] bytes, bool isLittleEndian)
        {
            if (BitConverter.IsLittleEndian != isLittleEndian)
            {
                Array.Reverse(bytes);
            }
        }

        private static byte[] GetSlice(byte[] bytes, int startIndex, int length, bool isLittleEndian)
        {
            var slice = new byte[length];
            Array.Copy(bytes, startIndex, slice, 0, length);
            if (BitConverter.IsLittleEndian != isLittleEndian)
            {
                Array.Reverse(slice);
            }
            return slice;
        }

        private static void EnsureLength(byte[] bytes, int startIndex, int requiredLength)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (startIndex < 0 || startIndex + requiredLength > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "字节数组长度不足或索引越界。");
        }

        #endregion

        #region Simple 测试
        public static void RunTest()
        {
            // 1. 字节码转 Hex 字符串
            byte[] rawBytes = new byte[] { 0x01, 0x0F, 0x2A, 0xFF };
            string hexStr1 = ByteConverterHelper.ToHex(rawBytes, separator: " "); // 输出: "01 0F 2A FF"
            string hexStr2 = ByteConverterHelper.ToHex(rawBytes, separator: "-"); // 输出: "01-0F-2A-FF"
            string hexStr3 = ByteConverterHelper.ToHex(rawBytes, separator: "");  // 输出: "010F2AFF"

            // 2. Hex 字符串转字节码
            byte[] restoredBytes = ByteConverterHelper.FromHex("01 0F 2A FF");

            // 3. 数字转字节码（常用于 PLC 寄存器写入，如发送一个 16 位整数 1000）
            short plcValue = 1000;
            byte[] intBytes = ByteConverterHelper.ToBytes(plcValue, isLittleEndian: true); // 小端模式
            string hexCommand = ByteConverterHelper.ToHex(intBytes, separator: "");

            // 4. 字节码转回数字（解析设备返回的数据报文）
            byte[] responseBytes = new byte[] { 0xE8, 0x03, 0x00, 0x00 }; // 代表 1000 的 Int32
            int parsedInt = ByteConverterHelper.ToInt32(responseBytes, startIndex: 0, isLittleEndian: true);

            // 5. 字符串与字节码互转
            string msg = "AyuUI 工业框架";
            byte[] textBytes = ByteConverterHelper.GetBytes(msg);
            string recoveredText = ByteConverterHelper.GetString(textBytes);
        }
        #endregion
    }
}

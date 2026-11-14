using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Helpers
{
    public static class ZipHelper
    {
        /// <summary>
        /// 压缩整个文件夹为 Zip 文件
        /// </summary>
        /// <param name="sourceDirectoryPath">源文件夹路径</param>
        /// <param name="destinationZipFilePath">目标 zip 文件保存路径</param>
        /// <param name="includeBaseDirectory">是否在压缩包中包含根文件夹本身</param>
        public static void CompressDirectory(string sourceDirectoryPath, string destinationZipFilePath, bool includeBaseDirectory = false)
        {
            if (!Directory.Exists(sourceDirectoryPath))
                throw new DirectoryNotFoundException($"源文件夹不存在: {sourceDirectoryPath}");

            var parentDir = Path.GetDirectoryName(destinationZipFilePath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            if (File.Exists(destinationZipFilePath))
            {
                File.Delete(destinationZipFilePath);
            }

            ZipFile.CreateFromDirectory(sourceDirectoryPath, destinationZipFilePath, CompressionLevel.Optimal, includeBaseDirectory);
        }

        /// <summary>
        /// 解压 Zip 文件到指定目标目录
        /// </summary>
        public static void ExtractZip(string sourceZipFilePath, string destinationDirectoryPath)
        {
            if (!File.Exists(sourceZipFilePath))
                throw new FileNotFoundException($"压缩文件不存在: {sourceZipFilePath}");

            if (!Directory.Exists(destinationDirectoryPath))
            {
                Directory.CreateDirectory(destinationDirectoryPath);
            }

            ZipFile.ExtractToDirectory(sourceZipFilePath, destinationDirectoryPath, overwriteFiles: true);
        }
    }
}

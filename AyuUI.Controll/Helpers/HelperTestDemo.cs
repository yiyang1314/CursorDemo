using AyuUI.Controll.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Helpers
{
    public class HelperTestDemo
    {
        public void RunDemo()
        {
            // 1. JSON 测试
            var user = new { Name = "AyuUI", Version = "1.0.0", IsActive = true };
            string jsonStr = JsonHelper.Serialize(user, pretty: true);
            JsonHelper.SaveToFile(user, "user_config.json");
            var loadedUser = JsonHelper.LoadFromFile<NavigationItem>("user_config.json");

            // 2. XML 测试
            string xmlStr = XmlHelper.Serialize(user);

            // 3. 压缩与解压测试
            // ZipHelper.CompressDirectory(@"D:\Logs", @"D:\Backup\Logs.zip");
            // ZipHelper.ExtractZip(@"D:\Backup\Logs.zip", @"D:\ExtractLogs");

            // 4. 加密与解密测试
            string originalText = "PLC_Password_123456";
            string encrypted = EncryptHelper.AesEncrypt(originalText);
            string decrypted = EncryptHelper.AesDecrypt(encrypted);

            // 5. MD5 校验
            string md5Hash = EncryptHelper.ToMd5("123456");
        }
    }
}

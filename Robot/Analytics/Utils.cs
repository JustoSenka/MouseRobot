using Robot.Abstractions;
using RobotRuntime;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Robot.Analytics.Utils
{
    public static class Utils
    {
        [RequestStaticDependency(typeof(IRegistryEditor))]
#pragma warning disable 649
        private static IRegistryEditor reg;
#pragma warning restore 649

        public static string PutDatum(string k32, string k16)
        {
            try
            {
                var byteArray = Encoding.UTF8.GetBytes(k32);
                var memoryStream = new MemoryStream();
                var stream = new CryptoStream(memoryStream, new DESCryptoServiceProvider().CreateEncryptor(Encoding.UTF8.GetBytes(k16), reg.Get("CachedKey") as byte[]), CryptoStreamMode.Write);
                stream.Write(byteArray, 0, byteArray.Length);
                stream.FlushFinalBlock();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, ex.Message);
            }
            return "";
        }

        public static string GetDatum(string k32, string k16)
        {
            try
            {
                var inputByteArray = Convert.FromBase64String(k32);
                var memoryStream = new MemoryStream();
                var stream = new CryptoStream(memoryStream, new DESCryptoServiceProvider().CreateDecryptor(Encoding.UTF8.GetBytes(k16), reg.Get("CachedKey") as byte[]), CryptoStreamMode.Write);
                stream.Write(inputByteArray, 0, inputByteArray.Length);
                stream.FlushFinalBlock();
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, ex.Message);
            }
            return "";
        }
    }
}

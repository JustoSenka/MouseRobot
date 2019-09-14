using Newtonsoft.Json;
using Robot.Analytics.Abstractions;
using RobotRuntime;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Robot.Analytics
{
    [RegisterTypeToContainer(typeof(INetwork))]
    public class Network : INetwork
    {
        public const string k_IpRegex = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$";
        public const string k_CountryIDRegex = @"^\w{2}$";

        public string GetLocalIP()
        {
            try
            {
                string localIP;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
                return localIP.Trim();
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Cannot get local IP: " + e.Message);
                return "";
            }
        }

        public string GetExternalIP()
        {
            var ip1 = GetExternalIP_1();
            if (Regex.IsMatch(ip1, k_IpRegex))
                return ip1;

            var ip2 = GetExternalIP_2();
            if (Regex.IsMatch(ip2, k_IpRegex))
                return ip2;

            var ip3 = GetExternalIP_3();
            if (Regex.IsMatch(ip3, k_IpRegex))
                return ip3;

            return "";
        }

        public string GetExternalIP_1()
        {
            return new WebClient().DownloadString("http://icanhazip.com").Trim();
        }

        public string GetExternalIP_2()
        {
            return new WebClient().DownloadString("https://api.ipify.org").Trim();
        }

        public string GetExternalIP_3()
        {
            return new WebClient().DownloadString("http://ifconfig.me").Trim();
        }

        public string GetCountryID(string ip = "")
        {
            if (ip.IsEmpty())
                ip = GetExternalIP();

            var id1 = GetCountryID_1(ip);
            if (Regex.IsMatch(id1, k_CountryIDRegex))
                return id1;

            var id2 = GetCountryID_2(ip);
            if (Regex.IsMatch(id2, k_CountryIDRegex))
                return id2;

            var id3 = GetCountryID_3(ip);
            if (Regex.IsMatch(id3, k_CountryIDRegex))
                return id3;

            return "";
        }

        public string GetCountryID_1(string ip)
        {
            try
            {
                var info = new WebClient().DownloadString("http://ipinfo.io/" + ip);
                dynamic o = JsonConvert.DeserializeObject(info);
                return o.country.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string GetCountryID_2(string ip)
        {
            try
            {
                var info = new WebClient().DownloadString("https://api.ipgeolocation.io/ipgeo?apiKey=4a9c1386454d44d6abfeaba5842b4831&ip=" + ip).Trim();
                dynamic o = JsonConvert.DeserializeObject(info);
                return o.country_code2.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string GetCountryID_3(string ip)
        {
            try
            {
                var info = new WebClient().DownloadString("http://ip-api.com/xml/" + ip).Trim();
                using (TextReader sr = new StringReader(info))
                {
                    using (System.Data.DataSet dataBase = new System.Data.DataSet())
                    {
                        dataBase.ReadXml(sr);
                        return dataBase.Tables[0].Rows[0][2].ToString(); // CountryCode
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}

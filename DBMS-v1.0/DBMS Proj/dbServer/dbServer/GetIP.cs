using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;

namespace dbServer
{
    public class GetIP
    {

        public GetIP()
        {
        }

        private static string CheckCurrentIP(NetworkInterfaceType networkType)
        {
            string userIPV4 = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == networkType && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            userIPV4 = ip.Address.ToString();
                        }
                    }
                }
            }
            return userIPV4;
        }

        private static NetworkInterfaceType getUserNetworkType()
        {
            if (isOnline())
            {
                try
                {
                    var checkConnectionProcess = new Process
                    {
                        StartInfo =
                          {
                              FileName = "netsh.exe",
                              Arguments = "wlan show interfaces",
                              UseShellExecute = false,
                              RedirectStandardOutput = true,
                              CreateNoWindow = true
                          }
                    };
                    checkConnectionProcess.Start();

                    var output = checkConnectionProcess.StandardOutput.ReadToEnd();
                    var line = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                     .FirstOrDefault(l => l.Contains("SSID") && !l.Contains("BSSID"));
                    if (line == null)
                    {
                        return NetworkInterfaceType.Unknown;
                    }
                    var ssid = line.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart();
                    return NetworkInterfaceType.Wireless80211;
                }
                catch(Win32Exception e)
                {
                    Console.WriteLine("Not connected to Wifi: " + e.StackTrace);
                    return NetworkInterfaceType.Ethernet;
                }
            }

            return NetworkInterfaceType.Unknown;
        }

        private static bool isOnline() {

            try
            {
                Ping myPing = new Ping();
                String host = "8.8.8.8";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else return false;
            }
            catch (Exception e) {
                Console.Write("Connection check error: " + e.StackTrace);
                return false;
            }
            

        }

        public static string returnCheckCurrentIP() {
            return (CheckCurrentIP(getUserNetworkType()));
        }

    }
}


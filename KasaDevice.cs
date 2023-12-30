using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace KasaAPI
{
    public class KasaDevice
    {
        readonly string deviceAddress = string.Empty;
        public bool EnergyMonitoring = false;

        public KasaDevice(string ipAddress)
        {
            deviceAddress = ipAddress;
            EnergyMonitoring = GetSystemInfo().System.GetSysInfo.Feature.Contains("ENE");
        }

        public static void Discovery(ConcurrentDictionary<string, KasaDevice> devices)
        {
            System.Diagnostics.Debug.WriteLine("Searching the network for devices, please wait...");

            UdpClient udpClient = new()
            {
                EnableBroadcast = true
            };
            udpClient.Client.ReceiveTimeout = 1000;
            byte[] sysInfoUdp = EncodeMessage(JsonSerializer.Serialize(new KasaCommands.GetSysInfoCommand.Root()));
            udpClient.Send(sysInfoUdp, sysInfoUdp.Length, "10.0.0.255", 9999);

            while (true)
            {
                try
                {
                    IPEndPoint RemoteIpEndPoint = new(IPAddress.Any, 0);
                    byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);

                    System.Diagnostics.Debug.WriteLine($"Received a response from {RemoteIpEndPoint.Address}");
                    var decodedResponse = JsonSerializer.Deserialize<KasaResponses.GetSysInfoResponse.Root>(DecodeMessage(receiveBytes));

                    if (decodedResponse == null)
                    {
                        return;
                    }

                    if (!devices.TryGetValue(decodedResponse.System.GetSysInfo.MAC, out _))
                    {
                        devices[decodedResponse.System.GetSysInfo.MAC] = new KasaDevice(RemoteIpEndPoint.Address.ToString());
                    }
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }

        private static byte[] EncodeMessage(string message)
        {
            byte[] encodedBytes = new byte[message.Length];
            byte key = 171;
            for (int i = 0; i < message.Length; i++)
            {
                char character = message[i];
                byte encodedChar = (byte)(character ^ key);
                encodedBytes[i] = encodedChar;
                key = encodedChar;
            }
            return encodedBytes;
        }

        private static string DecodeMessage(byte[] message)
        {
            char[] decodedChars = new char[message.Length];
            byte key = 171;
            for (int i = 0; i < message.Length; i++)
            {
                byte currentByte = message[i];
                char decodedChar = (char)(currentByte ^ key);
                decodedChars[i] = decodedChar;
                key = currentByte;
            }
            return new string(decodedChars);
        }

        private static byte[] AddLengthToMessage(byte[] message)
        {
            byte[] lengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(message.Length));
            return lengthBytes.Concat(message).ToArray();
        }

        private string SendMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine($"-> {message}");
            byte[] encodedMessage = AddLengthToMessage(EncodeMessage(message));

            try
            {
                TcpClient client = new(deviceAddress, 9999);
                NetworkStream networkStream = client.GetStream();

                networkStream.Write(encodedMessage, 0, encodedMessage.Length);

                byte[] data = new byte[1024];
                int bytesRead = networkStream.Read(data, 0, data.Length);

                int messageLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data.Take(4).ToArray(), 0));

                string decodedMessage = DecodeMessage(data.Skip(4).Take(messageLength).ToArray());
                client.Close();

                System.Diagnostics.Debug.WriteLine($"<- {decodedMessage}");

                return decodedMessage;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
                return string.Empty;
            }
        }

        public KasaResponses.GetSysInfoResponse.Root GetSystemInfo()
        {
            string response = SendMessage(JsonSerializer.Serialize(new KasaCommands.GetSysInfoCommand.Root()));
            return JsonSerializer.Deserialize<KasaResponses.GetSysInfoResponse.Root>(response);
        }

        public KasaResponses.GetRealTimeResponse.Root GetRealTimeStats()
        {
            string response = SendMessage(JsonSerializer.Serialize(new KasaCommands.GetRealTimeCommand.Root()));
            return JsonSerializer.Deserialize<KasaResponses.GetRealTimeResponse.Root>(response);
        }

        public bool OutletState
        {
            get => GetSystemInfo().System.GetSysInfo.RelayState == 1;
            set
            {
                var relayCommand = new KasaCommands.SetRelayStateCommand.Root();
                relayCommand.System.SetRelayState.State = value ? 1 : 0;
                System.Diagnostics.Debug.WriteLine(JsonSerializer.Serialize(relayCommand));
                SendMessage(JsonSerializer.Serialize(relayCommand));
            }
        }

        public void ToggleOutlet() => OutletState = !OutletState;

        public TimeSpan PoweredOnTime => TimeSpan.FromSeconds(GetSystemInfo().System.GetSysInfo.OnTime);
    }
}

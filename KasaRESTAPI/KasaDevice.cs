using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;

namespace KasaAPI
{
    public class KasaDevice
    {
        readonly string deviceAddress = string.Empty;
        public bool EnergyMonitoring = false;
        public bool CountdownTimer = false;

        public KasaDevice(string ipAddress)
        {
            deviceAddress = ipAddress;

            var sysInfo = GetSystemInfo().System.GetSysInfo;
            EnergyMonitoring = sysInfo.Feature.Contains("ENE");
            CountdownTimer = sysInfo.Feature.Contains("TIM");
        }

        private static byte[] GetBroadcastAddress(byte[] addressBytes, byte[] maskBytes)
        {
            byte[] broadcastAddressBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                broadcastAddressBytes[i] = (byte)(addressBytes[i] | ~maskBytes[i]);
            }

            return broadcastAddressBytes;
        }

        public static void Discovery(ConcurrentDictionary<string, KasaDevice> devices)
        {
            List<IPEndPoint> broadcastAddresses = new();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.OperationalStatus != OperationalStatus.Up || !adapter.Supports(NetworkInterfaceComponent.IPv4) || adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                var properties = adapter.GetIPProperties();

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    var broadcastAddress = new IPAddress(GetBroadcastAddress(address.Address.GetAddressBytes(), address.IPv4Mask.GetAddressBytes()));
                    broadcastAddresses.Add(new IPEndPoint(broadcastAddress, 9999));
                }
            }

            System.Diagnostics.Debug.WriteLine("Searching the network for devices, please wait...");

            foreach (var address in broadcastAddresses)
            {
                UdpClient udpClient = new()
                {
                    EnableBroadcast = true
                };
                udpClient.Client.ReceiveTimeout = 1000;
                byte[] sysInfoUdp = EncodeMessage(JsonSerializer.Serialize(new KasaCommands.GetSysInfoCommand.Root()));
                System.Diagnostics.Debug.WriteLine($"Broadcasting to {address}");
                udpClient.Send(sysInfoUdp, sysInfoUdp.Length, address);

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

                        if (!devices.TryGetValue(decodedResponse.System.GetSysInfo.MAC, out _) || devices[decodedResponse.System.GetSysInfo.MAC].deviceAddress != RemoteIpEndPoint.Address.ToString())
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

        public KasaResponses.GetAllCountdownRulesResponse.Root GetAllCountdownRules()
        {
            string response = SendMessage(JsonSerializer.Serialize(new KasaCommands.GetAllCountdownRulesCommand.Root()));
            return JsonSerializer.Deserialize<KasaResponses.GetAllCountdownRulesResponse.Root>(response);
        }

        public KasaResponses.AddCountdownRuleResponse.Root AddCountdownRule(bool enabled, int delaySeconds, bool action, string name)
        {
            var command = new KasaCommands.AddCountdownRuleCommand.Root();
            command.Countdown.AddRule.Enable = enabled ? 1 : 0;
            command.Countdown.AddRule.Delay = delaySeconds;
            command.Countdown.AddRule.Action = action ? 1 : 0;
            command.Countdown.AddRule.Name = name;
            string response = SendMessage(JsonSerializer.Serialize(command));
            return JsonSerializer.Deserialize<KasaResponses.AddCountdownRuleResponse.Root>(response);
        }

        public KasaResponses.DeleteAllCountdownRulesResponse.Root DeleteAllCountdownRules()
        {
            string response = SendMessage(JsonSerializer.Serialize(new KasaCommands.DeleteAllCountdownRulesCommand.Root()));
            return JsonSerializer.Deserialize<KasaResponses.DeleteAllCountdownRulesResponse.Root>(response);
        }

        public KasaResponses.DeleteCountdownRuleResponse.Root DeleteCountdownRule(string ruleId)
        {
            var command = new KasaCommands.DeleteCountdownRuleCommand.Root();
            command.Countdown.DeleteRule.Id = ruleId;
            string response = SendMessage(JsonSerializer.Serialize(command));
            return JsonSerializer.Deserialize<KasaResponses.DeleteCountdownRuleResponse.Root>(response);
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

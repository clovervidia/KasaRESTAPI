using System.Text.Json.Serialization;

namespace KasaAPI
{
    static class KasaCommands
    {
        public class GetSysInfoCommand
        {
            public class System
            {
                [JsonPropertyName("get_sysinfo")]
                public Dictionary<string, string> GetSysInfo { get; set; }

                public System()
                {
                    GetSysInfo = new Dictionary<string, string>();
                }
            }

            public class Root
            {
                [JsonPropertyName("system")]
                public System System { get; set; }

                public Root()
                {
                    System = new System();
                }
            }
        }

        public class GetRealTimeCommand
        {
            public class Emeter
            {
                [JsonPropertyName("get_realtime")]
                public Dictionary<string, string> GetRealTime { get; set; }

                public Emeter()
                {
                    GetRealTime = new Dictionary<string, string>();
                }
            }

            public class Root
            {
                [JsonPropertyName("emeter")]
                public Emeter Emeter { get; set; }

                public Root()
                {
                    Emeter = new Emeter();
                }
            }
        }

        public class SetRelayStateCommand
        {
            public class SetRelaystate
            {
                [JsonPropertyName("state")]
                public int State { get; set; } 
            }

            public class System
            {
                [JsonPropertyName("set_relay_state")]
                public SetRelaystate SetRelayState { get; set; } = new SetRelaystate();
            }

            public class Root
            {
                [JsonPropertyName("system")]
                public System System { get; set; }

                public Root()
                {
                    System = new System();
                }
            }
        }
    }
}

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
                public Dictionary<string, string> GetSysInfo { get; set; } = new();
            }

            public class Root
            {
                [JsonPropertyName("system")]
                public System System { get; set; } = new();
            }
        }

        public class GetRealTimeCommand
        {
            public class Emeter
            {
                [JsonPropertyName("get_realtime")]
                public Dictionary<string, string> GetRealTime { get; set; } = new();
            }

            public class Root
            {
                [JsonPropertyName("emeter")]
                public Emeter Emeter { get; set; } = new();
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
                public SetRelaystate SetRelayState { get; set; } = new();
            }

            public class Root
            {
                [JsonPropertyName("system")]
                public System System { get; set; } = new();
            }
        }

        public class GetAllCountdownRulesCommand
        {
            public class Countdown
            {
                [JsonPropertyName("get_rules")]
                public Dictionary<string, string> GetRules { get; set; } = new();
            }

            public class Root
            {
                [JsonPropertyName("count_down")]
                public Countdown Countdown { get; set; } = new();
            }
        }

        public class AddCountdownRuleCommand
        {
            public class AddRule
            {
                [JsonPropertyName("name")]
                public string Name { get; set; } = string.Empty;
                [JsonPropertyName("enable")]
                public int Enable { get; set; }
                [JsonPropertyName("delay")]
                public int Delay { get; set; }
                [JsonPropertyName("act")]
                public int Action { get; set; }
            }

            public class Countdown
            {
                [JsonPropertyName("add_rule")]
                public AddRule AddRule { get; set; } = new();
            }

            public class Root
            {
                [JsonPropertyName("count_down")]
                public Countdown Countdown { get; set; } = new();
            }
        }

        public class DeleteAllCountdownRulesCommand
        {
            public class Countdown
            {
                [JsonPropertyName("delete_all_rules")]
                public Dictionary<string, string> DeleteAllRules { get; set; } = new();
            }

            public class Root
            {
                [JsonPropertyName("count_down")]
                public Countdown Countdown { get; set; } = new();
            }
        }

        public class DeleteCountdownRuleCommand
        {
            public class DeleteRule
            {
                [JsonPropertyName("id")]
                public string Id { get; set; } = string.Empty;
            }

            public class Countdown
            {
                [JsonPropertyName("delete_rule")]
                public DeleteRule DeleteRule { get; set; } = new();
            }

            public class Root
            {
                [JsonPropertyName("count_down")]
                public Countdown Countdown { get; set; } = new();
            }
        }
    }
}

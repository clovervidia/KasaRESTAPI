using System.Text.Json.Serialization;

namespace KasaAPI
{
    public static class KasaResponses
    {
        public class GetRealTimeResponse
        {
            public class GetRealTime
            {
                [JsonPropertyName("current_ma")]
                public int CurrentMA { get; init; }
                [JsonPropertyName("voltage_mv")]
                public int VoltageMV { get; init; }
                [JsonPropertyName("power_mw")]
                public int PowerMW { get; init; }
                [JsonPropertyName("total_wh")]
                public int TotalWH { get; init; }
                [JsonPropertyName("err_code")]
                public int ErrCode { get; init; }
            }

            public class Emeter
            {
                [JsonPropertyName("get_realtime")]
                public GetRealTime GetRealTime { get; init; } = new GetRealTime();
            }

            public class Root
            {
                [JsonPropertyName("emeter")]
                public Emeter Emeter { get; init; } = new Emeter();
            }
        }

        public class GetSysInfoResponse
        {
            public class NextAction
            {
                [JsonPropertyName("type")]
                public int Type { get; init; }
            }

            public class GetSysInfo
            {
                [JsonPropertyName("sw_ver")]
                public string SWVer { get; init; } = string.Empty;
                [JsonPropertyName("hw_ver")]
                public string HWVer { get; init; } = string.Empty;
                [JsonPropertyName("model")]
                public string Model { get; init; } = string.Empty;
                [JsonPropertyName("deviceId")]
                public string DeviceID { get; init; } = string.Empty;
                [JsonPropertyName("oemId")]
                public string OEMID { get; init; } = string.Empty;
                [JsonPropertyName("hwId")]
                public string HWID { get; init; } = string.Empty;
                [JsonPropertyName("rssi")]
                public int RSSI { get; init; }
                [JsonPropertyName("latitude_i")]
                public int LatitiudeI { get; init; }
                [JsonPropertyName("longitude_i")]
                public int LongitudeI { get; init; }
                [JsonPropertyName("alias")]
                public string Alias { get; init; } = string.Empty;
                [JsonPropertyName("status")]
                public string Status { get; init; } = string.Empty;
                [JsonPropertyName("obd_src")]
                public string OBDSrc { get; init; } = string.Empty;
                [JsonPropertyName("mic_type")]
                public string MicType { get; init; } = string.Empty;
                [JsonPropertyName("feature")]
                public string Feature { get; init; } = string.Empty;
                [JsonPropertyName("mac")]
                public string MAC { get; init; } = string.Empty;
                [JsonPropertyName("updating")]
                public int Updating { get; init; }
                [JsonPropertyName("led_off")]
                public int LEDOff { get; init; }
                [JsonPropertyName("relay_state")]
                public int RelayState { get; init; }
                [JsonPropertyName("on_time")]
                public int OnTime { get; init; }
                [JsonPropertyName("icon_hash")]
                public string IconHash { get; init; } = string.Empty;
                [JsonPropertyName("dev_name")]
                public string DevName { get; init; } = string.Empty;
                [JsonPropertyName("active_mode")]
                public string ActiveMode { get; init; } = string.Empty;
                [JsonPropertyName("next_action")]
                public NextAction NextAction { get; init; } = new NextAction();
                [JsonPropertyName("ntc_state")]
                public int NTCState { get; init; }
                [JsonPropertyName("err_code")]
                public int ErrCode { get; init; }
            }

            public class System
            {
                [JsonPropertyName("get_sysinfo")]
                public GetSysInfo GetSysInfo { get; init; } = new GetSysInfo();
            }

            public class Root
            {
                [JsonPropertyName("system")]
                public System System { get; init; } = new System();
            }
        }
    }
}

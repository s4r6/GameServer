
using GameServer.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameServer.Infrastracture
{
    public static class PacketSerializer
    {
        public static string Serialize<T>(PacketModel<T> packet)
        {
            return JsonConvert.SerializeObject(packet);
        }

        public static PacketModel<T> Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<PacketModel<T>>(json);
        }

        public static PacketId ExtractPacketId(string json)
        {
            var jObject = JObject.Parse(json);
            return jObject.TryGetValue("PacketId", out var token) && Enum.TryParse(token.ToString(), out PacketId id)
                ? id
                : PacketId.None;
        }

        public static string ExtractPayloadJson(string json)
        {
            var jObject = JObject.Parse(json);
            return jObject["Payload"]?.ToString() ?? string.Empty;
        }
    }
}

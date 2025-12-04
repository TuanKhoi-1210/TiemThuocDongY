using System.Text.Json;
using System.Text.Json.Serialization;

namespace TiemThuocDongY.WinApp
{
    public class WebMessage
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}

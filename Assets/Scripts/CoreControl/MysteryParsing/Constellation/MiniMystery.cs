using Newtonsoft.Json;
using System.Collections.Generic;

public class MiniMystery
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("entry_points")]
    public List<string> EntryPoints { get; set; }

    [JsonProperty("key_nodes")]
    public List<string> KeyNodes { get; set; }

    [JsonProperty("revelation")]
    public string Revelation { get; set; }
}

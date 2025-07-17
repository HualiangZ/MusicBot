using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MusicBot.Config
{
    internal class JsonReader
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong guildId { get; set; }

        public async Task ReadJson()
        {
            StreamReader reader = new StreamReader("config.json");
            string json = await reader.ReadToEndAsync();
            JsonStruct data = JsonConvert.DeserializeObject<JsonStruct>(json);

            this.token = data.token;
            this.prefix = data.prefix;
            this.guildId = data.guildId;
        }


    }

    internal sealed class JsonStruct
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong guildId { get; set; }
    } 
}

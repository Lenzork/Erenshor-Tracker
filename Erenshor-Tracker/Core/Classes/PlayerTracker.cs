using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Erenshor_Tracker.Core.Classes
{
    internal class PlayerTracker
    {
        private static readonly HttpClient client = new HttpClient();

        private static string url;

        public PlayerTracker(string url) {
            PlayerTracker.url = url;
        }

        private static string GetModVersion()
        {
            var melonInfo = (MelonInfoAttribute)Attribute.GetCustomAttribute(typeof(Mod), typeof(MelonInfoAttribute));
            if (melonInfo != null)
            {
                return melonInfo.Version;
            }
            return "Unknown Version";
        }

        public async Task<string> AddCharacterAsync(Player ply)
        {
            var characterData = new
            {
                steamName = ply.SteamName,
                character = ply
            };

            var json = JsonConvert.SerializeObject(characterData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }

        public async Task<string> UpdateCharacterAsync(Player ply)
        {
            var url = "http://45.145.225.103:3000/updateCharacter";
            var characterData = new
            {
                steamName = ply.SteamName,
                character = ply
            };

            var json = JsonConvert.SerializeObject(characterData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }

        public async Task<List<Player>> GetPlayersAsync()
        {
            var url = "http://45.145.225.103:3000/getPlayers";
            var response = await client.GetStringAsync(url);

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            return JsonConvert.DeserializeObject<List<Player>>(response, settings);
        }

    }

    public class Player
    {
        public string Name { get; set; }
        public string CharacterClass { get; set; }
        public int Level { get; set; }
        public int CurrentXP { get; set; }
        public int NeededXP { get; set; }
        public bool IsAlive { get; set; }
        public DateTime LastUpdated { get; set; }
        public string SteamName { get; set; }
        public List<eqItem> Equipment { get; set; }
        public List<minQuest> CompletedQuests { get; set; }
        public List<InstalledMod> InstalledMods { get; set; }
    }

    public class eqItem
    {
        public eqItem(string itemName, int itemLevel) 
        {
            this.ItemName = itemName;
            this.ItemLevel = itemLevel;
        }

        public string ItemName;
        public int ItemLevel;
    }

    public class minQuest
    {
        public minQuest(string qName)
        {
            QuestName = qName;
        }

        public string QuestName;
    }
}

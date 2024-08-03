using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MelonLoader.Modules.MelonModule;

/*
 * Tracks installed compatible Lenzork Mods of the Player by using MelonLoader
 */

namespace Erenshor_Tracker.Core.Classes
{
    public class InstalledMod
    {
        public InstalledMod(string name, string version, string emoji)
        {
            this.Name = name;
            this.Version = version;
            this.Emoji = emoji;
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Emoji { get; set; }
    }

    internal class ModTracker
    {

        public static List<InstalledMod> installedMods = new List<InstalledMod>();
        public static List<string> supportedMods = new List<string>() 
        { 
            "Achievement Mod", 
            "All GM Mod", 
            "Randomizer" 
        };

        public static List<InstalledMod> TrackInstalledMods()
        {
            installedMods = new List<InstalledMod>();

            foreach (var mod in supportedMods)
            {
                var foundMod = MelonMod.FindMelon(mod, "Lenzork");
                if (foundMod != null)
                {
                    var info = foundMod.Info;
                    MelonLogger.Msg($"{info.Name}, {info.Version}");
                    string emoji;

                    switch (info.Name)
                    {
                        case "Achievement Mod":
                            emoji = "🛡️";
                            break;
                        case "All GM Mod":
                            emoji = "🛠️";
                            break;
                        case "Randomizer":
                            emoji = "🎲";
                            break;
                        default:
                            emoji = "🔗";
                            break;
                    }

                    installedMods.Add(new InstalledMod(info.Name, info.Version, emoji));
                }
                MelonLogger.Msg($"Checking {mod}...");
            }

            return installedMods;
        }
    }
}

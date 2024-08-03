using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Erenshor_Tracker.Core.Classes;
using MelonLoader;
using UnityEngine;
using Steamworks;
using HarmonyLib;

namespace Erenshor_Tracker
{
    public class Mod : MelonMod
    {
        private static PlayerTracker _playerTracker;
        private static Player ply;
        private static GameObject plyObj;
        private DateTime _lastUpdate;
        private string _playerName = "PlayerName"; // Setze den Spielernamen hier ein

        public override void OnApplicationStart()
        {
            _playerTracker = new PlayerTracker("http://45.145.225.103:3000/addCharacter");
            _lastUpdate = DateTime.UtcNow;
        }



        public override void OnLateInitializeMelon()
        {
            // Draw Version Text
            MelonEvents.OnGUI.Subscribe(Drawer.DrawVersionText, 100);
        }

        [HarmonyPatch(typeof(PlayerControl), "Start")]
        private static class Patch
        {
            private static void Postfix(PlayerControl __instance)
            {
                Melon<Mod>.Logger.Msg($"New Character created! {__instance.gameObject.GetComponent<Stats>().MyName}");

                if (!(__instance.gameObject.name == "Player"))
                    return;

                plyObj = GameObject.Find("Player");

                if (plyObj == null)
                {
                    //MelonLogger.Msg("Player GameObject not found.");
                    return;
                }
                else
                {
                    ply = new Player
                    {
                        Name = plyObj.GetComponent<Stats>().MyName,
                        CharacterClass = plyObj.GetComponent<Stats>().CharacterClass.ToString(),
                        Level = plyObj.GetComponent<Stats>().Level,
                        LastUpdated = DateTime.UtcNow,
                        Equipment = new List<eqItem>(),
                        CurrentXP = plyObj.GetComponent<Stats>().CurrentExperience,
                        NeededXP = plyObj.GetComponent<Stats>().ExperienceToLevelUp,
                        CompletedQuests = new List<minQuest>(),
                        SteamName = SteamFriends.GetPersonaName(),
                        InstalledMods = ModTracker.TrackInstalledMods()
                    };

                    plyObj.GetComponent<Inventory>().EquippedItems.ForEach(x =>
                    {
                        ply.Equipment.Add(new eqItem(x.ItemName, x.ItemLevel));
                    });

                    GameData.CompletedQuests.ForEach(x =>
                    {
                        ply.CompletedQuests.Add(new minQuest(x));
                    });

                    Task.Run(async () =>
                    {
                        await _playerTracker.AddCharacterAsync(ply);
                    });
                }
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName != "Stowaway")
                return;

            
        }


        public override void OnFixedUpdate()
        {
            var timeSinceLastUpdate = (DateTime.UtcNow - _lastUpdate).TotalSeconds;
            //MelonLogger.Msg($"Time since last update: {timeSinceLastUpdate} seconds");

            if(plyObj == null)
                plyObj = GameObject.Find("Player");

            // Überprüfe, ob seit der letzten Aktualisierung 5 Sekunden vergangen sind
            if (timeSinceLastUpdate >= 5)
            {
                if (ply != null )
                {
                    //MelonLogger.Msg("Updating player...");

                    // Aktualisiere die Spielerinformationen
                    UpdatePlayer();

                    // Sende die Aktualisierungen an die API
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _playerTracker.UpdateCharacterAsync(ply);
                        }
                        catch (Exception ex)
                        {
                            //MelonLogger.Msg($"An error occurred while updating player data: {ex.Message}");
                        }
                    });

                    // Aktualisiere die letzte Aktualisierungszeit
                    _lastUpdate = DateTime.UtcNow;
                }
                else
                {
                    //MelonLogger.Msg("Player object is null or player ID is not set.");
                }
            }
        }



        private void UpdatePlayer()
        {
            if (plyObj == null)
            {
                //MelonLogger.Msg("Player object is null.");
                return;
            }

            var character = plyObj.GetComponent<Character>();
            var stats = plyObj.GetComponent<Stats>();
            var inv = plyObj.GetComponent<Inventory>();
            if (stats == null)
            {
                //MelonLogger.Msg("Stats component is null.");
                return;
            }

            ply.Name = stats.MyName;
            ply.Level = stats.Level;
            ply.IsAlive = character.Alive;
            ply.LastUpdated = DateTime.UtcNow;
            ply.Equipment = new List<eqItem>();
            ply.CurrentXP = stats.CurrentExperience;
            ply.NeededXP = stats.ExperienceToLevelUp;
            ply.CompletedQuests = new List<minQuest>();
            ply.InstalledMods = ModTracker.TrackInstalledMods();

            inv.EquippedItems.ForEach(x =>
            {
                ply.Equipment.Add(new eqItem(x.ItemName, x.ItemLevel));
            });

            GameData.CompletedQuests.ForEach(x =>
            {
                ply.CompletedQuests.Add(new minQuest(x));
            });

            ply.CharacterClass = stats.CharacterClass.ToString();

            //MelonLogger.Msg($"Updated Tracker! Name: {ply.Name}, Level: {ply.Level}, IsAlive: {ply.IsAlive} LastUpdated: {ply.LastUpdated}");

            ply.Equipment.ForEach(x =>
            {

                //MelonLogger.Msg($"{x.ItemName}");

            });
        }
    }
}

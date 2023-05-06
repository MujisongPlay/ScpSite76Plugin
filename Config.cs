using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;
using Exiled.API.Enums;
using System.ComponentModel;
using PlayerRoles;
using UnityEngine;
using Exiled.API.Features.Roles;
using Exiled.API.Extensions;
using System.IO;
using Exiled.API.Features;
using Exiled.Loader;
using Respawning;

namespace Site76Plugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public string HighLevelPropertyFolderName { get; set; } = Path.Combine(Paths.Configs, "Site-76 Properties");
        public string HighLevelPropertyFileName { get; set; } = "global.yml";
        public bool UseCustomRespawnMethod { get; set; } = true;
        public float AirlockCloseTime { get; set; } = 6.5f;
        public float AirlockReworkingCoolTime { get; set; } = 1f;
        public float HeartBeatScannerCoolTime { get; set; } = 1f;
        [Description("It'll not detect cardiac arrested players, SCP-939, SCP-049-2.")]
        public bool HeartBeatScannerAdvanced { get; set; } = true;
        public Dictionary<RoleTypeId, string> HeartBeatScannerColorHighPriority { get; set; } = new Dictionary<RoleTypeId, string>
        {
            { RoleTypeId.ClassD, "orange" },
            { RoleTypeId.Scientist, "yellow" },
            { RoleTypeId.FacilityGuard, "grey" }
        };
        public Dictionary<Faction, string> HeartBeatScannerColorLowPriority { get; set; } = new Dictionary<Faction, string>
        {
            { Faction.FoundationStaff, "blue" },
            { Faction.SCP, "red" },
            { Faction.FoundationEnemy, "red" }
        };
        public Dictionary<string, string> CountTrackerColor { get; set; } = new Dictionary<string, string>
        {
            { "D", "orange" },
            { "G", "grey" },
            { "S", "yellow" },
            { "M", "blue" },
            { "O", "red" }
        };

        public bool HeartBeatTrackerErrorWhen2176 { get; set; } = true;

        [Description("This Message will be converted through 'Player List' convertor. Also there're additionally {year/month/day/hour/minute/second/ms} for real time.")]
        public Components.ConsoleSystem.MessageFormat[] ConsoleMessageFormats { get; set; } = new Components.ConsoleSystem.MessageFormat[]
        {
            new Components.ConsoleSystem.MessageFormat("SITE-76 ONLINE.\n{year}-{month}-{day}-{hour}.{minute}.{second}.{ms}\nPERSONNEL {subtract,{player_count},1}/{max_players}\nREPORTED DAMAGE {scp_kills}\nNEXT REINFORCE {next_respawn}\nREINFORCE TIME {respawn_time_m}.{respawn_time_s}", 10f, new string[] { "green", "red", "blue" })
        };
        public float ScpMaxHealthPowValuePerPerson { get; set; } = 0.35f;

        public string CustomSpawnQueue { get; set; } = "403441344413443413443143441443143413443143431434314343144341434314343144341434314343144341434314343144341434314343144341434314343144341434314343144341";

        public void LoadBinding()
        {
            if (!Directory.Exists(HighLevelPropertyFolderName))
            {
                Directory.CreateDirectory(HighLevelPropertyFolderName);
            }
            string filePath = Path.Combine(HighLevelPropertyFolderName, HighLevelPropertyFileName);
            Log.Info($"{filePath}");
            if (!File.Exists(filePath))
            {
                properties = new Properties();
                File.WriteAllText(filePath, Loader.Serializer.Serialize(properties));
            }
            else
            {
                properties = Loader.Deserializer.Deserialize<Properties>(File.ReadAllText(filePath));
                File.WriteAllText(filePath, Loader.Serializer.Serialize(properties));
            }
        }
        public Properties properties;
    }

    public class Properties
    {
        public Dictionary<char, int[]> BinaryValuesForConsole { get; set; } = new Dictionary<char, int[]>
        {
            { 'A', new int[] { 0, 1, 2, 3, 6, 7, 8, 9 } },
            { 'B', new int[] { 0, 1, 3, 4, 5, 7, 9, 14, 15 } },
            { 'C', new int[] { 0, 1, 4, 5, 6, 8 } },
            { 'D', new int[] { 0, 1, 4, 5, 7, 9, 14, 15 } },
            { 'E', new int[] { 0, 1, 2, 3, 4, 5, 6, 8 } },
            { 'F', new int[] { 0, 1, 2, 3, 6, 8 } },
            { 'G', new int[] { 0, 1, 3, 4, 5, 6, 8, 9 } },
            { 'H', new int[] { 2, 3, 6, 7, 8, 9 } },
            { 'I', new int[] { 0, 1, 4, 5, 14, 15 } },
            { 'J', new int[] { 4, 5, 7, 8, 9 } },
            { 'K', new int[] { 2, 6, 8, 11, 13 } },
            { 'L', new int[] { 4, 5, 6, 8 } },
            { 'M', new int[] { 6, 7, 8, 9, 10, 11 } },
            { 'N', new int[] { 6, 7, 8, 9, 10, 13 } },
            { 'O', new int[] { 0, 1, 4, 5, 6, 7, 8, 9 } },
            { 'P', new int[] { 0, 1, 2, 3, 6, 7, 8 } },
            { 'Q', new int[] { 0, 1, 4, 5, 6, 7, 8, 9, 13 } },
            { 'R', new int[] { 0, 1, 2, 3, 6, 7, 8, 13 } },
            { 'S', new int[] { 0, 1, 2, 3, 4, 5, 6, 9 } },
            { 'T', new int[] { 0, 1, 14, 15 } },
            { 'U', new int[] { 4, 5, 6, 7, 8, 9 } },
            { 'V', new int[] { 6, 8, 11, 12 } },
            { 'W', new int[] { 6, 7, 8, 9, 12, 13 } },
            { 'X', new int[] { 10, 11, 12, 13 } },
            { 'Y', new int[] { 2, 3, 6, 7, 15 } },
            { 'Z', new int[] { 0, 1, 4, 5, 11, 12 } },
            { '0', new int[] { 0, 1, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 } },
            { '1', new int[] { 7, 9, 11 } },
            { '2', new int[] { 0, 1, 2, 3, 4, 5, 7, 8 } },
            { '3', new int[] { 0, 1, 3, 4, 5, 7, 9 } },
            { '4', new int[] { 2, 3, 6, 7, 9 } },
            { '5', new int[] { 0, 1, 2, 3, 4, 5, 6, 9, 10, 13 } },
            { '6', new int[] { 0, 2, 3, 4, 5, 6, 8, 9 } },
            { '7', new int[] { 0, 1, 7, 9 } },
            { '8', new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 } },
            { '9', new int[] { 0, 1, 2, 3, 5, 6, 7, 9 } },
            { ' ', new int[] { } },
            { ',', new int[] { 12 } },
            { '\'', new int[] { 11 } },
            { '-', new int[] { 2, 3 } },
            { '+', new int[] { 2, 3, 14, 15 } },
            { '?', new int[] { 0, 2, 14, 16 } },
            { '[', new int[] { 0, 4, 6, 8 } },
            { ']', new int[] { 1, 5, 7, 9 } },
            { '/', new int[] { 11, 12 } },
            { '\\', new int[] { 10, 13 } },
            { '_', new int[] { 4, 5 } },
            { '<', new int[] { 11, 13 } },
            { '>', new int[] { 10, 12 } },
            { '$', new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 14, 15 } },
            { '@', new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 14 } },
            { '&', new int[] { 0, 1, 4, 5, 9, 10, 11, 12, 13 } },
            { '.', new int[] { 16 } },
            { '!', new int[] { 6, 16 } }
        };
        public Dictionary<string, RoleTypeId[]> CountTrackerTargets { get; set; } = new Dictionary<string, RoleTypeId[]>
        {
            { "D", new RoleTypeId[] { RoleTypeId.ClassD } },
            { "G", new RoleTypeId[] { RoleTypeId.FacilityGuard } },
            { "S", new RoleTypeId[] { RoleTypeId.Scientist } },
            { "M", new RoleTypeId[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfPrivate, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist } },
            { "O", new RoleTypeId[] { RoleTypeId.Scp049, RoleTypeId.Scp096, RoleTypeId.Scp106, RoleTypeId.Scp173, RoleTypeId.Scp939, RoleTypeId.ChaosConscript, RoleTypeId.ChaosMarauder, RoleTypeId.ChaosRepressor, RoleTypeId.ChaosRifleman} }
        };
        public Dictionary<string, ElevatorSerialize> elevatorSettings { get; set; } = new Dictionary<string, ElevatorSerialize>
        {
            { "Main Elevator", new ElevatorSerialize(new string[] {"LiftDoorOpen", "LiftDoorClose"}, 3f, 1.2f, new int[] { }, 2f )},
            { "Storage Elevator", new ElevatorSerialize(new string[] {"StorageLiftDoorAnimation", "StorageDoorClose"}, 3f, 1.2f, new int[] { }, 2f) }
        };
        public Dictionary<string, string> respawnEffectNames { get; set; } = new Dictionary<string, string>
        {
            { "ChaosCar", "ChaosEntering" },
            { "Helicopter", "Landing" }
        };
        public Dictionary<string, SpawnableTeamType> respawnTeamNames { get; set; } = new Dictionary<string, SpawnableTeamType>
        {
            { "ChaosCar", SpawnableTeamType.ChaosInsurgency },
            { "Helicopter", SpawnableTeamType.NineTailedFox }
        };
        public int[][] CounterActivatingLines { get; set; } = new int[][]
        {
            new int[] { 0, 2, 3, 4, 5, 6 },
            new int[] { 5, 6 },
            new int[] { 0, 1, 2, 3, 5 },
            new int[] { 0, 1, 2, 5, 6 },
            new int[] { 1, 4, 5, 6 },
            new int[] { 0, 1, 2, 4, 6 },
            new int[] { 0, 1, 2, 3, 4, 6 },
            new int[] { 0, 5, 6 },
            new int[] { 0, 1, 2, 3, 4, 5, 6 },
            new int[] { 0, 1, 2, 4, 5, 6 }
        };
    }
}

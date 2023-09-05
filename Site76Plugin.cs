using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using Mirror;
using SCPSLAudioApi.AudioCore;
using SCPSLAudioApi;
using MapEditorReborn.API.Features.Objects;
using PlayerRoles;
using MapEditorReborn.API;
using MapEditorReborn.API.Extensions;
using MapEditorReborn.API.Enums;
using MapEditorReborn.API.Features.Serializable;
using Interactables.Interobjects.DoorUtils;
using Interactables.Interobjects;
using InventorySystem.Items.Pickups;
using Exiled.API.Enums;

namespace Site76Plugin
{
    public class Site76Plugin : Plugin<Config>
    {
        public static Site76Plugin Instance;

        private EventHandler handler;

        public bool spawned = false;

        public GameObject schematicObject;

        public List<GameObject> PocketEscapePoints = new List<GameObject> { };

        public string prevQueue;

        public LeadingTeam LeadingTeam = LeadingTeam.Draw;

        //public Cullable Cullable;

        public override void OnEnabled()
        {
            Instance = this;
            handler = new EventHandler();

            RegisterEvents();
        }

        public override void OnDisabled()
        {
            if (prevQueue != null)
            GameCore.ConfigFile.ServerConfig.SetString("team_respawn_queue", prevQueue);
            Instance = null;

            UnregisterEvents();

            handler = null;
        }

        public void RegisterEvents()
        {
            Config.LoadBinding();
            MapEditorReborn.Events.Handlers.Schematic.SchematicSpawned += handler.OnSchematicSpawned;
        }

        public void ReGisterEvents()
        {
            Exiled.Events.Handlers.Player.SearchingPickup += handler.OnInteracted;
            Exiled.Events.Handlers.Player.Joined += handler.OnPlayerJoined;
            Exiled.Events.Handlers.Player.Spawned += handler.OnSpawned;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += handler.OnEscapePocket;
            Exiled.Events.Handlers.Player.Died += handler.OnDied;
            //Exiled.Events.Handlers.Map.ExplodingGrenade += handler.Scp2176exploded;
            Exiled.Events.Handlers.Server.EndingRound += handler.OnRoundEnd;
            Exiled.Events.Handlers.Server.RoundEnded += handler.OnRoundEnd;
            Exiled.Events.Handlers.Player.Left += handler.OnLeft;
            //Exiled.Events.Handlers.Player.VoiceChatting += handler.OnVoice;
        }

        public void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.SearchingPickup -= handler.OnInteracted;
            Exiled.Events.Handlers.Player.Joined -= handler.OnPlayerJoined;
            Exiled.Events.Handlers.Player.Spawned -= handler.OnSpawned;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= handler.OnEscapePocket;
            //Exiled.Events.Handlers.Map.ExplodingGrenade -= handler.Scp2176exploded;
            MapEditorReborn.Events.Handlers.Schematic.SchematicSpawned -= handler.OnSchematicSpawned;
            Exiled.Events.Handlers.Player.Died -= handler.OnDied;
            Exiled.Events.Handlers.Player.Left -= handler.OnLeft;
            Exiled.Events.Handlers.Server.EndingRound -= handler.OnRoundEnd;
            Exiled.Events.Handlers.Server.RoundEnded -= handler.OnRoundEnd;
            //Exiled.Events.Handlers.Player.VoiceChatting -= handler.OnVoice;
        }
    }

    public class EventHandler
    {

        public void OnSchematicSpawned(MapEditorReborn.Events.EventArgs.SchematicSpawnedEventArgs ev)
        {
            if (ev.Name.Equals("Site76", StringComparison.InvariantCultureIgnoreCase))
            {
                ServerConsole.AddLog("Site-76 detected. Activating Plugin...");
                //GameObject.FindObjectsOfType<MapGeneration.Distributors.Scp079Generator>().ForEach(x => UnityEngine.Object.Destroy(x));
                Site76Plugin.Instance.ReGisterEvents();
                Site76Plugin.Instance.schematicObject = ev.Schematic.gameObject;
                ChildRegister(ev.Schematic.gameObject);
                ev.Schematic.gameObject.AddComponent<Components.MinimapElement>().IsController = true;
                Site76Plugin.Instance.prevQueue = GameCore.ConfigFile.ServerConfig.GetString("team_respawn_queue");
                GameCore.ConfigFile.ServerConfig.SetString("team_respawn_queue", Site76Plugin.Instance.Config.CustomSpawnQueue);
            }
        }

        public void OnPlayerJoined(Exiled.Events.EventArgs.Player.JoinedEventArgs ev)
        {
            Exiled.API.Features.Toys.Primitive primitive = Exiled.API.Features.Toys.Primitive.Create(PrimitiveType.Cube, new Vector3(100f, 100f, 100f), Vector3.zero, Vector3.one * 0.05f);
            primitive.Base.gameObject.AddComponent<Components.MinimapElement>().Setup(ev.Player.ReferenceHub, primitive);
            primitive.Color = Color.red;
        }

        public void OnVoice(VoiceChattingEventArgs ev)
        {
            if (!ev.IsAllowed) return;
            if (new Bounds(Site76Plugin.Instance.schematicObject.transform.position + new Vector3(7.219f, 7.859f, -1.266f), new Vector3(6.038452f, 2.973262f, 4.97298f)).Contains(ev.Player.Position))
            {
                ev.VoiceMessage.Channel.SetFlag(VoiceChat.VoiceChatChannel.Intercom, true);
            }
            else
            {
                ev.VoiceMessage.Channel.SetFlag(VoiceChat.VoiceChatChannel.Intercom, false);
            }
        }

        public void Scp2176exploded(Exiled.Events.EventArgs.Map.ExplodingGrenadeEventArgs ev)
        {
            if (ev.Projectile.Type == ItemType.SCP2176)
            {
                Components.MinimapElement.Errored = true;
                Components.MinimapElement.ErrorTimer = 7.5f;
            }
        }

        public void OnSpawned(SpawnedEventArgs ev)
        {
            if (API.CurrentLoadedMap == null)
            {
                return;
            }
            if (ev.Player.RoleManager.CurrentRole.Team == Team.SCPs && ev.Player.RoleManager.CurrentRole.RoleTypeId != RoleTypeId.Scp0492)
            {
                ev.Player.MaxHealth = ev.Player.MaxHealth * Mathf.Pow(ReferenceHub.AllHubs.Count - 1, Site76Plugin.Instance.Config.ScpMaxHealthPowValuePerPerson);
                ev.Player.Heal(ev.Player.MaxHealth, false);
            }
            List<PlayerSpawnPointSerializable> list = API.CurrentLoadedMap.PlayerSpawnPoints.FindAll(x => keyValuePairs.TryGetValue(ev.Player.RoleManager.CurrentRole.RoleTypeId, out SpawnableTeam team) ? x.SpawnableTeam == team : false);
            if (list.Count != 0)
            {
                PlayerSpawnPointSerializable serializable = list.RandomItem();
                ev.Player.Teleport(API.GetRelativePosition(serializable.Position, API.GetRandomRoom(serializable.RoomType)));
            }
        }

        public void OnDied(DiedEventArgs ev)
        {
            switch (ev.TargetOldRole)
            {
                case RoleTypeId.Scp173:
                case RoleTypeId.Scp049:
                case RoleTypeId.Scp096:
                case RoleTypeId.Scp106:
                case RoleTypeId.Scp939:
                case RoleTypeId.Scp0492:
                    if (RoundSummary.singleton.CountTeam(Team.SCPs) > 0 || !(ev.DamageHandler.Base is PlayerStatsSystem.AttackerDamageHandler))
                    {
                        return;
                    }
                    Site76Plugin.Instance.LeadingTeam = ev.Attacker.LeadingTeam;
                    break;
                default:
                    break;
            }
        }

        public static Dictionary<RoleTypeId, SpawnableTeam> keyValuePairs = new Dictionary<RoleTypeId, SpawnableTeam>
        {
            { RoleTypeId.ChaosConscript, SpawnableTeam.Chaos },
            { RoleTypeId.ChaosMarauder, SpawnableTeam.Chaos },
            { RoleTypeId.ChaosRepressor, SpawnableTeam.Chaos },
            { RoleTypeId.ChaosRifleman, SpawnableTeam.Chaos },
            { RoleTypeId.ClassD, SpawnableTeam.ClassD },
            { RoleTypeId.FacilityGuard, SpawnableTeam.FacilityGuard },
            { RoleTypeId.NtfCaptain, SpawnableTeam.MTF },
            { RoleTypeId.NtfPrivate, SpawnableTeam.MTF },
            { RoleTypeId.NtfSergeant, SpawnableTeam.MTF },
            { RoleTypeId.NtfSpecialist, SpawnableTeam.MTF },
            { RoleTypeId.Scientist, SpawnableTeam.Scientist },
            { RoleTypeId.Scp049, SpawnableTeam.Scp049 },
            { RoleTypeId.Scp0492, SpawnableTeam.Scp0492 },
            { RoleTypeId.Scp096, SpawnableTeam.Scp096 },
            { RoleTypeId.Scp106, SpawnableTeam.Scp106 },
            { RoleTypeId.Scp173, SpawnableTeam.Scp173 },
            { RoleTypeId.Scp939, SpawnableTeam.Scp939 }
        };

        void ChildRegister(GameObject @object)
        {
            Components.MinimapElement.floorTransform.Clear();
            Components.MinimapElement.minimapTransform.Clear();
            Components.MinimapElement.schematicPosition = Site76Plugin.Instance.schematicObject.transform.position;
            Components.MinimapElement.minimapElements.Clear();
            Site76Plugin.Instance.PocketEscapePoints.Clear();
            foreach (Transform @transform in @object.GetComponentsInChildren<Transform>())
            {
                GameObject game = @transform.gameObject;
                if (transform.parent != null && (transform.parent.gameObject.name == "Letters" || transform.parent.gameObject.name == "Sign"))
                {
                    if (game.TryGetComponent(out PrimitiveObject primitive))
                    {
                        Color color = primitive.Primitive.Color;
                        color *= Site76Plugin.Instance.Config.ConsoleLetterReflectionAmount;
                        color.a = 0.99f;
                        primitive.Primitive.Color = color;
                    }
                }
                else if (transform.parent != null && transform.parent.parent != null && transform.parent.parent.gameObject.name == "Minimap")
                {
                    if (game.TryGetComponent(out PrimitiveObject primitive))
                    {
                        Color color = primitive.Primitive.Color;
                        color *= Site76Plugin.Instance.Config.MinimapElementReflectionAmount;
                        color.a = 0.99f;
                        primitive.Primitive.Color = color;
                    }
                }
                else if (transform.parent != null && transform.parent.gameObject.name.Contains("Light"))
                {
                    if (game.TryGetComponent(out PrimitiveObject primitive) && primitive.Primitive.Type == PrimitiveType.Cube)
                    {
                        Color color = primitive.Primitive.Color;
                        color *= Site76Plugin.Instance.Config.NormalLightReflectionAmount;
                        color.a = 0.99f;
                        primitive.Primitive.Color = color;
                    }
                }
                else if (transform.gameObject.name == "Transparent")
                {
                    if (game.TryGetComponent(out PrimitiveObject primitive))
                    {
                        Color color = primitive.Primitive.Color;
                        color *= Site76Plugin.Instance.Config.HighBeamReflectionAmount;
                        color.a = 0.99f;
                        primitive.Primitive.Color = color;
                    }
                }
                if (@transform.GetComponentInParent<Animator>() == null && @transform.TryGetComponent(out NetworkIdentity identity) && @transform.GetComponentInParent<ElevatorSystem>() == null && @transform.GetComponentInParent<Components.CountTracker>() == null && @transform.GetComponentInParent<LockerObject>() == null && transform.GetComponentInParent<Rigidbody>() == null)
                {
                    identity.NetworkBehaviours.ForEach(x => x.syncInterval = float.MaxValue);
                }
                switch (game.name)
                {
                    case "AirlockSystem":
                        game.AddComponent<AirlockSystem>();
                        break;
                    case "EleSystem":
                        if (game.transform.childCount == 0) break;
                        for (int j = 0; j < game.transform.childCount; j++)
                        {
                            if (Site76Plugin.Instance.Config.properties.elevatorSettings.TryGetValue(game.transform.GetChild(j).gameObject.name, out ElevatorSerialize serialize))
                            {
                                game.transform.GetChild(j).gameObject.AddComponent<ElevatorSystem>().ElevatorSerialize = serialize;
                            }
                        }
                        break;
                    case "Map System":
                        for (int i = 0; i < game.transform.childCount; i++)
                        {
                            if (int.TryParse(game.transform.GetChild(i).gameObject.name, out _))
                            {
                                Components.MinimapElement.floorTransform.Add(game.transform.GetChild(i).position.y);
                            }
                            else if (game.transform.GetChild(i).gameObject.name == "Minimap")
                            {
                                Transform transform1 = game.transform.GetChild(i);
                                for (int k = 0; k < transform1.transform.childCount; k++)
                                {
                                    if (int.TryParse(transform1.GetChild(k).gameObject.name, out _))
                                    {
                                        Components.MinimapElement.minimapTransform.Add(transform1.GetChild(k));
                                    }
                                    else
                                    {
                                        k--;
                                    }
                                }
                            }
                            else if (game.transform.GetChild(i).gameObject.name == "Sign")
                            {
                                Transform transform1 = game.transform.GetChild(i).GetChild(0);
                                for (int j = 0; j < transform1.childCount; j++)
                                {
                                    Transform transform2 = transform1.GetChild(j);
                                    if (Site76Plugin.Instance.Config.properties.CountTrackerTargets.TryGetValue(transform2.gameObject.name, out RoleTypeId[] ids))
                                    {
                                        transform2.gameObject.AddComponent<Components.CountTracker>().Setup(ids);
                                    }
                                }
                            }
                        }
                        break;
                    case "Console":
                        game.AddComponent<Components.ConsoleSystem>();
                        break;
                    case "RespawnSystem":
                        game.AddComponent<Components.RespawnAnimator>();
                        break;
                    case "PocketEscape":
                        Site76Plugin.Instance.PocketEscapePoints.Add(game);
                        break;
                    case "DoorInstallPoses":
                        MEC.Timing.CallDelayed(5f, () => { InstallDoors(game.transform); });
                        break;
                }
            }
            ServerConsole.AddLog("Plugin build completed! Players may now enter server.");
        }

        void InstallDoors(Transform transform)
        {
            foreach (Transform transform1 in transform.GetComponentsInChildren<Transform>())
            {
                if (transform == transform1) continue;
                int index;
                switch (transform1.gameObject.name.First())
                {
                    case 'L':
                        index = 3;
                        break;
                    case 'H':
                        index = 2;
                        break;
                    case 'E':
                        index = 1;
                        break;
                    default:
                        continue;
                }
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(NetworkClient.prefabs[NetworkClient.prefabs.Keys.ElementAt(index)]);
                gameObject.transform.position = transform1.position; gameObject.transform.rotation = transform1.rotation;
                if (gameObject.TryGetComponent(out DoorVariant door))
                {
                    door.RequiredPermissions.RequiredPermissions = (Interactables.Interobjects.DoorUtils.KeycardPermissions)ushort.Parse(transform1.gameObject.name.Remove(0, 2));
                }
                NetworkServer.Spawn(gameObject);
            }
        }

        public void OnInteracted(SearchingPickupEventArgs ev)
        {
            switch (ev.Pickup.GameObject.name)
            {
                case "StairTrigger":
                    if (ev.Pickup.Transform.parent.parent.gameObject.TryGetComponent(out Animator animator))
                    {
                        animator.Play("FoldStairAnimation");
                        ev.Pickup.Destroy();
                        break;
                    }
                    ev.IsAllowed = false;
                    break;
                case "ChemicalCabinetTrigger":
                    ev.IsAllowed = false;
                    if (ev.Pickup.Transform.parent.gameObject.TryGetComponent(out animator))
                    {
                        if (animator.GetBool("Opened"))
                        {
                            animator.Play("ChemicalsCabinetClose");
                            animator.SetBool("Opened", false);
                            break;
                        }
                        animator.Play("ChemicalsCabinet");
                        animator.SetBool("Opened", true);
                        break;
                    }
                    break;
                case "CallButton":
                    ev.IsAllowed = false;
                    if (ev.Pickup.Transform.parent.parent.gameObject.TryGetComponent(out ElevatorSystem elevator) && int.TryParse(ev.Pickup.Transform.parent.gameObject.name, out int i))
                    {
                        elevator.CallElevator(i);
                    }
                    else if (ev.Pickup.Transform.parent.parent.parent.gameObject.TryGetComponent(out elevator))
                    {
                        elevator.CallInternal(ev.Pickup.Transform.gameObject);
                    }
                    break;
            }
        }

        public void OnRoundEnd(Exiled.Events.EventArgs.Server.EndingRoundEventArgs ev)
        {
            if (Round.ElapsedTime.TotalSeconds <= 300f)
            {
                return;
            }
            if (Site76Plugin.Instance.LeadingTeam != LeadingTeam.Draw)
            {
                ev.LeadingTeam = Site76Plugin.Instance.LeadingTeam;
                ev.IsRoundEnded = true;
                Site76Plugin.Instance.LeadingTeam = LeadingTeam.Draw;
                return;
            }
            bool flag = ev.ClassList.mtf_and_guards + ev.ClassList.scientists != 0;
            bool flag1 = ev.ClassList.chaos_insurgents + ev.ClassList.class_ds != 0;
            if (flag^flag1)
            {
                if (flag && ev.ClassList.scientists >= RoundSummary.singleton.classlistStart.scientists / 3f)
                {
                    ev.LeadingTeam = LeadingTeam.FacilityForces;
                    ev.IsRoundEnded = true;
                }
                else if (flag1 && ev.ClassList.class_ds >= RoundSummary.singleton.classlistStart.class_ds / 3f)
                {
                    ev.LeadingTeam = LeadingTeam.ChaosInsurgency;
                    ev.IsRoundEnded = true;
                }
                else
                {
                    ev.IsRoundEnded = false;
                }
            }
            else if (!(flag|flag1))
            {
                ev.LeadingTeam = ev.ClassList.zombies + ev.ClassList.scps_except_zombies > 0 ? LeadingTeam.Anomalies : LeadingTeam.Draw;
                ev.IsRoundEnded = true;
            }
        }

        public void OnRoundEnd(Exiled.Events.EventArgs.Server.RoundEndedEventArgs ev)
        {
            GameCore.ConfigFile.ServerConfig.SetString("team_respawn_queue", Site76Plugin.Instance.prevQueue);
        }

        public void OnEscapePocket(EscapingPocketDimensionEventArgs ev)
        {
            if (Site76Plugin.Instance.PocketEscapePoints.Count != 0)
            {
                ev.TeleportPosition = Site76Plugin.Instance.PocketEscapePoints.RandomItem().transform.position;
            }
        }

        public void OnLeft(LeftEventArgs ev)
        {
            Components.MinimapElement element = Components.MinimapElement.minimapElements.Find(x => x.owner == ev.Player.ReferenceHub);
            if (element != null)
            {
                Components.MinimapElement.minimapElements.Remove(element);
                GameObject.Destroy(element.gameObject, 0.5f);
            }
        }
    }

    //public class Cullable : MonoBehaviour
    //{
    //    void Start()
    //    {
    //        bounds.center += this.transform.position;
    //    }

    //    void Update()
    //    {
    //        if (Player.List.Any(x => x.Role.IsAlive && bounds.Contains(x.Position)))
    //        {
    //            active = false;
    //        }
    //        else
    //        {
    //            active = true;
    //        }
    //    }

    //    public bool active = true;

    //    Bounds bounds = new Bounds(new Vector3(-8.943f, 8.638f, 0.376f), new Vector3(29.60682f, 4.927691f, 15.53502f));
    //}
}

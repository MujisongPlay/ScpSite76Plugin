using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Exiled.API.Enums;
using Respawning;
using Exiled.Events.EventArgs;
using Exiled.Events.Handlers;
using PluginAPI.Core.Interfaces;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;

namespace Site76Plugin.Components
{
    public class RespawnAnimator : MonoBehaviour
    {
        void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (Site76Plugin.Instance.Config.properties.respawnEffectNames.TryGetValue(transform.GetChild(i).gameObject.name, out string value) && transform.GetChild(i).TryGetComponent(out Animator animator) && Site76Plugin.Instance.Config.properties.respawnTeamNames.TryGetValue(transform.GetChild(i).gameObject.name, out SpawnableTeamType value1))
                {
                    keyValuePairs.Add(value1, new KeyValuePair<Animator, string>(animator, value));
                }
            }
        }

        void Update()
        {
            if (!GameCore.RoundStart.RoundStarted)
            {
                return;
            }
            if (Site76Plugin.Instance.Config.UseCustomRespawnMethod)
            {
                Timer -= Time.deltaTime;
                if (Timer <= 0)
                {
                    Timer = CoolTime;
                    bool Chaos = RoundSummary.singleton.CountTeam(PlayerRoles.Team.FoundationForces) < RoundSummary.singleton.CountTeam(PlayerRoles.Team.ChaosInsurgency);
                    RespawnTokensManager.GrantTokens(Chaos ? SpawnableTeamType.ChaosInsurgency : SpawnableTeamType.NineTailedFox, 100);
                }
            }
            if (RespawnManager.Singleton.TimeTillRespawn <= 0.5f && RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown && !Ignore)
            {
                Ignore = true;
                if (keyValuePairs.TryGetValue(RespawnTokensManager.DominatingTeam, out KeyValuePair<Animator, string> value))
                {
                    value.Key.Play(value.Value);
                }
            }
            if (RespawnManager.Singleton.TimeTillRespawn > 20f)
            {
                Ignore = false;
            }
        }

        bool Ignore = false;

        float Timer = 0;

        float CoolTime = 0.5f;

        public void PlayAnimation(SpawnableTeamType type)
        {
            if (keyValuePairs.TryGetValue(type, out KeyValuePair<Animator, string> value))
            {
                value.Key.Play(value.Value);
            }
        }

        Dictionary<SpawnableTeamType, KeyValuePair<Animator, string>> keyValuePairs = new Dictionary<SpawnableTeamType, KeyValuePair<Animator, string>> { };
    }
}

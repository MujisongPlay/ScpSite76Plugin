using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features.Toys;
using UnityEngine;
using PlayerRoles;
using MapEditorReborn.API.Features.Objects;

namespace Site76Plugin.Components
{
    public class MinimapElement : MonoBehaviour
    {
        void Start()
        {
            Exiled.Events.Handlers.Player.Left += Remove;
        }

        public void Setup(ReferenceHub hub, Primitive primitive)
        {
            Primitive = primitive;
            Primitive.MovementSmoothing = 0;
            owner = hub;
            minimapElements.Add(this);
        }

        void Remove(Exiled.Events.EventArgs.Player.LeftEventArgs ev)
        {
            if (ev.Player.ReferenceHub != owner) return;
            Exiled.Events.Handlers.Player.Left -= Remove;
            minimapElements.Remove(this);
            UnityEngine.Object.Destroy(this.gameObject);
        }

        void Update()
        {
            if (IsController)
            {
                Timer -= Time.deltaTime;
                ErrorTimer -= Time.deltaTime;
                if (Timer <= 0f)
                {
                    Timer = CoolTime;
                    minimapElements.ForEach(minimapElements => minimapElements.UpdatePos());
                }
                if (ErrorTimer <= 0f)
                {
                    Errored = false;
                }
                return;
            }
            if (Errored)
            {
                return;
            }
            Color color = Primitive.Color;
            color.a -= Mathf.Min(color.a, 1 * Time.deltaTime);
            Primitive.Color = color;
        }

        void UpdatePos()
        {
            RoleTypeId id = owner.roleManager.CurrentRole.RoleTypeId;
            if (id == RoleTypeId.Spectator || id == RoleTypeId.Overwatch)
            {
                return;
            }
            if (Site76Plugin.Instance.Config.HeartBeatScannerAdvanced && (id == RoleTypeId.Scp0492 || id == RoleTypeId.Scp939 || owner.playerEffectsController.GetEffect<CustomPlayerEffects.CardiacArrest>().IsEnabled))
            {
                return;
            }
            if (Errored)
            {
                return;
            }
            float min = float.MaxValue;
            int floor = 0;
            for (int i = 0; i < floorTransform.Count; i++)
            {
                float dis = owner.PlayerCameraReference.position.y - floorTransform[i];
                if (dis >= 0 && dis < min)
                {
                    min = dis;
                    floor = i;
                }
            }
            this.gameObject.transform.parent = minimapTransform[floor];
            Vector3 vector = (owner.PlayerCameraReference.position - schematicPosition) / 30;
            vector.y = 0.025f;
            this.gameObject.transform.localPosition = vector;
            Color color;
            if (Site76Plugin.Instance.Config.HeartBeatScannerColorLowPriority.TryGetValue(owner.GetFaction(), out string m))
            {
                color = MapEditorObject.GetColorFromString(m); 
                if (Site76Plugin.Instance.Config.HeartBeatScannerColorHighPriority.TryGetValue(owner.GetRoleId(), out string color1)) color = MapEditorObject.GetColorFromString(color1);
                color *= 5f;
                color.a = 1f;
                Primitive.Color = color;
            }
            else
            {
                Primitive.Color = Color.grey;
            }
        }

        public bool IsController = false;

        public float CoolTime = Site76Plugin.Instance.Config.HeartBeatScannerCoolTime;

        public ReferenceHub owner;

        public Primitive Primitive;

        public static bool Errored = false;

        public static float ErrorTimer = 0;

        public static float Timer = 0;

        public static Vector3 schematicPosition;

        public static List<float> floorTransform = new List<float> { };

        public static List<Transform> minimapTransform = new List<Transform> { };

        public static List<MinimapElement> minimapElements = new List<MinimapElement> { };
    }
}

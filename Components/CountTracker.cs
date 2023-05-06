using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PlayerRoles;
using MapEditorReborn.API.Features.Objects;

namespace Site76Plugin.Components
{
    public class CountTracker : MonoBehaviour
    {
        public void Setup(RoleTypeId[] id)
        {
            Condition = id;
        }

        void Start()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                digitControllers.Add(transform.GetChild(i).GetComponentsInChildren<PrimitiveObject>());
            }
            Color color = Color.red;
            if (Site76Plugin.Instance.Config.CountTrackerColor.TryGetValue(gameObject.name, out string color1)) color = MapEditorObject.GetColorFromString(color1);
            color *= 0.5f;
            color.a = 1f;
            foreach (PrimitiveObject primitive in transform.GetComponentsInChildren<PrimitiveObject>())
            {
                primitive.Primitive.Color = color;
            }
        }

        void Update()
        {
            List<ReferenceHub> founds = new List<ReferenceHub> { };
            int count = 0;
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.isLocalPlayer)
                {
                    continue;
                }
                if (Condition.Contains(hub.roleManager.CurrentRole.RoleTypeId))
                {
                    founds.Add(hub);
                    count++;
                }
            }
            Targets = founds;
            Count = count;
            if (MinimapElement.Errored)
            {
                Count = UnityEngine.Random.Range(0, 99);
            }
            for (int i = 0; i < digitControllers.Count; i++)
            {
                count = (Count / Mathf.Max(vs.Length * i, 1)) % vs.Length;
                int index = 0;
                Color color = Color.red;
                if (Site76Plugin.Instance.Config.CountTrackerColor.TryGetValue(gameObject.name, out string color1)) color = MapEditorObject.GetColorFromString(color1);
                foreach (PrimitiveObject primitive in digitControllers[i])
                {
                    Color color2 = color;
                    if (vs[count].Contains(index))
                    {
                        color2 *= 5f;
                        color2.a = 0.99f;
                        primitive.Primitive.Color = color2;
                    }
                    else
                    {
                        color2 *= 0.5f;
                        color2.a = 1f;
                        primitive.Primitive.Color = color2;
                    }
                    index++;
                }
            }
        }

        public List<PrimitiveObject[]> digitControllers = new List<PrimitiveObject[]> { };

        public RoleTypeId[] Condition;

        public int Count = 0;

        public List<ReferenceHub> Targets = new List<ReferenceHub> { };

        public static int[][] vs = Site76Plugin.Instance.Config.properties.CounterActivatingLines;
    }
}

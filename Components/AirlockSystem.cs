using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp244;
using MapEditorReborn.API.Features.Objects;

namespace Site76Plugin
{
    public class AirlockSystem : MonoBehaviour
    {
        void Start()
        {
            offset = transform.GetChild(0).position;
            size = transform.GetChild(0).localScale;
            UnityEngine.Object.Destroy(transform.GetChild(0).gameObject);
            animators.AddRange(gameObject.GetComponentsInChildren<Animator>());
            foreach (Animator animator in animators)
            {
                if (animator.transform.parent.gameObject.name.Contains("Door"))
                {
                    animator.Play("AirlockDoorOpen");
                }
            }
            foreach (LightSourceObject light in gameObject.GetComponentsInChildren<LightSourceObject>())
            {
                Exiled.API.Features.Toys.Light light1 = Exiled.API.Features.Toys.Light.Create(light.Position, light.Rotation.eulerAngles, Vector3.one * 0.01f, true);
                light1.Intensity = light.Base.Intensity;
                light1.Range = light.Base.Range;
                light1.Color = MapEditorObject.GetColorFromString(light.Base.Color);
                light.Destroy();
            }
            PrimitiveObject = this.transform.GetChild(3).GetChild(0).GetComponent<PrimitiveObject>();
            Color = PrimitiveObject.Primitive.Color;
        }

        void Update()
        {
            CooldownTimer -= Time.deltaTime;
            int count = 0;
            List<ReferenceHub> insidePlayers = new List<ReferenceHub> { };
            foreach (Collider collider in Physics.OverlapBox(offset, size / 2f, transform.rotation))
            {
                if (ReferenceHub.TryGetHub(collider.transform.root.gameObject, out ReferenceHub hub))
                {
                    insidePlayers.Add(hub);
                    count++;
                }
            }
            playersInRoom = insidePlayers;
            bool flag = count > PreviousCount;
            PreviousCount = count;
            if (CooldownTimer > 0)
            {
                return;
            }
            if (Activated)
            {
                foreach (Animator animator in animators)
                {
                    if (animator.transform.parent.gameObject.name.Contains("Door"))
                    {
                        animator.Play("AirlockDoorOpen");
                    }
                    else if (animator.gameObject.name.Contains("Alarm"))
                    {
                        animator.SetTrigger("AnimationExit");
                    }
                }
                Activated = false;
                PrimitiveObject.Primitive.Color = Color;
                CooldownTimer = Site76Plugin.Instance.Config.AirlockReworkingCoolTime;
            }
            else if (flag)
            {
                foreach (Animator animator in animators)
                {
                    if (animator.transform.parent.gameObject.name.Contains("Door"))
                    {
                        animator.Play("AirlockDoorClose");
                    }
                    else if (animator.gameObject.name.Contains("Alarm"))
                    {
                        animator.Play("AlarmBellRotation");
                    }
                }
                Activated = true;
                Color color = Color;
                color *= 5;
                color.a = 0.99f;
                PrimitiveObject.Primitive.Color = color;
                CooldownTimer = Site76Plugin.Instance.Config.AirlockCloseTime;
            }
        }

        public Vector3 offset;

        public Vector3 size;

        public int PreviousCount;

        public bool Activated = false;

        public float CooldownTimer = 0;

        readonly List<Animator> animators = new List<Animator> { };

        public List<ReferenceHub> playersInRoom;

        PrimitiveObject PrimitiveObject;

        Color Color;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MapEditorReborn.API.Features.Objects;
using Mirror;

namespace Site76Plugin.Components
{
    public class ConsoleSystem : MonoBehaviour
    {
        void Start()
        {
            Message = Convert(RawMessage);
            Stack<DigitDisplay> displays = new Stack<DigitDisplay> { };
            int index = -1;
            float yValue = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (yValue != transform.GetChild(i).position.y)
                {
                    if (index != -1)
                    {
                        DigitDisplays.Add(displays.ToList());
                        displays.Clear();
                    }
                    index++;
                    yValue = transform.GetChild(i).position.y;
                }
                displays.Push(transform.GetChild(i).gameObject.AddComponent<DigitDisplay>());
            }
            DigitDisplays.Add(displays.ToList());
            DigitDisplays.Reverse();
        }

        void Update()
        {
            MessageChangeTimer += Time.deltaTime;
            if (MessageChangeTimer > Message.duration)
            {
                if (MessageIndex == Site76Plugin.Instance.Config.ConsoleMessageFormats.Length - 1)
                {
                    MessageIndex = 0;
                }
                else
                {
                    MessageIndex++;
                }
                MessageChangeTimer = 0;
                RawMessage = Site76Plugin.Instance.Config.ConsoleMessageFormats[MessageIndex];
            }
            Message = Convert(RawMessage);
            if (MinimapElement.Errored)
            {
                Message = new MessageFormat(Mathf.Pow(UnityEngine.Random.Range(2f, 9f), 255f).ToString(), 1f, null);
            }
            if (prevMessage == Message)
            {
                return;
            }
            prevMessage = Message;
            int index = 0;
            bool changeLine = false;
            for (int i = 0; i < DigitDisplays.Count; i++)
            {
                Color color = Color.red;
                if (Message.colorPerLine.Length > i)
                {
                    color = MapEditorObject.GetColorFromString(Message.colorPerLine[i]);
                }
                for (int j = 0; j < DigitDisplays[i].Count; j++)
                {
                    char letter = ' ';
                    if (Message.content.Length > index && !changeLine)
                    {
                        if (Message.content[index] == '\n')
                        {
                            changeLine = true;
                        }
                        else
                        {
                            letter = Message.content[index];
                        }
                        index++;
                    }
                    DigitDisplays[i][j].ChangeValue(letter, color);
                }
                changeLine = false;
            }
        }

        MessageFormat Convert(MessageFormat x)
        {
            MessageFormat format = x;
            format.content = format.content.Replace("{year}", System.DateTime.Now.Year.ToString());
            format.content = format.content.Replace("{month}", System.DateTime.Now.Month.ToString());
            format.content = format.content.Replace("{day}", System.DateTime.Now.Day.ToString());
            format.content = format.content.Replace("{hour}", System.DateTime.Now.Hour.ToString());
            format.content = format.content.Replace("{minute}", System.DateTime.Now.Minute.ToString());
            format.content = format.content.Replace("{second}", System.DateTime.Now.Second.ToString());
            format.content = format.content.Replace("{ms}", Mathf.RoundToInt(System.DateTime.Now.Millisecond / 100).ToString());
            format.content = format.content.Replace("{next_respawn}", Exiled.API.Features.Respawn.NextKnownTeam.ToString());
            format.content = format.content.Replace("{respawn_time_m}", Exiled.API.Features.Respawn.TimeUntilSpawnWave.Minutes.ToString());
            format.content = format.content.Replace("{respawn_time_s}", Exiled.API.Features.Respawn.TimeUntilSpawnWave.Seconds.ToString());
            if (ServerConsole.singleton.NameFormatter.TryProcessExpression(format.content, "player list title", out string result))
            {
                format.content = result;
            }
            return format;
        }

        public List<List<DigitDisplay>> DigitDisplays = new List<List<DigitDisplay>> { };

        public int MessageIndex = 0;

        public MessageFormat Message;

        public MessageFormat RawMessage = Site76Plugin.Instance.Config.ConsoleMessageFormats[0];

        MessageFormat prevMessage = new MessageFormat();

        float MessageChangeTimer = 0;

        public class DigitDisplay : MonoBehaviour
        {
            void Start()
            {
                primitiveObjects = this.transform.GetComponentsInChildren<PrimitiveObject>();
                networkIdentities = this.transform.GetComponentsInChildren<NetworkIdentity>();
            }

            public void ChangeValue(char letter, Color color)
            {
                int[] value;
                if (!Site76Plugin.Instance.Config.properties.BinaryValuesForConsole.Keys.Contains(letter))
                {
                    value = Site76Plugin.Instance.Config.properties.BinaryValuesForConsole[' '];
                }
                else
                {
                    value = Site76Plugin.Instance.Config.properties.BinaryValuesForConsole[letter];
                }
                for (int i = 0; i < primitiveObjects.Length; i++)
                {
                    PrimitiveObject primitive = primitiveObjects[i];
                    NetworkIdentity identity = networkIdentities[i];
                    identity.NetworkBehaviours[0].syncInterval = float.MaxValue;
                    Color color1;
                    if (value.Contains(i))
                    {
                        color1 = color;
                        color1 *= 5f;
                        color1.a = 0.9f;
                    }
                    else
                    {
                        color1 = Color.grey;
                        color1 *= 0.5f;
                        color1.a = 1f;
                    }
                    if (primitive.Primitive.Color == color)
                    {
                        continue;
                    }
                    primitive.Primitive.Color = color1;
                    identity.NetworkBehaviours[0].syncInterval = Time.deltaTime;
                }
            }

            PrimitiveObject[] primitiveObjects;

            NetworkIdentity[] networkIdentities;
        }

        [Serializable]
        public struct MessageFormat
        {
            public string content { get; set; }
            public float duration { get; set; }
            public string[] colorPerLine { get; set; }

            public MessageFormat(string Content, float Duration, string[] ColorPerLine)
            {
                content = Content;
                duration = Duration;
                colorPerLine = ColorPerLine;
            }

            public static bool operator ==(MessageFormat a, MessageFormat b)
            {
                return a.content == b.content && a.colorPerLine == b.colorPerLine && a.duration == b.duration;
            }

            public static bool operator !=(MessageFormat a, MessageFormat b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return base.ToString();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;
using SCPSLAudioApi;
using SCPSLAudioApi.AudioCore;

namespace Site76Plugin
{
    public class ElevatorSystem : MonoBehaviour
    {
        void Start()
        {
            Car = transform.GetChild(0).gameObject;
            for (int i = 1; i < transform.childCount; i++)
            {
                floors.Add(transform.GetChild(i).gameObject);
                floorsHeight.Add(transform.GetChild(i).transform.position.y);
            }
            foreach (Animator animator in gameObject.GetComponentsInChildren<Animator>())
            {
                if (animator.transform.parent.gameObject.name == "Car")
                {
                    CarDoors.Add(animator);
                }
                else
                {
                    Doors.Add(animator);
                }
            }
        }

        void FixedUpdate()
        {
            DoorTimer -= Time.fixedDeltaTime;
            DoorWaitingTimer -= Time.fixedDeltaTime;
            if (DoorOpened || DoorWaitingTimer >= 0f || DoorTimer >= 0f || CalledFloor.Count == 0)
            {
                if (DoorWaitingTimer <= 0f && DoorOpened)
                {
                    ControlDoor(false);
                }
                return;
            }
            CantidatedFloor.Clear();
            foreach (int i in CalledFloor)
            {
                if ((GoingUp ? floorsHeight[i] > Car.transform.position.y : floorsHeight[i] < Car.transform.position.y))
                {
                    CantidatedFloor.Add(i);
                }
            }
            if (CantidatedFloor.Count == 0)
            {
                GoingUp = !GoingUp;
                return;
            }
            int MinDis = int.MaxValue;
            foreach (int i in CantidatedFloor)
            {
                if (Mathf.Abs(i - CurrentFloor) < MinDis)
                {
                    MinDis = Mathf.Abs(i - CurrentFloor);
                    Destination = i;
                }
            }
            Arrived = false;
            Car.transform.Translate(new Vector3(0f, ElevatorSerialize.ElevatorSpeed * (GoingUp ? 1 : -1) * Time.fixedDeltaTime, 0f));
            if (Mathf.Abs(Car.transform.position.y - floorsHeight[Destination]) <= 0.1f)
            {
                Car.transform.Translate(new Vector3(0f, floorsHeight[Destination] - Car.transform.position.y, 0f));
                CurrentFloor = Destination;
                Arrived = true;
                ControlDoor(true);
                CalledFloor.Remove(CurrentFloor);
            }
        }

        public void CallElevator(int CallFrom)
        {
            if (DisallowedFloors.Contains(CallFrom))
            {
                return;
            }
            if (!CalledFloor.Contains(CallFrom))
            {
                if (CallFrom == CurrentFloor && Arrived)
                {
                    ControlDoor(true);
                    return;
                }
                CalledFloor.Add(CallFrom);
            }
        }

        public void CallInternal(GameObject game)
        {
            if (ButtonInside.TryGetValue(game, out int value))
            {
                CallElevator(value);
                return;
            }
            for (int i = 0; i < game.transform.parent.childCount; i++)
            {
                if (game == game.transform.parent.GetChild(i).gameObject)
                {
                    CallElevator(i);
                    ButtonInside.Add(game, i);
                    return;
                }
            }
        }

        public void ControlDoor(bool Open)
        {
            if (DoorTimer > 0 || !Arrived)
            {
                return;
            }
            DoorWaitingTimer = ElevatorSerialize.DoorWaitingTime;
            if (Open && DoorOpened) return;
            DoorOpened = Open;
            DoorTimer = ElevatorSerialize.DoorAnimationCoolTime;
            Doors[CurrentFloor].Play(ElevatorSerialize.AnimationNames[Open ? 0 : 1]);
            CarDoors[int.Parse(Doors[CurrentFloor].gameObject.name)].Play(ElevatorSerialize.AnimationNames[Open ? 0 : 1]);
        }

        public GameObject Car;

        public bool DoorOpened = false;

        public bool GoingUp, Arrived = true;

        public int Destination, CurrentFloor = 0;

        public float DoorTimer, DoorWaitingTimer = 0;

        public ElevatorSerialize ElevatorSerialize;


        //Lists
        public List<int> CalledFloor = new List<int> { };

        public List<int> CantidatedFloor = new List<int> { };

        public List<Animator> Doors = new List<Animator> { };

        public List<Animator> CarDoors = new List<Animator> { };

        public List<GameObject> floors = new List<GameObject> { };

        public List<float> floorsHeight = new List<float> { };

        public Dictionary<GameObject, int> ButtonInside = new Dictionary<GameObject, int> { };

        //Preference
        public List<int> DisallowedFloors = new List<int> { };
    }

    [Serializable]
    public struct ElevatorSerialize
    {
        public ElevatorSerialize(string[] message, float wait, float cool, int[] dis, float speed)
        {
            AnimationNames = message;
            DoorWaitingTime = wait;
            DoorAnimationCoolTime = cool;
            DisallowedFloors = dis;
            ElevatorSpeed = speed;
        }

        public string[] AnimationNames { get; set; }
        public float DoorWaitingTime { get; set; }
        public float DoorAnimationCoolTime { get; set; }
        public int[] DisallowedFloors { get; set; }
        public float ElevatorSpeed { get; set; }
    }
}

using System.Collections;
using System.Threading;
using HeroVirtualTabletop.Desktop;
using System;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class CameraImpl : Camera
    {
        private ManagedCharacter _manueveringCharacter;

        public CameraImpl(KeyBindCommandGenerator generator)
        {
            Generator = generator;
            // base("V_Arachnos_Security_Camera", IdentityType.Model, "Camera")
            this.Identity = new IdentityImpl(null, "Camera", "V_Arachnos_Security_Camera", SurfaceType.Model, Generator, null);
        }

        public KeyBindCommandGenerator Generator { get; }

        private Position position;
        public Position Position
        {
            get
            {
                if (position == null)
                {
                    try
                    {
                        RefreshPosition();
                    }
                    catch
                    {

                    }
                }
                return position;
            }
            set
            {
                position = value;
            }
        }

        public Identity Identity { get; private set; }

        public ManagedCharacter ManueveringCharacter
        {
            get
            {
                return _manueveringCharacter;
            }
            set
            {
                if (_manueveringCharacter != null)
                {
                    ActivateCameraIdentity();
                    _manueveringCharacter.SpawnToDesktop();
                    _manueveringCharacter = null;
                }
                if (value != null)
                {
                    _manueveringCharacter = value;
                    _manueveringCharacter.Target(false);
                    MoveToTarget();
                    //if(Position == null)
                    //    RefreshPosition();
                    float dist = 13.23f, calculatedDistance;
                    var maxRecalculationCount = 5;
                        // We will allow the same distance to be calculated 5 times at max. After this we'll assume that the camera is stuck.
                    var distanceTable = new Hashtable();
                    while (Position.IsWithin(dist, _manueveringCharacter.Position, out calculatedDistance) == false)
                    {
                        if (distanceTable.Contains(calculatedDistance))
                        {
                            var count = (int) distanceTable[calculatedDistance];
                            count++;
                            if (count > maxRecalculationCount)
                                break;
                            distanceTable[calculatedDistance] = count;
                        }
                        else
                        {
                            distanceTable.Add(calculatedDistance, 1);
                        }
                        Thread.Sleep(500);
                    }
                    distanceTable.Clear();

                    _manueveringCharacter.ClearFromDesktop(true, false);
                    Identity = value.Identities.Active;
                    Identity.Play();
                }
            }
        }
        public void ReInitializePosition()
        {
            MemoryManager memoryManager = new MemoryManagerImpl(false, 1696336);
            DesktopMemoryCharacter desktopChar = new DesktopMemoryCharacterImpl(memoryManager);
            Position = new PositionImpl(desktopChar);
        }

        public void RefreshPosition()
        {
            ReInitializePosition();
            float x, y, z;
            int numberOfReInitialize = 0;
            while (true)
            {
                if (Math.Abs(position.X) < 0.01f || Math.Abs(position.Y) < 0.01f || Math.Abs(position.Z) < 0.01f)
                {
                    ReInitializePosition();
                    numberOfReInitialize++;
                }
                else
                {
                    x = position.X;
                    y = position.Y;
                    z = position.Z;
                    if (Math.Abs(x) >= 0.01f && Math.Abs(y) >= 0.01f && Math.Abs(z) >= 0.01f)
                        break;
                }

            }
        }
        public void MoveToTarget(bool completeEvent = true)
        {
            Generator.GenerateDesktopCommandText(DesktopCommand.Follow, "");
            if (completeEvent)
                Generator.CompleteEvent();
        }

        public void ActivateCameraIdentity()
        {
            //We need to untarget everything before loading camera skin
            this.Identity = new IdentityImpl(null, "Camera", "V_Arachnos_Security_Camera", SurfaceType.Model, Generator, null);
            Generator.GenerateDesktopCommandText(DesktopCommand.TargetEnemyNear);
            Identity.Play(true);
        }

        public void ActivateManueveringCharacterIdentity()
        {
        }

        public void DisableMovement()
        {
        }
    }
}
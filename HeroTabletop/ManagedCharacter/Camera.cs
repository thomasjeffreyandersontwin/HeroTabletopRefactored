using System.Collections;
using System.Threading;
using HeroVirtualTableTop.Desktop;

namespace HeroVirtualTableTop.ManagedCharacter
{
    internal class CameraImpl : Camera
    {
        private ManagedCharacter _manueveringCharacter;

        public CameraImpl(KeyBindCommandGenerator generator)
        {
            Generator = generator;
        }

        public KeyBindCommandGenerator Generator { get; }

        public Position Position { get; set; }
        public Identity Identity { get; private set; }

        public ManagedCharacter ManueveringCharacter
        {
            get { return _manueveringCharacter; }
            set
            {
                if (value != null)
                {
                    _manueveringCharacter = value;
                    _manueveringCharacter.Target(false);
                    MoveToTarget();

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

                    _manueveringCharacter.ClearFromDesktop();
                    Identity = value.Identities.Active;
                    Identity.Play();
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
        }

        public void ActivateManueveringCharacterIdentity()
        {
        }

        public void DisableMovement()
        {
        }
    }
}
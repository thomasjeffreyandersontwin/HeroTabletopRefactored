namespace HeroVirtualTableTop.Desktop
{
    internal class DesktopMemoryCharacterImpl : DesktopMemoryCharacter
    {
        public Position Position { get; set; }
        public string Label { get; set; }
        public float MemoryAddress { get; set; }

        public void Target()
        {
        }

        public MemoryManager memoryManager { get; set; }

        public dynamic GetAttributeFromAdress(float address, string varType)
        {
            return null;
        }

        public void SetTargetAttribute(float offset, dynamic value, string varType)
        {
        }

        public DesktopMemoryCharacter WaitUntilTargetIsRegistered()
        {
            var w = 0;
            var currentTarget = new DesktopMemoryCharacterImpl();
            while (Label != currentTarget.Label)
            {
                w++;
                currentTarget = new DesktopMemoryCharacterImpl();
                if (w > 5)
                {
                    currentTarget = null;
                    break;
                }
            }
            return currentTarget;
        }
    }
}
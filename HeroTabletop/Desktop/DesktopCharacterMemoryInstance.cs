using System;
using System.Text;

namespace HeroVirtualTabletop.Desktop
{
    public class DesktopMemoryCharacterImpl : DesktopMemoryCharacter
    {

        public string Label
        {
            get
            {
                try
                {
                    return MemoryManager?.GetAttributeAsString(12740, Encoding.UTF8);
                }
                catch(Exception ex)
                {
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    MemoryManager?.SetTargetAttribute(12740, value, Encoding.UTF8);
                }
                catch { }
            }
        }

        public string Name
        {
            get
            {
                string label = Label;
                int start = 7;
                int end = 1 + label.IndexOf("]", start);
                return label.Substring(start, end - start);
            }
        }
        public float MemoryAddress { get; set; }

        public bool IsReal
        {
            get
            {
                return (this.MemoryManager?.Pointer != 0);
            }
        }

        public MemoryManager MemoryManager { get; set; }

        public Position Position
        {
            get
            {
                return new PositionImpl();
            }

            set
            {
                
            }
        }

        public DesktopMemoryCharacterImpl(MemoryManager manager)
        {
            this.MemoryManager = manager;
        }

        public DesktopMemoryCharacterImpl()
        {
            this.MemoryManager = new MemoryManagerImpl();
        }

        public void Target()
        {
            MemoryManager?.WriteCurrentTargetToGameMemory();
        }

        public void UnTarget()
        {
            MemoryManager?.WriteToMemory((uint)0);
        }
    }

    public class DesktopCharacterTargeterImpl : DesktopCharacterTargeter
    {
        private DesktopMemoryCharacter targetedInstance;
        public DesktopMemoryCharacter TargetedInstance
        {
            get
            {
                targetedInstance = new DesktopMemoryCharacterImpl();
                return targetedInstance;
            }
            set
            {
                targetedInstance = value;
            }
        }
    }
}
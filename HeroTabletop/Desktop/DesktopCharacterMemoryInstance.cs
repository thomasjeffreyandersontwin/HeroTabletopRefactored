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
                if (!string.IsNullOrEmpty(label))
                {
                    int upto = label.IndexOf("[");
                    return label.Substring(0, upto -1).Trim();
                }
                return null;
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
            get;set;
        }

        public DesktopMemoryCharacterImpl(MemoryManager manager)
        {
            this.MemoryManager = manager;
            this.Position = new PositionImpl(this);
        }

        public DesktopMemoryCharacterImpl():this(new MemoryManagerImpl())
        {
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
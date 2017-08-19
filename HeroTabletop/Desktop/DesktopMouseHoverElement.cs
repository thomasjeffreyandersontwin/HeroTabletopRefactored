using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Desktop
{
    public interface DesktopMouseHoverElement
    {
        string CurrentHoveredInfo { get; }
        string Label { get; }
        string Name { get; }
        Vector3 PositionVector { get; }
        Position Position { get; }
    }

    public class DesktopMouseHoverElementImpl : DesktopMouseHoverElement
    {
        private IconInteractionUtility iconInteractionUtility;
        private DesktopMemoryCharacter desktopMemoryCharacter;
        public DesktopMouseHoverElementImpl(IconInteractionUtility iconInteractionUtility)
        {
            this.iconInteractionUtility = iconInteractionUtility;
            MemoryManager memMan = new MemoryManagerImpl(false);
            this.desktopMemoryCharacter = new DesktopMemoryCharacterImpl(memMan);
        }

        public string CurrentHoveredInfo
        {
            get
            {
                return iconInteractionUtility.GeInfoFromNpcMouseIsHoveringOver();
            }
        }

        public string Label
        {
            get
            {
                string hoveredInfo = CurrentHoveredInfo;
                if (!string.IsNullOrEmpty(hoveredInfo))
                {
                    int start = 7;
                    int end = 1 + hoveredInfo.IndexOf("]", start);
                    return hoveredInfo.Substring(start, end - start);
                }
                return "";
            }
        }

        public string Name
        {
            get
            {
                string hoveredInfo = CurrentHoveredInfo;
                if (hoveredInfo != "")
                {
                    int nameEnd = hoveredInfo.IndexOf("[", 7);
                    string name = hoveredInfo.Substring(7, nameEnd - 7).Trim();
                    if (name.EndsWith("] X:"))
                    {
                        name = name.Substring(0, name.LastIndexOf("]"));
                    }
                    return name;
                }
                return "";
            }
        }

        public Vector3 PositionVector
        {
            get
            {
                string mouseXYZInfo = iconInteractionUtility.GetMouseXYZString();
                Vector3 vector3 = new Vector3();
                float f;
                int xStart = mouseXYZInfo.IndexOf("[");
                int xEnd = mouseXYZInfo.IndexOf("]");
                string xStr = mouseXYZInfo.Substring(xStart + 1, xEnd - xStart - 1);
                if (float.TryParse(xStr, out f))
                    vector3.X = f;
                int yStart = mouseXYZInfo.IndexOf("[", xEnd);
                int yEnd = mouseXYZInfo.IndexOf("]", yStart);
                string yStr = mouseXYZInfo.Substring(yStart + 1, yEnd - yStart - 1);
                if (float.TryParse(yStr, out f))
                    vector3.Y = f;
                int zStart = mouseXYZInfo.IndexOf("[", yEnd);
                int zEnd = mouseXYZInfo.IndexOf("]", zStart);
                string zStr = mouseXYZInfo.Substring(zStart + 1, zEnd - zStart - 1);
                if (float.TryParse(zStr, out f))
                    vector3.Z = f;
                return vector3;
            }
        }

        public Position Position
        {
            get
            {
                Position position = this.desktopMemoryCharacter.Position;
                position.Vector = this.PositionVector;
                return position;
            }
        }
    }
}

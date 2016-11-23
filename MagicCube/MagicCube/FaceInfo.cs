using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    class FaceInfo
    {
        public string name;
        public string color;
        public uint direction;
        public Point location;
        
        public FaceInfo(string name, string color, uint direction, Point location)
        {
            this.name = name;
            this.color = color;
            this.direction = direction;
            this.location = location;
        }

        public static FaceInfo[] items =
        {
            new FaceInfo("Front", "White",  Direction.UP,    new Point(1, 1)),
            new FaceInfo("Back",  "Blue",   Direction.DOWN,  new Point(3, 1)),
            new FaceInfo("Up",    "Yellow", Direction.LEFT,  new Point(1, 0)),
            new FaceInfo("Down",  "Green",  Direction.RIGHT, new Point(1, 2)),
            new FaceInfo("Left",  "Orange", Direction.LEFT,  new Point(0, 1)),
            new FaceInfo("Right", "Red",    Direction.LEFT,  new Point(2, 1))
        };
    }
}

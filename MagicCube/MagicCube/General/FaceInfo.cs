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
            new FaceInfo("Front", "White",  Directions.Up,    new Point(1, 1)),
            new FaceInfo("Up",    "Yellow", Directions.Left,  new Point(1, 0)),
            new FaceInfo("Right", "Red",    Directions.Left,  new Point(2, 1)),
            new FaceInfo("Back",  "Blue",   Directions.Down,  new Point(3, 1)),
            new FaceInfo("Down",  "Green",  Directions.Right, new Point(1, 2)),
            new FaceInfo("Left",  "Orange", Directions.Left,  new Point(0, 1)),
        };
    }
}

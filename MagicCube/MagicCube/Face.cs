using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    class Face
    {
        const uint COUNT = Direction.TURN_COUNT;

        uint color;
        uint direction;

        uint[] middle_elements = new uint[COUNT];
        uint[] corner_elements = new uint[COUNT];
        Face[] neighbours = new Face[COUNT];       // adjoining faces
        uint[] directions = new uint[COUNT];         // neighbours directions


        public struct Edge
        {
            public uint left, middle, right;
            public Edge(uint left, uint middle, uint right)
            {
                this.left = left;
                this.middle = middle;
                this.right = right;
            }
        }

        public uint MiddleColor(uint direction)
        {
            return middle_elements[Direction.Sum(this.direction, direction)];
        }

        public void MiddleColor(uint direction, uint color)
        {
            middle_elements[Direction.Sum(this.direction, direction)] = color;
        }

        public uint CornerColor(uint direction)
        {
            return corner_elements[Direction.Sum(this.direction, direction)];
        }

        public void CornerColor(uint direction, uint color)
        {
            corner_elements[Direction.Sum(this.direction, direction)] = color;
        }


        Edge EdgeColors(uint direction)
        {
            return new Edge(
                CornerColor(Direction.TurnLeft(direction)),
                MiddleColor(direction),
                CornerColor(direction)
                );
        }

        void EdgeColors(uint direction, Edge edge)
        {
            CornerColor(Direction.TurnLeft(direction), edge.left);
            MiddleColor(direction, edge.middle);
            CornerColor(direction, edge.right);
        }

        // Rotate clockwise: 0,1,2,3 => 3,0,1,2
        public void RotateRight()
        {
            // face rotation
            direction = Direction.TurnRight(direction);

            // neighbours rotation
            uint i = COUNT - 1;
            Edge edge = neighbours[i].EdgeColors(directions[i]);
            while (i > 0)
            {
                neighbours[i].EdgeColors(directions[i],
                    neighbours[i - 1].EdgeColors(directions[i - 1]));
                i--;
            }
            neighbours[i].EdgeColors(directions[i], edge);
        }

        // Rotate counterclockwise: 0,1,2,3 => 1,2,3,0
        public void RotateLeft()
        {
            // face rotation
            direction = Direction.TurnLeft(direction);

            // neighbours rotation
            uint i = 0;
            Edge edge = neighbours[i].EdgeColors(directions[i]);
            while (i < COUNT - 1)
            {
                neighbours[i].EdgeColors(directions[i],
                    neighbours[i + 1].EdgeColors(directions[i + 1]));
                i++;
            }
            neighbours[i].EdgeColors(directions[i], edge);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    class Face
    {
        const int COUNT = Direction.TURN_COUNT;

        int color;
        int direction;

        int[] middle_elements = new int[COUNT];
        int[] corner_elements = new int[COUNT];
        Face[] neighbours = new Face[COUNT];       // adjoining faces
        int[] directions = new int[COUNT];         // neighbours directions


        public struct Edge
        {
            public int left, middle, right;
            public Edge(int left, int middle, int right)
            {
                this.left = left;
                this.middle = middle;
                this.right = right;
            }
        }

        public int MiddleColor(int direction)
        {
            return middle_elements[Direction.Sum(this.direction, direction)];
        }

        public void MiddleColor(int direction, int color)
        {
            middle_elements[Direction.Sum(this.direction, direction)] = color;
        }

        public int CornerColor(int direction)
        {
            return corner_elements[Direction.Sum(this.direction, direction)];
        }

        public void CornerColor(int direction, int color)
        {
            corner_elements[Direction.Sum(this.direction, direction)] = color;
        }


        Edge EdgeColors(int direction)
        {
            return new Edge(
                CornerColor(Direction.TurnLeft(direction)),
                MiddleColor(direction),
                CornerColor(direction)
                );
        }

        void EdgeColors(int direction, Edge edge)
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
            int last = COUNT - 1;
            Edge edge = neighbours[last].EdgeColors(directions[last]);
            for (int i = 1; i < COUNT; i++)
            {
                neighbours[i].EdgeColors(directions[i],
                    neighbours[i - 1].EdgeColors(directions[i - 1]));
            }
            neighbours[0].EdgeColors(directions[0], edge);
        }

        // Rotate counterclockwise: 0,1,2,3 => 1,2,3,0
        public void RotateLeft()
        {
            // face rotation
            direction = Direction.TurnLeft(direction);

            // neighbours rotation
            int last = COUNT - 1;
            Edge edge = neighbours[0].EdgeColors(directions[0]);
            for (int i = 1; i < COUNT; i++)
            {
                neighbours[i - 1].EdgeColors(directions[i - 1],
                    neighbours[i].EdgeColors(directions[i]));
            }
            neighbours[last].EdgeColors(directions[last], edge);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISK
{
    class GraphNode: IComparable<GraphNode>
    {
        private int id;
        public List<GraphNode> neighbours;
        private bool startNode;

        public GraphNode(int id, Boolean isStartNode)
        {
            this.Id = id;
            this.startNode = isStartNode;
            this.neighbours = new List<GraphNode>();
        }

        public bool StartNode
        {
            get { return startNode; }
            set { /*not implemented*/}
        }

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public void addNeighbour(GraphNode newNeighbour)
        {
            this.neighbours.Add(newNeighbour);
            newNeighbour.neighbours.Add(this);
        }

        public Boolean isNeighbour(GraphNode maybeNeighbour)
        {
            foreach(GraphNode neighbour in neighbours)
            {
                if(neighbour.GetHashCode() == maybeNeighbour.GetHashCode())
                    { return true; }
            }
            return false;
        }

        public int CompareTo(GraphNode other)
        {
            var thisCount = this.neighbours.Count;
            var thatCount = other.neighbours.Count;
            if (thatCount > thisCount) return 1;
            if (thatCount == thisCount) return 0;
            return -1;
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public override bool Equals(object obj)
        {
            var item = obj as GraphNode;
            return Equals(item);
        }

        protected bool Equals(GraphNode other)
        {
            return other.Id == this.id;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}

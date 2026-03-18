namespace WalkerSim
{
    public partial class Simulation
    {
        const int CellSize = 96;
        private int _cellCountY;

        void SetupGrid()
        {
            var cellCountX = (int)System.Math.Ceiling(WorldSize.X / CellSize);
            _cellCountY = (int)System.Math.Ceiling(WorldSize.Y / CellSize);
            var totalCells = cellCountX * _cellCountY;

            var grid = new int[totalCells];
            for (int i = 0; i < totalCells; i++)
                grid[i] = -1;

            _state.Grid = grid;
        }

        void UpdateGrid()
        {
            SetupGrid();
            foreach (var agent in _state.Agents)
            {
                agent.CellIndex = -1;
                agent.NextInCell = -1;
                agent.PrevInCell = -1;
            }
        }

        void RebuildGrid()
        {
            SetupGrid();
            var agents = _state.Agents;
            for (int i = 0; i < agents.Count; i++)
            {
                var agent = agents[i];
                agent.CellIndex = -1;
                agent.NextInCell = -1;
                agent.PrevInCell = -1;
                MoveInGrid(agent);
            }
        }

        int GetCellIndex(float x, float y)
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            float remapX = MathEx.Remap(x, worldMins.X, worldMaxs.X, 0f, WorldSize.X);
            float remapY = MathEx.Remap(y, worldMins.Y, worldMaxs.Y, 0f, WorldSize.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            return cellX * _cellCountY + cellY;
        }

        int GetCellIndex(Vector3 pos)
        {
            return GetCellIndex(pos.X, pos.Y);
        }

        private void RemoveFromCell(Agent agent)
        {
            var grid = _state.Grid;
            var agents = _state.Agents;
            int cell = agent.CellIndex;

            // Unlink prev.
            if (agent.PrevInCell != -1)
                agents[agent.PrevInCell].NextInCell = agent.NextInCell;
            else
                grid[cell] = agent.NextInCell; // Was head.

            // Unlink next.
            if (agent.NextInCell != -1)
                agents[agent.NextInCell].PrevInCell = agent.PrevInCell;

            agent.PrevInCell = -1;
            agent.NextInCell = -1;
            agent.CellIndex = -1;
        }

        private void InsertIntoCell(Agent agent, int cellIndex)
        {
            var grid = _state.Grid;
            var agents = _state.Agents;

            int oldHead = grid[cellIndex];
            grid[cellIndex] = agent.Index;

            agent.PrevInCell = -1;
            agent.NextInCell = oldHead;
            agent.CellIndex = cellIndex;

            if (oldHead != -1)
                agents[oldHead].PrevInCell = agent.Index;
        }

        public void MoveInGrid(Agent agent)
        {
            var grid = _state.Grid;
            if (grid.Length == 0)
                return;

            var newCellIndex = GetCellIndex(agent.Position);
            if (newCellIndex < 0)
                newCellIndex = 0;
            if (newCellIndex >= grid.Length)
                newCellIndex = grid.Length - 1;

            if (newCellIndex == agent.CellIndex)
                return;

            if (agent.CellIndex != -1)
                RemoveFromCell(agent);

            InsertIntoCell(agent, newCellIndex);
        }

#if DEBUG
        private void ValidateAgentInCorrectCell(Agent agent)
        {
            if (agent.CellIndex == -1)
                return;

            if (agent.CurrentState != Agent.State.Wandering)
                return;

            var correctCellIndex = GetCellIndex(agent.Position);
            if (agent.CellIndex != correctCellIndex)
                throw new System.Exception("Bug: agent in wrong cell");

            // Walk the chain to confirm agent is present.
            var grid = _state.Grid;
            var agents = _state.Agents;
            int idx = grid[agent.CellIndex];
            while (idx != -1)
            {
                if (idx == agent.Index)
                    return;
                idx = agents[idx].NextInCell;
            }
            throw new System.Exception("Bug: agent not found in cell chain");
        }
#endif

        private void QueryCell(Vector3 pos, int cellX, int cellY, int excludeIndex, float maxDist, FixedBufferList<Agent> res)
        {
            var grid = _state.Grid;
            var agents = _state.Agents;

            var cellIndex = cellX * _cellCountY + cellY;
            if (cellIndex < 0 || cellIndex >= grid.Length)
                return;

            var maxDistSqr = maxDist * maxDist;
            int idx = grid[cellIndex];
            while (idx != -1)
            {
                var other = agents[idx];
                idx = other.NextInCell;

                if (other.CurrentState != Agent.State.Wandering)
                    continue;

                if (other.Index == excludeIndex)
                    continue;

                var distance = Vector3.Distance2DSqr(pos, other.Position);
                if (distance < maxDistSqr)
                {
                    res.Add(other);
                    if (res.Full)
                        return;
                }
            }
        }

        private FixedBufferList<Agent> QueryCellsLockFree(Vector3 pos, int excludeIndex, float maxDistance, FixedBufferList<Agent> res = null)
        {
            if (res == null)
                res = new FixedBufferList<Agent>(1024);

            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            float remapX = MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, WorldSize.X);
            float remapY = MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, WorldSize.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            int cellRadius = (int)(maxDistance / CellSize) + 1;

            if (res.Full)
                return res;

            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int y = -cellRadius; y <= cellRadius; y++)
                {
                    QueryCell(pos, cellX + x, cellY + y, excludeIndex, maxDistance, res);
                    if (res.Full)
                        return res;
                }
            }

            return res;
        }

        public FixedBufferList<Agent> QueryCells(Vector3 pos, int excludeIndex, float maxDistance, FixedBufferList<Agent> res = null)
        {
            lock (_state)
            {
                return QueryCellsLockFree(pos, excludeIndex, maxDistance, res);
            }
        }

        private int QueryCellCount(Vector3 pos, int cellX, int cellY, float maxDist, int count, int maxCount)
        {
            var grid = _state.Grid;
            var agents = _state.Agents;

            var cellIndex = cellX * _cellCountY + cellY;
            if (cellIndex < 0 || cellIndex >= grid.Length)
                return count;

            var maxDistSqr = maxDist * maxDist;
            int idx = grid[cellIndex];
            while (idx != -1)
            {
                var other = agents[idx];
                idx = other.NextInCell;

                if (other.CurrentState != Agent.State.Wandering)
                    continue;

                var distance = Vector3.Distance2DSqr(pos, other.Position);
                if (distance < maxDistSqr)
                {
                    count++;
                    if (count >= maxCount)
                        return count;
                }
            }

            return count;
        }

        private int QueryNearbyCount(Vector3 pos, float maxDistance, int maxCount = 100)
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            float remapX = MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, WorldSize.X);
            float remapY = MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, WorldSize.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            int cellRadius = (int)(maxDistance / CellSize) + 1;

            int count = 0;

            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int y = -cellRadius; y <= cellRadius; y++)
                {
                    count = QueryCellCount(pos, cellX + x, cellY + y, maxDistance, count, maxCount);
                    if (count >= maxCount)
                        return count;
                }
            }

            return count;
        }

        public delegate void NeighborCallback(Agent neighbor);

        public interface INeighborProcessor
        {
            void Process(Agent neighbor);
        }

        public void ForEachNearby<T>(Vector3 pos, int excludeIndex, float maxDistance, ref T processor) where T : struct, INeighborProcessor
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;
            var grid = _state.Grid;
            var agents = _state.Agents;
            var gridLength = grid.Length;

            float remapX = MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, WorldSize.X);
            float remapY = MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, WorldSize.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            int cellRadius = (int)(maxDistance / CellSize) + 1;

            var maxDistSqr = maxDistance * maxDistance;

            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                int checkX = cellX + x;
                int baseIndex = checkX * _cellCountY;

                for (int y = -cellRadius; y <= cellRadius; y++)
                {
                    var cellIndex = baseIndex + (cellY + y);
                    if (cellIndex < 0 || cellIndex >= gridLength)
                        continue;

                    int idx = grid[cellIndex];
                    while (idx != -1)
                    {
                        var other = agents[idx];
                        idx = other.NextInCell;

                        if (other.CurrentState != Agent.State.Wandering)
                            continue;

                        if (other.Index == excludeIndex)
                            continue;

                        var distance = Vector3.Distance2DSqr(pos, other.Position);
                        if (distance < maxDistSqr)
                        {
                            processor.Process(other);
                        }
                    }
                }
            }
        }

    }
}

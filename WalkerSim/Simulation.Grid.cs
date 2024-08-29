using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        const int CellSize = 96;

        void SetupGrid()
        {
            var cellCountX = (int)System.Math.Ceiling(WorldSize.X / CellSize);
            var cellCountY = (int)System.Math.Ceiling(WorldSize.Y / CellSize);
            var totalCells = cellCountX * cellCountY;

            var grid = new List<int>[totalCells];
            for (int i = 0; i < totalCells; i++)
            {
                grid[i] = new List<int>();
            }
            _state.Grid = grid;
        }

        void UpdateGrid()
        {
            SetupGrid();
            foreach (var agent in _state.Agents)
            {
                agent.CellIndex = -1;
            }
        }

        int GetCellIndex(float x, float y)
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            // The grid uses 0, 0 as starting origin.
            float remapX = Math.Remap(x, worldMins.X, worldMaxs.X, 0f, WorldSize.X);
            float remapY = Math.Remap(y, worldMins.Y, worldMaxs.Y, 0f, WorldSize.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            var cellCountY = (int)System.Math.Ceiling(WorldSize.Y / CellSize);
            return cellX * cellCountY + cellY;
        }

        int GetCellIndex(Vector3 pos)
        {
            return GetCellIndex(pos.X, pos.Y);
        }

        public void MoveInGrid(Agent agent)
        {
            var grid = _state.Grid;
            var newCellIndex = GetCellIndex(agent.Position);
            if (newCellIndex != agent.CellIndex)
            {
                if (agent.CellIndex != -1)
                {
                    // Remove from old cell.
                    var oldCellList = grid[agent.CellIndex];
#if DEBUG
                    if (!oldCellList.Contains(agent.Index))
                    {
                        throw new System.Exception("Bug");
                    }
#endif
                    oldCellList.Remove(agent.Index);
                }

                if (newCellIndex < 0)
                    newCellIndex = 0;
                if (newCellIndex >= grid.Length)
                    newCellIndex = grid.Length - 1;

                // Add to new cell.
                var newCellList = grid[newCellIndex];
                newCellList.Add(agent.Index);

                agent.CellIndex = newCellIndex;
            }
#if DEBUG
            else
            {
                // Confirm in cell.
                var cell = grid[newCellIndex];
                if (!cell.Contains(agent.Index))
                {
                    throw new System.Exception("Bug");
                }
            }
#endif
        }

        private void ValidateAgentInCorrectCell(Agent agent)
        {
            if (agent.CellIndex == -1)
            {
                return;
            }

            if (agent.CurrentState != Agent.State.Wandering)
            {
                // There is a potential race condition for active agents as the position gets updated from
                // the main thread, this is in general fine as we skip querying active agents from the
                // simulation thread, this is just annoying.
                return;
            }

            var correctCellIndex = GetCellIndex(agent.Position);
            if (agent.CellIndex != correctCellIndex)
            {
                throw new System.Exception("Bug");
            }

            var cell = _state.Grid[agent.CellIndex];
            if (!cell.Contains(agent.Index))
            {
                throw new System.Exception("Bug");
            }
        }

        private void QueryCell(Vector3 pos, int cellX, int cellY, int excludeIndex, float maxDist, FixedBufferList<Agent> res)
        {
            var grid = _state.Grid;

            var cellCountY = (int)System.Math.Ceiling(WorldSize.Y / CellSize);
            var cellIndex = cellX * cellCountY + cellY;
            if (cellIndex < 0 || cellIndex >= grid.Length)
            {
                return;
            }

            var cell = grid[cellIndex];
            var maxDistSqr = maxDist * maxDist;
            for (int i = 0; i < cell.Count; i++)
            {
                var idx = cell[i];
                var other = _state.Agents[idx];

                if (other.CurrentState != Agent.State.Wandering)
                    continue;

                if (other.Index == excludeIndex)
                    continue;

                var distance = Vector3.DistanceSqr(pos, other.Position);
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
            {
                // This path is only used by the viewer.
                res = new FixedBufferList<Agent>(1024);
            }

            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            // The grid uses 0, 0 as starting origin.
            float remapX = Math.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, WorldSize.X);
            float remapY = Math.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, WorldSize.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            // Calculate the number of cells to search in each direction based on maxDistance
            int cellRadius = (int)(maxDistance / CellSize) + 1;

            if (res.Full)
                return res;

            // Iterate over all cells in the bounding box defined by maxDistance
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

            var cellCountY = (int)System.Math.Ceiling(WorldSize.Y / CellSize);
            var cellIndex = cellX * cellCountY + cellY;
            if (cellIndex < 0 || cellIndex >= grid.Length)
            {
                return count;
            }

            var cell = grid[cellIndex];
            var maxDistSqr = maxDist * maxDist;
            for (int i = 0; i < cell.Count; i++)
            {
                var idx = cell[i];
                var other = _state.Agents[idx];

                if (other.CurrentState != Agent.State.Wandering)
                    continue;

                var distance = Vector3.DistanceSqr(pos, other.Position);
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

            // The grid uses 0, 0 as starting origin.
            float remapX = Math.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, WorldSize.X);
            float remapY = Math.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, WorldSize.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            // Calculate the number of cells to search in each direction based on maxDistance
            int cellRadius = (int)(maxDistance / CellSize) + 1;

            int count = 0;

            // Iterate over all cells in the bounding box defined by maxDistance
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
    }
}

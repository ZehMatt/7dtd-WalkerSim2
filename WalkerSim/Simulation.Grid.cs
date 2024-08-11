using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        const int CellSize = 96;

        List<int>[] grid;
        int CellCountX = 0;
        int CellCountY = 0;
        int TotalCells = 0;

        void SetupGrid()
        {
            CellCountX = (int)System.Math.Ceiling(WorldSize.X / CellSize);
            CellCountY = (int)System.Math.Ceiling(WorldSize.Y / CellSize);

            TotalCells = CellCountX * CellCountY;
            grid = new List<int>[TotalCells];
            for (int i = 0; i < TotalCells; i++)
            {
                grid[i] = new List<int>();
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

            return cellX * CellCountY + cellY;
        }

        int GetCellIndex(Vector3 pos)
        {
            return GetCellIndex(pos.X, pos.Y);
        }

        public void MoveInGrid(Agent agent)
        {
            var newCellIndex = GetCellIndex(agent.Position);
            if (newCellIndex != agent.CellIndex)
            {
                if (agent.CellIndex != -1)
                {
                    // Remove from old cell.
                    var oldCellList = grid[agent.CellIndex];
                    oldCellList.Remove(agent.Index);
                }

                if (newCellIndex < 0)
                    newCellIndex = 0;
                if (newCellIndex >= TotalCells)
                    newCellIndex = TotalCells - 1;

                // Add to new cell.
                var newCellList = grid[newCellIndex];
                newCellList.Add(agent.Index);

                agent.CellIndex = newCellIndex;
            }
        }

        private void QueryCell(Vector3 pos, int cellX, int cellY, int excludeIndex, float maxDist, List<Agent> res)
        {
            var cellIndex = cellX * CellCountY + cellY;
            if (cellIndex < 0 || cellIndex >= grid.Length)
                return;

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
                }
            }
        }

        public List<Agent> QueryNearby(Vector3 pos, int excludeIndex, float maxDistance, List<Agent> res = null)
        {
            if (res == null)
            {
                res = new List<Agent>();
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

            // Iterate over all cells in the bounding box defined by maxDistance
            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int y = -cellRadius; y <= cellRadius; y++)
                {
                    QueryCell(pos, cellX + x, cellY + y, excludeIndex, maxDistance, res);
                }
            }

            return res;
        }


    }
}

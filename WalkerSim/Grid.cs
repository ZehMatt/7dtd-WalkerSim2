using System.Collections.Generic;

namespace WalkerSim
{
    internal class GridObject
    {
        public Vector3 Position = Vector3.Zero;
        public int CellIndex = -1;
    }

    internal class Grid<T> where T : GridObject
    {
        private Vector3 Mins;
        private Vector3 Maxs;
        private Vector3 Size;
        private int CellSize;
        private int CellCountX;
        private int CellCountY;
        private List<T>[] Cells;

        public Grid(Vector3 mins, Vector3 maxs, int cellSize)
        {
            Mins = mins;
            Maxs = maxs;
            CellSize = cellSize;

            Size = maxs - mins;
            CellCountX = (int)((Size.X + CellSize - 1) / CellSize);
            CellCountY = (int)((Size.Y + CellSize - 1) / CellSize);

            var totalCells = CellCountX * CellCountY;
            Cells = new List<T>[totalCells];
        }

        private int GetCellIndex(Vector3 positon)
        {
            // The grid uses 0, 0 as starting origin.
            float remapX = Math.Remap(positon.X, Mins.X, Maxs.X, 0f, Size.X);
            float remapY = Math.Remap(positon.Y, Mins.Y, Maxs.Y, 0f, Size.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            return cellX * CellCountY + cellY;
        }

        public void AddObject(T obj)
        {
            GridObject gridObject = (GridObject)obj;
            gridObject.CellIndex = GetCellIndex(gridObject.Position);

            var cell = Cells[gridObject.CellIndex];
            cell.Add(obj);
        }

        public void UpdateObject(T obj)
        {
            GridObject gridObject = (GridObject)obj;

            if (gridObject.CellIndex != -1)
            {
                var oldCell = Cells[gridObject.CellIndex];
                oldCell.Remove(obj);
            }

            var newCellIndex = GetCellIndex(gridObject.Position);
            gridObject.CellIndex = newCellIndex;

            var newCell = Cells[newCellIndex];
            newCell.Add(obj);
        }

        public void RemoveObject(T obj)
        {
            GridObject gridObject = (GridObject)obj;

            if (gridObject.CellIndex != -1)
            {
                var oldCell = Cells[gridObject.CellIndex];
                oldCell.Remove(obj);
            }
        }

        private void QueryCell(Vector3 pos, int cellX, int cellY, int excludeIndex, float maxDistSqr, List<T> res)
        {
            var cellIndex = cellX * CellCountY + cellY;
            if (cellIndex < 0 || cellIndex >= Cells.Length)
            {
                return;
            }

            var cell = Cells[cellIndex];
            for (int i = 0; i < cell.Count; i++)
            {
                var other = cell[i];

                var distance = Vector3.DistanceSqr(pos, other.Position);
                if (distance < maxDistSqr)
                {
                    res.Add(other);
                }
            }
        }

        private List<T> QueryCells(Vector3 position, int excludeIndex, float maxDistance, List<T> res = null)
        {
            if (res == null)
            {
                res = new List<T>();
            }

            var maxDistSqr = maxDistance * maxDistance;

            // The grid uses 0, 0 as starting origin.
            float remapX = Math.Remap(position.X, Mins.X, Maxs.X, 0f, Size.X);
            float remapY = Math.Remap(position.Y, Mins.Y, Maxs.Y, 0f, Size.Y);

            int cellX = (int)(remapX / CellSize);
            int cellY = (int)(remapY / CellSize);

            // Calculate the number of cells to search in each direction based on maxDistance
            int cellRadius = (int)(maxDistance / CellSize) + 1;

            // Iterate over all cells in the bounding box defined by maxDistance
            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int y = -cellRadius; y <= cellRadius; y++)
                {
                    QueryCell(position, cellX + x, cellY + y, excludeIndex, maxDistSqr, res);
                }
            }

            return res;
        }
    }
}

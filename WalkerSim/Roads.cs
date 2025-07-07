using System.Collections.Generic;
using System.Threading.Tasks;

namespace WalkerSim
{
    public enum RoadType : byte
    {
        None = 0,
        Asphalt,
        Offroad,
    }

    public class Roads
    {
        // Downscaled to 768x768
        const int ScaledWidth = 768;
        const int ScaledHeight = 768;

        public const int CellSize = 32;

        RoadType[,] _data = new RoadType[0, 0];
        int _width = 0;
        int _height = 0;
        string _name = string.Empty;

        public string Name
        {
            get => _name;
        }

        public struct RoadPoint
        {
            public static RoadPoint Invalid = new RoadPoint()
            {
                Type = RoadType.None,
                X = -1,
                Y = -1,
            };

            public RoadType Type;
            public int X;
            public int Y;
        }

        public class Cell
        {
            public List<RoadPoint> Points;
        }

        Cell[] _roadGrid = new Cell[0];
        int _columnCount = 0;
        int _rowCount = 0;

        public int Width
        {
            get => _width;
        }

        public int Height
        {
            get => _height;
        }

        public int ColumnCount
        {
            get => _columnCount;
        }

        public int RowCount
        {
            get => _rowCount;
        }

        public static Roads LoadFromFile(string splatPath)
        {
            using (var img = WalkerSim.Drawing.LoadFromFile(splatPath))
            {
                img.RemoveTransparency();

                return Roads.LoadFromBitmap(img, splatPath);
            }
        }

        private static Roads LoadFromBitmap(Drawing.IBitmap img, string name)
        {
            // Resize the image to 712x712
            var scaled = Drawing.Create(img, ScaledWidth, ScaledHeight);

            var height = scaled.Height;
            var width = scaled.Width;

            var data = new RoadType[width, height];

            scaled.LockPixels();
            Parallel.For(0, height * width, index =>
            {
                int y = index / width;
                int x = index % width;
                var pixel = scaled.GetPixel(x, y);
                if (pixel.R != 0)
                {
                    data[x, y] = RoadType.Asphalt;
                }
                else if (pixel.G != 0)
                {
                    data[x, y] = RoadType.Offroad;
                }
            });
            scaled.UnlockPixels();

            var res = new Roads();
            res._name = name;
            res._width = width;
            res._height = height;
            res._data = data;

            // Create the grid.
            res._columnCount = (int)MathEx.Ceiling((float)width / CellSize);
            res._rowCount = (int)MathEx.Ceiling((float)height / CellSize);
            res._roadGrid = new Cell[res._columnCount * res._rowCount];
            for (int i = 0; i < res._roadGrid.Length; i++)
            {
                res._roadGrid[i] = new Cell()
                {
                    Points = new List<RoadPoint>(),
                };
            }

            // Extract all the data and store it in the cells.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var roadType = data[x, y];
                    if (roadType == RoadType.None)
                        continue;

                    var cellX = x / CellSize;
                    var cellY = y / CellSize;
                    var cellIndex = cellY * res._columnCount + cellX;
                    var cell = res._roadGrid[cellIndex];

                    var point = new RoadPoint()
                    {
                        Type = roadType,
                        X = x,
                        Y = y,
                    };
                    cell.Points.Add(point);
                }
            }

            // Minimize the amount of points, we can delete every second point.
            for (int i = 0; i < res._roadGrid.Length; i++)
            {
                var cell = res._roadGrid[i];
                if (cell.Points.Count == 0)
                {
                    continue;
                }

                var newPoints = new List<RoadPoint>();
                for (int j = 0; j < cell.Points.Count; j++)
                {
                    if (j % 2 == 0)
                    {
                        newPoints.Add(cell.Points[j]);
                    }
                }

                cell.Points = newPoints;
            }

            return res;
        }

        // Gets the pixel information for the given bitmap coordinates in the scaled image.
        public RoadType GetRoadType(int x, int y)
        {
            int index = y * _width + x;
            if (index < 0 || index >= _data.Length)
                return RoadType.None;

            return _data[x, y];
        }


        public Cell GetCell(int cellX, int cellY)
        {
            if (cellX < 0 || cellX >= _columnCount || cellY < 0 || cellY >= _rowCount)
            {
                return null;
            }

            var index = cellY * _columnCount + cellX;
            return _roadGrid[index];
        }

        // Get the closest road point to the given bitmap coordinates, the search distance is limited to
        // 3x3 grid of cells.
        public RoadPoint GetClosestRoad(int x, int y, int dirX, int dirY)
        {
            var bestChoice = RoadPoint.Invalid;
            var closest = RoadPoint.Invalid;

            var cellX = x / CellSize;
            var cellY = y / CellSize;
            float closestDist = float.MaxValue;
            // Define a view cone angle (e.g., 90 degrees total, 45 degrees each side)
            const float viewConeCosThreshold = 0.7071f; // cos(45°)

            // Normalize direction vector
            float dirLength = (float)System.Math.Sqrt(dirX * dirX + dirY * dirY);
            float normDirX = dirLength > 0 ? dirX / dirLength : 0;
            float normDirY = dirLength > 0 ? dirY / dirLength : 0;

            // Iterate through neighboring cells (3x3 grid)
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    var neighborCellX = cellX + offsetX;
                    var neighborCellY = cellY + offsetY;

                    // Ensure the neighboring cell is within grid bounds
                    if (neighborCellX >= 0 && neighborCellX < _columnCount &&
                        neighborCellY >= 0 && neighborCellY < _rowCount)
                    {
                        var cell = GetCell(neighborCellX, neighborCellY);

                        // Check each point in the neighboring cell
                        for (int i = 0; i < cell.Points.Count; i++)
                        {
                            var point = cell.Points[i];
                            if (point.X == x && point.Y == y)
                            {
                                continue;
                            }

                            // Vector from current position to road point
                            float toPointX = point.X - x;
                            float toPointY = point.Y - y;
                            float toPointLength = (float)System.Math.Sqrt(toPointX * toPointX + toPointY * toPointY);
                            if (toPointLength == 0)
                                continue;

                            var dist = toPointLength * toPointLength; // Use squared distance
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closest = point;

                                // Normalize vector to road point
                                float normToPointX = toPointX / toPointLength;
                                float normToPointY = toPointY / toPointLength;

                                // Calculate dot product to check if point is within view cone
                                float dot = normDirX * normToPointX + normDirY * normToPointY;
                                if (dot >= viewConeCosThreshold) // Point is within view cone
                                {
                                    bestChoice = point;
                                }
                            }

                        }
                    }
                }
            }

            if (bestChoice.Type != RoadType.None)
            {
                // If we found a point within the view cone, return it
                return bestChoice;
            }

            return closest;
        }
    }
}

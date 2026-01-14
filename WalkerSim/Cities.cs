using System;
using System.Collections.Generic;
using System.Linq;

namespace WalkerSim
{
    public class Cities
    {
        public class City
        {
            public Vector3 Position { get; set; }
            public Vector3 Bounds { get; set; }
            public List<MapData.Decoration> POIs { get; set; }

            public City()
            {
                POIs = new List<MapData.Decoration>();
            }

            public float MinX => Position.X - Bounds.X / 2;
            public float MaxX => Position.X + Bounds.X / 2;
            public float MinY => Position.Y - Bounds.Y / 2;
            public float MaxY => Position.Y + Bounds.Y / 2;
        }

        public List<City> CityList { get; private set; }
        public float[] CityAreaWeights { get; private set; }
        public float TotalAreaWeight { get; private set; }

        public Cities()
        {
            CityList = new List<City>();
            CityAreaWeights = new float[0];
            TotalAreaWeight = 0;
        }

        /// <summary>
        /// Generates city boundaries from POI clusters using a density-based clustering algorithm (DBSCAN-inspired).
        /// Only areas with high POI density are considered cities.
        /// </summary>
        /// <param name="pois">Array of POI decorations to cluster</param>
        /// <param name="clusterDistance">Maximum edge-to-edge distance to consider POIs as neighbors</param>
        /// <param name="minNeighbors">Minimum number of neighbors required for a POI to be considered a core point (density threshold)</param>
        /// <param name="minPOIsPerCity">Minimum number of POIs required to form a city</param>
        public static Cities GenerateFromPOIs(MapData.Decoration[] pois, float clusterDistance = 20f, int minNeighbors = 4, int minPOIsPerCity = 12)
        {
            var cities = new Cities();

            if (pois == null || pois.Length == 0)
            {
                return cities;
            }

            var poiList = new List<MapData.Decoration>(pois);

            // Build neighbor lists for each POI
            var neighbors = new Dictionary<int, List<int>>();
            for (int i = 0; i < poiList.Count; i++)
            {
                neighbors[i] = new List<int>();
            }

            // Find neighbors for each POI
            for (int i = 0; i < poiList.Count; i++)
            {
                for (int j = i + 1; j < poiList.Count; j++)
                {
                    float distance = CalculateEdgeDistance(poiList[i], poiList[j]);
                    if (distance <= clusterDistance)
                    {
                        neighbors[i].Add(j);
                        neighbors[j].Add(i);
                    }
                }
            }

            // Identify core points (POIs with enough neighbors = high density areas)
            var corePoints = new HashSet<int>();
            for (int i = 0; i < poiList.Count; i++)
            {
                if (neighbors[i].Count >= minNeighbors)
                {
                    corePoints.Add(i);
                }
            }

            // DBSCAN-like clustering: only expand from core points
            var visited = new HashSet<int>();
            var clustered = new HashSet<int>();
            var clusters = new List<List<MapData.Decoration>>();

            foreach (int corePoint in corePoints)
            {
                if (visited.Contains(corePoint))
                    continue;

                // Start new cluster from this core point
                var cluster = new List<MapData.Decoration>();
                var queue = new Queue<int>();
                queue.Enqueue(corePoint);
                visited.Add(corePoint);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    cluster.Add(poiList[current]);
                    clustered.Add(current);

                    // Only expand through core points to maintain density
                    if (corePoints.Contains(current))
                    {
                        foreach (int neighbor in neighbors[current])
                        {
                            if (!visited.Contains(neighbor))
                            {
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }

                // Only add clusters that meet minimum size
                if (cluster.Count >= minPOIsPerCity)
                {
                    clusters.Add(cluster);
                }
            }

            // Convert clusters to cities with bounding boxes, filtering out sparse clusters
            foreach (var cluster in clusters)
            {
                // Calculate bounding box that encompasses all POIs in the cluster
                float minX = float.MaxValue;
                float maxX = float.MinValue;
                float minY = float.MaxValue;
                float maxY = float.MinValue;

                foreach (var poi in cluster)
                {
                    float poiMinX = poi.Position.X - poi.Bounds.X / 2;
                    float poiMaxX = poi.Position.X + poi.Bounds.X / 2;
                    float poiMinY = poi.Position.Y - poi.Bounds.Y / 2;
                    float poiMaxY = poi.Position.Y + poi.Bounds.Y / 2;

                    minX = Math.Min(minX, poiMinX);
                    maxX = Math.Max(maxX, poiMaxX);
                    minY = Math.Min(minY, poiMinY);
                    maxY = Math.Max(maxY, poiMaxY);
                }

                // Calculate cluster area and density
                float width = maxX - minX;
                float height = maxY - minY;
                float area = width * height;

                // Calculate total POI area
                float totalPoiArea = 0;
                foreach (var poi in cluster)
                {
                    totalPoiArea += poi.Bounds.X * poi.Bounds.Y;
                }

                // Density check: POIs should occupy at least 20% of the bounding box area
                // This filters out clusters that span large areas with lots of empty space
                float density = totalPoiArea / area;
                const float minDensity = 0.20f;

                if (density < minDensity)
                {
                    // Skip this cluster - it's too sparse (too much empty space)
                    continue;
                }

                // Aspect ratio check: cities should be relatively square, not elongated like roads
                // A road would have a very high aspect ratio (e.g., 10:1), while a city should be more compact
                float aspectRatio = Math.Max(width, height) / Math.Min(width, height);
                const float maxAspectRatio = 3.0f; // Allow up to 3:1 ratio

                if (aspectRatio > maxAspectRatio)
                {
                    // Skip this cluster - it's too elongated (likely a road)
                    continue;
                }

                // Subdivide this cluster recursively to eliminate empty space
                var subdividedCities = RecursivelySubdivideCluster(cluster, minX, maxX, minY, maxY, minPOIsPerCity);
                cities.CityList.AddRange(subdividedCities);
            }

            // Merge overlapping cities into single cities
            cities.MergeOverlappingCities();

            var poisInCities = cities.CityList.Sum(c => c.POIs.Count);
            Logging.Info("Generated {0} cities from {1} POIs ({2} POIs in cities, {3} isolated/low-density POIs).",
                cities.CityList.Count,
                pois.Length,
                poisInCities,
                pois.Length - poisInCities);

            // Precompute city area weights for efficient weighted selection
            cities.ComputeAreaWeights();

            return cities;
        }

        /// <summary>
        /// Recursively subdivides a cluster of POIs to minimize empty space in bounding boxes.
        /// Uses a quadtree-like approach to create tight-fitting city boundaries.
        /// </summary>
        private static List<City> RecursivelySubdivideCluster(List<MapData.Decoration> pois, float minX, float maxX, float minY, float maxY, int minPOIsPerCity)
        {
            var result = new List<City>();

            if (pois.Count < minPOIsPerCity)
            {
                return result; // Not enough POIs to form a city
            }

            // Calculate area coverage
            float boxWidth = maxX - minX;
            float boxHeight = maxY - minY;
            float boxArea = boxWidth * boxHeight;

            float totalPoiArea = 0;
            foreach (var poi in pois)
            {
                totalPoiArea += poi.Bounds.X * poi.Bounds.Y;
            }

            float density = totalPoiArea / boxArea;

            // If density is good (>40%), create a city from this box
            const float targetDensity = 0.40f;

            // Also check if box is small enough (don't subdivide tiny boxes)
            const float minBoxSize = 150f; // Don't subdivide below 150x150
            bool boxTooSmall = boxWidth < minBoxSize || boxHeight < minBoxSize;

            // Try subdividing first to see if we can split into multiple cities
            if (!boxTooSmall)
            {
                var subdivisionAttempt = TrySubdivide(pois, minX, maxX, minY, maxY, minPOIsPerCity);

                // If subdivision produced multiple valid cities, use them instead
                if (subdivisionAttempt.Count > 1)
                {
                    return subdivisionAttempt;
                }
            }

            if (density >= targetDensity || boxTooSmall)
            {
                // This box has good density or is too small to subdivide further
                // Calculate tight bounding box around actual POIs
                float tightMinX = float.MaxValue;
                float tightMaxX = float.MinValue;
                float tightMinY = float.MaxValue;
                float tightMaxY = float.MinValue;

                foreach (var poi in pois)
                {
                    float poiMinX = poi.Position.X - poi.Bounds.X / 2;
                    float poiMaxX = poi.Position.X + poi.Bounds.X / 2;
                    float poiMinY = poi.Position.Y - poi.Bounds.Y / 2;
                    float poiMaxY = poi.Position.Y + poi.Bounds.Y / 2;

                    tightMinX = Math.Min(tightMinX, poiMinX);
                    tightMaxX = Math.Max(tightMaxX, poiMaxX);
                    tightMinY = Math.Min(tightMinY, poiMinY);
                    tightMaxY = Math.Max(tightMaxY, poiMaxY);
                }

                // Apply small shrinkage to prevent overlaps between adjacent cities
                const float shrinkage = 1f; // 1 meter shrinkage on each edge
                tightMinX += shrinkage;
                tightMaxX -= shrinkage;
                tightMinY += shrinkage;
                tightMaxY -= shrinkage;

                var city = new City();
                city.POIs.AddRange(pois);
                city.Position = new Vector3((tightMinX + tightMaxX) / 2, (tightMinY + tightMaxY) / 2, 0);
                city.Bounds = new Vector3(tightMaxX - tightMinX, tightMaxY - tightMinY, 0);
                result.Add(city);
                return result;
            }

            // Density too low - subdivide into 4 quadrants
            return TrySubdivide(pois, minX, maxX, minY, maxY, minPOIsPerCity);
        }

        /// <summary>
        /// Attempts to subdivide a box into 4 quadrants and recursively process each.
        /// </summary>
        private static List<City> TrySubdivide(List<MapData.Decoration> pois, float minX, float maxX, float minY, float maxY, int minPOIsPerCity)
        {
            var result = new List<City>();

            float centerX = (minX + maxX) / 2;
            float centerY = (minY + maxY) / 2;

            var quadrants = new[]
            {
                new { MinX = centerX, MaxX = maxX, MinY = centerY, MaxY = maxY },     // Top-right
                new { MinX = minX, MaxX = centerX, MinY = centerY, MaxY = maxY },     // Top-left
                new { MinX = minX, MaxX = centerX, MinY = minY, MaxY = centerY },     // Bottom-left
                new { MinX = centerX, MaxX = maxX, MinY = minY, MaxY = centerY }      // Bottom-right
            };

            foreach (var quad in quadrants)
            {
                var poisInQuadrant = new List<MapData.Decoration>();
                foreach (var poi in pois)
                {
                    // Check if POI center is in this quadrant
                    if (poi.Position.X >= quad.MinX && poi.Position.X < quad.MaxX &&
                        poi.Position.Y >= quad.MinY && poi.Position.Y < quad.MaxY)
                    {
                        poisInQuadrant.Add(poi);
                    }
                }

                if (poisInQuadrant.Count >= minPOIsPerCity)
                {
                    // Recursively subdivide this quadrant
                    var subCities = RecursivelySubdivideCluster(poisInQuadrant, quad.MinX, quad.MaxX, quad.MinY, quad.MaxY, minPOIsPerCity);
                    result.AddRange(subCities);
                }
            }

            // Try to merge adjacent cities that should be combined
            if (result.Count > 1)
            {
                result = TryMergeCities(result, minPOIsPerCity);
            }

            return result;
        }

        /// <summary>
        /// Attempts to merge adjacent cities that should be combined based on density and shape criteria.
        /// </summary>
        private static List<City> TryMergeCities(List<City> cities, int minPOIsPerCity)
        {
            bool merged = true;
            int iterations = 0;
            const int maxIterations = 5;

            while (merged && iterations < maxIterations)
            {
                merged = false;
                iterations++;

                for (int i = 0; i < cities.Count; i++)
                {
                    for (int j = i + 1; j < cities.Count; j++)
                    {
                        // Check if cities are adjacent (within 30m)
                        float distance = CalculateCityDistance(cities[i], cities[j]);
                        if (distance > 30f)
                            continue;

                        // Try merging these two cities
                        var mergedCity = TryMergeTwoCities(cities[i], cities[j]);
                        if (mergedCity != null)
                        {
                            // Remove the two cities and add the merged one
                            var newCities = new List<City>();
                            for (int k = 0; k < cities.Count; k++)
                            {
                                if (k != i && k != j)
                                    newCities.Add(cities[k]);
                            }
                            newCities.Add(mergedCity);
                            cities = newCities;
                            merged = true;
                            break;
                        }
                    }

                    if (merged)
                        break;
                }
            }

            return cities;
        }

        /// <summary>
        /// Calculates the minimum distance between two city bounding boxes.
        /// </summary>
        private static float CalculateCityDistance(City city1, City city2)
        {
            float dx = 0;
            if (city1.MaxX < city2.MinX)
                dx = city2.MinX - city1.MaxX;
            else if (city2.MaxX < city1.MinX)
                dx = city1.MinX - city2.MaxX;

            float dy = 0;
            if (city1.MaxY < city2.MinY)
                dy = city2.MinY - city1.MaxY;
            else if (city2.MaxY < city1.MinY)
                dy = city1.MinY - city2.MaxY;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Attempts to merge two cities. Returns the merged city if valid, null otherwise.
        /// </summary>
        private static City TryMergeTwoCities(City city1, City city2)
        {
            // Critical check: Only merge if cities are very close (within 5m)
            // This prevents merging cities with gaps between them
            float distance = CalculateCityDistance(city1, city2);
            if (distance > 5f)
                return null; // Too far apart - would create empty space

            // Combine POIs
            var combinedPOIs = new List<MapData.Decoration>();
            combinedPOIs.AddRange(city1.POIs);
            combinedPOIs.AddRange(city2.POIs);

            // Calculate tight bounding box around actual POIs
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var poi in combinedPOIs)
            {
                float poiMinX = poi.Position.X - poi.Bounds.X / 2;
                float poiMaxX = poi.Position.X + poi.Bounds.X / 2;
                float poiMinY = poi.Position.Y - poi.Bounds.Y / 2;
                float poiMaxY = poi.Position.Y + poi.Bounds.Y / 2;

                minX = Math.Min(minX, poiMinX);
                maxX = Math.Max(maxX, poiMaxX);
                minY = Math.Min(minY, poiMinY);
                maxY = Math.Max(maxY, poiMaxY);
            }

            float mergedWidth = maxX - minX;
            float mergedHeight = maxY - minY;
            float mergedArea = mergedWidth * mergedHeight;

            // Check if merged box is significantly larger than sum of individual boxes
            // If it is, there's empty space between them
            float city1Area = city1.Bounds.X * city1.Bounds.Y;
            float city2Area = city2.Bounds.X * city2.Bounds.Y;
            float combinedOriginalArea = city1Area + city2Area;

            // If merged area is more than 1.3x the sum of original areas, there's too much gap
            if (mergedArea > combinedOriginalArea * 1.3f)
                return null; // Would create too much empty space

            // Calculate density of merged box
            float totalPoiArea = 0;
            foreach (var poi in combinedPOIs)
            {
                totalPoiArea += poi.Bounds.X * poi.Bounds.Y;
            }
            float density = totalPoiArea / mergedArea;

            // Density check
            const float targetDensity = 0.45f;
            if (density < targetDensity)
                return null;

            // Aspect ratio check
            float aspectRatio = Math.Max(mergedWidth, mergedHeight) / Math.Min(mergedWidth, mergedHeight);
            const float maxAspectRatio = 2.5f;
            if (aspectRatio > maxAspectRatio)
                return null;

            // Apply shrinkage to prevent overlaps
            const float shrinkage = 1f;
            minX += shrinkage;
            maxX -= shrinkage;
            minY += shrinkage;
            maxY -= shrinkage;

            // Create merged city
            var mergedCity = new City();
            mergedCity.POIs.AddRange(combinedPOIs);
            mergedCity.Position = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
            mergedCity.Bounds = new Vector3(maxX - minX, maxY - minY, 0);

            return mergedCity;
        }

        /// <summary>
        /// Checks if two city bounding boxes overlap.
        /// </summary>
        private static bool DoBoundingBoxesOverlap(City city1, City city2)
        {
            return !(city1.MaxX < city2.MinX || city1.MinX > city2.MaxX ||
                     city1.MaxY < city2.MinY || city1.MinY > city2.MaxY);
        }

        /// <summary>
        /// Calculates the edge-to-edge distance between two POI bounding boxes.
        /// Returns 0 if they overlap or touch.
        /// </summary>
        private static float CalculateEdgeDistance(MapData.Decoration poi1, MapData.Decoration poi2)
        {
            // Calculate the extent of each POI
            float poi1MinX = poi1.Position.X - poi1.Bounds.X / 2;
            float poi1MaxX = poi1.Position.X + poi1.Bounds.X / 2;
            float poi1MinY = poi1.Position.Y - poi1.Bounds.Y / 2;
            float poi1MaxY = poi1.Position.Y + poi1.Bounds.Y / 2;

            float poi2MinX = poi2.Position.X - poi2.Bounds.X / 2;
            float poi2MaxX = poi2.Position.X + poi2.Bounds.X / 2;
            float poi2MinY = poi2.Position.Y - poi2.Bounds.Y / 2;
            float poi2MaxY = poi2.Position.Y + poi2.Bounds.Y / 2;

            // Calculate the horizontal gap
            float dx = 0;
            if (poi1MaxX < poi2MinX)
                dx = poi2MinX - poi1MaxX;
            else if (poi2MaxX < poi1MinX)
                dx = poi1MinX - poi2MaxX;

            // Calculate the vertical gap
            float dy = 0;
            if (poi1MaxY < poi2MinY)
                dy = poi2MinY - poi1MaxY;
            else if (poi2MaxY < poi1MinY)
                dy = poi1MinY - poi2MaxY;

            // Return the Euclidean distance between the edges
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Gets the city at the specified world position, or null if not in any city.
        /// </summary>
        public City GetCityAt(Vector3 position)
        {
            foreach (var city in CityList)
            {
                if (position.X >= city.MinX && position.X <= city.MaxX &&
                    position.Y >= city.MinY && position.Y <= city.MaxY)
                {
                    return city;
                }
            }
            return null;
        }

        /// <summary>
        /// Resolves overlapping and adjacent cities. Merges cities that are close together or have significant overlap.
        /// For small overlaps, shrinks the city with fewer POIs in the overlap area.
        /// Should be called after all cities are generated but before weights are computed.
        /// </summary>
        public void MergeOverlappingCities()
        {
            bool modified;
            int iterations = 0;
            const int maxIterations = 100; // Prevent infinite loops
            const float significantOverlapThreshold = 0.3f; // Merge if overlap is 30% or more of smaller city

            do
            {
                modified = false;
                iterations++;

                for (int i = 0; i < CityList.Count; i++)
                {
                    for (int j = i + 1; j < CityList.Count; j++)
                    {
                        var cityA = CityList[i];
                        var cityB = CityList[j];

                        // Calculate overlap rectangle
                        float overlapMinX = Math.Max(cityA.MinX, cityB.MinX);
                        float overlapMaxX = Math.Min(cityA.MaxX, cityB.MaxX);
                        float overlapMinY = Math.Max(cityA.MinY, cityB.MinY);
                        float overlapMaxY = Math.Min(cityA.MaxY, cityB.MaxY);

                        // Check if there's any overlap
                        if (overlapMinX < overlapMaxX && overlapMinY < overlapMaxY)
                        {
                            // Calculate overlap size relative to each city
                            float overlapWidth = overlapMaxX - overlapMinX;
                            float overlapHeight = overlapMaxY - overlapMinY;
                            float overlapArea = overlapWidth * overlapHeight;

                            float areaA = cityA.Bounds.X * cityA.Bounds.Y;
                            float areaB = cityB.Bounds.X * cityB.Bounds.Y;
                            float smallerArea = Math.Min(areaA, areaB);
                            float overlapPercentage = overlapArea / smallerArea;

                            // If overlap is significant, merge the cities
                            if (overlapPercentage >= significantOverlapThreshold)
                            {
                                // Merge into one larger city
                                float mergedMinX = Math.Min(cityA.MinX, cityB.MinX);
                                float mergedMaxX = Math.Max(cityA.MaxX, cityB.MaxX);
                                float mergedMinY = Math.Min(cityA.MinY, cityB.MinY);
                                float mergedMaxY = Math.Max(cityA.MaxY, cityB.MaxY);

                                var mergedCity = new City();
                                mergedCity.Bounds = new Vector3(mergedMaxX - mergedMinX, mergedMaxY - mergedMinY, 0);
                                mergedCity.Position = new Vector3(
                                    (mergedMinX + mergedMaxX) / 2,
                                    (mergedMinY + mergedMaxY) / 2,
                                    0);

                                // Combine POIs from both cities
                                mergedCity.POIs.AddRange(cityA.POIs);
                                mergedCity.POIs.AddRange(cityB.POIs);

                                // Remove the two original cities and add the merged one
                                CityList.RemoveAt(j);
                                CityList.RemoveAt(i);
                                CityList.Add(mergedCity);

                                modified = true;
                                break;
                            }
                            // For smaller overlaps, leave them as is - they'll be handled naturally by agent behavior
                        }
                    }

                    if (modified)
                        break;
                }
            } while (modified && iterations < maxIterations);

            Logging.Info("Resolved city overlaps in {0} iterations, final count: {1}", iterations, CityList.Count);
        }

        public void ComputeAreaWeights()
        {
            int count = CityList.Count;
            CityAreaWeights = new float[count];
            TotalAreaWeight = 0;

            for (int i = 0; i < count; i++)
            {
                var city = CityList[i];
                float area = city.Bounds.X * city.Bounds.Y;
                CityAreaWeights[i] = area;
                TotalAreaWeight += area;
            }
        }
    }
}

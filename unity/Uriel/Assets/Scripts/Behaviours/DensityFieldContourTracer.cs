using Uriel.Domain;

namespace Uriel.Behaviours
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEngine;

    public class DensityFieldContourTracer : MonoBehaviour
    {
        [SerializeField] private Vector3Int steps = new Vector3Int(1, 1, 1);
        [SerializeField] private Lumen lumen;
        [Header("Density Field Settings")]
        [SerializeField] private float threshold = 0.5f;
        [SerializeField] private Vector3 boundMin = new Vector3(-10f, 0f, -10f);
        [SerializeField] private Vector3 boundMax = new Vector3(10f, 0f, 10f);
        [SerializeField] private float sampleResolution = 0.1f;
        
        [Header("Path Generation Settings")]
        [SerializeField] private bool smoothPath = true;
        [SerializeField] private int smoothingIterations = 2;
        [SerializeField] private float pathSimplificationTolerance = 0.01f;
        
        // References to other components
        [SerializeField] private GCodeGenerator gCodeGenerator;
  
        
        // Internal data structures
        private float[,] densityGrid;
        private int gridWidth, gridHeight;
        private List<Vector2> contourPoints;

        [SerializeField] private bool verboseLogging = true;
        [SerializeField] private float connectionThreshold = 0.5f; // Distance threshold for connecting segments

        // Store debug info
        private List<List<Vector2>> allSegments = new List<List<Vector2>>();
        private List<int> segmentLengths = new List<int>();
            
        // Function to sample your density field
        // Replace this with your actual density field sampling function
        private float SampleDensityField(Vector3 position)
        {
            float density = 0f;
            for (int y = - steps.y; y <= steps.y; y++)
            {
                for (int x = -steps.x; x <= steps.x; x++)
                {
                    for (int z = -steps.z; z <= steps.z; z++)
                    {
                        for (int i = 0; i < lumen.photons.Count; i++)
                        {
                            Photon photon = lumen.photons[i];
                            Vector3 offset = new Vector3(x, y, z) * photon.density;
                            float dist = Mathf.Clamp01(Vector3.Distance(position * photon.density, offset)) * photon.phase;

                            density += Mathf.Sin(dist * photon.frequency) * photon.amplitude;
                        }
                    }
                }
            }

            return density;
        }
        
        private void Start()
        {
            GenerateCompoundPath(new float[] { threshold });
        }
        
        
        private void SampleDensityGrid()
        {
            // Calculate grid dimensions based on bounds and resolution
            gridWidth = Mathf.CeilToInt((boundMax.x - boundMin.x) / sampleResolution) + 1;
            gridHeight = Mathf.CeilToInt((boundMax.z - boundMin.z) / sampleResolution) + 1;
            
            // Initialize density grid
            densityGrid = new float[gridWidth, gridHeight];
            
            // Sample the density field at each grid point
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    // Convert grid coordinates to world space
                    Vector3 samplePos = new Vector3(
                        boundMin.x + x * sampleResolution,
                        boundMin.y + y * sampleResolution,  // Fixed Y (assuming 2D contour on XZ plane)
                        0
                    );
                    
                    // Sample density and store in grid
                    densityGrid[x, y] = SampleDensityField(samplePos);
                }
            }
            
            Debug.Log($"Sampled density grid: {gridWidth}x{gridHeight}");
        }
      
         public void GenerateContourPaths()
    {
        // Sample the density grid
        SampleDensityGrid();
        
        // Trace and debug the contours with verbose logging
        TraceAndDebugContours();
        
        // Process the path only if we have enough points
        if (contourPoints.Count > 4)
        {
            if (smoothPath)
            {
                SmoothPath();
                SimplifyPath();
            }
            
            // Generate G-code
            GenerateGCodeFromPath();
        }
        else
        {
            Debug.LogWarning($"Not enough points to generate a valid path. Only found {contourPoints.Count} points.");
        }
    }
    
    private void TraceAndDebugContours()
    {
        // First pass: Extract all contour crossing segments
        ExtractContourSegments();
        
        // Debug log for all segments
        if (verboseLogging)
        {
            Debug.Log($"Found {allSegments.Count} raw segments:");
            for (int i = 0; i < allSegments.Count; i++)
            {
                Debug.Log($"  Segment {i}: {allSegments[i].Count} points");
                segmentLengths.Add(allSegments[i].Count);
            }
        }
        
        // Second pass: Connect segments into continuous paths
        ConnectSegmentsWithLogging();
    }
    
    private void ExtractContourSegments()
    {
        allSegments.Clear();
        
        // Create edge crossing map (true if edge has a contour crossing)
        bool[,] hasContourCrossing = new bool[gridWidth, gridHeight * 4];
        
        for (int x = 0; x < gridWidth - 1; x++)
        {
            for (int y = 0; y < gridHeight - 1; y++)
            {
                // For each cell, check which edges have contour crossings
                for (int edge = 0; edge < 4; edge++)
                {
                    // Get values for this edge
                    float[] values = GetEdgeValues(x, y, edge);
                    
                    // Check if contour crosses this edge
                    bool crosses = (values[0] < threshold && values[1] >= threshold) || 
                                   (values[0] >= threshold && values[1] < threshold);
                                   
                    hasContourCrossing[x, y * 4 + edge] = crosses;
                    
                    if (crosses)
                    {
                        // Calculate crossing point and create a minimal segment
                        Vector2 point = CalculateCrossingPoint(x, y, edge, values[0], values[1]);
                        
                        // Create a new segment with just this crossing point
                        // (we'll connect these into longer segments later)
                        List<Vector2> segment = new List<Vector2>() { point };
                        allSegments.Add(segment);
                    }
                }
            }
        }
        
        if (verboseLogging)
        {
            Debug.Log($"Found {allSegments.Count} edge crossings in the grid");
        }
    }
    
    private float[] GetEdgeValues(int x, int y, int edge)
    {
        float[] values = new float[2];
        
        switch (edge)
        {
            case 0: // Bottom edge
                values[0] = densityGrid[x, y];      // bottom-left
                values[1] = densityGrid[x+1, y];    // bottom-right
                break;
            case 1: // Right edge
                values[0] = densityGrid[x+1, y];    // bottom-right
                values[1] = densityGrid[x+1, y+1];  // top-right
                break;
            case 2: // Top edge
                values[0] = densityGrid[x+1, y+1];  // top-right
                values[1] = densityGrid[x, y+1];    // top-left
                break;
            case 3: // Left edge
                values[0] = densityGrid[x, y+1];    // top-left
                values[1] = densityGrid[x, y];      // bottom-left
                break;
        }
        
        return values;
    }
    
    private Vector2 CalculateCrossingPoint(int x, int y, int edge, float value1, float value2)
    {
        Vector2 cellMin = new Vector2(
            boundMin.x + x * sampleResolution,
            boundMin.z + y * sampleResolution
        );
        
        Vector2 pos1, pos2;
        switch (edge)
        {
            case 0: // Bottom edge
                pos1 = cellMin;
                pos2 = cellMin + new Vector2(sampleResolution, 0);
                break;
            case 1: // Right edge
                pos1 = cellMin + new Vector2(sampleResolution, 0);
                pos2 = cellMin + new Vector2(sampleResolution, sampleResolution);
                break;
            case 2: // Top edge
                pos1 = cellMin + new Vector2(sampleResolution, sampleResolution);
                pos2 = cellMin + new Vector2(0, sampleResolution);
                break;
            case 3: // Left edge
                pos1 = cellMin + new Vector2(0, sampleResolution);
                pos2 = cellMin;
                break;
            default:
                return Vector2.zero;
        }
        
        // Calculate interpolation factor
        float t = (threshold - value1) / (value2 - value1);
        t = Mathf.Clamp01(t); // Ensure valid range
        
        // Interpolate to get the exact crossing point
        return Vector2.Lerp(pos1, pos2, t);
    }
    
    private void ConnectSegmentsWithLogging()
    {
        // Try to connect segments based on proximity
        List<List<Vector2>> connectedSegments = new List<List<Vector2>>();
        bool[] used = new bool[allSegments.Count];
        
        // Expanded connection threshold for more reliable connections
        float connectDist = connectionThreshold * sampleResolution;
        
        // First, connect segments that are close together
        for (int i = 0; i < allSegments.Count; i++)
        {
            if (used[i]) continue;
            
            List<Vector2> currentPath = new List<Vector2>(allSegments[i]);
            used[i] = true;
            
            bool foundConnection;
            do {
                foundConnection = false;
                
                // Find the best connection
                int bestSegment = -1;
                bool connectToStart = false;
                bool reverseSegment = false;
                float minDist = connectDist;
                
                // Current path endpoints
                Vector2 start = currentPath[0];
                Vector2 end = currentPath[currentPath.Count - 1];
                
                for (int j = 0; j < allSegments.Count; j++)
                {
                    if (used[j]) continue;
                    
                    Vector2 otherStart = allSegments[j][0];
                    Vector2 otherEnd = allSegments[j][allSegments[j].Count - 1];
                    
                    // Try all possible connections
                    float dist;
                    
                    // Connect current end to other start
                    dist = Vector2.Distance(end, otherStart);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestSegment = j;
                        connectToStart = false;
                        reverseSegment = false;
                    }
                    
                    // Connect current end to other end (reverse other)
                    dist = Vector2.Distance(end, otherEnd);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestSegment = j;
                        connectToStart = false;
                        reverseSegment = true;
                    }
                    
                    // Connect current start to other end
                    dist = Vector2.Distance(start, otherEnd);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestSegment = j;
                        connectToStart = true;
                        reverseSegment = false;
                    }
                    
                    // Connect current start to other start (reverse other)
                    dist = Vector2.Distance(start, otherStart);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestSegment = j;
                        connectToStart = true;
                        reverseSegment = true;
                    }
                }
                
                // If we found a good connection, join the segments
                if (bestSegment >= 0)
                {
                    List<Vector2> toAdd = new List<Vector2>(allSegments[bestSegment]);
                    if (reverseSegment)
                    {
                        toAdd.Reverse();
                    }
                    
                    // Join paths
                    if (connectToStart)
                    {
                        // Prepend to current path
                        toAdd.AddRange(currentPath);
                        currentPath = toAdd;
                    }
                    else
                    {
                        // Append to current path
                        currentPath.AddRange(toAdd);
                    }
                    
                    used[bestSegment] = true;
                    foundConnection = true;
                    
                    if (verboseLogging)
                    {
                        Debug.Log($"Connected segment {bestSegment} to path (now {currentPath.Count} points)");
                    }
                }
            } while (foundConnection);
            
            // Add the connected path
            if (currentPath.Count >= 2)
            {
                connectedSegments.Add(currentPath);
                if (verboseLogging)
                {
                    Debug.Log($"Added connected path with {currentPath.Count} points");
                }
            }
        }
        
        // Debug point counts
        if (verboseLogging)
        {
            Debug.Log($"Created {connectedSegments.Count} connected paths:");
            for (int i = 0; i < connectedSegments.Count; i++)
            {
                Debug.Log($"  Path {i}: {connectedSegments[i].Count} points");
            }
        }
        
        // Use the longest path as our contour
        if (connectedSegments.Count > 0)
        {
            connectedSegments.Sort((a, b) => b.Count.CompareTo(a.Count));
            contourPoints = connectedSegments[0];
            
            // Try to close the loop if the endpoints are close
            if (contourPoints.Count > 3)
            {
                Vector2 start = contourPoints[0];
                Vector2 end = contourPoints[contourPoints.Count - 1];
                
                if (Vector2.Distance(start, end) < connectDist)
                {
                    Debug.Log("Closing the loop - start and end points are close");
                    contourPoints.Add(start); // Close the loop
                }
            }
        }
        else
        {
            contourPoints = new List<Vector2>();
        }
        
        Debug.Log($"Final contour has {contourPoints.Count} points from {allSegments.Count} original segments");
    }
    
   
        
        private void SmoothPath()
        {
            if (contourPoints.Count < 3) return;
            
            List<Vector2> smoothedPoints = new List<Vector2>(contourPoints);
            
            for (int iteration = 0; iteration < smoothingIterations; iteration++)
            {
                List<Vector2> newPoints = new List<Vector2>(smoothedPoints.Count);
                
                // Keep the first point
                newPoints.Add(smoothedPoints[0]);
                
                // Smooth middle points
                for (int i = 1; i < smoothedPoints.Count - 1; i++)
                {
                    Vector2 prev = smoothedPoints[i - 1];
                    Vector2 current = smoothedPoints[i];
                    Vector2 next = smoothedPoints[i + 1];
                    
                    // Simple average smoothing
                    Vector2 smoothed = (prev + current * 2 + next) / 4f;
                    newPoints.Add(smoothed);
                }
                
                // Keep the last point
                newPoints.Add(smoothedPoints[smoothedPoints.Count - 1]);
                smoothedPoints = newPoints;
            }
            
            contourPoints = smoothedPoints;
            Debug.Log($"Smoothed path ({smoothingIterations} iterations)");
        }
        
        private void SimplifyPath()
        {
            if (contourPoints.Count < 3) return;
            
            // Douglas-Peucker algorithm for path simplification
            List<Vector2> simplifiedPoints = new List<Vector2> { contourPoints[0] };
            DouglasPeuckerSimplify(0, contourPoints.Count - 1, pathSimplificationTolerance);
            
            Debug.Log($"Simplified path from {contourPoints.Count} to {simplifiedPoints.Count} points");
            
            // Local function for recursive simplification
            void DouglasPeuckerSimplify(int startIdx, int endIdx, float epsilon)
            {
                if (endIdx <= startIdx + 1) return;
                
                float maxDistance = 0;
                int maxDistanceIndex = 0;
                
                Vector2 start = contourPoints[startIdx];
                Vector2 end = contourPoints[endIdx];
                
                for (int i = startIdx + 1; i < endIdx; i++)
                {
                    float distance = PointLineDistance(contourPoints[i], start, end);
                    
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        maxDistanceIndex = i;
                    }
                }
                
                if (maxDistance > epsilon)
                {
                    DouglasPeuckerSimplify(startIdx, maxDistanceIndex, epsilon);
                    simplifiedPoints.Add(contourPoints[maxDistanceIndex]);
                    DouglasPeuckerSimplify(maxDistanceIndex, endIdx, epsilon);
                }
            }
            
            // Add the last point
            simplifiedPoints.Add(contourPoints[contourPoints.Count - 1]);
            contourPoints = simplifiedPoints;
        }
        
        private float PointLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            if (lineStart == lineEnd) return Vector2.Distance(point, lineStart);
            
            float lengthSquared = (lineEnd - lineStart).sqrMagnitude;
            if (lengthSquared < 0.0000001f) return Vector2.Distance(point, lineStart);
            
            // Calculate projection of point onto line
            float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, lineEnd - lineStart) / lengthSquared);
            Vector2 projection = lineStart + t * (lineEnd - lineStart);
            
            return Vector2.Distance(point, projection);
        }
        
        private void GenerateGCodeFromPath()
        {
            if (contourPoints.Count < 2 || gCodeGenerator == null)
            {
                Debug.LogWarning("Not enough points to generate G-code or gCodeGenerator is missing");
                return;
            }
            
            // Initialize G-code header
            gCodeGenerator.AppendGCode("; Contour path from density field");
            gCodeGenerator.AppendGCode(string.Format(CultureInfo.InvariantCulture, "; Threshold: {0}", threshold));
            gCodeGenerator.AppendGCode(string.Format(CultureInfo.InvariantCulture, "; Points: {0}", contourPoints.Count));
            gCodeGenerator.AppendGCode("G90"); // Absolute positioning
            gCodeGenerator.AppendGCode("G21"); // Millimeters
            gCodeGenerator.AppendGCode("M4 S0"); // Initialize laser off
            
            // First point - rapid move to starting position
            Vector3 firstPoint = new Vector3(contourPoints[0].x, 0, contourPoints[0].y);
            Vector3 laserStart = gCodeGenerator.WorldToLaserCoordinates(firstPoint);
            gCodeGenerator.AppendGCode(string.Format(CultureInfo.InvariantCulture, 
                "G0 X{0:F3} Y{1:F3}", laserStart.x, laserStart.y));
            
            // Turn on laser
            gCodeGenerator.AppendGCode(string.Format(CultureInfo.InvariantCulture, "S{0}", 1000)); // Max power
            
            // Trace the contour
            for (int i = 1; i < contourPoints.Count; i++)
            {
                Vector3 point = new Vector3(contourPoints[i].x, 0, contourPoints[i].y);
                Vector3 laserPos = gCodeGenerator.WorldToLaserCoordinates(point);
                gCodeGenerator.AppendGCode(string.Format(CultureInfo.InvariantCulture, 
                    "G1 X{0:F3} Y{1:F3} F{2:F0}", laserPos.x, laserPos.y, 1000)); // Feed rate 1000
            }
            
            // Close the loop if needed
            if (Vector2.Distance(contourPoints[0], contourPoints[contourPoints.Count - 1]) > 0.1f)
            {
                Vector3 finalPoint = new Vector3(contourPoints[0].x, 0, contourPoints[0].y);
                Vector3 laserFinal = gCodeGenerator.WorldToLaserCoordinates(finalPoint);
                gCodeGenerator.AppendGCode(string.Format(CultureInfo.InvariantCulture, 
                    "G1 X{0:F3} Y{1:F3}", laserFinal.x, laserFinal.y));
            }
            
            // Turn off laser and finish
            gCodeGenerator.AppendGCode("S0");
            gCodeGenerator.AppendGCode("M5");
            
            Debug.Log("G-code generated for contour path");
        }

        private void OnDrawGizmos()
        {
            // Draw the normal grid and contour visualization

            // Additionally, draw the raw segments for debugging
            if (allSegments != null && allSegments.Count > 0)
            {
                // Draw each segment in a different color
                for (int i = 0; i < allSegments.Count; i++)
                {
                    // Create a distinct color based on index
                    float hue = (float)i / allSegments.Count;
                    Color segmentColor = Color.HSVToRGB(hue, 0.7f, 0.9f);
                    Gizmos.color = segmentColor;

                    // Draw points and connections
                    var segment = allSegments[i];
                    for (int j = 0; j < segment.Count; j++)
                    {
                        // Draw point
                        Vector3 pos = new Vector3(segment[j].x, 0.15f, segment[j].y);
                        Gizmos.DrawSphere(pos, sampleResolution * 0.1f);

                        // Draw line
                        if (j > 0)
                        {
                            Vector3 prevPos = new Vector3(segment[j - 1].x, 0.15f, segment[j - 1].y);
                            Gizmos.DrawLine(prevPos, pos);
                        }
                    }
                }
            }
        }
        
        // Method to allow overriding the SampleDensityField function
        public void SetDensityFieldFunction(Func<Vector3, float> densityFunction)
        {
            // Store the provided function for later use
            _customDensityFunction = densityFunction;
        }
        
        private Func<Vector3, float> _customDensityFunction;
        
        // Additional helper methods for more complex contours
        
        // Extract multiple contours at different thresholds
        public void GenerateMultipleContours(float[] thresholds)
        {
            foreach (float t in thresholds)
            {
                threshold = t;
                GenerateContourPaths();
            }
        }
        
        // Create a continuous G-code path for multiple contours
        public void GenerateCompoundPath(float[] thresholds, bool innerToOuter = true)
        {
            // Sort thresholds (inner to outer or outer to inner)
            Array.Sort(thresholds);
            if (!innerToOuter) Array.Reverse(thresholds);
            
            // Generate G-code for each threshold in sequence
            foreach (float t in thresholds)
            {
                threshold = t;
                GenerateContourPaths();
            }
        }
    }
}
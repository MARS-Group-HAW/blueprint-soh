using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace SOHPlayground
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var wasserflaeche = new GeometryCollection(new GeoJsonReader().Read<FeatureCollection>(
                    File.ReadAllText(Path.Combine("res", "vector_data", "Wasserflaeche_Hamburg.geojson"))).Features
                .Select(feature => feature.Geometry).ToArray());

            var mesh = new GeoJsonReader().Read<FeatureCollection>(
                File.ReadAllText(Path.Combine("res", "vector_data", "Wasserflaeche_Hamburg_Mesh_Reduced.geojson")));

            var filteredMesh = new Collection<IFeature>();
            var elbWasser =
                (IMultiPolygon) wasserflaeche.Geometries.OrderByDescending(geometry => geometry.Area).First();
            var lineCount = 100000;
            foreach (var feature in mesh.Features)
            {
                if (lineCount < 0) break;
                var g = feature.Geometry;
                if (g is ILineString line)
                {
                    if (LineReallyIntersectsPolygon(line, elbWasser))
                        filteredMesh.Add(new Feature(line, feature.Attributes));
                    Console.WriteLine($"Line transformation: {lineCount}");
                    lineCount--;
                }
            }

            var json = new GeoJsonWriter().Write(new FeatureCollection(filteredMesh));

            File.WriteAllText("Wasserflaeche_Hamburg_Mesh_Filtered.geojson", json);
        }

        public static bool LineReallyIntersectsPolygon(ILineString ls, IMultiPolygon p)
        {
            return ls.Intersects(p) && !ls.Touches(p);
        }
    }
}
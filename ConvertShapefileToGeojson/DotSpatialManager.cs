
using DotSpatial.Data;
using DotSpatial.Projections;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ConvertShapefileToGeojson
{
    public static class DotSpatialManager
    {
            public static string ConvertShapefileToJSON(string shapefilePath)
            {
                try
                {
                    IFeatureSet featureSet = FeatureSet.OpenFile(shapefilePath);

                featureSet.Reproject(KnownCoordinateSystems.Geographic.World.WGS1984);

                    DataTable featureTable = featureSet.DataTable;
                    featureTable.TableName = Path.GetFileNameWithoutExtension(shapefilePath);


                    //   Dictionary<int, string> shapefileWktList = GetShapefileWKT(featureSet);
                    List<IFeature> featuresList = featureSet.Features.ToList();

                    //   RemoveEmptyShapefile(featureTable, featuresList);

                    int i = 0;
                    var envelope = new
                    {
                        type = "FeatureCollection",
                        name = featureTable.TableName,
                        features = featureTable.AsEnumerable().Select(record => new
                        {
                            type = "Feature",

                            geometry = new
                            {
                                type = featuresList[i].Geometry.GeometryType.ToString(),
                                coordinates = GetCoordinate(featuresList[i++]),
                            },
                            properties = new JObject(
                                                     featureTable.Columns.Cast<DataColumn>()
                                                    .Select(c => new JProperty(c.ColumnName, JToken.FromObject(record[c])))
                                                     ),
                        }).ToArray()
                    };


                    return JsonConvert.SerializeObject(envelope);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            private static object GetCoordinate(IFeature feature)
            {
                string Type = feature.Geometry.GeometryType;
                if (string.Equals(Type, "Point"))
                {
                    return GetPointCoordinate(feature.Geometry);
                }
                if (string.Equals(Type, "LineString"))
                {
                    return GetLineCoordinate(feature.Geometry);
                }
                if (string.Equals(Type, "Polygon"))
                {
                    return GetPolygonCoordinate(feature.Geometry);
                }
                if (string.Equals(Type, "MultiPoint"))
                {
                    return GetMultiPointCoordinate(feature);
                }
                if (string.Equals(Type, "MultiLineString"))
                {
                    return GetMultiLineCoordinate(feature);
                }
                if (string.Equals(Type, "MultiPolygon"))
                {
                    return GetMultiPolygonCoordinate(feature);
                }
                else
                {
                    throw new Exception("Unknown shape file!");
                }
            }
            private static object GetPointCoordinate(IGeometry geometry)
            {
                return new double[]
                {
                geometry.Coordinate.X,
                geometry.Coordinate.Y
                };
            }
            private static Object GetLineCoordinate(IGeometry geometry)
            {
                return geometry.Coordinates.ToList().Select(
                                        coordinate => new double[]
                                        { coordinate.X,
                                      coordinate.Y,
                                        });
            }
            private static object GetPolygonCoordinate(IGeometry geometry)
            {
                return new[] {
                                (geometry.Coordinates.ToList().Select(
                                    coordinate => new double[]
                                    { coordinate.X,
                                     coordinate.Y
                                    }))
                            };
            }
            private static object GetMultiPointCoordinate(IFeature Feature)
            {
                int Count = Feature.ShapeIndex.NumParts;
                List<IGeometry> PointGeometries = new List<IGeometry>();
                for (int i = 0; i < Count; i++)
                {
                    PointGeometries.Add(Feature.Geometry.GetGeometryN(i));
                }

                object PointCoordinate = PointGeometries.Select(geometry => GetPointCoordinate(geometry));

                return PointCoordinate;
            }
            private static object GetMultiLineCoordinate(IFeature feature)
            {
                int Count = feature.ShapeIndex.NumParts;
                List<IGeometry> LineGeometries = new List<IGeometry>();
                for (int i = 0; i < Count; i++)
                {
                    LineGeometries.Add(feature.Geometry.GetGeometryN(i));
                }

                object lineCoordinate = LineGeometries.Select(geometry => GetLineCoordinate(geometry));

                return lineCoordinate;
            }
            private static object GetMultiPolygonCoordinate(IFeature Feature)
            {
                int Count = Feature.Geometry.NumGeometries;
                List<IGeometry> PolygonGeometries = new List<IGeometry>();

                for (int i = 0; i < Count; i++)
                {
                    PolygonGeometries.Add(Feature.Geometry.GetGeometryN(i));
                }


                object PolygonCoordinate = PolygonGeometries.Select(geometry => GetPolygonCoordinate(geometry));

                return PolygonCoordinate;
            }
        }
    }

using System;
using System.IO;

namespace ConvertShapefileToGeojson
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please Enter shapefile path:");
            string path = Console.ReadLine();
            string geoJson = DotSpatialManager.ConvertShapefileToJSON(path);

            string filePath = Path.ChangeExtension(path, ".geojson");
            WriteFile(geoJson, filePath);

            Console.WriteLine("File created in this path: ");
            Console.WriteLine(filePath);
            Console.ReadLine();
        }

        public static void WriteFile(string fileContent, string path) 
        {

            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new System.IO.StreamWriter(path: path, append: false, encoding: System.Text.Encoding.UTF8);

                streamWriter.WriteLine(fileContent);

                streamWriter.Close();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                    streamWriter = null;
                }
            }
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace rwf_plugin_1
{
    internal class WindowJsonData
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double SillHeight { get; set; }

        static public WindowJsonData Parse(string jsonPath)
        {

            try
            {
                if (!File.Exists(jsonPath))
                    return new WindowJsonData();

                var jsonContents = File.ReadAllText(jsonPath);
                return JsonConvert.DeserializeObject<WindowJsonData>(jsonContents);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception when parsing json file: " + ex);
                return null;
            }

        }
    }
}

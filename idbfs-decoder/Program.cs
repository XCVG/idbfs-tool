using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace idbfs_decoder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool contentsAreSigned = args.Contains("-s");

            string inputFileName = "idbfs.json";
            if((!contentsAreSigned && args.Length > 0) || (contentsAreSigned && args.Length > 1))
                inputFileName = args[args.Length - 1];
            string inputString = File.ReadAllText(inputFileName);
            JObject dataObject = JObject.Parse(inputString);

            string baseOutputFolder = Path.GetFileNameWithoutExtension(inputFileName);
            string outputFolder = baseOutputFolder;
            for(int i = 0; Directory.Exists(outputFolder); i++)
            {
                outputFolder = baseOutputFolder + "_" + i.ToString();
            }

            Directory.CreateDirectory(outputFolder);

            //do directories
            foreach(var kvp in dataObject)
            {
                var entryObject = kvp.Value as JObject;

                if (entryObject == null || entryObject.ContainsKey("contents"))
                    continue;

                string path = kvp.Key.TrimStart('/').TrimStart('\\');
                string fullPath = Path.Combine(outputFolder, path);
                Directory.CreateDirectory(fullPath);

                if(entryObject.ContainsKey("timestamp"))
                {
                    DateTime timestamp = DateTime.Parse(entryObject["timestamp"].ToString());
                    Directory.SetCreationTimeUtc(fullPath, timestamp);
                    Directory.SetLastWriteTimeUtc(fullPath, timestamp);
                }
                
            }

            //do files
            foreach(var kvp in dataObject)
            {
                var entryObject = kvp.Value as JObject;

                if (entryObject == null || !entryObject.ContainsKey("contents"))
                    continue;

                string path = kvp.Key.TrimStart('/').TrimStart('\\');
                string fullPath = Path.Combine(outputFolder, path);

                JObject contents = (JObject)entryObject["contents"];
                bool fileContentsAreSigned = contentsAreSigned;
                if (entryObject.ContainsKey("$contentsType"))
                {
                    string contentsTypeString = entryObject["$contentsType"].ToString();
                    fileContentsAreSigned = contentsTypeString == "Int8Array";
                }
                byte[] fileData = fileContentsAreSigned ? DecodeSignedFile(contents) : DecodeFile(contents);
                File.WriteAllBytes(fullPath, fileData);

                if (entryObject.ContainsKey("timestamp"))
                {
                    DateTime timestamp = DateTime.Parse(entryObject["timestamp"].ToString());
                    File.SetCreationTimeUtc(fullPath, timestamp);
                    File.SetLastWriteTimeUtc(fullPath, timestamp);
                }
            }

            Console.WriteLine("done");
        }

        private static byte[] DecodeFile(JObject contents)
        {
            Dictionary<int, byte> contentsObj = new Dictionary<int, byte>();
            foreach (var kvp in contents)
            {
                contentsObj[int.Parse(kvp.Key)] = byte.Parse(kvp.Value.ToString());
            }

            List<byte> bytes = new List<byte>();
            foreach (var key in contentsObj.Keys.OrderBy(k => k))
            {
                bytes.Add(contentsObj[key]);
            }

            return bytes.ToArray();
        }

        private static byte[] DecodeSignedFile(JObject contents)
        {
            Dictionary<int, sbyte> contentsObj = new Dictionary<int, sbyte>();
            foreach (var kvp in contents)
            {
                contentsObj[int.Parse(kvp.Key)] = sbyte.Parse(kvp.Value.ToString());
            }

            List<sbyte> sbytes = new List<sbyte>();
            foreach (var key in contentsObj.Keys.OrderBy(k => k))
            {
                sbytes.Add(contentsObj[key]);
            }

            sbyte[] signed = sbytes.ToArray();
            byte[] unsigned = new byte[signed.Length];
            Buffer.BlockCopy(signed, 0, unsigned, 0, signed.Length);

            return unsigned;
        }
    }
}

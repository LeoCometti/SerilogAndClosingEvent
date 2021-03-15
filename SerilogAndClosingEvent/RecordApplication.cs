using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerilogAndClosingEvent
{
    class RecordApplication
    {
        public static void Starting(
            string baseDirectory,
            string appName,
            string appVersion,
            bool isAppService)
        {
            CreateOrAppendFile( "Opening", baseDirectory, appName, appVersion, isAppService);
        }

        public static void Closing(
            string baseDirectory,
            string appName,
            string appVersion,
            bool isAppService)
        {
            CreateOrAppendFile("Closing", baseDirectory, appName, appVersion, isAppService);
        }

        private static void CreateOrAppendFile(
            string appStatus,
            string baseDirectory,
            string appName,
            string appVersion,
            bool isAppService)
        {
            var path = @$"{baseDirectory}{appName}.json";

            var record = new RecordModel
            {
                status = appStatus,
                timestamp = DateTime.Now,
                version = appVersion,
                isService = isAppService
            };

            var json_record = JsonConvert.SerializeObject(record);

            var fileExistis = File.Exists(path);

            using (Stream stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                var hasData = (fileExistis && stream.Length > 0);

                var value = hasData
                    ? Encoding.ASCII.GetBytes($", {Environment.NewLine}{json_record}{Environment.NewLine}]")
                    : Encoding.ASCII.GetBytes($"[{Environment.NewLine}{json_record}]");

                if (hasData)
                {
                    stream.Seek(-3, SeekOrigin.End);
                }

                stream.Write(value, 0, value.Length);
            }
        }

        private class RecordModel
        {
            public string status { get; set; }
            public DateTime timestamp { get; set; }
            public string version { get; set; }
            public bool isService { get; set; }
        }
    }
}

using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SheetLoader
{
    class Program
    {
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google Sheets API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var spreadsheetId = "1vcv6nDsn7EUuyaE1U93mMVfmGJT3m-suv-CJOMYSo38";
            var a = service.Spreadsheets.Get(spreadsheetId);
            var b = a.Execute();
            var rootJson = new JObject();
            var dataObj = new JObject();
            foreach (var sheet in b.Sheets)
            {
                var c = service.Spreadsheets.Values.Get(spreadsheetId, sheet.Properties.Title);
                var d = c.Execute();
                var e = d.Values;
                dataObj.Add(sheet.Properties.Title, ToArray(e));
            }
            rootJson.Add("Data", dataObj);
            File.WriteAllText("../Output.json", rootJson.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public static JArray ToArray(IList<IList<object>> data)
        {
            var arr = new JArray();
            var headers = new Dictionary<int, string>();
            var rowIndex = 0;

            // if (!TrySkipRow(data, ref rowIndex)) return null;

            for (int i = 0; i < data[rowIndex].Count; i++)
            {
                // var str = data
                var str = data[rowIndex][i]?.ToString();
                if(string.IsNullOrWhiteSpace(str) || str.StartsWith("#")) continue;
                headers.Add(i, str);
            }

            rowIndex++;

            while (TrySkipRow(data, ref rowIndex))
            {
                var elem = new JObject();
                foreach (var kv in headers)
                {
                    var row = data[rowIndex];
                    if (kv.Key >= row.Count) continue;
                    var value = row[kv.Key];
                    if ((string)value == string.Empty) continue;
                    elem.Add(kv.Value, new JValue(value));
                }
                arr.Add(elem);
                rowIndex++;
            }

            if (arr.Count == 0) return null;


            return arr;
        }

        public static bool TrySkipRow(IList<IList<object>> data, ref int index)
        {
            for (int i = index; i < data.Count; i++)
            {
                var row = data[i];
                var str = row.Count > 0 ? row[0]?.ToString() : null;
                if (string.IsNullOrWhiteSpace(str) || str.StartsWith("#")) continue;
                index = i;
                return true;
            }
            return false;
        }
    }
}

using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Composition;
using Microsoft.Extensions.Logging;
using SuperNova.Data.GoogleSheets.Contract;
using SuperNova.AWS.Logging;
using SuperNova.MEF.NetCore;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Google.Apis.Util;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;

namespace SuperNova.Data.GoogleSheets
{
    /// <summary> 
    /// C# mapping of SuperNova coord-sheet 
    /// </summary> 

    [Export(typeof(IGoogleSheetsProxy))]
    [Shared]
    public class GoogleSheetsProxy : LoggingResource, IDisposable, IGoogleSheetsProxy
    {
        
        /// <summary> 
        /// Spreadsheet identifier used to point to a specific spreadsheet 
        /// Used for subsequent requests  
        /// </summary> 
        
        public string ApiKey { get; set; }
        public string Name { get; set; } = "SuperNova.Data.GoogleSheetsProxy";

        private SheetsService _sheetService { get; set; }
        private bool _init = false;

        public GoogleSheetsProxy() : base(nameof(GoogleSheetsProxy))
        {
            MEFLoader.SatisfyImportsOnce(this);
        }

        private async Task InitAsync()
        {
            _sheetService = new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = string.IsNullOrEmpty(ApiKey) ? await GetApiKeyAsync() : ApiKey,
                ApplicationName = Name,
            });
            Logger.LogInformation("GoogleSheetsProxy initialized.");
        }
        private async Task<string> GetApiKeyAsync()
        {
            return await EvokeProxyAction("GetApiKeyAsync", async () =>
            {
                using var client = new AmazonSecretsManagerClient();
                var response = await client.GetSecretValueAsync(new GetSecretValueRequest
                {
                    SecretId = "supernova/auth/googleapikey"
                });

                response.SecretString.ThrowIfNullOrEmpty<Exception>("Unable to retrieve Google API Key!");
                return response.SecretString;
            }, false);
        }

        public async Task<CommodityInfo> GetCorpCommodityInfoAsync(string spreadsheetId, string commodityTicker)
        {
            return await EvokeProxyAction("GetCorpCommodityInfoAsync", async () =>
            {
                var range = "Corp-Prices!C45:N386"; //TODO: some better way to handle spreadsheet data (we need to assume that spreadsheet magicians will change something at some point) 
                var request = _sheetService.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = await request.ExecuteAsync(); //TODO: cache, approx wait time ~1-3 sec  
                var values = response.Values.Select(x => new CommodityInfo(x.ToArray())).ToList();
                return values.FirstOrDefault(x => x?.Ticker == commodityTicker);
            });

        }


        public async Task<string> UpdateData(string spreadsheetId, string range, List<IList<object>> data)
        {
            var requestData = new BatchUpdateValuesRequest()
            {
                ValueInputOption = "USER_INPUT",
                Data = new List<ValueRange>()
                {
                    new ValueRange
                    {
                        Range = range,
                        Values = data
                    }
                }
            };
            var request = _sheetService.Spreadsheets.Values.BatchUpdate(requestData, spreadsheetId);

            var response = await request.ExecuteAsync();

            return JsonConvert.SerializeObject(response);

        }

        private async Task<T> EvokeProxyAction<T>(string name, Func<Task<T>> action, bool init = true)
        {
            try
            {
                if (!_init && init) await InitAsync();
                Logger.LogInformation($"Starting {name}");
                var result = await action();
                Logger.LogInformation($"Finishing {name}");
                return result;
            }
            catch(Exception ex)
            {
                Logger.LogError($"Error in {name}");
                Logger.LogError(ex.ConcatMessages());
                return default;
            }
        }

        public void Dispose()
        {
            _sheetService?.Dispose();
        }
    }



    //public async Task<T> NameMe<T>(string sheetId, string range) 
    //{
    //    var test = _sheetService.Spreadsheets.Values.BatchGet(sheetId);
    //    test.Ranges= new Repeatable<string>(new[] { "A:B", "K:S" });
    //    test.MajorDimension = SpreadsheetsResource.ValuesResource.BatchGetRequest.MajorDimensionEnum.ROWS;
    //    var result = await test.ExecuteAsync();
    //    result.ValueRanges

    //}



    //public class BuildingCostInfo
    //{
    //    public string Key { get; set; }
    //    public string Name { get; set; }
    //    public decimal Rocky { get; set; }
    //    public decimal Gas { get; set; }
    //    public decimal LowPressure { get; set; }
    //    public decimal HighPressure { get; set; }
    //    public decimal LowGravity { get; set; }
    //    public decimal LowTemp { get; set; }
    //    public decimal HighTemp { get; set; }

    //    public BuildingCostInfo(IEnumerable<ValueRange> ranges)
    //    {
    //        var columns = new List<IList<object>>();
    //        foreach(var range in ranges)
    //        {
    //            foreach(var column in range.Values)
    //            {
    //                columns.Add(column);
    //            }
    //        }
    //        return new BuildingCostInfo
    //        {
    //            Key = column
    //        }
    //    }
    //}
}
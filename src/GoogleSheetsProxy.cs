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
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;
using Amazon.S3;
using System.IO;
using Amazon.S3.Model;
using Google.Apis.Auth.OAuth2;

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

        private string _sheetId;
        private bool _init = false;

        public GoogleSheetsProxy() : base(nameof(GoogleSheetsProxy))
        {
            MEFLoader.SatisfyImportsOnce(this);
        }

        public async Task InitAsync(string configPath = null)
        {
            Logger.LogInformation("GoogleSheetsProxy::Init");
            try
            {
                string[] scopes = { SheetsService.Scope.Spreadsheets };
                Stream responseStream;

                if (configPath != null)
                {
                    responseStream = File.Open(configPath, FileMode.Open);
                }
                else
                {
                    var request = new GetObjectRequest
                    {
                        BucketName = "supernova-discordbot",
                        Key = "credentials.json"
                    };

                    using (var client = new AmazonS3Client())
                    using (var response = await client.GetObjectAsync(request))
                        responseStream = response.ResponseStream;
                }

                using (responseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    var credential = (ServiceAccountCredential)GoogleCredential.FromStream(responseStream).UnderlyingCredential;

                    var initializer = new ServiceAccountCredential.Initializer(credential.Id)
                    {
                        User = credential.User,
                        Key = credential.Key,
                        Scopes = scopes
                    };
                    credential = new ServiceAccountCredential(initializer);
                    _sheetService = new SheetsService(new BaseClientService.Initializer()
                    {
                        //ApiKey = string.IsNullOrEmpty(ApiKey) ? await GetApiKeyAsync() : ApiKey,
                        ApplicationName = Name,
                        HttpClientInitializer = credential
                    });
                    Logger.LogInformation("GoogleSheetsProxy initialized.");
                }
                _init = true;
                

            }
            catch (AmazonS3Exception e)
            {
                Logger.LogError($"Error encountered ***. Message:'{e.Message}' when reading object");
            }
            catch (Exception e)
            {
                Logger.LogError($"Unknown encountered on server. Message:'{e.Message}' when reading object");
            }
            
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

        public async Task<string> GetCoordSheetId()
        {
            
            return await EvokeProxyAction("GetApiKeyAsync", async () =>
            {
                if (string.IsNullOrEmpty(_sheetId))
                {
                    using var client = new AmazonSecretsManagerClient();
                    var response = await client.GetSecretValueAsync(new GetSecretValueRequest
                    {
                        SecretId = "supernova/auth/coordsheetid"
                    });

                    response.SecretString.ThrowIfNullOrEmpty<Exception>("Unable to retrieve Google Sheets Id!");
                    _sheetId = response.SecretString;
                    return _sheetId;
                }
                else
                {
                    return _sheetId;
                }

                
            }, false);
        }


        public async Task<CommodityInfo> GetCorpCommodityInfoAsync(string spreadsheetId, string pricesRange, string commodityTicker)
        {
            return await EvokeProxyAction("GetCorpCommodityInfoAsync", async () =>
            {
                //look into caching?
                var valueRange = await GetRange(spreadsheetId, pricesRange);
                var values = valueRange.Values.Select(x => new CommodityInfo(x.ToArray())).ToList();
                return values.FirstOrDefault(x => x?.Ticker == commodityTicker);
            });

        }

        public async Task<ValueRange> GetRange(string spreadsheetId, string range)
        {
            return await EvokeProxyAction("GetCorpCommodityInfoAsync", async () =>
            {
                var request = _sheetService.Spreadsheets.Values.Get(spreadsheetId, range);
                return await request.ExecuteAsync(); //TODO: cache, approx wait time ~1-3 sec  
            });
            
        }


        public async Task<BatchUpdateValuesResponse> UpdateRange(string spreadsheetId, string range, List<IList<object>> data)
        {
            var requestData = new BatchUpdateValuesRequest()
            {
                ValueInputOption = "USER_ENTERED",
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
            return await request.ExecuteAsync();

        }



        public async Task<AppendValuesResponse> AppendRange(string spreadsheetId, string range, List<IList<object>> data)
        {
            var valueRange = new ValueRange
            {
                Range = range,
                Values = data
            };
            var request = _sheetService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);

            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            return await request.ExecuteAsync();
            

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
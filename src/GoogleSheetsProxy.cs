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

namespace SuperNova.Data.GoogleSheets
{
    /// <summary> 
    /// C# mapping of SuperNova coord-sheet 
    /// TODO: Add logging 
    /// </summary> 

    [Export(typeof(IGoogleSheetsProxy))]
    [Shared]
    public class GoogleSheetsProxy : LoggingResource, IDisposable, IGoogleSheetsProxy
    {
        private static string _applicationName;
        private SheetsService _sheetService { get; set; }

        /// <summary> 
        /// Spreadsheet identifier used to point to a specific spreadsheet 
        /// Used for subsequent requests  
        /// </summary> 
        public string SpreadSheetID { get; set; } = "1tyYLfgAqD7Mm1Lv8-fc59RuPdPZ_pa0HYjY7TVI_KKo"; //PrUn Coord Sheet 
        public string ApiKey { get; set; }
        public string Name { get; set; } = "SuperNova.Data.GoogleSheetsProxy";

        private bool _init = false;

        public GoogleSheetsProxy() : base(nameof(GoogleSheetsProxy))
        {
            MEFLoader.SatisfyImportsOnce(this);
            Logger.LogInformation("GoogleSheetsProxy initialized.");
        }

        private async Task InitAsync()
        {
            _sheetService = new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = string.IsNullOrEmpty(ApiKey) ? await GetApiKeyAsync() : ApiKey,
                ApplicationName = Name,
            });
        }
        private async Task<string> GetApiKeyAsync()
        {
            using var client = new AmazonSecretsManagerClient();
            var response = await client.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = "supernova/auth/googleapikey"
            });

            if (response.SecretString == null)
            {
                Logger.LogError("FATAL: Unable to retrieve google api key!!");
                return string.Empty;
            }
            return response.SecretString;
        }

        public async Task<CommodityInfo> GetCorpCommodityInfoAsync(string commodityTicker)
        {

            if (!_init) await InitAsync();

            var range = "Corp-Prices!C45:N386"; //TODO: some better way to handle spreadsheet data (we need to assume that spreadsheet magicians will change something at some point) 
            var request = _sheetService.Spreadsheets.Values.Get(SpreadSheetID, range);

            try
            {
                // Define request parameters. 
                var response = await request.ExecuteAsync(); //TODO: cache, approx wait time ~1-3 sec  
                var values = response.Values.Select(x => new CommodityInfo(x.ToArray())).ToList();
                return values.FirstOrDefault(x => x?.Ticker == commodityTicker);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ConcatMessages());
                return new CommodityInfo();
            }

        }

        public void Dispose()
        {
            _sheetService?.Dispose();
        }
    }
}
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Composition;
using Common;
using SuperNova.AWS.Logging.Contract;
using Microsoft.Extensions.Logging;
using SuperNova.Data.GoogleSheets.Contract;

namespace SuperNova.Data.GoogleSheets
{
    /// <summary>
    /// C# mapping of SuperNova coord-sheet
    /// TODO: Add logging
    /// </summary>

    [Export(typeof(IGoogleSheetsProxy))]
    [Shared]
    public class GoogleSheetsProxy : IDisposable
    {
        private static string _applicationName;
        private SheetsService _sheetService { get; set; }

        [Import]
        private IServiceLoggerFactory _logFactory { get; set; } = null;
        private ILogger _logger;


        /// <summary>
        /// Spreadsheet identifier used to point to a specific spreadsheet
        /// Used for subsequent requests 
        /// </summary>
        private readonly string _spreadSheetID;
        private readonly UserCredential _creds;
        private bool _init = false;

        /// <param name="credential">GoogleSheetsAPI credentials</param>
        /// <param name="SpreadSheetID">SuperNova coord spreadSheet ID</param>
        public GoogleSheetsProxy(UserCredential creds, string spreadSheetID = "1tyYLfgAqD7Mm1Lv8-fc59RuPdPZ_pa0HYjY7TVI_KKo", string applicationName = "SuperNova GoogleSheets Proxy Microservice")
        {
            MEFLoader.SatisfyImportsOnce(this);
            _logger = _logFactory.GetLogger(applicationName);
            _applicationName = applicationName;
            _spreadSheetID = spreadSheetID;
            _creds = creds;
        }

        public void Init()
        {
            if (_init) return;
            try
            {
                // Create Google Sheets API service.
                _sheetService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _creds,
                    ApplicationName = _applicationName,
                });
                _init = true;
            }
            catch (Exception ex)
            {
                //this is fatal - log and throw 
                _logger.LogError(ex.ConcatMessages());
                throw new Exception("Failed to connect to Google Sheets. Please verify your information and try again.");
            }

            _logger.LogInformation("GoogleSheetsProxy initialized.");
        }

        

        public async Task<CommodityInfo> GetCorpCommodityInfoAsync(string commodityTicker)
        {
            
            if (!_init) Init();

            var range = "Corp-Prices!C45:N386"; //TODO: some better way to handle spreadsheet data (we need to assume that spreadsheet magicians will change something at some point)
            var request = _sheetService.Spreadsheets.Values.Get(_spreadSheetID, range);

            try
            {
                // Define request parameters.
                var response = await request.ExecuteAsync(); //TODO: cache, approx wait time ~1-3 sec 
                var values = response.Values.Select(x => new CommodityInfo(x.ToArray())).ToList();
                return values.FirstOrDefault(x => x?.Ticker == commodityTicker);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ConcatMessages());
                return new CommodityInfo();
            }
           
        }

        public void Dispose()
        {
            if (!_init) return;

            _sheetService?.Dispose();
        }
    }
}

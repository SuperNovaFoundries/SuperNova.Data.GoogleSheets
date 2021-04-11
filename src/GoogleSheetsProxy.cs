using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SuperNova.Coord
{
    /// <summary>
    /// C# mapping of SuperNova coord-sheet
    /// </summary>
    public class GoogleSheetsProxy
    {
        private static string _applicationName;
        private readonly SheetsService _sheets;

        /// <summary>
        /// Spreadsheet identifier used to point to a specific spreadsheet
        /// Used for subsequent requests 
        /// </summary>
        private readonly string _spreadSheetID;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="credential">GoogleSheetsAPI credentials</param>
        /// <param name="SpreadSheetID">SuperNova coord spreadSheet ID</param>
        public GoogleSheetsProxy(UserCredential credential, string applicationName = "SuperNova GoogleSheets Proxy Microservice", string spreadSheetID = "1tyYLfgAqD7Mm1Lv8-fc59RuPdPZ_pa0HYjY7TVI_KKo")
        {
            // Create Google Sheets API service.
            _sheets = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });
            _spreadSheetID = spreadSheetID;
        }


        public async Task<CommodityInfo> GetCorpCommodityInfoAsync(string commodityTicker)
        {
            // Define request parameters.
            string range = "Corp-Prices!C45:N386"; //TODO: some better way to handle spreadsheet data (we need to assume that spreadsheet magicians will change something at some point)
            SpreadsheetsResource.ValuesResource.GetRequest request = _sheets.Spreadsheets.Values.Get(_spreadSheetID, range);
            ValueRange response = await request.ExecuteAsync(); //TODO: cache? wait time ~1 sec 
            IList<CommodityInfo> values = response.Values.Select(x => CommodityInfoFactory.CreateCommodityInfo(x)).ToList();
            return values.FirstOrDefault(x => x?.Ticker == commodityTicker);
        }
    }
}

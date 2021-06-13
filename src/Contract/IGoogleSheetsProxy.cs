using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperNova.Data.GoogleSheets.Contract
{
    public interface IGoogleSheetsProxy
    {
        Task<CommodityInfo> GetCorpCommodityInfoAsync(string spreadsheetID, string pricesRange, string commodityTicker);
        Task<BatchUpdateValuesResponse> UpdateRange(string spreadsheetId, string range, List<IList<object>> data);
        Task<AppendValuesResponse> AppendRange(string spreadsheetId, string range, List<IList<object>> data);

        Task<ValueRange> GetRange(string spreadsheetId, string range);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperNova.Data.GoogleSheets.Contract
{
    public interface IGoogleSheetsProxy
    {
        Task<CommodityInfo> GetCorpCommodityInfoAsync(string spreadsheetID, string commodityTicker);
        Task<string> UpdateData(string spreadsheetId, string range, List<IList<object>> data);
    }
}

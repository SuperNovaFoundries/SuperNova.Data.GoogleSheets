using System.Threading.Tasks;

namespace SuperNova.Data.GoogleSheets.Contract
{
    internal interface IGoogleSheetsProxy
    {
        Task<CommodityInfo> GetCorpCommodityInfoAsync(string commodityTicker);
    }
}

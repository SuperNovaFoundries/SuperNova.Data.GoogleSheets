using System.Threading.Tasks;

namespace SuperNova.Data.GoogleSheets.Contract
{
    public interface IGoogleSheetsProxy
    {
        Task<CommodityInfo> GetCorpCommodityInfoAsync(string commodityTicker);
    }
}

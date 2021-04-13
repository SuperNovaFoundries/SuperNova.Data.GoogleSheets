using System.Linq;
using System.Collections.Generic;

namespace SuperNova.Data.GoogleSheets
{
    public class CommodityInfo
    {

        public string Ticker { get; set; } = null;
        public string Recipe { get; set; } = null;
        public decimal? UnitsPerBatch { get; set; }
        public decimal? InputsCost { get; set; }
        public decimal? PopUpkeep { get; set; }
        public decimal? BuildingROI { get; set; }
        public decimal? CorpRRP { get; set; }
        public decimal? FirstCXAvg { get; set; }
        public decimal? CXShipping { get; set; }
        public decimal? CorpPrice { get; set; }
        public decimal? ActualROI { get; set; }
        public RecSource RecSource { get; set; } = RecSource.NONE;

        public CommodityInfo() { }
        
        /// <summary>
        /// Creates CommodityInfo out of single sheet row
        /// in non strict manner.
        /// </summary>
        /// <param name="sheetRow">Row of data to be converted into concrete type,
        ///  if length of an array will be < 12, then all the elements higher than length, will be interpreted as null
        ///  if length > 12 then those elements will be ignored
        ///  values not compliant with record definition will be ignored and written as null </param>
        /// <returns>Returns complete or partially filled CommodityInfo or if unsuccessful, null</returns>
        public CommodityInfo(IEnumerable<object> sheetRow)
        {
            object[] prepared = sheetRow.ToArray();
            if (prepared.Length < 12)
                prepared = prepared.Concat(new object[12 - prepared.Length]).ToArray();

            Ticker = prepared[0].ToString();
            Recipe = prepared[1].ToString();
            UnitsPerBatch = prepared[2].ToDecimal();
            InputsCost = prepared[3].ToDecimal();
            PopUpkeep = prepared[4].ToDecimal();
            BuildingROI = prepared[5].ToDecimal();
            CorpRRP = prepared[6].ToDecimal();
            FirstCXAvg = prepared[7].ToDecimal();
            CXShipping = prepared[8].ToDecimal();
            CorpPrice = prepared[9].ToDecimal();
            ActualROI = prepared[10].ToDecimal();
            RecSource = prepared[11].ToString().ToEnum<RecSource>();
        }
    }

    

}

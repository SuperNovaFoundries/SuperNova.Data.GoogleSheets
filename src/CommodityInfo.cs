
using System;
using System.Linq;
using System.Collections.Generic;


namespace SuperNova.Data.GoogleSheets
{
    using Utils;
    public class CommodityInfo
    {

        public string Ticker = null;
        public string Recipe = null;
        public decimal? UnitsPerBatch = null;
        public decimal? InputsCost = null;
        public decimal? PopUpkeep = null;
        public decimal? BuildingROI = null;
        public decimal? CorpRRP = null;
        public decimal? FirstCXAvg = null;
        public decimal? CXShipping = null;
        public decimal? CorpPrice = null;
        public decimal? ActualROI = null;
        public RecSource RecSource = RecSource.NONE;

    }




    internal static class CommodityInfoFactory
    {

        private static readonly Dictionary<string, RecSource> spreadSheetRecSourceToRecSource = new Dictionary<string, RecSource>()
        {
            ["Corp - RRP"] = RecSource.CorpRRP,
            ["Corp"] = RecSource.Corp,
            ["CX"] = RecSource.CX
        };

        private static RecSource RecSourceStringToEnum(string recString)
        {
            RecSource result = RecSource.NONE;
            if (recString != null)
                spreadSheetRecSourceToRecSource.TryGetValue(recString, out result);
            return result;
        }
        /// <summary>
        /// Creates CommodityInfo out of single sheet row
        /// in non strict manner.
        /// /// </summary>
        /// <param name="sheetRow">Row of data to be converted into concrete type,
        ///  if length of an array will be < 12, then all the elements higher than length, will be interpreted as null
        ///  if length > 12 then those elements will be ignored
        ///  values not compliant with record definition will be ignored and written as null </param>
        /// <returns>Returns complete or partially filled CommodityInfo or if unsuccessful, null</returns>
        public static CommodityInfo CreateCommodityInfo(IEnumerable<object> sheetRow)
        {
            object[] prepared = sheetRow.ToArray();
            if (prepared.Length < 12)
                prepared = prepared.Concat(new object[12 - prepared.Length]).ToArray();
            try
            {
                decimal r = 0; //local for TryParses

                return new CommodityInfo()
                {
                    Ticker = (string)prepared[0],
                    Recipe = (string)prepared[1],
                    UnitsPerBatch = decimal.TryParse((string)prepared[2], out r) ? (decimal?)r : null,
                    InputsCost = decimal.TryParse((string)prepared[3], out r) ? (decimal?)r : null,
                    PopUpkeep = decimal.TryParse((string)prepared[4], out r) ? (decimal?)r : null,
                    BuildingROI = decimal.TryParse((string)prepared[5], out r) ? (decimal?)r : null,
                    CorpRRP = decimal.TryParse((string)prepared[6], out r) ? (decimal?)r : null,
                    FirstCXAvg = decimal.TryParse((string)prepared[7], out r) ? (decimal?)r : null,
                    CXShipping = decimal.TryParse((string)prepared[8], out r) ? (decimal?)r : null,
                    CorpPrice = decimal.TryParse((string)prepared[9], out r) ? (decimal?)r : null,
                    ActualROI = decimal.TryParse((string)prepared[10], out r) ? (decimal?)r : null,
                    RecSource = RecSourceStringToEnum((string)prepared[11])
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }


    public enum RecSource
    {
        Corp,
        CX,
        CorpRRP,
        NONE,
    }


}

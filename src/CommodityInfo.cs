
using System;
using System.Linq;
using System.Collections.Generic;


namespace SuperNova.Coord
{
    using Utils;
    public record CommodityInfo(
        string Ticker = null,
       string Recipe = null,
       decimal? UnitsPerBatch = null,
       decimal? InputsCost = null,
       decimal? PopUpkeep = null,
       decimal? BuildingROI = null,
       decimal? CorpRRP = null,
       decimal? FirstCXAvg = null,
       decimal? CXShipping = null,
       decimal? CorpPrice = null,
       decimal? ActualROI = null,
       RecSource RecSource = RecSource.NONE)
    {

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

                return new CommodityInfo(
                (string)prepared[0],
                (string)prepared[1],
                RecSource: RecSourceStringToEnum((string)prepared[11]))
                {
                    UnitsPerBatch = decimal.TryParse((string)prepared[2], out r) ? r : null,
                    InputsCost = decimal.TryParse((string)prepared[3], out r) ? r : null,
                    PopUpkeep = decimal.TryParse((string)prepared[4], out r) ? r : null,
                    BuildingROI = decimal.TryParse((string)prepared[5], out r) ? r : null,
                    CorpRRP = decimal.TryParse((string)prepared[6], out r) ? r : null,
                    FirstCXAvg = decimal.TryParse((string)prepared[7], out r) ? r : null,
                    CXShipping = decimal.TryParse((string)prepared[8], out r) ? r : null,
                    CorpPrice = decimal.TryParse((string)prepared[9], out r) ? r : null,
                    ActualROI = decimal.TryParse((string)prepared[10], out r) ? r : null,
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

using System.ComponentModel;

namespace SuperNova.Data.GoogleSheets
{
    //another way of handling this
    // retrieve by doing RecSoourceAlternate.CorpRRP.GetDescription();
    public enum RecSource
    {
        NONE = 0,
        [Description("Corp - RRP")] CorpRRP,
        [Description("Corp")] Corp,
        [Description("CX")] CX

    }

    //public enum RecSource
    //{
    //    Corp,
    //    CX,
    //    CorpRRP,
    //    NONE,
    //}

}

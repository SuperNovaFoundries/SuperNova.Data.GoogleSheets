using System;
using System.ComponentModel;

namespace SuperNova.Data.GoogleSheets.Utils
{
    internal static class TypeUtil
    {
        public static T CastOrDefault<T>(object o)
        {
            try
            {
                T result = (T)o;
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default(T);
            }
        }

        
    }
}

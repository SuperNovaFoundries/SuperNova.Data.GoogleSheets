
using System;
using System.Linq;


namespace SuperNova.Data.GoogleSheets
{
    using System.ComponentModel;
    using System.Text;

    public static class Extensions
    {

        public static string ConcatMessages(this Exception that, string delimiter = ", ")
        {
            var messages = new StringBuilder();
            ConcatMessages(that, ref messages, delimiter);
            return messages.ToString();
        }

        private static void ConcatMessages(Exception ex, ref StringBuilder Messages, string delimiter) {

            if ((ex == null) || Messages == null) return;
            if (ex.InnerException == null)
                ConcatMessages(ex.InnerException, ref Messages, delimiter);

            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                if(Messages.Length > 0)
                {
                    Messages.Append(delimiter);
                }
                Messages.Append(ex.Message);
            }
        }
        public static string GetDescription<T>(this T that) where T : struct
        {
            AssertIsEnum<T>(false);
            var name = Enum.GetName(typeof(T), that);
            if (name == null) return string.Empty;

            var field = typeof(T).GetField(name);
            if(field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
            {
                return attr.Description;
            }
            return string.Empty;
        }

        public static T ToEnum<T>(this string that) where T: struct
        {
            return Enum.GetNames(typeof(T))
                .Select(Enum.Parse<T>)
                .FirstOrDefault(parsed => that.ToLower() == parsed.GetDescription());
        }

        public static void AssertIsEnum<T>(bool withFlags)
        {
            if (!typeof(T).IsEnum) 
                throw new ArgumentException($"Type '{typeof(T).FullName}' is not an enum");
            if (withFlags && !Attribute.IsDefined(typeof(T), typeof(FlagsAttribute))) 
                throw new ArgumentException($"Type '{typeof(T).FullName}' does not have the 'Flags' attribute.");
        }

        public static decimal? ToDecimal(this object o)
        {
            return decimal.TryParse(o.ToString(), out var r) ? (decimal?)r : null;
        }

        public static T CastOrDefault<T>(this object o)
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

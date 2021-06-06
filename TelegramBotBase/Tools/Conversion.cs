using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Form;

namespace TelegramBotBase.Tools
{
    public static class Conversion
    {
        public static void CustomConversionChecks(FormBase form, KeyValuePair<string, object> p, System.Reflection.PropertyInfo f)
        {
            //Newtonsoft Int64/Int32 converter issue
            if (f.PropertyType == typeof(Int32))
            {
                int i = 0;
                if (int.TryParse(p.Value.ToString(), out i))
                {
                    f.SetValue(form, i);
                }
                return;
            }

            //Newtonsoft Double/Decimal converter issue
            if (f.PropertyType == typeof(Decimal) | f.PropertyType == typeof(Nullable<Decimal>))
            {
                decimal d = 0;
                if (decimal.TryParse(p.Value.ToString(), out d))
                {
                    f.SetValue(form, d);
                }
                return;
            }


        }

    }
}

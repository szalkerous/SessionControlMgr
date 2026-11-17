using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SessionControlMgr
{
    public class NumericValidationRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }
        //public Type ValidationType { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int valueint = 0;

            try
            {
                if (((string)value).Length > 0)
                    valueint = Int32.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Illegal characters or {e.Message}");
            }

            if ((valueint < Min) || (valueint > Max))
            {
                return new ValidationResult(false,
                  $"Enter {Min}-{Max}");
            }
            return ValidationResult.ValidResult;

            //string strValue = Convert.ToString(value);

            //if (string.IsNullOrEmpty(strValue))
            //    return new ValidationResult(false, $"Value cannot be coverted to string.");
            //bool canConvert = false;
            //switch (ValidationType.Name)
            //{

            //    case "Boolean":
            //        bool boolVal = false;
            //        canConvert = bool.TryParse(strValue, out boolVal);
            //        return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of boolean");
            //    case "Int32":
            //        int intVal = 0;
            //        canConvert = int.TryParse(strValue, out intVal);
            //        return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Int32");
            //    case "Double":
            //        double doubleVal = 0;
            //        canConvert = double.TryParse(strValue, out doubleVal);
            //        return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Double");
            //    case "Int64":
            //        long longVal = 0;
            //        canConvert = long.TryParse(strValue, out longVal);
            //        return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Int64");
            //    default:
            //        throw new InvalidCastException($"{ValidationType.Name} is not supported");
            //}
        }
    }
}

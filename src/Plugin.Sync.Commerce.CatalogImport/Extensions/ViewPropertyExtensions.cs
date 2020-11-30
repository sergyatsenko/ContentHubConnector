using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.EntityViews;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public static class ViewPropertyExtensions
    {
        public static void ParseValueAndSetEntityView(this ViewProperty fieldProperty, string value)
        {
            try
            {
                if (fieldProperty != null)
                {
                    switch (fieldProperty.OriginalType)
                    {
                        case "System.String":
                            if (string.IsNullOrEmpty(value) || value.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                            {
                                value = null;
                            }
                            fieldProperty.RawValue = value;
                            fieldProperty.Value = value;
                            return;
                        case "System.Int64":
                            if (Int64.TryParse(value, out Int64 intValue))
                            {
                                fieldProperty.Value = intValue.ToString();
                                fieldProperty.RawValue = intValue;
                            }
                            else
                            {
                                fieldProperty.Value = "";
                                fieldProperty.RawValue = "";
                            }

                            break;
                        case "System.Int32":
                            if (Int32.TryParse(value, out Int32 int32Value))
                            {
                                fieldProperty.Value = int32Value.ToString();
                                fieldProperty.RawValue = int32Value;
                            }
                            else
                            {
                                fieldProperty.Value = "";
                                fieldProperty.RawValue = "";
                            }
                            break;
                        case "System.DateTimeOffset":

                            if (DateTimeOffset.TryParse(value, out DateTimeOffset dateTimeOffsetValue))
                            {
                                fieldProperty.Value = dateTimeOffsetValue.ToString("s");
                                fieldProperty.RawValue = dateTimeOffsetValue;
                            }
                            else
                            {
                                fieldProperty.Value = "";
                                fieldProperty.RawValue = "";
                            }

                            break;
                        case "System.Decimal":
                            if (Decimal.TryParse(value, out decimal decimalValue))
                            {
                                fieldProperty.Value = decimalValue.ToString();
                                fieldProperty.RawValue = decimalValue;
                            }
                            else
                            {
                                fieldProperty.Value = "";
                                fieldProperty.RawValue = "";
                            }

                            break;
                        case "System.Boolean":
                            if (Boolean.TryParse(value, out Boolean boolValue))
                            {
                                fieldProperty.Value = boolValue.ToString();
                                fieldProperty.RawValue = boolValue;
                            }
                            else
                            {
                                throw new Exception($"Boolean field Error: Only True and False allow in {fieldProperty.Name} ");
                                // fieldProperty.Value = "";
                                // fieldProperty.RawValue = "";
                            }

                            break;
                        default:
                            throw new ArgumentException("DataType is not supported");
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = $"[{fieldProperty.Name}] Composer Field error, All composer fields are case sensitive, Error=[{ex.Message}]";
                throw new Exception(msg);
            }
        }
    }
}
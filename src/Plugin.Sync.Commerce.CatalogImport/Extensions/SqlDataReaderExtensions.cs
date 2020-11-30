using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Serilog;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public static class SqlDataReaderExtensions
    {
        private static int BatchSize = 500;

        //slow code - reflection is slow
        public static List<T> CreateList<T>(this SqlDataReader reader)
        {
            var results = new List<T>();
            var properties = typeof(T).GetProperties();
            var recordCount = 0;
            while (reader.Read())
            {
                recordCount++;
                if (recordCount % BatchSize == 0)
                {
                    Log.Information($"Finished fetching {recordCount} rows so far from UVS...");
                }
                var item = Activator.CreateInstance<T>();
                foreach (var property in properties)
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                    {
                        Type convertTo = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        property.SetValue(item, Convert.ChangeType(reader[property.Name], convertTo), null);
                    }
                }
                results.Add(item);
            }
            return results;
        }
    }
}
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    /// <summary>
    /// Json helper methods
    /// </summary>
    public static class JObjectExtensions
    {
        /// <summary>
        /// Get Catalog name from input Json or fallback to default Catalog name in Mapping Policy configuration
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="mappingPolicy"></param>
        /// <returns></returns>
        public static string GetCatalogName(this JObject jsonData, MappingConfiguration mappingConfiguration)
        {
            return jsonData.SelectValue<string>(mappingConfiguration.CatalogName);
        }

        /// <summary>
        /// Get Parent Category name from input Json or fallback to default Category name in Mapping Policy configuration
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="mappingPolicy"></param>
        /// <returns></returns>
        //public static string GetParentCategoryName(this JObject jsonData, MappingPolicyBase mappingPolicy)
        //{
        //    var categoryName = jsonData.SelectValue<string>(mappingPolicy.ParentCategoryName);
        //    if (string.IsNullOrEmpty(categoryName) && !string.IsNullOrEmpty(mappingPolicy.DefaultCategoryName))
        //    {
        //        categoryName = mappingPolicy.DefaultCategoryName;
        //    }

        //    return categoryName;
        //}

        public static string GetFieldValue(JObject jsonData, string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath))
            {
                return null;
            }

            var token = jsonData.SelectToken(jsonPath);
            if (token != null)
            {
                var value = token.Value<string>();
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }

        //public static CommerceEntityData GetFieldValues<T>(this JObject requestJson, T mappingPolicy) where T : MappingPolicyBase
        //{
        //    var comparer = StringComparer.OrdinalIgnoreCase;
        //    var entityData = new CommerceEntityData { Fields = new Dictionary<string, string>()};
        //    //entityData.Id = 
        //    //var fieldValues = new Dictionary<string, string>(comparer);

        //    foreach (var mapping in mappingPolicy.ComposerFieldsPaths)
        //    {
        //        if (!string.IsNullOrEmpty(mapping.Key) && !string.IsNullOrEmpty(mapping.Value))
        //        {
        //            var fieldValue = GetFieldValue(requestJson, mapping.Value);
        //            if (!string.IsNullOrEmpty(fieldValue))
        //            {
        //                entityData.Fields.Add(mapping.Key, fieldValue);
        //            }
        //        }
        //    }

        //    if (mappingPolicy.RootPaths != null)
        //    {
        //        foreach (var rootPath in mappingPolicy.RootPaths)
        //        {
        //            var roots = requestJson.SelectTokens(rootPath);
        //            if (roots != null)
        //            {
        //                foreach (var root in roots)
        //                {
        //                    if (root != null)
        //                    {
        //                        foreach (JProperty field in root.Children())
        //                        {
        //                            if (field != null && !string.IsNullOrEmpty(field.Name))
        //                            {
        //                                var fieldValue = field.Value<string>();
        //                                if (!string.IsNullOrEmpty(fieldValue))
        //                                {
        //                                    entityData.Fields.Add(field.Name, fieldValue); 
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return entityData;
        //}

        public static Dictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null) return new Dictionary<string, T>();

            var comparer = StringComparer.OrdinalIgnoreCase;
            var dictionary = new Dictionary<string, T>(comparer);
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);
                if (IsOfType<T>(value))
                {
                    dictionary.Add(property.Name, (T)value);
                }
                else if (IsOfTypeListOfStrings(value))
                {
                    var arr = ((IEnumerable)value).Cast<object>().Select(x => x.ToString()).ToArray();
                    object convertedValue = string.Join(",", arr);
                    dictionary.Add(property.Name, (T)convertedValue);
                }
            }
            return dictionary;
        }

       

        public static Dictionary<string, T> ToDictionary<T>(this JObject requestJson, string token)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            var collection = new Dictionary<string, T>(comparer);

            var roots = requestJson.SelectTokens(token);
            if (roots != null)
            {
                foreach (JProperty field in roots.Children())
                {
                    if (field != null && field.Value != null)
                    {
                        //var token = field as JToken;
                        var fieldValue = field.Value?.ToString();
                        if (!string.IsNullOrEmpty(fieldValue))
                        {
                            collection.Add(field.Name, (T)Convert.ChangeType(fieldValue, typeof(T)));
                        }
                    }
                }
            }
            return collection;
        }

        private static bool IsOfTypeListOfStrings(object value)
        {
            return value is List<string>;
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
        }
    }
}
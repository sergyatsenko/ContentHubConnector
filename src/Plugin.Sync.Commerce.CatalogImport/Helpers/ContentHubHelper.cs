using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Diagnostics;
using Sitecore.Framework.Caching;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Helpers
{
    public class ContentHubHelper
    {
        private static string CACHE_NAME = "CH_tokens";
        ICacheManager _cacheManager;
        public ContentHubHelper(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task<JObject> GetEntityByUrl(string entityUrl, ContentHubConnectionSettings contentHubConnectionSettings)
        {
            var request = WebRequest.Create(entityUrl);
            request.Method = "GET";
            var token = await GetToken(contentHubConnectionSettings).ConfigureAwait(false);
            request.Headers.Add("X-Auth-Token", token);

            string responseContent = null;
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        responseContent = streamReader.ReadToEnd();
                    }
                }
            }
            if (!string.IsNullOrEmpty(responseContent))
            {
                return JObject.Parse(responseContent);
            }

            return null;
        }

        public async Task<JObject> GetEntityById(string entityId, ContentHubConnectionSettings contentHubConnectionSettings)
        {
            var entityUrl = $"{contentHubConnectionSettings.ProtocolAndHost}/api/entities/{entityId}";
            return await GetEntityByUrl(entityUrl, contentHubConnectionSettings);
        }

        /// <summary>
        /// Get security token to use for Content Hub API calls
        /// </summary>
        /// <param name="contentHubPolicy"></param>
        /// <returns></returns>
        public async Task<string> GetToken(ContentHubConnectionSettings contentHubConnectionSettings)
        {
            try
            {
                var cache = _cacheManager.GetCache(CACHE_NAME);
                if (cache == null)
                {
                    cache = _cacheManager.CreateCache(CACHE_NAME);
                }

                string securityToken = await cache.Get<string>(contentHubConnectionSettings.InstanceName).ConfigureAwait(false);
                if (string.IsNullOrEmpty(securityToken))
                {
                    var request = WebRequest.Create(string.Format("{0}/api/authenticate", contentHubConnectionSettings.ProtocolAndHost));
                    request.Method = "POST";
                    request.ContentType = "application/json";

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        var jsonObject = new
                        {
                            user_name = contentHubConnectionSettings.UserName,
                            password = contentHubConnectionSettings.Password,
                            discard_existing = false
                        };

                        streamWriter.Write(JsonConvert.SerializeObject(jsonObject));
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    string responseContent = null;
                    using (var response = request.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                responseContent = streamReader.ReadToEnd();
                            }
                        }
                    }

                    var o = JObject.Parse(responseContent);
                    securityToken = (string)o["token"];

                    var cacheEntryOptions = new CacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3000)
                    };
                    await cache.SetString(contentHubConnectionSettings.InstanceName, securityToken, cacheEntryOptions).ConfigureAwait(false);
                }
                //Log.Warn($"retrieved CH token: {securityToken}", this);
                return securityToken;
            }
            catch (Exception ex)
            {
                Log.Error("Error retrieving Content Hub token", ex, this);
                throw;
            }
        }
    }
}

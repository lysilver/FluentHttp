using FluentHttp.Ext.LoadBalancing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FluentHttp.Ext
{
    public static class HttpExt
    {
        public static string GetUrl(IContext context, IChooseUrl chooseUrl, string appId, string url)
        {
            if (context is not null)
            {
                var services = context.GetUrls().ConfigureAwait(false).GetAwaiter().GetResult();
                if (services is null)
                {
                    return url;
                }
                if (services.TryGetValue(appId, out ClusterConfig? cluster) && cluster is not null)
                {
                    string? baseUrl;
                    if (chooseUrl is not null)
                    {
                        baseUrl = chooseUrl.GetUrl(cluster);
                    }
                    else
                    {
                        baseUrl = cluster.Destinations.FirstOrDefault()?.BaseUrl;
                    }
                    // client.BaseAddress = new Uri(baseUrl);
                    if (!string.IsNullOrWhiteSpace(baseUrl))
                    {
                        url = $"{baseUrl}/{url}";
                    }
                }
            }
            return url;
        }

        private static void CreateHeader(HttpClient client, Dictionary<string, object>? header = null, string? auth = null)
        {
            if (client is not null && header is not null)
            {
                if (string.IsNullOrWhiteSpace(auth))
                {
                    foreach (var item in header)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value.ToString());
                    }
                }
                else
                {
                    foreach (var item in header)
                    {
                        if (auth != item.Key)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value.ToString());
                        }
                    }
                    if (header.TryGetValue(auth, out object? authorization) && authorization is not null)
                    {
                        client.DefaultRequestHeaders.Add("Authorization", authorization.ToString());
                    }
                }
            }
        }

        public static async Task<TResponse?> HttpGet<TResponse>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            return await client.GetFromJsonAsync<TResponse>(url, context.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpPost<TResponse, TRequest>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, TRequest data,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            var res = await client.PostAsJsonAsync(url, data);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpPut<TResponse, TRequest>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, TRequest data,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            var res = await client.PutAsJsonAsync(url, data);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpDelete<TResponse, TRequest>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, TRequest data,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Delete,
                Content = new StringContent(JsonSerializer.Serialize(data, context.JsonSerializerOptions()))
            };
            var res = await client.SendAsync(httpRequest);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpDelete<TResponse>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            var res = await client.DeleteAsync(url);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        /// <summary>
        /// 单文件上传
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="client"></param>
        /// <param name="context"></param>
        /// <param name="appId"></param>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, string filePath,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(filePath);
            var scontent = new StreamContent(fileStream, (int)fileStream.Length);
            content.Add(scontent, "file", fileName);
            var res = await client.PostAsync(url, content);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        /// <summary>
        /// 多文件上传
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="client"></param>
        /// <param name="context"></param>
        /// <param name="appId"></param>
        /// <param name="url"></param>
        /// <param name="filePaths"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, List<string> filePaths,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                var scontent = new StreamContent(fileStream, (int)fileStream.Length);
                content.Add(scontent, "files", fileName);
            }
            var res = await client.PostAsync(url, content);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, List<FileStream> streams,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            foreach (var stream in streams)
            {
                var scontent = new StreamContent(stream, (int)stream.Length);
                content.Add(scontent, "files", stream.Name);
            }
            var res = await client.PostAsync(url, content);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        public static Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, FileStream stream,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            var scontent = new StreamContent(stream, (int)stream.Length);
            content.Add(scontent, "file", stream.Name);
            var res = client.PostAsync(url, content).ConfigureAwait(false).GetAwaiter().GetResult();
            return res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        public static async Task<TResponse?> Http<TResponse, TRequest>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, TRequest data, string method,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(method)
            };
            // httpRequest.Headers.Add("Accept", "application/json");
            switch (method)
            {
                case "POST":
                case "PUT":
                    httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, context.JsonSerializerOptions()));
                    break;

                default:
                    break;
            }
            var res = await client.SendAsync(httpRequest);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        public static async Task<TResponse?> Http<TResponse, TRequest>(this HttpClient client, IContext context,
            HttpRequestValues<TRequest> requestValues)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            CreateHeader(client, requestValues.Header, requestValues.Auth);
            HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(requestValues.Url),
                Method = new HttpMethod(requestValues.Method)
            };
            switch (requestValues.Method)
            {
                case "POST":
                case "PUT":
                    httpRequest.Content = new StringContent(JsonSerializer.Serialize(requestValues.Value, context.JsonSerializerOptions()));
                    break;

                default:
                    break;
            }
            var res = await client.SendAsync(httpRequest);
            return await res.Content.ReadFromJsonAsync<TResponse>(context.JsonSerializerOptions());
        }

        public static async Task<string> Http<TRequest>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, TRequest data, string method,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(method)
            };
            switch (method)
            {
                case "POST":
                case "PUT":
                    httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, context.JsonSerializerOptions()));
                    break;

                case "DELETE":
                    if (data is not null)
                    {
                        httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, context.JsonSerializerOptions()));
                    }
                    break;

                default:
                    break;
            }
            var res = await client.SendAsync(httpRequest);
            return await res.Content.ReadAsStringAsync();
        }

        public static async Task<Stream?> HttpStream<TRequest>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, TRequest data, string method,
            Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(method)
            };
            switch (method)
            {
                case "POST":
                case "PUT":
                    httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, context.JsonSerializerOptions()));
                    break;

                case "DELETE":
                    if (data is not null)
                    {
                        httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, context.JsonSerializerOptions()));
                    }
                    break;

                default:
                    break;
            }
            var res = await client.SendAsync(httpRequest);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsStreamAsync();
            }
            return await Task.FromResult<Stream?>(null);
        }

        public static async Task DownloadRangeChunkAsync<TRequest>(this HttpClient client,
            IContext context, IChooseUrl chooseUrl, string appId, string url, TRequest data, string method,
            long fileSize, Dictionary<string, object>? header = null, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(client));
            url = GetUrl(context, chooseUrl, appId, url);
            CreateHeader(client, header, auth);
            var bufferSize = 1024 * 1024;
            var tasks = new Task[(int)Math.Ceiling((double)fileSize / bufferSize)];
            for (var i = 0; i < tasks.Length; i++)
            {
                var rangeStart = i * bufferSize;
                var rangeEnd = Math.Min(rangeStart + bufferSize - 1, fileSize - 1);
                tasks[i] = DownloadChunkAsync(client, context, url, data, method, rangeStart, rangeEnd, "fileName");
            }
            await Task.WhenAll(tasks);
        }

        private static async Task DownloadChunkAsync<TRequest>(HttpClient httpClient,
            IContext context, string url, TRequest data, string method,
            long rangeStart, long rangeEnd, string fileName)
        {
            HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(method)
            };
            switch (method)
            {
                case "POST":
                    httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, context.JsonSerializerOptions()));
                    break;

                default:
                    break;
            }
            httpRequest.Headers.Range = new RangeHeaderValue(rangeStart, rangeEnd);
            var response = await httpClient.SendAsync(httpRequest);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
            fileStream.Seek(rangeStart, SeekOrigin.Begin);
            await stream.CopyToAsync(fileStream);
        }
    }
}
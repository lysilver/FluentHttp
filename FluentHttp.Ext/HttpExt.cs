﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FluentHttp.Ext
{
    public static class HttpExt
    {
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
            IHttpClientAdapter clientAdapter, string appId, string url, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            return await client.GetFromJsonAsync<TResponse>(url, clientAdapter.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpPost<TResponse, TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data,
            string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            var res = await client.PostAsJsonAsync(url, data);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpPut<TResponse, TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data,
            string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            var res = await client.PutAsJsonAsync(url, data);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpPatch<TResponse, TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data,
            string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            var res = await client.HttpResponseMessageAsync(clientAdapter, appId,
                url, data, "PATCH", auth);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpDelete<TResponse, TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data,
            string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Delete,
                Content = new StringContent(JsonSerializer.Serialize(data, clientAdapter.JsonSerializerOptions()))
            };
            var res = await client.SendAsync(httpRequest);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        public static async Task<TResponse?> HttpDelete<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url,
            string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            var res = await client.DeleteAsync(url);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        /// <summary>
        /// 单文件上传
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="client"></param>
        /// <param name="clientAdapter"></param>
        /// <param name="appId"></param>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// <param name="auth"></param>
        /// <param name="fromData">传递参数</param>
        /// <returns></returns>
        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, string filePath,
            string? auth = null, Dictionary<string, string>? fromData = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(filePath);
            var scontent = new StreamContent(fileStream, (int)fileStream.Length);
            if (fromData is not null)
            {
                foreach (var (key, value) in fromData)
                {
                    content.Add(new StringContent(value), key);
                }
            }
            content.Add(scontent, "file", fileName);
            var res = await client.PostAsync(url, content);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        /// <summary>
        /// 多文件上传
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="client"></param>
        /// <param name="clientAdapter"></param>
        /// <param name="appId"></param>
        /// <param name="url"></param>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, List<string> filePaths,
            string? auth = null, Dictionary<string, string>? fromData = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                var scontent = new StreamContent(fileStream, (int)fileStream.Length);
                content.Add(scontent, "files", fileName);
            }
            if (fromData is not null)
            {
                foreach (var (key, value) in fromData)
                {
                    content.Add(new StringContent(value), key);
                }
            }
            var res = await client.PostAsync(url, content);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, List<FileStream> streams,
            string? auth = null, Dictionary<string, string>? fromData = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            foreach (var stream in streams)
            {
                var scontent = new StreamContent(stream, (int)stream.Length);
                content.Add(scontent, "files", stream.Name);
            }
            if (fromData is not null)
            {
                foreach (var (key, value) in fromData)
                {
                    content.Add(new StringContent(value), key);
                }
            }
            var res = await client.PostAsync(url, content);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, FileStream stream,
            string? auth = null, Dictionary<string, string>? fromData = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            var scontent = new StreamContent(stream, (int)stream.Length);
            content.Add(scontent, "file", stream.Name);
            if (fromData is not null)
            {
                foreach (var (key, value) in fromData)
                {
                    content.Add(new StringContent(value), key);
                }
            }
            var res = await client.PostAsync(url, content);
            return await res.Content.ReadFromJsonAsync<TResponse>(clientAdapter.JsonSerializerOptions());
        }

        public static async Task<string> Http<TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data, string method,
            string? auth = null)
        {
            var res = await client.HttpResponseMessageAsync(clientAdapter, appId,
                url, data, method, auth);
            return await res.Content.ReadAsStringAsync();
        }

        public static async Task<HttpResponseMessage> HttpResponseMessageAsync<TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data, string method,
            string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(method)
            };

            switch (method)
            {
                case "POST":
                case "PUT":
                    httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, clientAdapter.JsonSerializerOptions()), Encoding.UTF8, "application/json");
                    break;

                case "PATCH":
                    httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, clientAdapter.JsonSerializerOptions()), Encoding.UTF8, "application/json-patch+json");
                    break;

                case "DELETE":
                    if (data is not null)
                    {
                        httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, clientAdapter.JsonSerializerOptions()), Encoding.UTF8, "application/json");
                    }
                    break;

                default:
                    break;
            }
            var res = await client.SendAsync(httpRequest);
            return res;
        }

        public static async Task<Stream?> HttpStream<TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data, string method,
            string? auth = null)
        {
            var res = await client.HttpResponseMessageAsync(clientAdapter, appId,
                url, data, method, auth);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsStreamAsync();
            }
            return await Task.FromResult<Stream?>(null);
        }

        public static async Task DownloadRangeChunkAsync<TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data, string method,
            long fileSize, string fileName, string? auth = null)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            CreateHeader(client, await clientAdapter.GetHeader(appId), auth);
            var bufferSize = 1024 * 1024;
            var tasks = new Task[(int)Math.Ceiling((double)fileSize / bufferSize)];
            for (var i = 0; i < tasks.Length; i++)
            {
                var rangeStart = i * bufferSize;
                var rangeEnd = Math.Min(rangeStart + bufferSize - 1, fileSize - 1);
                tasks[i] = DownloadChunkAsync(client, clientAdapter, url, data, method, rangeStart, rangeEnd, fileName);
            }
            await Task.WhenAll(tasks);
        }

        private static async Task DownloadChunkAsync<TRequest>(HttpClient httpClient,
            IHttpClientAdapter clientAdapter, string url, TRequest data, string method,
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
                    httpRequest.Content = new StringContent(JsonSerializer.Serialize(data, clientAdapter.JsonSerializerOptions()));
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
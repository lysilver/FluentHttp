using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FluentHttp.Ext
{
    public static class HttpExt
    {
        private static void ApplyHeaders(HttpRequestMessage request, Dictionary<string, string>? headers, string? auth = null)
        {
            if (headers is null) return;
            foreach (var kv in headers)
            {
                if (!string.Equals(kv.Key, auth, StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }
            if (!string.IsNullOrWhiteSpace(auth) && headers.TryGetValue(auth, out var authorization) && authorization is not null)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);
            }
        }

        private static string GetUploadFileName(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                return fileName;
            }
            return filePath?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? string.Empty;
        }

        public static async Task<TResponse?> HttpGet<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            using var response = await client.HttpResponseMessageAsync<object>(
               clientAdapter,
               appId,
               url,
               HttpMethod.Get,
               options: options,
               cancellationToken: cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(
                options,
                cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse?> HttpPost<TResponse, TRequest>(
            this HttpClient client, IHttpClientAdapter clientAdapter,
            string appId, string url, TRequest data,
            CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            using var response = await client.HttpResponseMessageAsync<TRequest>(
               clientAdapter,
               appId,
               url,
               HttpMethod.Post,
               data: data,
               options: options,
               cancellationToken: cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(
                options,
                cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse?> HttpPut<TResponse, TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data,
            CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            using var response = await client.HttpResponseMessageAsync<TRequest>(
               clientAdapter,
               appId,
               url,
               HttpMethod.Put,
               data: data,
               options: options,
               cancellationToken: cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(
                options,
                cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse?> HttpPatch<TResponse, TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data,
            CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            using var response = await client.HttpResponseMessageAsync<TRequest>(
               clientAdapter,
               appId,
               url,
               HttpMethod.Patch,
               data: data,
               options: options,
               contentType: HttpContentType.JsonPatch,
               cancellationToken: cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(
                options,
                cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse?> HttpDelete<TResponse, TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data,
            CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            using var response = await client.HttpResponseMessageAsync<TRequest>(
               clientAdapter,
               appId,
               url,
               HttpMethod.Delete,
               data: data,
               options: options,
               contentType: HttpContentType.Json,
               cancellationToken: cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(
                options,
                cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse?> HttpDelete<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url,
            CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            using var response = await client.HttpResponseMessageAsync<object>(
               clientAdapter,
               appId,
               url,
               HttpMethod.Delete,
               options: options,
               contentType: HttpContentType.Json,
               cancellationToken: cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(
                options,
                cancellationToken).ConfigureAwait(false);
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
            Dictionary<string, string>? fromData = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));

            var options = clientAdapter.GetJsonSerializerOptions(appId);
            url = await clientAdapter.GetUrl(appId, url);

            using MultipartFormDataContent content = new();
            if (fromData is not null)
            {
                foreach (var (key, value) in fromData)
                {
                    content.Add(new StringContent(value), key);
                }
            }

            var fileName = GetUploadFileName(filePath);
            using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var scontent = new StreamContent(fileStream);
            content.Add(scontent, "file", fileName);

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            await clientAdapter.ConfigureRequest(appId, request).ConfigureAwait(false);
            using var res = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken).ConfigureAwait(false);
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
            Dictionary<string, string>? fromData = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            ArgumentNullException.ThrowIfNull(filePaths, nameof(filePaths));
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            url = await clientAdapter.GetUrl(appId, url);

            using MultipartFormDataContent content = new();
            if (fromData is not null)
            {
                foreach (var (key, value) in fromData)
                {
                    content.Add(new StringContent(value), key);
                }
            }

            foreach (var filePath in filePaths)
            {
                var fileName = GetUploadFileName(filePath);
                FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                var scontent = new StreamContent(fileStream);
                content.Add(scontent, "files", fileName);
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            await clientAdapter.ConfigureRequest(appId, request).ConfigureAwait(false);
            using var res = await client.SendAsync(request, cancellationToken);
            return await res.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken);
        }

        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, List<FileStream> streams,
            Dictionary<string, string>? fromData = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            var options = clientAdapter.GetJsonSerializerOptions(appId);

            url = await clientAdapter.GetUrl(appId, url);
            using MultipartFormDataContent content = new();

            foreach (var stream in streams)
            {
                var scontent = new StreamContent(stream);
                content.Add(scontent, "files", stream.Name);
            }

            if (fromData is not null)
            {
                foreach (var (key, value) in fromData)
                {
                    content.Add(new StringContent(value), key);
                }
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            await clientAdapter.ConfigureRequest(appId, request).ConfigureAwait(false);
            using var res = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return await res.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken);
        }

        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, FileStream stream,
            Dictionary<string, string>? fromData = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            url = await clientAdapter.GetUrl(appId, url);
            var options = clientAdapter.GetJsonSerializerOptions(appId);

            using MultipartFormDataContent content = new();
            var scontent = new StreamContent(stream);
            content.Add(scontent, "file", stream.Name);
            if (fromData is not null)
            {
                foreach (var (key, value) in fromData)
                {
                    content.Add(new StringContent(value), key);
                }
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            await clientAdapter.ConfigureRequest(appId, request).ConfigureAwait(false);
            using var res = await client.SendAsync(request, cancellationToken);
            return await res.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken);
        }

        public static async Task DownloadRangeChunkAsync<TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data, string method,
            long fileSize, string fileName)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            url = await clientAdapter.GetUrl(appId, url);
            var options = clientAdapter.GetJsonSerializerOptions(appId);

            var bufferSize = 1024 * 1024;
            var tasks = new Task[(int)Math.Ceiling((double)fileSize / bufferSize)];
            for (var i = 0; i < tasks.Length; i++)
            {
                var rangeStart = i * bufferSize;
                var rangeEnd = Math.Min(rangeStart + bufferSize - 1, fileSize - 1);
                tasks[i] = DownloadChunkAsync(client, clientAdapter, appId, url, data, method, rangeStart, rangeEnd, fileName, options);
            }
            await Task.WhenAll(tasks);
        }

        private static async Task DownloadChunkAsync<TRequest>(HttpClient httpClient,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data, string method,
            long rangeStart, long rangeEnd, string fileName, JsonSerializerOptions? options = null)
        {
            using HttpRequestMessage httpRequest = new()
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(method)
            };
            switch (method)
            {
                case "POST":
                    httpRequest.Content = JsonContent.Create(data, options: options);
                    break;

                default:
                    break;
            }
            await clientAdapter.ConfigureRequest(appId, httpRequest).ConfigureAwait(false);
            httpRequest.Headers.Range = new RangeHeaderValue(rangeStart, rangeEnd);
            using var response = await httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
            fileStream.Seek(rangeStart, SeekOrigin.Begin);
            await stream.CopyToAsync(fileStream);
        }

        public static async Task<HttpResponseMessage> HttpResponseMessageAsync<TRequest>(
            this HttpClient client,
            IHttpClientAdapter adapter,
            string appId,
            string url,
            HttpMethod method,
            TRequest? data = default,
            HttpContentType contentType = HttpContentType.Json,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(method);

            url = await adapter.GetUrl(appId, url);
            using var request = new HttpRequestMessage(method, url);
            if (data is not null && method != HttpMethod.Get)
            {
                request.Content = CreateContent(data, contentType, options);
            }
            await adapter.ConfigureRequest(appId, request).ConfigureAwait(false);

            return await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> SendAsync<TRequest>(
            this HttpClient client,
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);

            return await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
        }

        private static HttpContent CreateContent<TRequest>(
            TRequest data,
            HttpContentType contentType, JsonSerializerOptions? options = null)
        {
            switch (contentType)
            {
                case HttpContentType.Json:
                    return CreateJsonContent(data, "application/json", options);

                case HttpContentType.JsonPatch:
                    return CreateJsonContent(data, "application/json-patch+json", options);

                case HttpContentType.FormUrlEncoded:
                    if (data is IEnumerable<KeyValuePair<string, string>> form)
                    {
                        return new FormUrlEncodedContent(form);
                    }

                    throw new ArgumentException(
                        "FormUrlEncoded requires key-value pairs.");

                case HttpContentType.MultipartFormData:
                    if (data is MultipartFormDataContent multipart)
                    {
                        return multipart;
                    }

                    throw new ArgumentException(
                        "MultipartFormData requires MultipartFormDataContent.");

                default:
                    throw new ArgumentOutOfRangeException(nameof(contentType));
            }
        }

        private static JsonContent CreateJsonContent<T>(
            T data, string mediaType, JsonSerializerOptions? options = null)
        {
            var content = JsonContent.Create(data, options: options);
            content.Headers.ContentType!.MediaType = mediaType;
            return content;
        }
    }
}
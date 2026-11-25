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

        private static void AddFormData(MultipartFormDataContent content, Dictionary<string, string>? formData)
        {
            if (formData is null) return;
            foreach (var (key, value) in formData)
            {
                content.Add(new StringContent(value), key);
            }
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

        /// <summary>
        /// HEAD 请求，返回响应内容（不包含实体体，适合探测资源是否存在、获取响应头等）。
        /// </summary>
        public static async Task<TResponse?> HttpHead<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            using var response = await client.HttpResponseMessageAsync<object>(
               clientAdapter,
               appId,
               url,
               HttpMethod.Head,
               options: options,
               cancellationToken: cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(
                options,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// GET 请求，返回响应流（ResponseHeadersRead），适合大文件、SSE 等流式场景。
        /// 返回的 Stream 由调用方 Dispose，Dispose 时会同时释放 HttpResponseMessage。
        /// </summary>
        public static Task<Stream> HttpGetStream(this HttpClient client, IHttpClientAdapter clientAdapter,
            string appId, string url, CancellationToken cancellationToken = default)
        {
            return client.HttpSendStream<object>(clientAdapter, appId, url, HttpMethod.Get, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// POST 请求，返回响应流（ResponseHeadersRead），适合大文件、SSE 等流式场景。
        /// 返回的 Stream 由调用方 Dispose，Dispose 时会同时释放 HttpResponseMessage。
        /// </summary>
        public static Task<Stream> HttpPostStream<TRequest>(this HttpClient client, IHttpClientAdapter clientAdapter,
            string appId, string url, TRequest data, CancellationToken cancellationToken = default)
        {
            return client.HttpSendStream(clientAdapter, appId, url, HttpMethod.Post, data, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 发送请求并返回响应流（ResponseHeadersRead）。
        /// 非成功状态码抛出 HttpRequestException 并释放响应；
        /// 返回的 Stream 由调用方 Dispose，Dispose 时会同时释放 HttpResponseMessage（连接随之归还连接池）。
        /// </summary>
        public static async Task<Stream> HttpSendStream<TRequest>(this HttpClient client, IHttpClientAdapter clientAdapter,
            string appId, string url, HttpMethod method, TRequest? data = default,
            HttpContentType contentType = HttpContentType.Json, CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            var response = await client.HttpResponseMessageAsync<TRequest>(
                clientAdapter,
                appId,
                url,
                method,
                data: data,
                contentType: contentType,
                options: options,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                response.Dispose();
                throw;
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return new ResponseStream(stream, response);
        }

        /// <summary>
        /// 包装响应流：Dispose 时同时释放 HttpResponseMessage，确保连接归还连接池。
        /// </summary>
        private sealed class ResponseStream : Stream
        {
            private readonly Stream _inner;
            private readonly HttpResponseMessage _response;

            public ResponseStream(Stream inner, HttpResponseMessage response)
            {
                _inner = inner;
                _response = response;
            }

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => _inner.CanSeek;
            public override bool CanWrite => _inner.CanWrite;
            public override long Length => _inner.Length;

            public override long Position
            {
                get => _inner.Position;
                set => _inner.Position = value;
            }

            public override void Flush() => _inner.Flush();
            public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
            public override int Read(Span<byte> buffer) => _inner.Read(buffer);
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => _inner.ReadAsync(buffer, offset, count, cancellationToken);
            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
                => _inner.ReadAsync(buffer, cancellationToken);
            public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
            public override void SetLength(long value) => _inner.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
            public override void Write(ReadOnlySpan<byte> buffer) => _inner.Write(buffer);
            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => _inner.WriteAsync(buffer, offset, count, cancellationToken);
            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
                => _inner.WriteAsync(buffer, cancellationToken);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _inner.Dispose();
                    _response.Dispose();
                }
                base.Dispose(disposing);
            }

            public override async ValueTask DisposeAsync()
            {
                await _inner.DisposeAsync().ConfigureAwait(false);
                _response.Dispose();
                GC.SuppressFinalize(this);
                await base.DisposeAsync().ConfigureAwait(false);
            }
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
            HttpContentType contentType = HttpContentType.JsonPatch, CancellationToken cancellationToken = default)
        {
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            using var response = await client.HttpResponseMessageAsync<TRequest>(
               clientAdapter,
               appId,
               url,
               HttpMethod.Patch,
               data: data,
               options: options,
               contentType: contentType,
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
        /// <param name="formData">传递参数</param>
        /// <returns></returns>
        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, string filePath,
            Dictionary<string, string>? formData = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));

            var options = clientAdapter.GetJsonSerializerOptions(appId);
            url = await clientAdapter.GetUrl(appId, url);

            using MultipartFormDataContent content = new();
            AddFormData(content, formData);

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
            Dictionary<string, string>? formData = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            ArgumentNullException.ThrowIfNull(filePaths, nameof(filePaths));
            var options = clientAdapter.GetJsonSerializerOptions(appId);
            url = await clientAdapter.GetUrl(appId, url);

            using MultipartFormDataContent content = new();
            AddFormData(content, formData);

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
            using var res = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, List<FileStream> streams,
            Dictionary<string, string>? formData = null, CancellationToken cancellationToken = default)
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

            if (formData is not null)
            {
                foreach (var (key, value) in formData)
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
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 上传单个文件，请求完成后会 Dispose 传入的 stream。
        /// </summary>
        public static async Task<TResponse?> UploadFile<TResponse>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, FileStream stream,
            Dictionary<string, string>? formData = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            url = await clientAdapter.GetUrl(appId, url);
            var options = clientAdapter.GetJsonSerializerOptions(appId);

            using MultipartFormDataContent content = new();
            var scontent = new StreamContent(stream);
            content.Add(scontent, "file", GetUploadFileName(stream.Name));
            AddFormData(content, formData);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            await clientAdapter.ConfigureRequest(appId, request).ConfigureAwait(false);
            using var res = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken).ConfigureAwait(false);
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

        /// <summary>
        /// 以有界并发下载：同时最多 maxConcurrent 个分块任务并发，
        /// 某个分块下载完成后立即继续下一个，直到所有分块完成。
        /// </summary>
        /// <param name="maxConcurrent">同时并发的最大下载任务数，默认 8</param>
        public static async Task DownloadFileAsync<TRequest>(this HttpClient client,
            IHttpClientAdapter clientAdapter, string appId, string url, TRequest data, string method,
            long fileSize, string fileName, int maxConcurrent = 8)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));
            ArgumentOutOfRangeException.ThrowIfNegative(fileSize);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrent, 1);

            url = await clientAdapter.GetUrl(appId, url).ConfigureAwait(false);
            var options = clientAdapter.GetJsonSerializerOptions(appId);

            var bufferSize = 1024 * 1024;
            var chunkCount = (int)Math.Ceiling((double)fileSize / bufferSize);
            var inFlight = new HashSet<Task>();
            var index = 0;

            while (index < chunkCount || inFlight.Count > 0)
            {
                while (index < chunkCount && inFlight.Count < maxConcurrent)
                {
                    var rangeStart = (long)index * bufferSize;
                    var rangeEnd = Math.Min(rangeStart + bufferSize - 1, fileSize - 1);
                    inFlight.Add(DownloadChunkAsync(client, clientAdapter, appId, url, data, method, rangeStart, rangeEnd, fileName, options));
                    index++;
                }
                if (inFlight.Count == 0)
                {
                    break;
                }
                var completed = await Task.WhenAny(inFlight).ConfigureAwait(false);
                inFlight.Remove(completed);
                await completed.ConfigureAwait(false);
            }
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
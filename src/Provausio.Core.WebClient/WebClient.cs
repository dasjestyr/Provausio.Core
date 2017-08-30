using System;
using System.Net.Http;
using System.Threading.Tasks;
using Provausio.Core.Ext;
using Provausio.Core.Logging;
using Newtonsoft.Json;

namespace Provausio.Core.WebClient
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IDisposable" />
    public class WebClient : IDisposable
    {
        internal readonly HttpClient HttpClient;
        private ILogger _logger;

        /// <summary>
        /// The max length of the response body that will be logged.
        /// </summary>
        public int MaxBodyLogLength { get; set; } = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebClient"/> class.
        /// </summary>
        public WebClient()
        {
            HttpClient = new HttpClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebClient"/> class.
        /// </summary>
        /// <param name="client">The HTTP client.</param>
        public WebClient(HttpClient client)
        {
            HttpClient = client;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebClient"/> class.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public WebClient(HttpMessageHandler handler)
        {
            HttpClient = new HttpClient(handler);
        }

        /// <summary>
        /// Executes the web request and returns a raw <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendAsync(WebRequest request)
        {
            var httpRequest = request.GetHttpRequest();
            var headers = request.ResourceBuilder.GetHeaders();
            
            foreach(var header in headers)
                httpRequest.Headers.Add(header.Key, header.Value);

            var response = await SendAsync(httpRequest).ConfigureAwait(false);
            request.ResetBuilder();
            return response;
        }

        /// <summary>
        /// Executes the web request and returns a raw <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            try
            {
                _logger?.Verbose("Making request {@Request}", this, new
                {
                    Method = request.Method.ToString(),
                    Uri = request.RequestUri
                });

                var response = await HttpClient
                    .SendAsync(request)
                    .ConfigureAwait(false);

                _logger?.Verbose("{@Result}", this, new
                {
                    RequestUrl = $"{request.Method} {request.RequestUri}",
                    StatusCode = $"{(int) response.StatusCode} {response.StatusCode}",
                    Body = response.Content?.ReadAsStringAsync()
                        .Result
                        .Truncate(MaxBodyLogLength)
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger?.Fatal("Request failed {@Request}", this, ex, new
                {
                    ex.Message,
                    Request = $"{request.Method} {request.RequestUri}"
                });
                throw;
            }
        }

        /// <summary>
        /// Executes the request and attempts to deserialize the response body. If the body is null or empty, a null will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<T> SendAsync<T>(WebRequest request)
            where T : class, new()
        {
            var response = await SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;

            var obj = await DeserializeBody<T>(request.GetHttpRequest(), response);

            return obj;
        }

        /// <summary>
        /// Executes the request and runs the provider mapper delgate, using the response body as an input parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns></returns>
        public async Task<T> SendAsync<T>(WebRequest request, Func<string, T> mapper)
            where T : class
        {
            var response = await SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            var body = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            return mapper(body);
        }

        private async Task<T> DeserializeBody<T>(HttpRequestMessage request, HttpResponseMessage response)
            where T : class, new()
        {
            // TODO: support other content types.
            var readAsStringAsync = response.Content?.ReadAsStringAsync();
            if (readAsStringAsync == null)
                return null;
            
            var body = await readAsStringAsync;
            try
            {
                
                if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(body))
                    return null;

                return await Task.Run(() => JsonConvert.DeserializeObject<T>(body)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.Fatal("Deserialization Failed {@Details}", this, ex, new
                {
                    Status = $"{(int) response.StatusCode} {response.StatusCode}",
                    Request = $"{request.Method} {request.RequestUri}",
                    Body = body
                });
                throw;
            }
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            HttpClient.Dispose();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Provausio.Core.WebClient.Infrastructure;

namespace Provausio.Core.WebClient
{
    /*****************************************************************************************************************
     * There are some confusing design decisions here. The reason that this client implements the resource builder
     * is simply for convenience. A client can choose to use separate request/client objects or just use the client
     * directly to build a request and execute it. I'm not sure if I like this... 
     * 
     * TODO: reconsider this functionality, or at least make it more clear as to its intent
     * Some of the design side effects is that the the builder is forced to attach the client so that it can provide
     * AsClient() in order to return the original client while using the Fluid builder pattern which is kind of 
     * awkward. As a result, there are ctors that do and do not accept instances of the resource builder which is also 
     * awkward.
     *****************************************************************************************************************/

    /// <summary>
    /// The <see cref="RestClient"/> provides an interface responsible for building the final request and sending it.
    /// </summary>
    /// <seealso cref="IResourceBuilder" />
    public class RestClient : IResourceBuilder, IDisposable
    {
        private readonly IResourceBuilder _builder;
        private WebClient _webClient;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient"/> class.
        /// </summary>
        public RestClient()
            : this(new ResourceBuilder())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient"/> class.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="host">The host.</param>
        public RestClient(Scheme scheme, string host) 
            : this()
        {
            _builder
                .WithScheme(scheme)
                .WithHost(host)
                .WithClient(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient"/> class.
        /// This supports the attachment of a builder. For example, if a builder is being passed around for modification before initializing the client and the caller still wants to be able to use the fluid interface ON the client itself.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public RestClient(IResourceBuilder builder)
        {
            _webClient = new WebClient();   
            _builder = builder;
            _builder
                .WithClient(this);
        }

        /// <summary>
        /// Sets the http message handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void SetHandler(HttpMessageHandler handler)
        {
            _webClient = new WebClient(handler);
        }

        /// <summary>
        /// Executes a GET request asychronously
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _builder.BuildUri());
            return await Send(request);
        }
        
        /// <summary>
        /// Executes a DELETE request asychronously
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, _builder.BuildUri());
            return await Send(request);
        }
        
        /// <summary>
        /// Executes a POST request asychronously
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _builder.BuildUri());
            return await Send(request);
        }
        
        /// <summary>
        /// Executes a POST request asychronously. Will also attach a payload with the request.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _builder.BuildUri());
            return await Send(request, content);
        }

        /// <summary>
        /// Executes a PUT request asychronously. Will also attach a payload with the request.
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Put, _builder.BuildUri());
            return await Send(request);
        }
        
        /// <summary>
        /// Executes a PUT request asychronously. Will also attach a payload with the request.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutAsync(HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, _builder.BuildUri());
            return await Send(request, content);
        }

        private async Task<HttpResponseMessage> Send(HttpRequestMessage request, HttpContent content = null)
        {
            if(_disposed)
                throw new ObjectDisposedException(nameof(_webClient));

            if (content != null)
                request.Content = content;
            
            foreach (var header in _builder.GetHeaders())
                request.Headers.Add(header.Key, header.Value);

            var userAgent = _builder.GetUserAgent();
            if (!string.IsNullOrEmpty(userAgent))
                _webClient.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(userAgent));

            return await _webClient
                .SendAsync(request)
                .ConfigureAwait(false);
        }
        
        #region -- IResourceBuilder Implementation --
        
        public IResourceBuilder WithScheme(Scheme scheme)
        {
            return _builder.WithScheme(scheme);
        }

        public IResourceBuilder WithHost(string host)
        {
            return _builder.WithHost(host);
        }

        public IResourceBuilder WithPort(uint port)
        {
            return _builder.WithPort(port);
        }

        public IResourceBuilder WithPath(string path)
        {
            return _builder.WithPath(path);
        }

        public IResourceBuilder WithQueryParameters(object parameters)
        {
            return _builder.WithQueryParameters(parameters);
        }

        public IResourceBuilder WithQueryParameters(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            return _builder.WithQueryParameters(parameters);
        }

        public IResourceBuilder WithSegmentPair(string name, string value)
        {
            return _builder.WithSegmentPair(name, value);
        }

        public IResourceBuilder WithHeaders(IDictionary<string, string> headers)
        {
            return _builder.WithHeaders(headers);
        }

        public Uri BuildUri()
        {
            return _builder.BuildUri();
        }

        public IDictionary<string, string> GetHeaders()
        {
            return _builder.GetHeaders();
        }

        public IResourceBuilder WithClient(RestClient client)
        {
            _builder.WithClient(client);
            return this;
        }

        public RestClient AsClient()
        {
            return _builder.AsClient();
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
                return;
            
            _webClient.Dispose();
        }

        public IResourceBuilder WithUserAgent(string userAgent)
        {
            return _builder.WithUserAgent(userAgent);
        }

        public string GetUserAgent()
        {
            return _builder.GetUserAgent();
        }

        public void AddTrailingSlashBeforeQuery()
        {
            _builder.AddTrailingSlashBeforeQuery();
        }

        public void Reset()
        {
            _builder.Reset();
        }
    }
}

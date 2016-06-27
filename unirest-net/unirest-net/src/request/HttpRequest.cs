﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using unirest_net.http;

namespace unirest_net.request
{
    public class HttpRequest
    {
        private bool hasFields;

        private bool hasExplicitBody;

        public NetworkCredential NetworkCredentials { get; protected set; }

        public Func<HttpRequestMessage, bool> Filter { get; protected set; }

        public Uri URL { get; protected set; }

        public HttpMethod HttpMethod { get; protected set; }

        public Dictionary<String, String> Headers { get; protected set; }

        public MultipartFormDataContent Body { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequest"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="url">The URL.</param>
        public HttpRequest(HttpMethod method, string url) : this(method, CreateUriFromUrl(url))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequest"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="uri">The URI.</param>
        public HttpRequest(HttpMethod method, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            if (!(uri.IsAbsoluteUri && (uri.Scheme == "http" || uri.Scheme == "https")) || !uri.IsAbsoluteUri)
            {
                throw new ArgumentException("The url passed to the HttpMethod constructor is not a valid HTTP/S URL");
            }

            URL = uri;
            HttpMethod = method;
            Headers = new Dictionary<string, string>();
            Body = new MultipartFormDataContent();
        }

        public HttpRequest header(string name, object value)
        {
            if (value != null)
                Headers.Add(name, value.ToString());

            return this;
        }
        
        public HttpRequest headers(Dictionary<string, object> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if(header.Value != null)
                        Headers.Add(header.Key, header.Value.ToString());
                }
            }

            return this;
        }

        public HttpRequest field(string name, object value)
        {
            if ((HttpMethod == HttpMethod.Get) || (HttpMethod == HttpMethod.Head) || (HttpMethod == HttpMethod.Trace))
            {
                throw new InvalidOperationException(string.Format("Can't add body to {0} request.", HttpMethod));
            }

            if (hasExplicitBody)
            {
                throw new InvalidOperationException("Can't add fields to a request with an explicit body");
            }

            if (value == null)
                return this;

            Body.Add(new StringContent(value.ToString()), name);

            hasFields = true;           

            return this;
        }

        public HttpRequest field(string name, byte[] data)
        {
            if ((HttpMethod == HttpMethod.Get) || (HttpMethod == HttpMethod.Head) || (HttpMethod == HttpMethod.Trace))
            {
                throw new InvalidOperationException(string.Format("Can't add body to {0} request.", HttpMethod));
            }

            if (hasExplicitBody)
            {
                throw new InvalidOperationException("Can't add fields to a request with an explicit body");
            }

            if (data == null)
                return this;

            //    here you can specify boundary if you need---^
            var imageContent = new ByteArrayContent(data);
            imageContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("image/jpeg");

            Body.Add(imageContent, name, "image.jpg");

            hasFields = true;
            return this;
        }

        public HttpRequest basicAuth(string userName, string passWord)
        {
            if (this.NetworkCredentials != null)
            {
                throw new InvalidOperationException("Basic authentication credentials are already set.");
            }

            this.NetworkCredentials = new NetworkCredential(userName, passWord);
            return this;
        }

        /// <summary>
        /// Set a delegate to a message filter. This is particularly useful for using external
        /// authentication libraries such as uniauth (https://github.com/zeeshanejaz/uniauth-net)
        /// </summary>
        /// <param name="handler">Filter accepting HttpRequestMessage and returning bool</param>
        /// <returns>updated reference</returns>
        public HttpRequest filter(Func<HttpRequestMessage, bool> filter)
        {            
            if (this.Filter != null)
            {
                throw new InvalidOperationException("Processing filter is already set.");
            }

            this.Filter = filter;
            return this;
        }

        public HttpRequest field(Stream value)
        {
            if ((HttpMethod == HttpMethod.Get) || (HttpMethod == HttpMethod.Head) || (HttpMethod == HttpMethod.Trace))
            {
                throw new InvalidOperationException(string.Format("Can't add body to {0} request.", HttpMethod));
            }

            if (hasExplicitBody)
            {
                throw new InvalidOperationException("Can't add fields to a request with an explicit body");
            }

            if (value == null)
                return this;

            Body.Add(new StreamContent(value));
            hasFields = true;
            return this;
        }

        public HttpRequest fields(Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return this;

            if ((HttpMethod == HttpMethod.Get) || (HttpMethod == HttpMethod.Head) || (HttpMethod == HttpMethod.Trace))
            {
                throw new InvalidOperationException(string.Format("Can't add body to {0} request.", HttpMethod));
            }

            if (hasExplicitBody)
            {
                throw new InvalidOperationException("Can't add fields to a request with an explicit body");
            }

            Body.Add(new FormUrlEncodedContent(parameters.Where(kv => isPrimitiveType(kv.Value)).Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value.ToString()))));

            foreach (var stream in parameters.Where(kv => kv.Value is Stream).Select(kv => kv.Value))
            {
                if (stream == null)
                    continue;

                Body.Add(new StreamContent(stream as Stream));
            }

            hasFields = true;
            return this;
        }

        public HttpRequest query(string parameter, string value = null)
        {
            var url = new UriBuilder(this.URL);

            var newQueryString = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { parameter, value }
            }).ReadAsStringAsync().Result;

            if (value == null)
            {
                newQueryString = newQueryString.Substring(0, newQueryString.Length - 1);
            }

            if (url.Query == "")
            {
                url.Query = newQueryString;
            }
            else
            {
                url.Query = string.Join("&", url.Query.Substring(1), newQueryString);
            }

            URL = url.Uri;

            return this;
        }

        public HttpRequest queries(Dictionary<string, string> values)
        {
            if (values == null)
            {
                return this;
            }

            foreach (var value in values)
            {
                query(value.Key, value.Value);
            }

            return this;
        }

        private bool isPrimitiveType(object obj)
        {
            if (obj == null)
                return false;

            return obj.GetType().IsPrimitive;
        }

        public HttpRequest body(string body)
        {
            if ((HttpMethod == HttpMethod.Get) || (HttpMethod == HttpMethod.Head) || (HttpMethod == HttpMethod.Trace))
            {
                throw new InvalidOperationException(string.Format("Can't add body to {0} request.", HttpMethod));
            }

            if (hasFields)
            {
                throw new InvalidOperationException("Can't add explicit body to request with fields");
            }

            if (body == null)
                return this;

            Body = new MultipartFormDataContent { new StringContent(body) };
            hasExplicitBody = true;
            return this;
        }

        public HttpRequest body<T>(T body)
        {
            if ((HttpMethod == HttpMethod.Get) || (HttpMethod == HttpMethod.Head) || (HttpMethod == HttpMethod.Trace))
            {
                throw new InvalidOperationException(string.Format("Can't add body to {0} request.", HttpMethod));
            }

            if (hasFields)
            {
                throw new InvalidOperationException("Can't add explicit body to request with fields");
            }

            if (body == null)
                return this;

            if (body is Stream)
            {
                Stream inputStream = (body as Stream);
                if (!inputStream.CanRead)
                    throw new ArgumentException("Excepting a readable stream");

                StreamReader reader = new StreamReader(inputStream);
                string fileContent = reader.ReadToEnd();
                Body = new MultipartFormDataContent { new StringContent(fileContent) };
            }
            else
            {
                Body = new MultipartFormDataContent { new StringContent(JsonConvert.SerializeObject(body)) };
            }

            hasExplicitBody = true;
            return this;
        }

        public HttpResponse<String> asString()
        {
            return HttpClientHelper.Request<String>(this);
        }

        public Task<HttpResponse<String>> asStringAsync()
        {
            return HttpClientHelper.RequestAsync<String>(this);
        }

        public HttpResponse<Stream> asBinary()
        {
            return HttpClientHelper.Request<Stream>(this);
        }

        public Task<HttpResponse<Stream>> asBinaryAsync()
        {
            return HttpClientHelper.RequestAsync<Stream>(this);
        }

        public HttpResponse<T> asJson<T>()
        {
            return HttpClientHelper.Request<T>(this);
        }

        public dynamic asJson()
        {
            var response = HttpClientHelper.Request<object>(this);

            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(response.Raw))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return serializer.Deserialize(jsonTextReader);
                }
            }
        }

        public Task<HttpResponse<T>> asJsonAsync<T>()
        {
            return HttpClientHelper.RequestAsync<T>(this);
        }

        /// <summary>
        /// Creates the URI from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The URI or an exception.</returns>
        private static Uri CreateUriFromUrl(string url)
        {
            Uri locurl;
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out locurl))
            {
                throw new ArgumentException("The url passed to the HttpMethod constructor is not a valid HTTP/S URL");
            }

            return locurl;
        }
    }
}

using System;
using System.Net.Http;

using unirest_net.request;

namespace unirest_net.http
{
    public class Unirest
    {
        public static HttpRequest get(string url)
        {
            return new HttpRequest(HttpMethod.Get, url);
        }

        public static HttpRequest get(Uri url)
        {
            return new HttpRequest(HttpMethod.Get, url);
        }

        public static HttpRequest post(string url)
        {
            return new HttpRequest(HttpMethod.Post, url);
        }

        public static HttpRequest post(Uri url)
        {
            return new HttpRequest(HttpMethod.Post, url);
        }

        public static HttpRequest delete(string url)
        {
            return new HttpRequest(HttpMethod.Delete, url);
        }

        public static HttpRequest delete(Uri url)
        {
            return new HttpRequest(HttpMethod.Delete, url);
        }

        public static HttpRequest patch(string url)
        {
            return new HttpRequest(new HttpMethod("PATCH"), url);
        }

        public static HttpRequest patch(Uri url)
        {
            return new HttpRequest(new HttpMethod("PATCH"), url);
        }

        public static HttpRequest put(string url)
        {
            return new HttpRequest(HttpMethod.Put, url);
        }

        public static HttpRequest put(Uri url)
        {
            return new HttpRequest(HttpMethod.Put, url);
        }

        public static HttpRequest options(string url)
        {
            return new HttpRequest(HttpMethod.Options, url);
        }

        public static HttpRequest options(Uri url)
        {
            return new HttpRequest(HttpMethod.Options, url);
        }

        public static HttpRequest head(string url)
        {
            return new HttpRequest(HttpMethod.Head, url);
        }

        public static HttpRequest head(Uri url)
        {
            return new HttpRequest(HttpMethod.Head, url);
        }

        public static HttpRequest trace(string url)
        {
            return new HttpRequest(HttpMethod.Trace, url);
        }

        public static HttpRequest trace(Uri url)
        {
            return new HttpRequest(HttpMethod.Trace, url);
        }
    }
}

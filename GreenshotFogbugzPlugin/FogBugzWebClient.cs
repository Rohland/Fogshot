using System;
using System.Net;

namespace GreenshotFogbugzPlugin
{
    public class FogBugzWebClient : WebClient
    {
        private readonly CookieContainer cookieContainer = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.CookieContainer = cookieContainer;
            }
            return request;
        }
    }
}

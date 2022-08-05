using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AnimeBack.Services
{
    public class DataClient
    {
        private readonly IConfiguration _config;

        private string SecretKey;
        public DataClient(IConfiguration config)
        {
            _config = config;

        SecretKey = _config["FlutterKeys:SecretKey"];
        }
        public async Task<string> Post(object data , string url = "")
        {
            using (var client = new HttpClient())
            {

               var secretKey = this.SecretKey;

               
              
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;



                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.SecretKey}");
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response;
                if (data != null)
                {

                    // had to install Microsoft.AspNet.WebApi.Client Nuget for access to PostAsJsonSynce 
                    response = await client.PostAsJsonAsync(url, data);
                }
                else
                {
                    response = await client.PostAsJsonAsync(url, new { });
                }
                var httpStatusCode = (int)response.StatusCode;
                if (httpStatusCode == 200 || httpStatusCode == 201 || httpStatusCode == 202)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> Get(string url = "")
        {

           


            using (var clientx = new HttpClient())
            {


               
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                clientx.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.SecretKey}");
                clientx.Timeout = TimeSpan.FromMinutes(5);
                clientx.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response;
                if (url != null)
                {
                    response = await clientx.GetAsync(url);
                }
                else
                {
                    response = await clientx.GetAsync(url);
                }
                var httpStatusCode = (int)response.StatusCode;
                if (httpStatusCode == 200 || httpStatusCode == 201 || httpStatusCode == 202)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
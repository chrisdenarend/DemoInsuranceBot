using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web;

namespace InsuranceBot.Helpers
{
    public class Translator
    {
        private string _authToken;
        private const string From = "en";
        private const string To = "nl";

        private void Init()
        {
            var authTokenSource = new AzureAuthToken(ConfigurationManager.AppSettings["TranslatorApi"]);
            try
            {
                _authToken = authTokenSource.GetAccessToken();
            }
            catch (HttpRequestException)
            {
                switch (authTokenSource.RequestStatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        Console.WriteLine("Request to token service is not authorized (401). Check that the Azure subscription key is valid.");
                        return;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Request to token service is not authorized (403). For accounts in the free-tier, check that the account quota is not exceeded.");
                        return;
                }
                throw;
            }
        }


        public IEnumerable<string> Translate(IEnumerable<string> tagList)
        {
            Init();

            var resultList = new List<string>();
            foreach (var tag in tagList)
            {
                resultList.Add(Translate(tag));
            }

            return resultList;
        }

        private string Translate(string tag)
        {
            var uri = "https://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + HttpUtility.UrlEncode(tag) + "&from=" + From + "&to=" + To;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", _authToken);

            using (var response = httpWebRequest.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    var dcs = new DataContractSerializer(type: Type.GetType("System.String"));
                    var translation = (string)dcs.ReadObject(stream);
                    return translation;
                }
            }
        }
    }
}
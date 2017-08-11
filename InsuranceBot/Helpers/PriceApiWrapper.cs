using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InsuranceBot.Helpers
{
    public class PriceApiWrapper
    {
        private readonly string _priceApiToken;
        private readonly string _priceApiRequest;
        private readonly string _priceApiJobQueue;
        private readonly string _priceApiResults;

        public PriceApiWrapper()
        {
            _priceApiToken = ConfigurationManager.AppSettings["PriceApiToken"];
            _priceApiRequest = ConfigurationManager.AppSettings["PriceApiRequest"];
            _priceApiJobQueue = ConfigurationManager.AppSettings["PriceApiJobQueue"];
            _priceApiResults = ConfigurationManager.AppSettings["PriceApiResults"];
        }

        private string GetJobId(string text)
        {
            // Get JobID
            using (var client = new WebClient())
            {
                var reqparm = new System.Collections.Specialized.NameValueCollection
                        {
                            { "token", _priceApiToken },
                            { "country", "nl" },
                            { "source", "google-shopping" },
                            { "key", "keyword" },
                            { "values", text.Trim() }
                        };
                var responsebytes = client.UploadValues(_priceApiRequest, "POST", reqparm);
                var responsebody = Encoding.UTF8.GetString(responsebytes);

                dynamic jobrequest = JsonConvert.DeserializeObject(responsebody);
                return jobrequest.job_id;
            }
        }

        internal double GetAveragePrice(JObject pricingResponse)
        {
            double price = 0;
            foreach (var priceObject in pricingResponse["products"].First()["offers"])
            {
                price += double.Parse(priceObject["price"].ToString());
            }
            return price /= pricingResponse["products"].First()["offers"].Count();
        }

        internal string GetPriceName(JObject pricingResponse)
        {
            return pricingResponse["products"].FirstOrDefault() != null ? pricingResponse["products"].First()["name"].ToString() : "Unknown";
        }

        internal async Task<JObject> GetPriceResponseAsync(string text)
        {
            var jobId = GetJobId(text);

            // Wait for API...
            var success = false;
            var httpClient = new HttpClient();
            var response = new HttpResponseMessage();
            while (!success)
            {
                response = await httpClient.GetAsync(string.Format(_priceApiJobQueue, _priceApiToken, jobId));

                //will throw an exception if not successful
                response.EnsureSuccessStatusCode();

                var jobcontent = await response.Content.ReadAsStringAsync();

                dynamic jobstatus = JsonConvert.DeserializeObject(jobcontent);
                if (jobstatus.completed.ToString().Equals("1") &&
                    jobstatus.status.ToString().Equals("finished"))
                {
                    success = true;
                }
            }

            // Parse results
            httpClient = new HttpClient();
            response = await httpClient.GetAsync(string.Format(_priceApiResults, _priceApiToken, jobId));

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }


    }
}
using GetInfra.Standard.Queue.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GetInfra.Standard.Queue.Implementations.VkSQS
{
    public class VkSQSPublisher : IQueuePublisher
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        string awsAccessKeyId = "F7F13AA0CBAB68AD3615";
        string awsSecretAccessKey = "21A0F18F9473F3AF0566CEDE237DDC4462CFE678";

        public VkSQSPublisher(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("");
        }

        public async Task Enqueue(QMessage msg)
        {
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("Action", "SendMessage"));
            nvc.Add(new KeyValuePair<string, string>("MessageBody", "TEST2"));
            nvc.Add(new KeyValuePair<string, string>("Expires", "2022-06-07T20:00:00Z"));
            nvc.Add(new KeyValuePair<string, string>("Version", "2012-11-05"));
            nvc.Add(new KeyValuePair<string, string>("AUTHPARAMS", "TEST2"));

            using (var response = await _client.PostAsync("https://sqs.mcs.mail.ru/mcs1261003933/ariel-dev.fifo", new FormUrlEncodedContent(nvc)))
            {
                response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync();
                //var companies = await JsonSerializer.DeserializeAsync<List<CompanyDto>>(stream, _options);
            }
        }
    }
}

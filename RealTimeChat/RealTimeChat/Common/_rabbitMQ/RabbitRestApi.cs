using Newtonsoft.Json.Linq;
using RabbitMQApi;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeChat.Common
{
    public class RabbitRestApi : IRestApiSet, IRestApiType
    {
        private string _hostName;
        private string _userName;
        private string _password;
        private string _virtualHost;
        private static RabbitRestApi _rabbitRestApi;

        public static IRestApiSet Create()
        {
            if(_rabbitRestApi == null)
            {
                return new RabbitRestApi();
            }
            return _rabbitRestApi;
        }

        private RabbitRestApi()
        {
            
        }

        public List<Consumer> ConsumerNumOfQueue()
        {
            var result = new List<Consumer>();

            var client = new RestClient("http://" + _hostName + ":15672/api/consumers/chat?columns=channel_details.number,queue.name,consumer_tag");
            client.Authenticator = new HttpBasicAuthenticator(_userName, _password);
            var request = new RestRequest(Method.GET);
            try
            {
                var response = client.Execute(request);

                var jArr = JArray.Parse(response.Content);

                foreach (var item in jArr)
                {
                    result.Add(new Consumer()
                    {
                        Queue = item["queue"]["name"].ToString(),
                        ConsumerNum = Convert.ToInt32(item["channel_details"]["number"].ToString()),
                        ConsumerTag = item["consumer_tag"].ToString()
                    });
                }
                return result;
            }
            catch(Exception ex)
            {
                return Enumerable.Empty<Consumer>().ToList();
            }
        }

        public RabbitQueue ConsumersOfExchange(string exchange)
        {
            var queues = ConsumerNumOfQueue();
            var queuesOfExchange = QueuesOfExchange(exchange);
            var result = new RabbitQueue() { Name = exchange };

            foreach (var queue in queuesOfExchange)
            {
                var nums = queues.Where(x => x.Queue == queue).Select(y => y.ConsumerNum).ToList();
                int sum = nums.Sum();

                result.Consumers += nums.Sum();
            }
            return result;
        }

        public List<RabbitQueue> ConsumersOfExchangeList()
        {
            var rabbitQueues = new List<RabbitQueue>();
            var exchanges = ExchangeList();
            var queues = ConsumerNumOfQueue();
            foreach (var exchange in exchanges)
                rabbitQueues.Add(ConsumersOfExchange(exchange));

            return rabbitQueues;
        }

        public List<string> ConsumerTagsOfExchange(string exchange)
        {
            var queues = ConsumerNumOfQueue();
            var queuesOfExchange = QueuesOfExchange(exchange);
            var result = new List<string>();

            foreach (var queue in queuesOfExchange)
            {
                var consumers = queues.Where(x => x.Queue == queue).Select(y => y.ConsumerTag).ToList();
                result.AddRange(consumers);
            }
            return result;
        }

        public List<string> ExchangeList()
        {
            var client = new RestClient("http://" + _hostName + ":15672/api/definitions?columns=exchanges.name");
            client.Authenticator = new HttpBasicAuthenticator(_userName, _password);
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            var content = JObject.Parse(response.Content);

            var queues = new List<string>();
            foreach (var name in content["exchanges"])
                queues.Add(name["name"].ToString());

            return queues;
        }

        public IRestApiType Get()
        {
            return this;
        }

        public List<string> QueuesOfExchange(string exchange)
        {
            var client = new RestClient("http://" + _hostName + ":15672/api/exchanges/chat/" + exchange + "/bindings/source?columns=destination");
            client.Authenticator = new HttpBasicAuthenticator(_userName, _password);
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            var jArr = JArray.Parse(response.Content);

            var queues = new List<string>();

            foreach (var item in jArr)
                queues.Add(item["destination"].ToString());
            return queues;
        }

        public IRestApiSet SetHostName(string param)
        {
            _hostName = param;
            return this;
        }

        public IRestApiSet SetPassword(string param)
        {
            _password = param;
            return this;
        }

        public IRestApiSet SetUserName(string param)
        {
            _userName = param;
            return this;
        }

        public IRestApiSet SetVirtualHost(string param)
        {
            _virtualHost = param;
            return this;
        }
    }
}

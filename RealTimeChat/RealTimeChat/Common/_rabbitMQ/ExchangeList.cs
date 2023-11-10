using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
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
    public class ExchangeList
    {
        private readonly ConnectionFactory _connectionFactory;

        public ExchangeList(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<string> GetExchangeList()
        {
            var client = new RestClient("http://" + _connectionFactory.HostName + ":15672/api/definitions?columns=exchanges.name");
            client.Authenticator = new HttpBasicAuthenticator(_connectionFactory.UserName, _connectionFactory.Password);
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            var content = JObject.Parse(response.Content);

            var queues = new List<string>();
            foreach(var name in content["exchanges"])
                queues.Add(name["name"].ToString());

            return queues;
        }

        public List<Consumer> GetConsumerNumOfQueue()
        {
            var result = new List<Consumer>();

            var client = new RestClient("http://" + _connectionFactory.HostName + ":15672/api/consumers/chat?columns=channel_details.number,queue.name,consumer_tag");
            client.Authenticator = new HttpBasicAuthenticator(_connectionFactory.UserName, _connectionFactory.Password);
            var request = new RestRequest(Method.GET);
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

        public List<string> GetQueuesOfExchange(string exchange)
        {
            var client = new RestClient("http://" + _connectionFactory.HostName + ":15672/api/exchanges/chat/"+exchange+"/bindings/source?columns=destination");
            client.Authenticator = new HttpBasicAuthenticator(_connectionFactory.UserName, _connectionFactory.Password);
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            var jArr = JArray.Parse(response.Content);

            var queues = new List<string>();

            foreach (var item in jArr)
                queues.Add(item["destination"].ToString());
            return queues;
        }

        public List<string> GetConsumerTagsOfExchange(string exchange)
        {
            var queues = GetConsumerNumOfQueue();
            var queuesOfExchange = GetQueuesOfExchange(exchange);
            var result = new List<string>();

            foreach (var queue in queuesOfExchange)
            {
                var consumers = queues.Where(x => x.Queue == queue).Select(y => y.ConsumerTag).ToList();
                result.AddRange(consumers);
            }
            return result;
        }

        public RabbitQueue GetConsumersOfExchange(string exchange)
        {
            var queues = GetConsumerNumOfQueue();
            var queuesOfExchange = GetQueuesOfExchange(exchange);
            var result = new RabbitQueue() { Name = exchange };

            foreach (var queue in queuesOfExchange)
            {
                var nums = queues.Where(x => x.Queue == queue).Select(y => y.ConsumerNum).ToList();
                int sum = nums.Sum();

                result.Consumers += nums.Sum();
            }
            return result;
        }

        public List<RabbitQueue> GetQueues()
        {
            var rabbitQueues = new List<RabbitQueue>();
            var exchanges = GetExchangeList();
            var queues = GetConsumerNumOfQueue();
            foreach(var exchange in exchanges)
                rabbitQueues.Add(GetConsumersOfExchange(exchange));

            return rabbitQueues;
        }
    }
}

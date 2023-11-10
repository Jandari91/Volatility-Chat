using System.Collections.Generic;

namespace RabbitMQApi
{
    public interface IRestApiSet
    {
        IRestApiSet SetUserName(string param);
        IRestApiSet SetPassword(string param);
        IRestApiSet SetVirtualHost(string param);
        IRestApiSet SetHostName(string param);
        IRestApiType Get();
    }

    public interface IRestApiType
    {
        List<string> ExchangeList();
        List<Consumer> ConsumerNumOfQueue();
        List<string> QueuesOfExchange(string exchange);
        List<string> ConsumerTagsOfExchange(string exchange);
        RabbitQueue ConsumersOfExchange(string exchange);
        List<RabbitQueue> ConsumersOfExchangeList();
    }
}

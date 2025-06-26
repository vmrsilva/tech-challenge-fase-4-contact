using MassTransit;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechChallange.Common.MessagingService
{
    public class MessagingService : IMessagingService
    {
        private readonly IBus _bus;
        private readonly IConfiguration _configuration;
        public MessagingService(IBus bus, IConfiguration configuration)
        {
            _bus = bus;
            _configuration = configuration;
        }
        public async Task<bool> SendMessage<T>(string queueName, T message)
        {
            try
            {
                if (message == null)
                    return false;

                var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:{queueName}"));

                await endpoint.Send(message);



                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
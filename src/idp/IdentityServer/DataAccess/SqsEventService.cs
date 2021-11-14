using System;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json;

namespace IdentityServer.DataAccess
{
    /// <summary>
    /// Sends events to SQS for later analytics or neglect. (SNS might have been a better choice.)
    /// </summary>
    public class SqsEventService : IEventService
    {
        private readonly IAmazonSQS _amazonSqs;
        private readonly Uri _queueUrl;

        public SqsEventService(IAmazonSQS amazonSqs, Uri queueUrl)
        {
            _amazonSqs = amazonSqs;
            _queueUrl = queueUrl;
        }

        public Task RaiseAsync(Event evt)
        {
            SendMessageRequest sendRequest = new SendMessageRequest
            {
                MessageBody = JsonConvert.SerializeObject(evt), QueueUrl = _queueUrl.ToString(),
            };
            return _amazonSqs.SendMessageAsync(sendRequest);
        }

        public bool CanRaiseEventType(EventTypes evtType)
        {
            return true;
        }
    }
}
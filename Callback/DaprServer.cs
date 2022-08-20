using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TestDapr.Callback
{
    public class DaprServer : AppCallback.AppCallbackBase
    {
        private readonly JsonSerializerOptions _jsonOptions;
        /// <summary>
        /// Constructor
        /// </summary>
        public DaprServer()
        {
            _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        /// <summary>
        /// implement ListTopicSubscriptions to register all events
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request,
            ServerCallContext context)
        {
            var result = new ListTopicSubscriptionsResponse();
            result.Subscriptions.AddRange(new List<TopicSubscription>
            {
                new TopicSubscription
            {
                PubsubName = "pubsub",
                Topic = "DataChanged"
            }});
            return Task.FromResult(result);
        }

        /// <summary>
        /// implement OnTopicEvent to handle all events
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
        {
            var input = JsonSerializer.Deserialize<object>(request.Data.ToStringUtf8(), _jsonOptions);
            var task = (Task)typeof(DaprServer).GetMethod(request.Topic)?.Invoke(this, new[] { input, request.Topic, request.Id });
            if (task != null)
            {
                await task;
            }
           
            return await Task.FromResult(new TopicEventResponse());
        }

        /// <summary>
        /// implement DataChanged
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventKey"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DataChanged(object obj, string eventKey, string id)
        {
            string myfile = @"write.txt";

            if (!File.Exists(myfile))
            {
                using (StreamWriter writetext = File.CreateText(myfile))
                {
                    await writetext.WriteLineAsync(id);
                }
            }

            using (StreamWriter writetext = File.AppendText(myfile))
            {
                await writetext.WriteLineAsync(id);
            }
        }
    }
}
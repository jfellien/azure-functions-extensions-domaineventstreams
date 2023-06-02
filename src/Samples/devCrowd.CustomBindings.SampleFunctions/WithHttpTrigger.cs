using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using devCrowd.CustomBindings.EventSourcing;
using devCrowd.CustomBindings.SampleFunctions.Events;
using devCrowd.CustomBindings.SampleFunctions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace devCrowd.CustomBindings.SampleFunctions;

public static class WithHttpTrigger
{
    [FunctionName("WithHttpTrigger")]
    public static async Task<HttpStatusCode> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = null)] 
        SampleEntity sampleEntity,
        [DomainEventStream("%DOMAIN_CONTEXT_NAME%","%DOMAIN_ENTITY_NAME%", "{entityId}")]
        DomainEventStream eventStream,
        ILogger log)
    {
        IEnumerable<IDomainEvent> events = await eventStream.Events();
        
        log.LogDebug($"Count of events: { events.Count() }");
        
        await eventStream.Append(new SampleEvent("sample")
        {
            EntityId = sampleEntity.EntityId,
            EntityName = sampleEntity.Name
        }, sampleEntity.EntityId);
        
        log.LogInformation("Entity has been appended to DomainStream");

        return HttpStatusCode.Accepted;
    }
}
using Azure.Functions.Extensions.DomainEventStreams;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(DomainEventStreamBindingRegistration))]

namespace Azure.Functions.Extensions.DomainEventStreams
{
    public class DomainEventStreamBindingRegistration : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<DomainEventStreamBindingConfiguration>();
        }
    }
}
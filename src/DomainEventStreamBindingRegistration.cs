using AzureFunctions.Extensions.EventSourcing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(DomainEventStreamBindingRegistration))]

namespace AzureFunctions.Extensions.EventSourcing
{
    public class DomainEventStreamBindingRegistration : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<DomainEventStreamBindingConfiguration>();
        }
    }
}
using devCrowd.CustomBindings.EventSourcing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(DomainEventStreamBindingRegistration))]

namespace devCrowd.CustomBindings.EventSourcing
{
    public class DomainEventStreamBindingRegistration : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<DomainEventStreamBindingConfiguration>();
        }
    }
}
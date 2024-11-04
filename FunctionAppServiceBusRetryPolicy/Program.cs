using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FunctionAppServiceBusRetryPolicy;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder => {
        //builder.UseWhen<ServiceBusRetryHandlingMiddleware>(i => i.FunctionDefinition.InputBindings.Any(b => b.Value.Type == "serviceBusTrigger"));
    })
    .ConfigureServices((hstCtx, services) =>
    {
        var configuration = hstCtx.Configuration;
        services.AddOptions<MessagingOptions>()
            .Bind(configuration.GetSection(MessagingOptions.Messaging));
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddAzureClients(clientBuilder =>
        {   
            clientBuilder.AddServiceBusClient(hstCtx.Configuration.GetSection("ServiceBusConnection"));
            TokenCredential credential = new AzureCliCredential();
            clientBuilder.UseCredential(credential);
        });
    })
    .Build();

host.Run();
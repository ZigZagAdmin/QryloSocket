using Microsoft.Extensions.DependencyInjection;

namespace QryloSocketAPI.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssembliesOf(typeof(Startup))
            .AddClasses(classes => classes.InNamespaces("QryloSocket.API.Services"))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        services.Scan(scan => scan.FromAssembliesOf(typeof(Startup))
            .AddClasses(classes => classes.InNamespaces("QryloSocket.API.Repositories"))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
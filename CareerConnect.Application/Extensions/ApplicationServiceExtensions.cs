using CareerConnect.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CareerConnect.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceExtensions).Assembly;

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        // Register the validation pipeline behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

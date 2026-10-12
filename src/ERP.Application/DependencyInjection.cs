using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Assistente Inteligente de Voz
        services.AddScoped<ERP.Application.Vendas.Voice.IVoiceAssistantService, ERP.Application.Vendas.Voice.VoiceAssistantService>();

        return services;
    }
}

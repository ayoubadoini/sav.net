using Microsoft.Extensions.DependencyInjection;
using SAV.Application.Interfaces.Services;
using SAV.Infrastructure.Identity;
using SAV.Infrastructure.Repositories;
using SAV.Infrastructure.Services;

namespace SAV.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repository & Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // PDF and File Services
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}

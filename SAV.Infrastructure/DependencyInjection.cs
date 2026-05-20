using Microsoft.Extensions.DependencyInjection;
using SAV.Infrastructure.Identity;
using SAV.Infrastructure.Repositories;
using SAV.Infrastructure.Services;

namespace SAV.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}

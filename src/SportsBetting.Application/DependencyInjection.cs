using Microsoft.Extensions.DependencyInjection;
using SportsBetting.Application.Bets;

namespace SportsBetting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateBetUseCase>();
        services.AddScoped<SettleBetUseCase>();
        services.AddScoped<GetBetResultUseCase>();
        return services;
    }
}

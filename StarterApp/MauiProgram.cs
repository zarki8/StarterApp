using Microsoft.Extensions.Logging;
using StarterApp.ViewModels;
using StarterApp.Database.Data;
using StarterApp.Views;
using System.Diagnostics;
using StarterApp.Services;
using StarterApp.Repositories;
 
namespace StarterApp;
 
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
 
       
const bool useSharedApi = true;
 
if (useSharedApi)
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
    };
 
    builder.Services.AddSingleton(httpClient);
    builder.Services.AddSingleton<ApiAuthenticationService>();
    builder.Services.AddSingleton<IAuthenticationService>(sp => sp.GetRequiredService<ApiAuthenticationService>());
    builder.Services.AddSingleton<IApiTokenProvider>(sp => sp.GetRequiredService<ApiAuthenticationService>());
    builder.Services.AddDbContext<AppDbContext>();
    builder.Services.AddTransient<IItemRepository, ApiItemRepository>();
    builder.Services.AddTransient<IRentalRepository, ApiRentalRepository>();
}
else
{
    builder.Services.AddDbContext<AppDbContext>();
    builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
    builder.Services.AddTransient<IItemRepository, ItemRepository>();
}
 
 
        builder.Services.AddSingleton<INavigationService, NavigationService>();
 
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();
 
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserDetailPage>();
        builder.Services.AddTransient<UserDetailViewModel>();

        builder.Services.AddTransient<ItemListViewModel>();
        builder.Services.AddTransient<ItemListPage>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<RentalRequestViewModel>();
        builder.Services.AddTransient<RentalRequestPage>();
        builder.Services.AddTransient<RentalsViewModel>();
        builder.Services.AddTransient<RentalsPage>();

        builder.Services.AddSingleton<TempViewModel>();
        builder.Services.AddTransient<TempPage>();
 
#if DEBUG
        builder.Logging.AddDebug();
#endif
 
        return builder.Build();
    }
}

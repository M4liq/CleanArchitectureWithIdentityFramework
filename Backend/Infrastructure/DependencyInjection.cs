using System.Text;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Core;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Settings;
using Domain.Identity;
using Infrastructure.Persistence;
using Infrastructure.Services.Core;
using Infrastructure.Services.Identity;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.InitializeConfiguration(configuration);
        services.InitializeAuthorization(configuration);
        
        services.AddDbContext<DataContext>(optionsBuilder =>
            optionsBuilder
                .UseSqlServer(
                    configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>().ConnectionString,
                    x => x.MigrationsAssembly("Infrastructure"))
                .EnableSensitiveDataLogging());

        services.AddIdentity<ApplicationUserEntity, ApplicationRoleEntity>(_ => _.SignIn.RequireConfirmedAccount = true)
            .AddUserManager<ApplicationUserManager>()
            .AddUserStore<ApplicationUserStore>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IDataContext, DataContext>();
        services.InitializeServices();
    }

    private static void InitializeConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
        services.AddSingleton(databaseSettings);
        
        var smtpClientSettings = configuration.GetSection("SmtpClientSettings").Get<SmtpClientSettings>();
        services.AddSingleton(smtpClientSettings);
        
        var errorsAndMessagesSettings = configuration.GetSection("ErrorsAndMessagesSettings").Get<ErrorsAndMessagesSettings>();
        services.AddSingleton<IErrorsAndMessagesSettings>(errorsAndMessagesSettings);
    }

    private static void InitializeServices(this IServiceCollection services)
    {
        services.AddScoped<IApplicationUserManager, ApplicationUserManager>();
        services.AddScoped<IErrorManager, ErrorManager>();
        services.AddScoped<IMessageManager, MessageManager>();
        services.AddScoped<IEmailClient, EmailClient>();
    }

    private static void InitializeAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        services.AddSingleton<IJwtSettings>(jwtSettings);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
            ValidateAudience = false,
            ValidateIssuer = false,
            ClockSkew = TimeSpan.Zero
        };

        services.AddSingleton(tokenValidationParameters);

        services.AddAuthentication(_ =>
            {
                _.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                _.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                _.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(_ =>
            {
                _.SaveToken = true;
                _.TokenValidationParameters = tokenValidationParameters;
            });
    }
}
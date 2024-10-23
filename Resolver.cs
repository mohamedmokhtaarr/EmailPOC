using EmailPOC.BackgroundJobs;
using EmailPOC.DataAccess;
using EmailPOC.Helpers;
using EmailPOC.Interfaces;
using EmailPOC.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore; // Import MediatR

namespace EmailPOC
{
    public static class Resolver
    {
        public static IUmbracoBuilder AddCoreApplication(this IUmbracoBuilder umbracoBuilder)
        {
            umbracoBuilder
                .AddNewsletterMailScheduling(umbracoBuilder.Config)
                .AddMediatRHandlers() // Register MediatR Handlers
                .Services
                .AddValidationHelper(umbracoBuilder.Config)
                .AddCorePersistence(umbracoBuilder.Config)
                .AddApplicationServices(umbracoBuilder.Config);

            return umbracoBuilder;
        }

        // Register MediatR
        private static IUmbracoBuilder AddMediatRHandlers(this IUmbracoBuilder umbracoBuilder)
        {
            umbracoBuilder.Services.AddMediatR(typeof(Resolver).Assembly); // Register MediatR
            return umbracoBuilder;
        }

        private static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSingleton<IEmailHelper, EmailHelper>()// Register IEmailHelper
                 .AddScoped<IVerificationEmailHelper, VerificationEmailHelper>()
                 .AddScoped<IResetPasswordHelper, ResetPasswordHelper>();
            return services;
        }

        private static IServiceCollection AddValidationHelper(this IServiceCollection services, IConfiguration configuration)
        {
            ValidationHelperSettings validationHelperSettings = new();
            configuration.Bind(nameof(ValidationHelperSettings), validationHelperSettings);

            return services
                .AddSingleton<IValidationHelper, ValidationHelper>() // Register IValidationHelper
                .AddSingleton(validationHelperSettings);
        }
        // Register Background Job
        private static IUmbracoBuilder AddNewsletterMailScheduling(this IUmbracoBuilder umbracoBuilder, IConfiguration configuration)
        {
            NewsletterSettings newsletterSettings = new();
            umbracoBuilder.Services.AddRecurringBackgroundJob<NewsletterMailSchedulerBackgroundJob>();
            return umbracoBuilder;
        }
        public static IServiceCollection AddCorePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseSettings>(configuration.GetSection(key: nameof(DatabaseSettings)));
            services.AddDbContext<NewsletterMailDbContext>(cfg =>
            {
                cfg.UseSqlServer(configuration.GetConnectionString("umbracoDbDSN"));
            });

      
            return services;
        }

    }
}

using EmailPOC.BackgroundJobs;
using EmailPOC.Helpers;
using EmailPOC.Interfaces;
using EmailPOC.Settings;

namespace EmailPOC
{
    public static class Resolver
    {
        public static IUmbracoBuilder AddCoreApplication(this IUmbracoBuilder umbracoBuilder)
        {

            umbracoBuilder
                .AddNewsletterMailScheduling(umbracoBuilder.Config);

            umbracoBuilder.Services
                .AddValidationHelper(umbracoBuilder.Config)
                 .AddApplicationServices(umbracoBuilder.Config);
            return umbracoBuilder;
        }

        private static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
              .AddSingleton<IEmailHelper, EmailHelper>();
            return services;
        }

        private static IServiceCollection AddValidationHelper(this IServiceCollection services, IConfiguration configuration)
        {
            ValidationHelperSettings validationHelperSettings = new();
            configuration.Bind(nameof(ValidationHelperSettings), validationHelperSettings);

            return services
                .AddSingleton<IValidationHelper, ValidationHelper>()
                .AddSingleton(validationHelperSettings);
        }

        private static IUmbracoBuilder AddNewsletterMailScheduling(this IUmbracoBuilder umbracoBuilder, IConfiguration configuration)
        {
            NewsletterSettings newsletterSettings = new();
            umbracoBuilder.Services.AddRecurringBackgroundJob<NewsletterMailSchedulerBackgroundJob>();
            return umbracoBuilder;

        }
    }
}

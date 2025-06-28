using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechChallange.Common.MessagingService;
using TechChallange.Contact.Domain.Contact.Messaging;
using TechChallange.Contact.Integration.Service;
using TechChallange.Domain.Base.Repository;
using TechChallange.Domain.Cache;
using TechChallange.Domain.Contact.Repository;
using TechChallange.Domain.Contact.Service;
using TechChallange.Infrastructure.Cache;
using TechChallange.Infrastructure.Context;
using TechChallange.Infrastructure.Repository.Base;
using TechChallange.Infrastructure.Repository.Contact;

namespace TechChallange.IoC
{
    public static class DomainInjection
    {
        public static void AddInfraestructure(this IServiceCollection services, IConfiguration configuration)
        {
            ConfigureContext(services, configuration);
            ConfigureBase(services);
            ConfigureContact(services);
            ConfigureCache(services, configuration);
            ConfigureIntegration(services);
            ConfigureMessagingService(services, configuration);
        }

        public static void ConfigureContext(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TechChallangeContext>(options => options.UseSqlServer(configuration.GetConnectionString("Database")));

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var dbContext = serviceProvider.GetRequiredService<TechChallangeContext>();
             //   dbContext.Database.Migrate();
            }
        }

        public static void ConfigureBase(IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        }

        public static void ConfigureContact(IServiceCollection services)
        {
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IContactService, ContactService>();
        }
        public static void ConfigureCache(IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options => {
                options.InstanceName = nameof(CacheRepository);
                options.Configuration = configuration.GetConnectionString("Cache");
            });
            services.AddScoped<ICacheRepository, CacheRepository>();
            services.AddScoped<ICacheWrapper, CacheWrapper>();
        }

        private static void ConfigureIntegration(IServiceCollection services)
        {
            services.AddScoped<IIntegrationService, IntegrationService>();
        }

        public static void ConfigureMessagingService(IServiceCollection services, IConfiguration configuration)
        {
            var servidor = configuration.GetSection("MassTransit")["Server"] ?? string.Empty;
            var usuario = configuration.GetSection("MassTransit")["User"] ?? string.Empty;
            var senha = configuration.GetSection("MassTransit")["Password"] ?? string.Empty;

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(servidor, "/", h =>
                    {
                        h.Username(usuario);
                        h.Password(senha);
                    });

                    cfg.Message<ContactCreateMessageDto>(m =>
                    {
                        m.SetEntityName("contact-insert-exchange");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IMessagingService, MessagingService>();
        }
    }
}

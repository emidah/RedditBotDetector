using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RedditBotDetector {
    public static class ServiceProviderBuilder {
        public static IServiceProvider GetServiceProvider() {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(typeof(Program).Assembly)
                .Build();
            var services = new ServiceCollection();

            services.Configure<Secrets>(configuration.GetSection(typeof(Secrets).FullName));

            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}
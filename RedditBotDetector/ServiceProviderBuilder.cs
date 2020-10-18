using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RedditBotDetector {
    public static class ServiceProviderBuilder {
        public static IServiceProvider GetServiceProvider(string[] args) {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(typeof(Program).Assembly)
                .AddCommandLine(args)
                .Build();
            var services = new ServiceCollection();

            services.Configure<Secrets>(configuration.GetSection(typeof(Secrets).FullName));

            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}
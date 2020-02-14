using GPS.SettingsSync.FilePersistence.Yaml.Providers;
using GPS.SettingsSync.FilePersistence.Yaml.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace GPS.SettingsSync.FilePersistence.Yaml
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddYamlProvider(this IServiceCollection services)
            => services
                   .AddTransient<YamlFileWriter>()
                   .AddTransient<YamlFileReader>()
                   .AddTransient<YamlSettingsSerializer>()
                   .AddTransient<YamlPersistenceProvider>();
    }
}
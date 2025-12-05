using ExportOrderWebServer.Areas.ImportDocument.Services;

namespace ExportOrderWebServer.Areas.ImportDocument.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddXMLParserService(this IServiceCollection services,
        Action<ParserConfiguration> configure = null)
    {
        var config = new ParserConfiguration();
        configure?.Invoke(config);

        services.AddSingleton(config);
        services.AddSingleton<IXMLParserService, XMLParserService>();

        return services;
    }
}

namespace Ecommerce.MonitoringApp.Observability;

public static class Env
{
    public static string Get(string key, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}

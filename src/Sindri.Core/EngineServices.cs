namespace Sindri.Core;

public sealed class EngineServices
{
    private readonly Dictionary<Type, object> _services = new();

    public void Register<TService>(TService service)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(service);
        _services[typeof(TService)] = service;
    }

    public TService GetRequiredService<TService>()
        where TService : class
    {
        if (_services.TryGetValue(typeof(TService), out var service) && service is TService typedService)
        {
            return typedService;
        }

        throw new InvalidOperationException($"Required Sindri service was not registered: {typeof(TService).FullName}");
    }

    public bool TryGetService<TService>(out TService? service)
        where TService : class
    {
        if (_services.TryGetValue(typeof(TService), out var value) && value is TService typedService)
        {
            service = typedService;
            return true;
        }

        service = null;
        return false;
    }
}

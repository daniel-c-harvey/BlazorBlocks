using NetBlocks.Models;

namespace Web.Shared.ApiClients;

public class ModelClientConfig : ClientConfig
{
    public string ControllerName { get; protected set; }
    
    public ModelClientConfig(string baseURL, int port, string controllerName) : base(baseURL, port)
    {
        ControllerName = controllerName;
    }

    public ModelClientConfig(string url, string controllerName) : base(url)
    {
        ControllerName = controllerName;
    }
}
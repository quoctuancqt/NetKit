using OpenTelemetry.Resources;

namespace NetKit.OpenTelemetry
{
    public sealed class OpenTelemetryInfo
    {
        public string ServiceName { get; set; }
        public string Exporter { get; set; }
        public ResourceBuilder ResourceBuilder { get; set; }

        public OpenTelemetryInfo(string serviceName, string exporter, ResourceBuilder resourceBuilder)
        {
            ServiceName = serviceName;
            ResourceBuilder = resourceBuilder;
            Exporter = exporter;
        }
    }
}

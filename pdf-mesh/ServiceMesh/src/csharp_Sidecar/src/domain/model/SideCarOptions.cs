namespace csharp_Sidecar.Src.Domain.Model
{
    public class SideCarOptions
    {
        public string ServiceName { get; set; } = string.Empty;
        public string ControlPlaneUrl { get; set; } = string.Empty;
        public string LocalServiceUrl { get; set; } = string.Empty;
        public int RefreshSeconds { get; set; }
    }
}
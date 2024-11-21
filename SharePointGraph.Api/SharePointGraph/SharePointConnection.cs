namespace SharePointGraph.Api.SharePointGraph;

public class SharePointConnection
{
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string SiteId { get; set; }
    public string ReportsFolderId { get; set; }
    public string ReviewRegisterId { get; set; }
}
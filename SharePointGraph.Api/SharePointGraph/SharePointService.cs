using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace SharePointGraph.Api.SharePointGraph;

public interface ISharePointService
{
    GraphServiceClient GetApplicationClient();
    Task UploadFile(Stream file, string reportName, string folderName, CancellationToken cancellationToken);
    Task<byte[]> DownloadFileAsBytesAsync(string folderName, string fileName, CancellationToken cancellationToken);
}

public class SharePointService : ISharePointService
{
    private GraphServiceClient _applicationClient;
    private readonly SharePointConnection _connection;
    public SharePointService(IOptions<SharePointConnection> connection)
    {
        _connection = connection.Value;
    }

    public GraphServiceClient GetApplicationClient()
    {
        if (_applicationClient == null)
        {
            string clientId = _connection.ClientId;
            TokenCredential credential;
            if (!string.IsNullOrEmpty(clientId))
            {
                credential = new ManagedIdentityCredential(clientId);
            }
            else
            {
                credential = new ClientSecretCredential(_connection.TenantId, _connection.ClientId, _connection.ClientSecret);
            }

            _applicationClient = new GraphServiceClient(credential);
        }

        return _applicationClient;
    }

    public async Task UploadFile(Stream stream, string reportName, string folderName, CancellationToken cancellationToken)
    {
        if (folderName == "Reports")
            await _applicationClient.Sites[_connection.SiteId].Drive
                .Items[_connection.ReportsFolderId]
                .ItemWithPath(reportName)
                .Content
                .Request()
                .PutAsync<DriveItem>(stream, cancellationToken);
    }

    public async Task<byte[]> DownloadFileAsBytesAsync(string folderName, string fileName, CancellationToken cancellationToken)
    {
        if (folderName == "Reviews")
        {
            var driveItem = await _applicationClient.Sites[_connection.SiteId].Drive
                .Items[_connection.ReviewRegisterId]
                .ItemWithPath(fileName)
                .Request()
                .GetAsync(cancellationToken);

            var downloadUrl = driveItem.AdditionalData["@microsoft.graph.downloadUrl"].ToString();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
                else
                {
                    throw new Exception($"Failed to download file: {response.ReasonPhrase}");
                }
            }
        }
        throw new Exception($"Failed to download file");
    }
}
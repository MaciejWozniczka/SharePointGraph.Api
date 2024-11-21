namespace SharePointGraph.Api.SharePointGraph;

[ApiController]
public class SharePointGraphDownloadController : ControllerBase
{
    private readonly IMediator _mediator;
    public SharePointGraphDownloadController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("/api/download")]
    public async Task<SharePointGraphDownloadResult> Import(string reportName, string folder)
    {
        return await _mediator.Send(new SharePointGraphDownloadQuery(reportName, folder));
    }

    public class SharePointGraphDownloadQuery : IRequest<SharePointGraphDownloadResult>
    {
        public SharePointGraphDownloadQuery(string reportName, string folder)
        {
            ReportName = reportName;
            Folder = folder;
        }

        public string ReportName { get; set; }
        public string Folder { get; set; }
    }

    public class SharePointGraphDownloadResult : IRequest<SharePointGraphDownloadQuery>
    {
        public byte[]? FileStream { get; set; }
    }

    public class SharePointGraphDownloadHandler : IRequestHandler<SharePointGraphDownloadQuery, SharePointGraphDownloadResult>
    {
        private readonly ISharePointService _sharePointService;
        public SharePointGraphDownloadHandler(ISharePointService sharePointService)
        {
            _sharePointService = sharePointService;
        }

        public async Task<SharePointGraphDownloadResult> Handle(SharePointGraphDownloadQuery stream, CancellationToken cancellationToken)
        {
            _sharePointService.GetApplicationClient();

            var fileStream = await _sharePointService.DownloadFileAsBytesAsync(stream.Folder, stream.ReportName, cancellationToken);

            return new SharePointGraphDownloadResult
            {
                FileStream = fileStream
            };
        }
    }
}
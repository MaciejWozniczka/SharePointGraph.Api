namespace SharePointGraph.Api.SharePointGraph;

[ApiController]
public class SharePointGraphUploadController : ControllerBase
{
    private readonly IMediator _mediator;
    public SharePointGraphUploadController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPut("/api/upload")]
    public async Task<IActionResult> Import(IFormFile file, string reportName, string folder)
    {
        var result = await _mediator.Send(new SharePointGraphUploadQuery(file, reportName, folder));

        return Ok(result);
    }

    public class SharePointGraphUploadQuery : IRequest<Result>
    {
        public SharePointGraphUploadQuery(IFormFile file, string reportName, string folder)
        {
            File = file;
            ReportName = reportName;
            Folder = folder;
        }

        public IFormFile File { get; set; }
        public string ReportName { get; set; }
        public string Folder { get; set; }
    }

    public class SharePointGraphUploadHandler : IRequestHandler<SharePointGraphUploadQuery, Result>
    {
        private readonly ISharePointService _sharePointService;
        public SharePointGraphUploadHandler(ISharePointService sharePointService)
        {
            _sharePointService = sharePointService;
        }

        public async Task<Result> Handle(SharePointGraphUploadQuery stream, CancellationToken cancellationToken)
        {
            _sharePointService.GetApplicationClient();

            await _sharePointService.UploadFile(stream.File.OpenReadStream(), stream.ReportName, stream.Folder, cancellationToken);

            return Result.Ok();
        }
    }
}
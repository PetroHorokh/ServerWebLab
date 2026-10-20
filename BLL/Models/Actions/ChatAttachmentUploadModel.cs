namespace BLL.Models.Actions;

public class ChatAttachmentUploadModel
{
    public required string FileName { get; init; }

    public required long Length { get; init; }

    public required Func<Stream> OpenReadStream { get; init; }
}

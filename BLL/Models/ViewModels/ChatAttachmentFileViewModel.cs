using BLL.Common;

namespace BLL.Models.ViewModels;

public class ChatAttachmentFileViewModel
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required byte[] Content { get; init; }

    public bool IsImage => ChatAttachmentRules.IsImageContentType(ContentType);
}

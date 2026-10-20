namespace DAL.Constraints;

public static class ChatAttachmentConstraints
{
    public const int FileNameMaxLength = 255;

    public const int ContentTypeMaxLength = 100;

    public const int MaxFileSizeBytes = 5 * 1024 * 1024;

    public const int MaxFilesPerMessage = 5;
}

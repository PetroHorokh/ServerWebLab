using BLL.Models.Actions;
using DAL.Constraints;

namespace BLL.Common;

public static class ChatAttachmentRules
{
    private const int TextProbeLength = 8192;

    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];

    private static readonly Dictionary<string, AllowedFileType> TypesByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = AllowedFileType.WithSignature("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".jpeg"] = AllowedFileType.WithSignature("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".png"] = AllowedFileType.WithSignature("image/png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        [".gif"] = AllowedFileType.WithSignature("image/gif", "GIF87a"u8.ToArray(), "GIF89a"u8.ToArray()),
        [".webp"] = AllowedFileType.Custom("image/webp", IsWebP),
        [".pdf"] = AllowedFileType.WithSignature("application/pdf", "%PDF-"u8.ToArray()),
        [".txt"] = AllowedFileType.Custom("text/plain", IsText),
        [".csv"] = AllowedFileType.Custom("text/csv", IsText),
        [".docx"] = AllowedFileType.WithSignature("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ZipSignature),
        [".xlsx"] = AllowedFileType.WithSignature("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ZipSignature),
        [".pptx"] = AllowedFileType.WithSignature("application/vnd.openxmlformats-officedocument.presentationml.presentation", ZipSignature),
        [".zip"] = AllowedFileType.WithSignature("application/zip", ZipSignature),
    };

    public static int MaxFileSizeBytes => ChatAttachmentConstraints.MaxFileSizeBytes;

    public static int MaxFilesPerMessage => ChatAttachmentConstraints.MaxFilesPerMessage;

    public static string MaxFileSizeText { get; } = FileSizeFormatter.Format(ChatAttachmentConstraints.MaxFileSizeBytes);

    public static string AcceptAttribute { get; } = string.Join(",", TypesByExtension.Keys);

    public static string AllowedTypesText { get; } = "JPG, PNG, GIF, WebP, PDF, TXT, CSV, DOCX, XLSX, PPTX and ZIP";

    public static bool IsImageContentType(string contentType) =>
        contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    internal static async Task<(string? Error, List<AcceptedFile> Files)> ReadAsync(
        IReadOnlyList<ChatAttachmentUploadModel> uploads,
        CancellationToken cancellationToken)
    {
        if (uploads.Count > ChatAttachmentConstraints.MaxFilesPerMessage)
        {
            return ($"You can attach at most {ChatAttachmentConstraints.MaxFilesPerMessage} files to a message.", []);
        }

        var files = new List<AcceptedFile>(uploads.Count);
        foreach (var upload in uploads)
        {
            var fileName = CleanFileName(upload.FileName);
            if (fileName.Length == 0)
            {
                return ("One of the files has no name.", []);
            }

            if (upload.Length <= 0)
            {
                return ($"“{fileName}” is empty.", []);
            }

            if (upload.Length > ChatAttachmentConstraints.MaxFileSizeBytes)
            {
                return ($"“{fileName}” is larger than {MaxFileSizeText}.", []);
            }

            if (!TypesByExtension.TryGetValue(Path.GetExtension(fileName), out var type))
            {
                return ($"“{fileName}” can't be attached. Allowed file types: {AllowedTypesText}.", []);
            }

            var content = new byte[upload.Length];
            await using (var stream = upload.OpenReadStream())
            {
                await stream.ReadExactlyAsync(content, cancellationToken);
            }

            if (!type.Matches(content))
            {
                return ($"“{fileName}” doesn't look like a real {Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant()} file.", []);
            }

            files.Add(new AcceptedFile(fileName, type.ContentType, content));
        }

        return (null, files);
    }

    private static string CleanFileName(string? fileName)
    {
        var name = (fileName ?? string.Empty).Replace('\\', '/');
        name = name[(name.LastIndexOf('/') + 1)..];
        name = new string(name.Where(c => !char.IsControl(c)).ToArray()).Trim();

        if (name.Length > ChatAttachmentConstraints.FileNameMaxLength)
        {
            var extension = Path.GetExtension(name);
            name = name[..(ChatAttachmentConstraints.FileNameMaxLength - extension.Length)] + extension;
        }

        return name;
    }

    private static bool IsWebP(byte[] content) =>
        content.Length >= 12 && content.AsSpan(0, 4).SequenceEqual("RIFF"u8) && content.AsSpan(8, 4).SequenceEqual("WEBP"u8);

    private static bool IsText(byte[] content) =>
        !content.AsSpan(0, Math.Min(content.Length, TextProbeLength)).Contains((byte)0);

    internal sealed record AcceptedFile(string FileName, string ContentType, byte[] Content);

    private sealed class AllowedFileType
    {
        private readonly Func<byte[], bool> matches;

        private AllowedFileType(string contentType, Func<byte[], bool> matches)
        {
            ContentType = contentType;
            this.matches = matches;
        }

        public string ContentType { get; }

        public bool Matches(byte[] content) => matches(content);

        public static AllowedFileType WithSignature(string contentType, params byte[][] signatures) =>
            new(contentType, content => signatures.Any(signature => content.AsSpan().StartsWith(signature)));

        public static AllowedFileType Custom(string contentType, Func<byte[], bool> matches) => new(contentType, matches);
    }
}

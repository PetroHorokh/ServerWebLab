using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IChatAttachmentRepository : IRepository<ChatAttachment>
{
    Task<ChatAttachment?> GetWithContentAsync(Guid id, CancellationToken cancellationToken = default);
}

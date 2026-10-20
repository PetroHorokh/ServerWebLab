using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public class ChatAttachmentRepository(CinemaDbContext context) : Repository<ChatAttachment>(context), IChatAttachmentRepository
{
    public Task<ChatAttachment?> GetWithContentAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
            .Include(a => a.ChatMessage)
            .Include(a => a.Data)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
}

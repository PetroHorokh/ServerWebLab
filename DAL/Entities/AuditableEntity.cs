namespace DAL.Entities;

public abstract class AuditableEntity
{
    public Guid CreatedBy { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? ModifiedDateTime { get; set; }
}

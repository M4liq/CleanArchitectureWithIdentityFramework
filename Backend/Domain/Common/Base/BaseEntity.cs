using Domain.Identity;

namespace Domain.Common;

public abstract class BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public virtual ApplicationUserEntity CreatedUser { get; set; }
    public Guid CreatedUserId { get; set; }
    public Guid Id { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public virtual ApplicationUserEntity LastModifiedUser { get; set; }
    public Guid LastModifiedUserId { get; set; }
}
using DripChip.Entities;

namespace DripChip.Domain.Requests;

public interface IRequestContext
{
    public int? UserId { get; }
    public Role? UserRole { get; set; }
}

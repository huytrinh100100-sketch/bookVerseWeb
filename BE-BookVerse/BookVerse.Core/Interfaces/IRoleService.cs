using BookVerse.Core.Common;
using BookVerse.Core.Models.Role;

namespace BookVerse.Core.Interfaces;

public interface IRoleService
{
    Task<ServiceResult<PagedResult<RoleListItemResponse>>> SearchAsync(RoleSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<RoleDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<RoleDetailResponse>> CreateAsync(CreateRoleRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<RoleDetailResponse>> UpdateAsync(Guid id, UpdateRoleRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<RoleDetailResponse>> AssignPermissionsAsync(Guid id, AssignPermissionsRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> RestoreAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Nhân bản vai trò (FE: POST /roles/{id}/clone).</summary>
    Task<ServiceResult<RoleDetailResponse>> CloneAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Gán một vai trò cho một tài khoản (FE: POST /roles/assign).</summary>
    Task<ServiceResult<bool>> AssignRoleAsync(AssignRoleRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Danh sách nhân viên (tài khoản có vai trò) (FE: GET /users/staff).</summary>
    Task<ServiceResult<IReadOnlyCollection<StaffMemberResponse>>> GetStaffAsync(CancellationToken cancellationToken = default);
}

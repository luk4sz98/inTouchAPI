namespace inTouchAPI.Services;

public interface IUserService
{
    Task<Response> AcceptInviteAsync(string userId, string userIdToAccept);
    Task<Response> BlockUserAsync(string requestorId, string userIdToBlock);
    Task<Response> CancelInviteAsync(string userId, string userIdToCancel);
    Task<PagedList<RelationUserDto>> GetRelationUsers(PaginationQueryParameters paginationQueryParameters, string userId, RelationType relationType);
    Task<PagedList<RelationUserDto>> GetWaitingsAsync(string userId, PaginationQueryParameters paginationQueryParameters);
    Task<Response> InviteToFriendsAsync(string requestorId, string userIdToInvite);
    Task<Response> RejectInviteAsync(string userId, string userIdToReject);
    Task<Response> UnblockUserAsync(string requestorId, string userIdToUnblock);
}
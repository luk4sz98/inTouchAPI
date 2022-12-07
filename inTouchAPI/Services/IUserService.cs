namespace inTouchAPI.Services;

public interface IUserService
{
    Task<Response> AddToFriendAsync(string requestorId, string userToAdd);
    Task<Response> BlockUserAsync(string requestorId, string userToBlockEmail);
    Task<Response> UnblockUserAsync(string requestorId, string userEmailToUnblock);
}
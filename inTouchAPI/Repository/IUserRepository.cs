using System.Linq.Expressions;

namespace inTouchAPI.Repository;

public interface IUserRepository
{
    Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters, string userId);
    Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters, string userId, Expression<Func<User, bool>> condition);
    Task<User?> GetUser(Expression<Func<User, bool>> condition);
    Task<User> GetUser(string userId);
    Task UpdateUser(UserUpdateDto userUpdateDto, string userId);
}

using System.Linq.Expressions;

namespace inTouchAPI.Repository;

public interface IUserRepository
{
    Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters);
    Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters, Expression<Func<User, bool>> condition);
    Task<User?> GetUser(Expression<Func<User, bool>> condition);
    Task UpdateUser(UserUpdateDto userUpdateDto, string userId);
}

using System.Linq.Expressions;

namespace inTouchAPI.Repository;

public interface IUserRepository
{
    Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters);
    Task<PagedList<User>> GetUsersByCondition(PaginationQueryParameters paginationQueryParameters, Expression<Func<User, bool>> condition);
    Task<User?> GetUserByCondition(Expression<Func<User, bool>> condition);
    Task UpdateUser(UserUpdateDto userUpdateDto, string userId);
}

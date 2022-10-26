using System.Linq.Expressions;

namespace inTouchAPI.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;

    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public async Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters)
    {
        IQueryable<User> users = _appDbContext.Users;
        return await PagedList<User>.ToPagedListAsync(users, paginationQueryParameters.PageNumber, paginationQueryParameters.PageSize);
    }

    public async Task<User?> GetUserByCondition(Expression<Func<User, bool>> condition)
    {
        return await _appDbContext.Users.FirstOrDefaultAsync(condition);
    }

    public async Task<PagedList<User>> GetUsersByCondition(PaginationQueryParameters paginationQueryParameters, Expression<Func<User, bool>> condition)
    {
        IQueryable<User> users = _appDbContext.Users.Where(condition);
        return await PagedList<User>.ToPagedListAsync(users, paginationQueryParameters.PageNumber, paginationQueryParameters.PageSize);
    }
}

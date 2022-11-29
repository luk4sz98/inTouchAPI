using System.Linq.Expressions;

namespace inTouchAPI.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;

    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<User> GetUser(string userId)
    {
        return await _appDbContext.Users.FirstAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUser(Expression<Func<User, bool>> condition)
    {
        return await _appDbContext.Users.FirstOrDefaultAsync(condition);
    }

    public async Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters)
    {
        IQueryable<User> users = _appDbContext.Users;
        return await PagedList<User>.ToPagedListAsync(users, paginationQueryParameters.PageNumber, paginationQueryParameters.PageSize);
    }

    public async Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters, Expression<Func<User, bool>> condition)
    {
        IQueryable<User> users = _appDbContext.Users.Where(condition);
        return await PagedList<User>.ToPagedListAsync(users, paginationQueryParameters.PageNumber, paginationQueryParameters.PageSize);
    }

    public async Task UpdateUser(UserUpdateDto userUpdateDto, string userId)
    {
        var user = await _appDbContext.Users.FirstAsync(u => u.Id == userId);

        if (!string.IsNullOrEmpty(userUpdateDto.FirstName)) user.FirstName = userUpdateDto.FirstName;
        if (!string.IsNullOrEmpty(userUpdateDto.LastName)) user.LastName = userUpdateDto.LastName;
        if (userUpdateDto.Age != 0) user.Age = userUpdateDto.Age;
        if (userUpdateDto.Sex != SexType.NOTSPECIFIED) user.Sex = userUpdateDto.Sex;
        
        await _appDbContext.SaveChangesAsync();
    }
}

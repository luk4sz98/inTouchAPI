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
}

namespace inTouchAPI.Repository;

public interface IUserRepository
{
    Task<PagedList<User>> GetUsers(PaginationQueryParameters paginationQueryParameters);
}

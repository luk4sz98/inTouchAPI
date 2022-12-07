namespace inTouchAPI.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<Response> AddToFriendAsync(string requestorId, string userToAdd)
    {
        throw new NotImplementedException();
    }

    public async Task<Response> BlockUserAsync(string requestorId, string userToBlockEmail)
    {
        var response = new Response();
        try
        {
            var user = await _dbContext.Users.FirstAsync(u => u.Id == requestorId);
            var userToBlock = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userToBlockEmail.ToUpper()); 
            if (userToBlock is null)
            {
                throw new ArgumentException($"Nie znaleziono użytkownika z tym adresem {userToBlockEmail}!");
            }
            if (user.Relations.Any(r => r.RequestedToUser == userToBlock.Id && r.Type == RelationType.BLOCKED))
            {
                throw new InvalidOperationException($"Użytkownik {userToBlockEmail} jest już na liście zablokowanych!");
            }

            Relation? requestorRelation;
            requestorRelation = await _dbContext.Relations.FirstOrDefaultAsync(r => 
                r.RequestedByUser == user.Id
                && r.RequestedToUser == userToBlock.Id);
            if (requestorRelation is not null)
            {
                requestorRelation.RequestedAt = DateTime.Now;
                requestorRelation.Type = RelationType.BLOCKED;
            }
            else
            {
                requestorRelation = new Relation
                {
                    RequestedAt = DateTime.Now,
                    Type = RelationType.BLOCKED,
                    RequestedToUser = userToBlock.Id,
                    RequestedByUser = user.Id
                };
            }
            var blockedUserRelation = await _dbContext.Relations.FirstOrDefaultAsync(r =>
                r.RequestedByUser == userToBlock.Id && r.RequestedToUser == user.Id && r.Type != RelationType.BLOCKED);
            if (blockedUserRelation is not null)
                _dbContext.Relations.Remove(blockedUserRelation);

            await _dbContext.Relations.AddAsync(requestorRelation);
            await _dbContext.SaveChangesAsync();
            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    public async Task<Response> UnblockUserAsync(string requestorId, string userEmailToUnblock)
    {
        var response = new Response();
        try
        {
            var requestor = await _dbContext.Users.FirstAsync(u => u.Id == requestorId);
            var userToUnblock = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmailToUnblock.ToUpper());
            
            if (userToUnblock is null)
            {
                throw new ArgumentException($"Nie ma użytkownika z tym adresem {userEmailToUnblock}!");
            }

            var relation = await _dbContext.Relations.FirstOrDefaultAsync(r => 
                r.RequestedByUser== requestorId && r.RequestedToUser == userToUnblock.Id && r.Type == RelationType.BLOCKED);
            if (relation is null)
            {
                throw new InvalidOperationException($"Użytkownik {userEmailToUnblock} nie jest zablokowany!");
            }
            
            _dbContext.Relations.Remove(relation);
            await _dbContext.SaveChangesAsync();
            
            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }
}

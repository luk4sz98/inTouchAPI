using inTouchAPI.Models;

namespace inTouchAPI.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _config;

    public UserService(AppDbContext dbContext, IConfiguration config)
    {
        _dbContext = dbContext;
        _config = config;
    }

    public async Task<Response> AcceptInviteAsync(string userId, string userIdToAccept)
    {
        var response = new Response();
        try
        {
            var relation = await _dbContext.Relations
                .FirstOrDefaultAsync(r =>
                    r.RequestedByUser == userIdToAccept &&
                    r.RequestedToUser == userId &&
                    r.Type == RelationType.INVITED);
            if (relation is null)
                throw new InvalidOperationException($"Użytkownik nie jest zaproszony!");
            
            var now = DateTime.Now;
            relation.Type = RelationType.FRIEND;
            relation.RequestedAt = now;

            var invitedUserRelation = new Relation
            {
                RequestedByUser = userId,
                RequestedToUser = userIdToAccept,
                Type = RelationType.FRIEND,
                RequestedAt = now
            };

            await _dbContext.Relations.AddAsync(invitedUserRelation);
            await _dbContext.SaveChangesAsync();

            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    public async Task<Response> BlockUserAsync(string requestorId, string userIdToBlock)
    {
        var response = new Response();
        try
        {
            var user = await _dbContext.Users.FirstAsync(u => u.Id == requestorId);
            var userToBlock = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdToBlock); 
            if (userToBlock is null)
            {
                throw new ArgumentException($"Nie znaleziono użytkownika z tym ID {userIdToBlock}!");
            }
            if (user.Relations.Any(r => r.RequestedToUser == userToBlock.Id && r.Type == RelationType.BLOCKED))
            {
                throw new InvalidOperationException($"Użytkownik z tym ID {userIdToBlock} jest już na liście zablokowanych!");
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
                await _dbContext.Relations.AddAsync(requestorRelation);
            }
            var blockedUserRelation = await _dbContext.Relations.FirstOrDefaultAsync(r =>
                r.RequestedByUser == userToBlock.Id && r.RequestedToUser == user.Id && r.Type != RelationType.BLOCKED);
            if (blockedUserRelation is not null)
                _dbContext.Relations.Remove(blockedUserRelation);

            await _dbContext.SaveChangesAsync();
            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    public async Task<Response> CancelInviteAsync(string userId, string userIdToCancel)
    {
        var response = new Response();
        try
        {
            var inviteRelation = await _dbContext.Relations
                .FirstOrDefaultAsync(r => 
                    r.RequestedToUser == userIdToCancel && 
                    r.RequestedByUser == userId && 
                    r.Type == RelationType.INVITED);

            if (inviteRelation is null)
                throw new InvalidOperationException("Użytkownik nie jest zaproszony");
            _dbContext.Relations.Remove(inviteRelation);
            await _dbContext.SaveChangesAsync();
            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    public async Task<PagedList<RelationUserDto>> GetRelationUsers(PaginationQueryParameters paginationQueryParameters, string userId,
        RelationType relationType)
    {
        IQueryable<RelationUserDto> relationUsers = _dbContext.Relations
            .Where(r => r.RequestedByUser == userId && r.Type == relationType)
            .Select(r => new { User = _dbContext.Users.First(u => u.Id == r.RequestedToUser), Date = r.RequestedAt })
            .Select(a => new RelationUserDto 
            { 
                FirstName = a.User.FirstName,
                LastName = a.User.LastName,
                Email = a.User.Email,
                AvatarSource = string.IsNullOrEmpty(a.User.Avatar.Source) 
                    ? "" 
                    : _config.GetSection("BlobStorage").GetValue<string>("AvatarsUrl") + a.User.Avatar.Source,
                Id = a.User.Id,
                RequestAt = a.Date
            });
        return await PagedList<RelationUserDto>
            .ToPagedListAsync(relationUsers, paginationQueryParameters.PageNumber, paginationQueryParameters.PageSize);
    }

    public async Task<PagedList<RelationUserDto>> GetWaitingsAsync(string userId, PaginationQueryParameters paginationQueryParameters)
    {
        IQueryable<RelationUserDto> relationUsers = 
            _dbContext.Relations
            .Where(r => r.RequestedToUser == userId && r.Type == RelationType.INVITED)
            .Select(r => new { User = _dbContext.Users.First(u => u.Id == r.RequestedByUser), Date = r.RequestedAt })
            .Select(a => new RelationUserDto
            {
                FirstName = a.User.FirstName,
                LastName = a.User.LastName,
                Email = a.User.Email,
                AvatarSource = string.IsNullOrEmpty(a.User.Avatar.Source)
                    ? ""
                    : _config.GetSection("BlobStorage").GetValue<string>("AvatarsUrl") + a.User.Avatar.Source,
                Id = a.User.Id,
                RequestAt = a.Date
            });
        return await PagedList<RelationUserDto>
            .ToPagedListAsync(relationUsers, paginationQueryParameters.PageNumber, paginationQueryParameters.PageSize);
    }

    public async Task<Response> InviteToFriendsAsync(string requestorId, string userIdToInvite)
    {
        var response = new Response();
        try
        {
            var userToInvite = await _dbContext.Users
                .Include(u => u.Relations)
                .FirstOrDefaultAsync(u => u.Id == userIdToInvite);
            
            if (userToInvite is null)
                throw new ArgumentException($"Brak użytkownika z tym adresem email!");
            
            var userToInviteAlreadyInvitedRequestor = 
                userToInvite.Relations
                .Any(r => r.RequestedToUser == requestorId && r.Type == RelationType.INVITED);            
            if (userToInviteAlreadyInvitedRequestor)
                throw new InvalidOperationException($"Użytkownik zapraszający już jest zaproszony przez {userToInvite.Email}!");
            
            var requestor = await _dbContext.Users
                .Include(u => u.Relations)
                .FirstAsync(u => u.Id == requestorId);
            
            var usersIsAlreadyInvitedOrBlocked =
                requestor.Relations
                .Any(r => r.RequestedToUser == userToInvite.Id && (r.Type == RelationType.INVITED || r.Type == RelationType.BLOCKED));
            if (usersIsAlreadyInvitedOrBlocked)
                throw new InvalidOperationException($"Użytkownik {userToInvite.Email} jest już zaproszony lub zablokowany!");

            var requestorRelation = new Relation
            {
                RequestedAt = DateTime.Now,
                RequestedByUser = requestorId,
                RequestedToUser = userToInvite.Id,
                Type = RelationType.INVITED
            };

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

    public async Task<Response> RejectInviteAsync(string userId, string userIdToReject)
    {
        var response = new Response();
        try
        {
            var relation = await _dbContext.Relations
                .FirstAsync(r =>
                    r.RequestedByUser == userIdToReject &&
                    r.RequestedToUser == userId &&
                    r.Type == RelationType.INVITED);

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

    public async Task<Response> UnblockUserAsync(string requestorId, string userIdToUnblock)
    {
        var response = new Response();
        try
        {
            var requestor = await _dbContext.Users.FirstAsync(u => u.Id == requestorId);
            var userToUnblock = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdToUnblock);
            
            if (userToUnblock is null)
            {
                throw new ArgumentException($"Nie ma użytkownika z tym ID {userIdToUnblock}!");
            }

            var relation = await _dbContext.Relations.FirstOrDefaultAsync(r => 
                r.RequestedByUser== requestorId && r.RequestedToUser == userToUnblock.Id && r.Type == RelationType.BLOCKED);
            if (relation is null)
            {
                throw new InvalidOperationException($"Użytkownik z tym ID {userIdToUnblock} nie jest zablokowany!");
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

namespace inTouchAPI.DataContext;

public class AppDbContext : IdentityDbContext<User>
{
    public virtual DbSet<Avatar> Avatars { get; set; }
    public virtual DbSet<Chat> Chats { get; set; }
    public virtual DbSet<ChatUser> ChatUsers { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Relation> Relations { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .ToTable("Users");
        builder.Entity<ChatUser>()
            .HasKey(c => new {c.ChatId, c.UserId});
        builder.Entity<Relation>()
            .HasKey(r => new { r.RequestedByUser, r.RequestedToUser });

        builder.Entity<Chat>()
            .HasMany(c => c.Messages)
            .WithOne(msg => msg.Chat)
            .HasForeignKey(msg => msg.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Chat>()
            .HasMany(c => c.Users)
            .WithOne(chU => chU.Chat)
            .HasForeignKey(chU => chU.ChatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<User>()
            .HasMany(u => u.Chats)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<User>()
            .HasMany(u => u.Relations)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.RequestedByUser)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Avatar>()
            .HasOne(a => a.User)
            .WithOne(u => u.Avatar);

        builder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<User>()
            .HasMany(u => u.SendedMessages)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<User>()
            .HasMany(u => u.Chats)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

namespace inTouchAPI.Models
{
    public class RefreshToken
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual string UserId { get; set; }
        public virtual string Token { get; set; }
        public virtual string JwtId { get; set; }
        public virtual bool IsUsed { get; set; }
        public virtual bool IsRevoked { get; set; }
        public virtual DateTime AddedDate { get; set; }
        public virtual DateTime ExpireDate { get; set; }
        public virtual User User { get; set; }
    }
}

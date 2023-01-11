namespace inTouchAPI.Dtos
{
    public class ChatUserDto
    {
        public Guid ChatId { get; set; }
        public string UserId { get; set; }
        public virtual Chat Chat { get; set; }
        public virtual User User { get; set; }
    }
}

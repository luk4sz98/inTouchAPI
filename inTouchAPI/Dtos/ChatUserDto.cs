namespace inTouchAPI.Dtos
{
    public class ChatUserDto
    {
        public Guid ChatId { get; set; }
        public string UserId { get; set; }
        public virtual UserDto User { get; set; }
    }
}

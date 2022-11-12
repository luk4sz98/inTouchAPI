namespace inTouchAPI.Dtos
{
    public class ChatDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ChatType Type { get; set; }

        public ICollection<MessageDto> Messages { get; set; } = new List<MessageDto>();
    }
}

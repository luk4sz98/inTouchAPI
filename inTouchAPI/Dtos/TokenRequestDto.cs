namespace inTouchAPI.Dtos
{
    public class TokenRequestDto
    {
        [Required]
        public string Token { get; set; }
        
        [Required]
        public string RefreshToken { get; set; }
    }
}

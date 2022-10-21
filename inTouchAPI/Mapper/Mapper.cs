namespace inTouchAPI.Mapper;

public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<UserRegistrationDto, User>()
            .ForMember(dest => dest.RegistrationDate, y => y.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.UserName, y => y.MapFrom(src => src.Email));
        CreateMap<User, UserDto>();
    }
}

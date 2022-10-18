using AutoMapper;

namespace inTouchAPI.Mapper;

public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<UserRegisterDto, User>()
            .ForMember(dest => dest.RegistrationDate, y => y.MapFrom(src => DateTime.Now));
    }
}

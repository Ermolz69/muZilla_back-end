using muZilla.Application.DTOs.User;

namespace muZilla.ResponseRequestModels
{
    public class RegisterUserRequest
    {
        public RegisterDTO registerDTO {  get; set; }
        public IFormFile? profile {  get; set; }
    }
}

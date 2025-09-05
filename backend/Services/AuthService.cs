using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.Models;
using backend.DTOs;
using backend.Configurations;
using BCrypt.Net;

namespace backend.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<List<UserDto>>> GetPendingUsersAsync();
        Task<ApiResponse<UserDto>> ApproveUserAsync(int userId, int approvedById);
        Task<ApiResponse<UserDto>> RejectUserAsync(int userId);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly HrmDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(HrmDbContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.ApprovedBy)
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không chính xác"
                    };
                }

                if (!user.IsActive)
                {
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Tài khoản đã bị vô hiệu hóa"
                    };
                }

                if (!user.IsApproved && user.Role != UserRole.Admin)
                {
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Tài khoản chưa được phê duyệt"
                    };
                }

                var token = GenerateJwtToken(user);
                var userDto = MapToUserDto(user);

                return new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Data = new LoginResponseDto
                    {
                        Token = token,
                        User = userDto
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đăng nhập",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    };
                }

                // Kiểm tra username đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Tên đăng nhập đã được sử dụng"
                    };
                }

                // Kiểm tra xem có user nào trong hệ thống chưa
                var hasAnyUsers = await _context.Users.AnyAsync();
                
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    FullName = registerDto.FullName,
                    PhoneNumber = registerDto.PhoneNumber,
                    Role = hasAnyUsers ? UserRole.Employee : UserRole.Admin, // Người đăng ký đầu tiên là Admin
                    IsActive = true,
                    IsApproved = !hasAnyUsers, // Người đầu tiên tự động được phê duyệt
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = MapToUserDto(user);

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = hasAnyUsers 
                        ? "Đăng ký thành công! Vui lòng chờ HR Manager phê duyệt tài khoản." 
                        : "Đăng ký thành công! Bạn là Admin đầu tiên của hệ thống.",
                    Data = userDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đăng ký",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<UserDto>>> GetPendingUsersAsync()
        {
            try
            {
                var pendingUsers = await _context.Users
                    .Where(u => !u.IsApproved && u.IsActive)
                    .Include(u => u.ApprovedBy)
                    .OrderBy(u => u.CreatedAt)
                    .ToListAsync();

                var userDtos = pendingUsers.Select(MapToUserDto).ToList();

                return new ApiResponse<List<UserDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách người dùng chờ phê duyệt thành công",
                    Data = userDtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<UserDto>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách người dùng",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> ApproveUserAsync(int userId, int approvedById)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.ApprovedBy)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                if (user.IsApproved)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Người dùng đã được phê duyệt"
                    };
                }

                user.IsApproved = true;
                user.ApprovedById = approvedById;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var userDto = MapToUserDto(user);

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "Phê duyệt người dùng thành công",
                    Data = userDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi phê duyệt người dùng",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> RejectUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.ApprovedBy)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var userDto = MapToUserDto(user);

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "Từ chối người dùng thành công",
                    Data = userDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi từ chối người dùng",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.ApprovedBy)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                var userDto = MapToUserDto(user);

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "Lấy thông tin người dùng thành công",
                    Data = userDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin người dùng",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("FullName", user.FullName),
                new("PhoneNumber", user.PhoneNumber ?? ""),
                new("IsActive", user.IsActive.ToString()),
                new("IsApproved", user.IsApproved.ToString()),
                new("CreatedAt", user.CreatedAt.ToString("o"))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                IsApproved = user.IsApproved,
                CreatedAt = user.CreatedAt,
                ApprovedBy = user.ApprovedBy?.FullName
            };
        }
    }
}

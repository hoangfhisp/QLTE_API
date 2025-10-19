using Backend_QLTE.AuthService.Domain.Entities;

namespace Backend_QLTE.AuthService.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default); // Thêm mới Refresh Token
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default); // Lấy Refresh Token theo chuỗi token
        Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default); // Cập nhật Refresh Token
    }
}

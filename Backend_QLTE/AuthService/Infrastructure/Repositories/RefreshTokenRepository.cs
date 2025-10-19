using Backend_QLTE.AuthService.Application.Interfaces.Repositories;
using Backend_QLTE.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_QLTE.AuthService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;
        public RefreshTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        // Thêm mới Refresh Token
        public async Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return refreshToken;
        }

        // Lấy Refresh Token theo token
        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
            return refreshToken;
        }
    }
}

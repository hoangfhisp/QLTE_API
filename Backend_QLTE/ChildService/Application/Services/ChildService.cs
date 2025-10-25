using Backend_QLTE.ChildService.Application.DTOs.Client.Child;
using Backend_QLTE.ChildService.Application.DTOs.Common;
using Backend_QLTE.ChildService.Application.Interfaces.Services;
using Backend_QLTE.ChildService.Domain.Entities;
using Backend_QLTE.ChildService.Infrastructure.Data;
using Backend_QLTE.ChildService.shared.Exceptions;
using Backend_QLTE.UserService.Application.DTOs.Client.User;
using Backend_QLTE.UserService.Application.Interfaces.Services;
using Backend_QLTE.UserService.Domain.Entities;
using Backend_QLTE.UserService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend_QLTE.ChildService.Application.Services
{
    public class ChildService : IChildService
    {
        private readonly ChildDbContext _childContext;
        private readonly UserDbContext _userContext;
        private readonly IZoneService _zoneService;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;

        public ChildService(ChildDbContext childContext, UserDbContext userContext
            ,IZoneService zoneService, IUserService userService
            , UserManager<User> userManager)
        {
            _childContext = childContext;
            _userContext = userContext;
            _zoneService = zoneService;
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<ResultListDTO<ChildrenResponseDTO>> GetChildrenByUserIdAsync (string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ApiException("Userid không hợp lệ hoặc không được để trống!", 409);
            }

            var get = await _childContext.Children.Where(c => c.UserId == userId).ToListAsync();

            if (get.Count == 0)
            {
                throw new ApiException($"Không tìm thấy children nào của userId '{userId}'.",404);
            }

            var result = get.Select(c => new ChildrenResponseDTO
            {
                ChildId = c.ChildId,
                HoTen = c.HoTen,
                GioiTinh = c.GioiTinh,
                NgaySinh = c.NgaySinh,
                AnhDaiDien = c.AnhDaiDien,
                NhomTuoi = c.NhomTuoi,
                TrangThai = c.TrangThai,
                UserId = c.UserId,
            }).ToList();

            return ResultListDTO<ChildrenResponseDTO>.Success(result, $"Lấy danh sách children thuộc UserId '{userId}' thành công.");

        }

        public async Task<ResultDTO> CreateChildAsync(CreateChildRequestDTO request)
        {
            var parent = await _userService.FindUserByUserIdAsync(new UserService.Application.DTOs.Admin.User.FindUserByIdRequestDTO { UserId = request.ParentId });
            if(parent.Status == false)
            {
                throw new ApiException("Parent không tồn tại hoặc sai role", 404);
            }

            var user = new UserRegisterDTO
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
            };

            var createUser = await _userService.RegisterUserAsync(user);

            var userId = await _userManager.FindByEmailAsync(request.Email);
            if (userId == null)
            {
                throw new ApiException("User không tồn tại",404);
            }

            var update = new InfoUserUpdateRequestDTO
            {
                NameND = request.FullName,
                AvatarND = request.AvatarND,
                GioiTinh = request.GioiTinh,
                UserName = userId.UserName,
                MoTa = userId.MoTa,
                PhoneNumber = userId.PhoneNumber,
                UserId = userId.Id,   
            };


            var updateUser = await _userService.UpdateInfoUserAsync(update);

            var role = await _userManager.GetRolesAsync(userId);
            await _userManager.RemoveFromRoleAsync(userId, role.FirstOrDefault().ToString());
            await _userManager.AddToRoleAsync(userId, "Children");

            var age = DateTime.Now.Year - request.NgaySinh.Year;
            if (DateTime.Now.DayOfYear < request.NgaySinh.DayOfYear)
            {
                age--;
            }

            // Xác định nhóm tuổi
            string nhomTuoi;
            if (age < 6)
                nhomTuoi = "Mẫu giáo";
            else if (age < 11)
                nhomTuoi = "Thiếu nhi";
            else if (age < 16)
                nhomTuoi = "Thiếu niên";
            else if (age < 18)
                nhomTuoi = "Vị thành niên";
            else
                nhomTuoi = "Trưởng thành";

            var create = new Child
            {
                ChildId = Guid.Parse(userId.Id),
                AnhDaiDien = userId.AvatarND,
                HoTen = userId.NameND,
                GioiTinh = userId.GioiTinh.ToString(),
                NgaySinh = request.NgaySinh,
                UserId = request.ParentId,
                NhomTuoi = nhomTuoi,
                TrangThai = "True"

            };

            var createChild = await _childContext.Children.AddAsync(create);
            await _childContext.SaveChangesAsync();

            return ResultDTO.Success("Tạo Children thành công");
        }

        public async Task<ResultDTO<ChildrenResponseDTO>> UpdateChildAsync(UpdateChildRequestDTO request)
        {
            var parent = await _userService.FindUserByUserIdAsync(new UserService.Application.DTOs.Admin.User.FindUserByIdRequestDTO { UserId = request.ChildrenId.ToString() });
            if (parent.Status == false || parent.Data.Role == "Parent")
            {
                throw new ApiException("Parent không tồn tại hoặc sai role", 404);
            }

            var user = new InfoUserUpdateRequestDTO
            {
                UserId = parent.Data.UserId,
                NameND = request.NameND,
                AvatarND = request.AvatarND,
                GioiTinh = request.GioiTinh,
                UserName = request.UserName,
                MoTa = request.MoTa,
                PhoneNumber = request.PhoneNumber,
            };

            var updateUser = await _userService.UpdateInfoUserAsync(user);

            var userId = await _userManager.FindByIdAsync(user.UserId);

            var age = DateTime.Now.Year - request.NgaySinh.Year;
            if (DateTime.Now.DayOfYear < request.NgaySinh.DayOfYear)
            {
                age--;
            }

            // Xác định nhóm tuổi
            string nhomTuoi;
            if (age < 6)
                nhomTuoi = "Mẫu giáo";
            else if (age < 11)
                nhomTuoi = "Thiếu nhi";
            else if (age < 16)
                nhomTuoi = "Thiếu niên";
            else if (age < 18)
                nhomTuoi = "Vị thành niên";
            else
                nhomTuoi = "Trưởng thành";

            var children = await _childContext.Children.FirstOrDefaultAsync(c => c.ChildId == request.ChildrenId);
            children.AnhDaiDien = userId.AvatarND;
            children.HoTen = userId.NameND;
            children.GioiTinh = userId.GioiTinh.ToString();
            children.NgaySinh = request.NgaySinh;
            children.NhomTuoi = nhomTuoi;
            children.TrangThai = request.TrangThai.ToString();

            var updateChild = _childContext.Children.Update(children);
            if(updateChild == null)
            {
                throw new ApiException($"Update cho children '{userId.Email}' thất bại!",400);
            }
            await _childContext.SaveChangesAsync();

            var result = new ChildrenResponseDTO
            {
                ChildId = request.ChildrenId,
                UserId = children.UserId,
                HoTen = children.HoTen,
                AnhDaiDien= children.AnhDaiDien,
                GioiTinh= children.GioiTinh,
                NgaySinh= children.NgaySinh,
                NhomTuoi= children.NhomTuoi,
                TrangThai = children.TrangThai,
            };

            return ResultDTO<ChildrenResponseDTO>.Success(result,"Update Children thành công");
        }

        public async Task<ResultDTO> DeleteChildAsync(Guid childrenId)
        {
            var child = await _childContext.Children.FirstOrDefaultAsync(c => c.ChildId == childrenId);
            if(child == null)
            {
                throw new ApiException($"Không tồn tại childrenId '{childrenId}' trong hệ thống!", 404);
            }
            
            var safeZone = await _childContext.SafeZones.Where(s => s.ChildrenId == childrenId).ToListAsync();
            if (safeZone.Any())
            {
                // xóa safe zone
                await _zoneService.DeleteSafeZoneByChildrenIdAsync(childrenId);
            }

            var dangerZone = await _childContext.DangerZones.Where(s => s.ChildrenId == childrenId).ToListAsync();
            if(dangerZone.Any())
            {
                await _zoneService.DeleteDangerZoneByChildrenIdAsync(childrenId);
            }

            var delete = _childContext.Remove(child);
            if(delete == null)
            {
                throw new ApiException($"Xóa childrenId '{childrenId}' thất bại!");
            }
            await _childContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(childrenId.ToString());
            if(user == null)
            {
                throw new ApiException($"Không tồn tại user children '{childrenId}' trong hệ thống!",404);
            }
            var deleteUser = await _userManager.DeleteAsync(user);
            if(deleteUser.Succeeded == false)
            {
                throw new ApiException($"Xóa user children '{childrenId}' thất bại!");
            }

            return ResultDTO.Success($"Xóa childrenId '{childrenId}' thành công!");
        }

        public async Task<ResultDTO<ChildrenResponseDTO>> GetChildByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ApiException("UserId không hợp lệ hoặc bị trống!", 400);
            }

            // 🔍 Truy vấn child dựa vào UserId của chính trẻ (chính là ChildId)
            var child = await _childContext.Children
                .FirstOrDefaultAsync(c => c.ChildId.ToString() == userId);

            if (child == null)
            {
                throw new ApiException($"Không tìm thấy child tương ứng với UserId '{userId}'", 404);
            }

            var result = new ChildrenResponseDTO
            {
                ChildId = child.ChildId,
                HoTen = child.HoTen,
                GioiTinh = child.GioiTinh,
                NgaySinh = child.NgaySinh,
                AnhDaiDien = child.AnhDaiDien,
                NhomTuoi = child.NhomTuoi,
                TrangThai = child.TrangThai,
                UserId = child.UserId, // đây là ParentId
            };

            return ResultDTO<ChildrenResponseDTO>.Success(result,
                $"Lấy thông tin child của userId '{userId}' thành công.");
        }
    }
}

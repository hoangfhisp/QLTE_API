using Backend_QLTE.ChildService.Application.DTOs.Client.DeviceInfo;
using Backend_QLTE.ChildService.Application.DTOs.Common;
using Backend_QLTE.ChildService.Application.Exceptions.Duplicates;
using Backend_QLTE.ChildService.Application.Exceptions.Failed;
using Backend_QLTE.ChildService.Application.Exceptions.NotFounds;
using Backend_QLTE.ChildService.Application.Interfaces.Mappers;
using Backend_QLTE.ChildService.Application.Interfaces.Orchestrators;
using Backend_QLTE.ChildService.Application.Interfaces.Repositories;
using Backend_QLTE.ChildService.Application.Interfaces.Services;
using Backend_QLTE.ChildService.Api.Hubs; // ✅ SignalR Hub
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Backend_QLTE.ChildService.Application.Services
{
    public class DeviceInfoService : IDeviceInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDeviceInfoOrchestrator _orchestrator;
        private readonly IDeviceInfoResponseMapper _mapper;
        private readonly IPaginationMapper _paginationMapper;
        private readonly IHubContext<DeviceHub> _hubContext; // ✅ SignalR
        private readonly ILogger<DeviceInfoService> _logger;

        public DeviceInfoService(
            IUnitOfWork unitOfWork,
            IDeviceInfoOrchestrator orchestrator,
            IDeviceInfoResponseMapper mapper,
            IPaginationMapper paginationMapper,
            IHubContext<DeviceHub> hubContext,
            ILogger<DeviceInfoService> logger)
        {
            _unitOfWork = unitOfWork;
            _orchestrator = orchestrator;
            _mapper = mapper;
            _paginationMapper = paginationMapper;
            _hubContext = hubContext;
            _logger = logger;
        }

        // ⚡ Helper broadcast method
        private async Task BroadcastToGroup(Guid childId, string eventName, object payload)
        {
            await _hubContext.Clients.Group(childId.ToString()).SendAsync(eventName, payload);
            _logger.LogInformation("SignalR: Gửi event {EventName} tới ChildId={ChildId}", eventName, childId);
        }

        // 🟢 Lấy tất cả thiết bị
        public async Task<ResultListDTO<List<DeviceInfoResponseDTO>>> GetAllAsync(int page, int limit, CancellationToken cancellationToken = default)
        {
            var (entities, total, last) = await _unitOfWork.DeviceInfos.GetAllAsync(page, limit, cancellationToken);
            if (total == 0)
                throw new DeviceInfoNotFoundException();

            var dtoList = _mapper.ToDtoList(entities);
            var pagination = _paginationMapper.ToDto(page, limit, total, last);
            return ResultListDTO<List<DeviceInfoResponseDTO>>.Success(dtoList, "Lấy danh sách thiết bị thành công", pagination);
        }

        // 🔵 Lấy chi tiết
        public async Task<ResultDTO<DeviceInfoResponseDTO>> GetDetailAsync(Guid deviceId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.DeviceInfos.GetByIdAsync(deviceId, cancellationToken)
                ?? throw new DeviceInfoNotFoundException(deviceId);

            var dto = _mapper.ToDto(entity);
            return ResultDTO<DeviceInfoResponseDTO>.Success(dto, "Lấy chi tiết thiết bị thành công");
        }

        // 🟡 Lấy thiết bị theo ChildId
        public async Task<ResultListDTO<List<DeviceInfoResponseDTO>>> GetByChildAsync(Guid childId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.DeviceInfos.GetByChildIdAsync(childId, cancellationToken);
            if (entity == null)
                throw new DeviceInfoNotFoundException();

            var dto = _mapper.ToDto(entity);
            return ResultListDTO<List<DeviceInfoResponseDTO>>.Success(new List<DeviceInfoResponseDTO> { dto }, "Lấy thiết bị theo trẻ thành công");
        }

        // 🟢 Tạo mới
        public async Task<ResultDTO> CreateAsync(DeviceInfoCreateDTO dto, string userId, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.DeviceInfos.ExistsByIMEIAsync(dto.IMEI, cancellationToken);
            if (exists)
                throw new DuplicateDeviceInfoException(dto.IMEI);

            var entity = await _orchestrator.CreateAsync(dto, userId, cancellationToken);
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var result = await _unitOfWork.DeviceInfos.CreateAsync(entity, cancellationToken);

            if (result is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new CreateDeviceInfoFailedException(dto.ChildId);
            }

            await transaction.CommitAsync(cancellationToken);
            await BroadcastToGroup(dto.ChildId, "DeviceCreated", _mapper.ToDto(result));
            return ResultDTO.Success("Tạo thiết bị thành công");
        }

        // 🟠 Cập nhật
        public async Task<ResultDTO<DeviceInfoResponseDTO>> UpdateAsync(DeviceInfoUpdateDTO dto, string userId, CancellationToken cancellationToken = default)
        {
            var entity = await _orchestrator.UpdateAsync(dto, userId, cancellationToken);
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var result = await _unitOfWork.DeviceInfos.UpdateAsync(entity, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = _mapper.ToDto(result);
            await BroadcastToGroup(dto.ChildId, "DeviceUpdated", response);
            return ResultDTO<DeviceInfoResponseDTO>.Success(response, "Cập nhật thiết bị thành công");
        }

        // 🔴 Xóa
        public async Task<ResultDTO> DeleteAsync(DeviceInfoDeleteDTO dto, string userId, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.DeviceInfos.GetByIdAsync(dto.DeviceId, cancellationToken)
                ?? throw new DeviceInfoNotFoundException(dto.DeviceId);

            await _orchestrator.DeleteAsync(dto, userId, cancellationToken);
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.DeviceInfos.DeleteAsync(entity, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await BroadcastToGroup(entity.ChildId, "DeviceDeleted", new { dto.DeviceId });
            return ResultDTO.Success("Xóa thiết bị thành công");
        }

        // ⚡ Cập nhật trạng thái pin / online
        public async Task<ResultDTO> UpdateStatusAsync(Guid deviceId, int? pin, bool? online, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.DeviceInfos.GetByIdAsync(deviceId, cancellationToken)
                ?? throw new DeviceInfoNotFoundException(deviceId);

            if (pin.HasValue) entity.Pin = pin.Value;
            if (online.HasValue) entity.TrangThaiOnline = online.Value;
            entity.LanCapNhatCuoi = DateTime.Now;

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.DeviceInfos.UpdateAsync(entity, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await BroadcastToGroup(entity.ChildId, "DeviceStatusUpdated", new
            {
                entity.DeviceId,
                entity.Pin,
                entity.TrangThaiOnline,
                entity.LanCapNhatCuoi
            });

            return ResultDTO.Success("Cập nhật trạng thái thiết bị thành công");
        }

        // 🔒 Khoá thiết bị (Realtime)
        public async Task<ResultDTO> LockDeviceAsync(Guid childId, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Phụ huynh {UserId} gửi lệnh khoá thiết bị cho ChildId={ChildId}", userId, childId);
            await _hubContext.Clients.Group(childId.ToString())
                .SendAsync("DeviceLocked", new { ChildId = childId, LockedBy = userId }, cancellationToken);
            return ResultDTO.Success("Đã gửi lệnh khóa thiết bị thành công.");
        }

        // 🔓 Mở khoá thiết bị (Realtime)
        public async Task<ResultDTO> UnlockDeviceAsync(Guid childId, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Phụ huynh {UserId} gửi lệnh mở khoá thiết bị cho ChildId={ChildId}", userId, childId);
            await _hubContext.Clients.Group(childId.ToString())
                .SendAsync("DeviceUnlocked", new { ChildId = childId, UnlockedBy = userId }, cancellationToken);
            return ResultDTO.Success("Đã gửi lệnh mở khóa thiết bị thành công.");
        }
    }
}

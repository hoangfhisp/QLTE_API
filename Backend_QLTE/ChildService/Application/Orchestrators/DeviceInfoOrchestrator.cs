using Backend_QLTE.ChildService.Application.DTOs.Client.DeviceInfo;
using Backend_QLTE.ChildService.Application.Interfaces.Factories;
using Backend_QLTE.ChildService.Application.Interfaces.Orchestrators;
using Backend_QLTE.ChildService.Application.Interfaces.Repositories;
using Backend_QLTE.ChildService.Domain.Entities;
using Backend_QLTE.ChildService.Domain.Services.Interfaces;
using Backend_QLTE.ChildService.Infrastructure.Data.HttpClients.Interfaces;
using Backend_QLTE.ChildService.Domain.Exceptions.Duplicates;
using Backend_QLTE.ChildService.Domain.Exceptions.NotFounds;
using Microsoft.Extensions.Logging;

namespace Backend_QLTE.ChildService.Application.Orchestrators
{
    public class DeviceInfoOrchestrator : IDeviceInfoOrchestrator
    {
        private readonly IDeviceInfoFactory _factory;
        private readonly IDeviceInfoDomainService _domainService;
        private readonly IDeviceInfoRepository _repository;
        private readonly IChildRepository _childRepository; // 🔹 để xác minh Child thuộc User này
        private readonly IUserServiceClient _userServiceClient; // 🔹 để xác minh phụ huynh tồn tại
        private readonly ILogger<DeviceInfoOrchestrator> _logger;

        public DeviceInfoOrchestrator(
            IDeviceInfoFactory factory,
            IDeviceInfoDomainService domainService,
            IDeviceInfoRepository repository,
            IChildRepository childRepository,
            IUserServiceClient userServiceClient,
            ILogger<DeviceInfoOrchestrator> logger)
        {
            _factory = factory;
            _domainService = domainService;
            _repository = repository;
            _childRepository = childRepository;
            _userServiceClient = userServiceClient;
            _logger = logger;
        }

        // 🟢 Tạo mới DeviceInfo
        public async Task<DeviceInfo> CreateAsync(DeviceInfoCreateDTO dto, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Orchestrator: Bắt đầu tạo DeviceInfo cho ChildId={ChildId}, UserId={UserId}", dto.ChildId, userId);

            // 1️⃣ Kiểm tra phụ huynh tồn tại qua UserService
            var userResponse = await _userServiceClient.FindUserByUserIdAsync(userId, cancellationToken);
            if (userResponse?.Data == null || !userResponse.Data.Status)
                throw new KeyNotFoundException($"Không tìm thấy phụ huynh với UserId={userId}");

            // 2️⃣ Xác minh ChildId hợp lệ và thuộc quyền phụ huynh này
            var child = await _childRepository.GetByIdAsync(dto.ChildId, cancellationToken);
            if (child == null)
                throw new ChildNotFoundException(dto.ChildId.ToString());
            if (child.UserId != userId)
                throw new UnauthorizedAccessException("Không có quyền thêm thiết bị cho trẻ này.");

            // 3️⃣ Kiểm tra IMEI trùng
            var exists = await _repository.ExistsByIMEIAsync(dto.IMEI, cancellationToken);
            if (exists)
                throw new DuplicateDeviceInfoException(dto.IMEI);

            // 4️⃣ Tạo entity và validate logic domain
            var entity = _factory.Create(dto);
            _domainService.EnsureCanCreate(entity);

            // 5️⃣ Ghi DB
            var created = await _repository.CreateAsync(entity, cancellationToken);
            _logger.LogInformation("Orchestrator: DeviceInfo {Id} đã được tạo thành công cho ChildId={ChildId}", created.DeviceId, dto.ChildId);

            return created;
        }

        // 🟡 Cập nhật
        public async Task<DeviceInfo> UpdateAsync(DeviceInfoUpdateDTO dto, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Orchestrator: Bắt đầu cập nhật DeviceInfo Id={DeviceId} cho UserId={UserId}", dto.DeviceId, userId);

            var existing = await _repository.GetByIdAsync(dto.DeviceId, cancellationToken);
            if (existing == null)
                throw new DeviceInfoNotFoundException(dto.DeviceId.ToString());

            // Xác minh quyền sở hữu
            var child = await _childRepository.GetByIdAsync(dto.ChildId, cancellationToken);
            if (child == null)
                throw new ChildNotFoundException(dto.ChildId.ToString());
            if (child.UserId != userId)
                throw new UnauthorizedAccessException("Không có quyền chỉnh sửa thiết bị của trẻ này.");

            var updatedEntity = _factory.Update(dto);
            _domainService.EnsureCanUpdate(updatedEntity);

            var updated = await _repository.UpdateAsync(updatedEntity, cancellationToken);
            _logger.LogInformation("Orchestrator: DeviceInfo Id={DeviceId} cập nhật thành công.", dto.DeviceId);

            return updated;
        }

        // 🔴 Xóa thiết bị
        public async Task DeleteAsync(DeviceInfoDeleteDTO dto, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Orchestrator: Bắt đầu xóa DeviceInfo Id={DeviceId} cho UserId={UserId}", dto.DeviceId, userId);

            var entity = await _repository.GetByIdAsync(dto.DeviceId, cancellationToken);
            if (entity == null)
                throw new DeviceInfoNotFoundException(dto.DeviceId.ToString());

            // Xác minh quyền phụ huynh
            var child = await _childRepository.GetByIdAsync(entity.ChildId, cancellationToken);
            if (child == null)
                throw new ChildNotFoundException(entity.ChildId.ToString());
            if (child.UserId != userId)
                throw new UnauthorizedAccessException("Không có quyền xóa thiết bị của trẻ này.");

            _domainService.EnsureCanDelete(entity);

            await _repository.DeleteAsync(entity, cancellationToken);
            _logger.LogInformation("Orchestrator: DeviceInfo Id={DeviceId} đã được xóa thành công.", dto.DeviceId);
        }
    }
}

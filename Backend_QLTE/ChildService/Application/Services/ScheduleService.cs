using Backend_QLTE.ChildService.Application.DTOs.Client.Schedule;
using Backend_QLTE.ChildService.Application.DTOs.Common;
using Backend_QLTE.ChildService.Application.Exceptions.Duplicates;
using Backend_QLTE.ChildService.Application.Exceptions.Failed;
using Backend_QLTE.ChildService.Application.Exceptions.NotFounds;
using Backend_QLTE.ChildService.Application.Interfaces.Mappers;
using Backend_QLTE.ChildService.Application.Interfaces.Orchestrators;
using Backend_QLTE.ChildService.Application.Interfaces.Repositories;
using Backend_QLTE.ChildService.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Backend_QLTE.ChildService.Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IScheduleOrchestrator _orchestrator;
        private readonly IScheduleResponseMapper _mapper;
        private readonly IPaginationMapper _paginationMapper;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(
            IUnitOfWork unitOfWork,
            IScheduleOrchestrator orchestrator,
            IScheduleResponseMapper mapper,
            IPaginationMapper paginationMapper,
            ILogger<ScheduleService> logger)
        {
            _unitOfWork = unitOfWork;
            _orchestrator = orchestrator;
            _mapper = mapper;
            _paginationMapper = paginationMapper;
            _logger = logger;
        }

        // 🟢 Lấy danh sách có phân trang
        public async Task<ResultListDTO<ScheduleResponseDTO>> GetAllAsync(int page, int limit, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetAllAsync Schedule với page={Page}, limit={Limit}", page, limit);

            var (entities, total, last) = await _unitOfWork.Schedules.GetAllAsync(page, limit, cancellationToken);
            if (total == 0)
            {
                _logger.LogWarning("Service: Không tìm thấy Schedule nào");
                throw new ScheduleNotFoundException();
            }

            var dtoList = _mapper.ToDtoList(entities);
            var pagination = _paginationMapper.ToDto(page, limit, total, last);

            return ResultListDTO<ScheduleResponseDTO>.Success(dtoList, "Lấy danh sách Schedule thành công", pagination);
        }

        // 🔵 Lấy chi tiết
        public async Task<ResultDTO<ScheduleResponseDTO>> GetDetailAsync(Guid scheduleId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetDetailAsync Schedule Id={Id}", scheduleId);

            var entity = await _unitOfWork.Schedules.GetByIdAsync(scheduleId, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Service: Không tìm thấy Schedule Id={Id}", scheduleId);
                throw new ScheduleNotFoundException(scheduleId);
            }

            var dto = _mapper.ToDto(entity);
            return ResultDTO<ScheduleResponseDTO>.Success(dto, "Lấy chi tiết Schedule thành công");
        }

        // 🟣 Lấy danh sách lịch học theo trẻ
        public async Task<ResultListDTO<ScheduleResponseDTO>> GetAllByChildAsync(Guid childId, int page, int limit, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetAllByChildAsync Schedule với ChildId={ChildId}", childId);

            var (entities, total, last) = await _unitOfWork.Schedules.GetByChildPagedAsync(childId, page, limit, cancellationToken);
            if (total == 0)
            {
                _logger.LogWarning("Service: Không tìm thấy Schedule cho ChildId={ChildId}", childId);
                throw new ScheduleNotFoundException();
            }

            var dtoList = _mapper.ToDtoList(entities);
            var pagination = _paginationMapper.ToDto(page, limit, total, last);

            return ResultListDTO<ScheduleResponseDTO>.Success(dtoList, "Lấy danh sách Schedule theo Child thành công", pagination);
        }

        // 🟡 Tạo mới
        public async Task<ResultDTO> CreateAsync(ScheduleCreateDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Bắt đầu CreateAsync Schedule cho ChildId={ChildId}", dto.ChildId);

            // Kiểm tra trùng lịch
            var existing = await _unitOfWork.Schedules.GetByChildIdAsync(dto.ChildId, cancellationToken);
            bool isOverlap = existing.Any(s =>
                s.Thu == dto.Thu &&
                (dto.GioBatDau < s.GioKetThuc && dto.GioKetThuc > s.GioBatDau));

            if (isOverlap)
                throw new DuplicateScheduleException(dto.TenMonHoc, dto.Thu, dto.GioBatDau, dto.GioKetThuc);

            await _orchestrator.CreateAsync(dto, cancellationToken);

            _logger.LogInformation("Service: Tạo Schedule cho ChildId={ChildId} thành công", dto.ChildId);
            return ResultDTO.Success("Tạo Schedule thành công");
        }

        // 🟠 Cập nhật
        public async Task<ResultDTO<ScheduleResponseDTO>> UpdateAsync(ScheduleUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Bắt đầu UpdateAsync Schedule Id={Id}", dto.ScheduleId);

            var updatedEntity = await _orchestrator.UpdateAsync(dto, cancellationToken);
            var response = _mapper.ToDto(updatedEntity);

            return ResultDTO<ScheduleResponseDTO>.Success(response, "Cập nhật Schedule thành công");
        }

        // 🔴 Xóa
        public async Task<ResultDTO> DeleteAsync(ScheduleDeleteDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Bắt đầu DeleteAsync Schedule Id={Id}", dto.ScheduleId);

            await _orchestrator.DeleteAsync(dto, cancellationToken);

            _logger.LogInformation("Service: Xóa Schedule Id={Id} thành công", dto.ScheduleId);
            return ResultDTO.Success("Xóa Schedule thành công");
        }
    }
}

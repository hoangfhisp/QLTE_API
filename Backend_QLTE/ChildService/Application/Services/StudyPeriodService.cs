using Backend_QLTE.ChildService.Application.DTOs.Client.StudyPeriod;
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
    public class StudyPeriodService : IStudyPeriodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudyPeriodOrchestrator _orchestrator;
        private readonly IStudyPeriodResponseMapper _mapper;
        private readonly IPaginationMapper _paginationMapper;
        private readonly ILogger<StudyPeriodService> _logger;

        public StudyPeriodService(
            IUnitOfWork unitOfWork,
            IStudyPeriodOrchestrator orchestrator,
            IStudyPeriodResponseMapper mapper,
            IPaginationMapper paginationMapper,
            ILogger<StudyPeriodService> logger)
        {
            _unitOfWork = unitOfWork;
            _orchestrator = orchestrator;
            _mapper = mapper;
            _paginationMapper = paginationMapper;
            _logger = logger;
        }

        // 🟢 Lấy danh sách có phân trang
        public async Task<ResultListDTO<StudyPeriodResponseDTO>> GetAllAsync(int page, int limit, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetAllAsync với trang {Page}, giới hạn {Limit}", page, limit);

            var (entities, total, last) = await _unitOfWork.StudyPeriods.GetAllAsync(page, limit, cancellationToken);
            if (total == 0)
            {
                _logger.LogWarning("Service: Không tìm thấy StudyPeriod nào");
                throw new StudyPeriodNotFoundException();
            }

            var dtoList = _mapper.ToDtoList(entities);
            var pagination = _paginationMapper.ToDto(page, limit, total, last);

            return ResultListDTO<StudyPeriodResponseDTO>.Success(dtoList, "Lấy danh sách StudyPeriod thành công", pagination);
        }

        // 🔵 Lấy chi tiết theo Id
        public async Task<ResultDTO<StudyPeriodResponseDTO>> GetDetailAsync(Guid studyPeriodId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetDetailAsync với Id={Id}", studyPeriodId);

            var entity = await _unitOfWork.StudyPeriods.GetByIdAsync(studyPeriodId, cancellationToken);
            if (entity is null)
            {
                _logger.LogWarning("Service: Không tìm thấy StudyPeriod Id={Id}", studyPeriodId);
                throw new StudyPeriodNotFoundException(studyPeriodId);
            }

            var dto = _mapper.ToDto(entity);
            return ResultDTO<StudyPeriodResponseDTO>.Success(dto, "Lấy chi tiết StudyPeriod thành công");
        }

        public async Task<ResultListDTO<StudyPeriodResponseDTO>> GetAllStudyPeriodsByChildAsync(Guid childId, int page, int limit, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetAllStudyPeriodsByChildAsync với ChildId={ChildId}", childId);

            var (entities, total, last) = await _unitOfWork.StudyPeriods.GetByChildPagedAsync(childId, page, limit, cancellationToken);
            if (total == 0)
            {
                _logger.LogWarning("Service: Không tìm thấy StudyPeriod cho ChildId={ChildId}", childId);
                throw new StudyPeriodNotFoundException();
            }

            var dtoList = _mapper.ToDtoList(entities);
            var pagination = _paginationMapper.ToDto(page, limit, total, last);

            return ResultListDTO<StudyPeriodResponseDTO>.Success(dtoList, "Lấy danh sách khung giờ học theo trẻ thành công", pagination);
        }

        // 🟡 Tạo mới
        public async Task<ResultDTO> CreateAsync(StudyPeriodCreateDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Bắt đầu CreateAsync cho ChildId={ChildId}", dto.ChildId);

            var existingPeriods = await _unitOfWork.StudyPeriods.GetByChildIdAsync(dto.ChildId, cancellationToken);
            if (existingPeriods.Any(p => dto.StartTime < p.EndTime && dto.EndTime > p.StartTime))
                throw new DuplicateStudyPeriodException(dto.StartTime, dto.EndTime);

            await _orchestrator.CreateAsync(dto, cancellationToken);

            _logger.LogInformation("Service: Tạo StudyPeriod cho ChildId={ChildId} thành công", dto.ChildId);
            return ResultDTO.Success("Tạo StudyPeriod thành công");
        }

        // 🟠 Cập nhật
        public async Task<ResultDTO<StudyPeriodResponseDTO>> UpdateAsync(StudyPeriodUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Bắt đầu UpdateAsync cho StudyPeriod Id={Id}", dto.StudyPeriodId);

            var updatedEntity = await _orchestrator.UpdateAsync(dto, cancellationToken);
            var response = _mapper.ToDto(updatedEntity);

            return ResultDTO<StudyPeriodResponseDTO>.Success(response, "Cập nhật StudyPeriod thành công");
        }

        // 🔴 Xóa
        public async Task<ResultDTO> DeleteAsync(StudyPeriodDeleteDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Bắt đầu DeleteAsync cho StudyPeriod Id={Id}", dto.StudyPeriodId);

            await _orchestrator.DeleteAsync(dto, cancellationToken);

            _logger.LogInformation("Service: Xoá StudyPeriod Id={Id} thành công", dto.StudyPeriodId);
            return ResultDTO.Success("Xóa StudyPeriod thành công");
        }

        public async Task<ResultDTO> ToggleActiveAsync(Guid studyPeriodId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: ToggleActiveAsync cho StudyPeriod Id={Id}", studyPeriodId);

            var entity = await _unitOfWork.StudyPeriods.GetByIdAsync(studyPeriodId, cancellationToken);
            if (entity is null)
            {
                _logger.LogWarning("Service: Không tìm thấy StudyPeriod Id={Id}", studyPeriodId);
                throw new StudyPeriodNotFoundException(studyPeriodId);
            }

            entity.IsActive = !entity.IsActive;

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.StudyPeriods.UpdateAsync(entity, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var message = entity.IsActive ? "Đã bật khung giờ học" : "Đã tắt khung giờ học";
            _logger.LogInformation("Service: {Message} Id={Id}", message, studyPeriodId);
            return ResultDTO.Success(message);
        }
    }
}

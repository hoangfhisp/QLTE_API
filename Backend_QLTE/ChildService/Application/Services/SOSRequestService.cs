using Backend_QLTE.ChildService.Application.DTOs.Client.SOSRequest;
using Backend_QLTE.ChildService.Application.DTOs.Common;
using Backend_QLTE.ChildService.Application.Exceptions.Duplicates;
using Backend_QLTE.ChildService.Application.Exceptions.Failed;
using Backend_QLTE.ChildService.Application.Exceptions.NotFounds;
using Backend_QLTE.ChildService.Application.Interfaces.Mappers;
using Backend_QLTE.ChildService.Application.Interfaces.Orchestrators;
using Backend_QLTE.ChildService.Application.Interfaces.Repositories;
using Backend_QLTE.ChildService.Application.Interfaces.Services;
using Backend_QLTE.ChildService.Api.Hubs; 
using Microsoft.AspNetCore.SignalR;       
using Microsoft.Extensions.Logging;

namespace Backend_QLTE.ChildService.Application.Services
{
    public class SOSRequestService : ISOSRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISOSRequestOrchestrator _orchestrator;
        private readonly ISOSRequestResponseMapper _mapper;
        private readonly IPaginationMapper _paginationMapper;
        private readonly ILogger<SOSRequestService> _logger;
        private readonly IHubContext<SOSHub> _hubContext; // ✅ thêm realtime hub

        public SOSRequestService(
            IUnitOfWork unitOfWork,
            ISOSRequestOrchestrator orchestrator,
            ISOSRequestResponseMapper mapper,
            IPaginationMapper paginationMapper,
            ILogger<SOSRequestService> logger,
            IHubContext<SOSHub> hubContext) // ✅ inject hub
        {
            _unitOfWork = unitOfWork;
            _orchestrator = orchestrator;
            _mapper = mapper;
            _paginationMapper = paginationMapper;
            _logger = logger;
            _hubContext = hubContext;
        }

        // 🟢 Lấy tất cả SOSRequests
        public async Task<ResultListDTO<SOSRequestResponseDTO>> GetAllAsync(int page, int limit, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetAllAsync SOSRequest với page={Page}, limit={Limit}", page, limit);

            var (entities, total, last) = await _unitOfWork.SOSRequests.GetAllAsync(page, limit, cancellationToken);
            if (total == 0)
            {
                _logger.LogWarning("Service: Không có SOSRequest nào.");
                throw new SOSRequestNotFoundException();
            }

            var dtoList = _mapper.ToDtoList(entities);
            var pagination = _paginationMapper.ToDto(page, limit, total, last);
            return ResultListDTO<SOSRequestResponseDTO>.Success(dtoList, "Lấy danh sách SOSRequest thành công", pagination);
        }

        // 🔵 Lấy chi tiết theo Id
        public async Task<ResultDTO<SOSRequestResponseDTO>> GetDetailAsync(Guid sosId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetDetailAsync SOS Id={Id}", sosId);

            var entity = await _unitOfWork.SOSRequests.GetByIdAsync(sosId, cancellationToken);
            if (entity is null)
            {
                _logger.LogWarning("Service: Không tìm thấy SOSRequest Id={Id}", sosId);
                throw new SOSRequestNotFoundException(sosId);
            }

            var dto = _mapper.ToDto(entity);
            return ResultDTO<SOSRequestResponseDTO>.Success(dto, "Lấy chi tiết SOSRequest thành công");
        }

        // 🟡 Lấy danh sách theo ChildId
        public async Task<ResultListDTO<SOSRequestResponseDTO>> GetByChildAsync(Guid childId, int page, int limit, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: GetByChildAsync SOS cho ChildId={ChildId}", childId);

            var (entities, total, last) = await _unitOfWork.SOSRequests.GetByChildPagedAsync(childId, page, limit, cancellationToken);
            if (total == 0)
            {
                _logger.LogWarning("Service: Không có SOSRequest nào cho ChildId={ChildId}", childId);
                throw new SOSRequestNotFoundException();
            }

            var dtoList = _mapper.ToDtoList(entities);
            var pagination = _paginationMapper.ToDto(page, limit, total, last);
            return ResultListDTO<SOSRequestResponseDTO>.Success(dtoList, "Lấy danh sách SOS theo trẻ thành công", pagination);
        }

        // 🟢 Tạo mới (Realtime)
        public async Task<ResultDTO> CreateAsync(SOSRequestCreateDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Bắt đầu tạo SOSRequest cho ChildId={ChildId}", dto.ChildId);

            // 1️⃣ Kiểm tra trùng yêu cầu đang xử lý
            var existingRequests = await _unitOfWork.SOSRequests.GetByChildIdAsync(dto.ChildId, cancellationToken);
            if (existingRequests.Any(r => r.TrangThai == "Đang xử lý"))
            {
                _logger.LogWarning("Service: Đã có SOSRequest đang xử lý cho ChildId={ChildId}", dto.ChildId);
                throw new DuplicateSOSRequestException(dto.ChildId, dto.ThoiGian);
            }

            // 2️⃣ Orchestrator xử lý logic nghiệp vụ
            var entity = await _orchestrator.CreateAsync(dto, cancellationToken);

            // 3️⃣ Ghi DB
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var result = await _unitOfWork.SOSRequests.CreateAsync(entity, cancellationToken);

            if (result is null)
            {
                _logger.LogError("Service: Tạo SOSRequest thất bại!");
                await transaction.RollbackAsync(cancellationToken);
                throw new CreateSOSRequestFailedException(dto.ChildId);
            }

            await transaction.CommitAsync(cancellationToken);

            // ✅ Phát tín hiệu realtime tới tất cả client (hoặc sau này nhóm user)
            await _hubContext.Clients.All.SendAsync("ReceiveSOS", new
            {
                SOSId = result.SOSId,
                ChildId = dto.ChildId,
                ViDo = dto.ViDo,
                KinhDo = dto.KinhDo,
                TrangThai = result.TrangThai,
                Message = "🚨 SOS mới được gửi!",
                Time = DateTime.Now
            }, cancellationToken);

            _logger.LogInformation("Service: Tạo SOSRequest thành công và đã gửi realtime tới client");
            return ResultDTO.Success("Tạo yêu cầu SOS thành công");
        }

        // 🟠 Cập nhật (Realtime)
        public async Task<ResultDTO<SOSRequestResponseDTO>> UpdateAsync(SOSRequestUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Cập nhật SOSRequest Id={Id}", dto.SOSId);

            var entity = await _orchestrator.UpdateAsync(dto, cancellationToken);

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var result = await _unitOfWork.SOSRequests.UpdateAsync(entity, cancellationToken);

            if (result is null)
            {
                _logger.LogError("Service: Cập nhật SOSRequest thất bại!");
                await transaction.RollbackAsync(cancellationToken);
                throw new UpdateSOSRequestFailedException(dto.SOSId);
            }

            await transaction.CommitAsync(cancellationToken);

            var response = _mapper.ToDto(result);

            // ✅ Gửi realtime cho client về trạng thái cập nhật
            await _hubContext.Clients.All.SendAsync("SOSUpdated", new
            {
                SOSId = result.SOSId,
                TrangThai = result.TrangThai,
                Message = "✅ Cập nhật trạng thái SOS thành công",
                Time = DateTime.Now
            }, cancellationToken);

            return ResultDTO<SOSRequestResponseDTO>.Success(response, "Cập nhật yêu cầu SOS thành công");
        }

        // 🔴 Xóa (Realtime)
        public async Task<ResultDTO> DeleteAsync(SOSRequestDeleteDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Service: Xóa SOSRequest Id={Id}", dto.SOSId);

            var entity = await _unitOfWork.SOSRequests.GetByIdAsync(dto.SOSId, cancellationToken);
            if (entity is null)
            {
                _logger.LogWarning("Service: Không tìm thấy SOSRequest để xóa.");
                throw new SOSRequestNotFoundException(dto.SOSId);
            }

            await _orchestrator.DeleteAsync(dto, cancellationToken);

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.SOSRequests.DeleteAsync(entity, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // ✅ Thông báo realtime cho client biết SOS đã bị xóa
            await _hubContext.Clients.All.SendAsync("SOSDeleted", new
            {
                SOSId = dto.SOSId,
                Message = "🗑️ SOSRequest đã bị xóa",
                Time = DateTime.Now
            }, cancellationToken);

            _logger.LogInformation("Service: Đã xóa SOSRequest Id={Id} và phát tín hiệu realtime", dto.SOSId);
            return ResultDTO.Success("Xóa yêu cầu SOS thành công");
        }
    }
}

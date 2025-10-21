//using Backend_QLTE.ChildService.Domain.Exceptions.Duplicates;
//using Backend_QLTE.ChildService.Domain.Exceptions.Failed;
//using Backend_QLTE.ChildService.Domain.Exceptions.Invalid;
//using Backend_QLTE.ChildService.Domain.Exceptions.NotFounds;
using Backend_QLTE.ChildService.shared.Exceptions;
using Backend_QLTE.ChildService.Shared.Exceptions;
using System.Net;
//using Backend_QLTE.ChildService.Domain.Exceptions.Invalids;
//using Backend_QLTE.ChildService.Domain.Exceptions;
//using Backend_QLTE.ChildService.Domain.Exceptions.Unauthorized;

namespace Backend_QLTE.ChildService.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        private static readonly Dictionary<Type, int> ExceptionStatusMap = new()
        {
            //// Pet
            //{ typeof(DuplicatePetException), StatusCodes.Status500InternalServerError },
            //{ typeof(PetFailedException), StatusCodes.Status500InternalServerError },
            //{ typeof(InvalidPetException), StatusCodes.Status500InternalServerError },
            //{ typeof(PetNotFoundException), StatusCodes.Status500InternalServerError },

            //// AdoptionRequest
            //{ typeof(DuplicateAdoptionRequestException), StatusCodes.Status500InternalServerError },
            //{ typeof(AdoptionRequestFailedException), StatusCodes.Status500InternalServerError },
            //{ typeof(InvalidAdoptionRequestException), StatusCodes.Status500InternalServerError },
            //{ typeof(AdoptionRequestNotFoundException), StatusCodes.Status500InternalServerError },

            //// VaccinationSchedule
            //{ typeof(DuplicateVaccinationScheduleException), StatusCodes.Status500InternalServerError },
            //{ typeof(VaccinationScheduleFailedException), StatusCodes.Status500InternalServerError },
            //{ typeof(InvalidVaccinationScheduleException), StatusCodes.Status500InternalServerError },
            //{ typeof(VaccinationScheduleNotFoundException), StatusCodes.Status500InternalServerError },

            //// Chat
            //{ typeof(UnauthorizedChatAccessException), StatusCodes.Status500InternalServerError },
            //{ typeof(InvalidChatMessageException), StatusCodes.Status500InternalServerError },
            //{ typeof(ChatMessageNotFoundException), StatusCodes.Status500InternalServerError },

        };

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api/Child") 
                && !context.Request.Path.StartsWithSegments("/api/SafeZone")

                )
            {
                await _next(context);
                return;
            }
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                var result = new
                {
                    Status = false,
                    Msg = ex.Message
                };

                await context.Response.WriteAsJsonAsync(result);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error occurred");

                int statusCode = ExceptionStatusMap.TryGetValue(ex.GetType(), out var code)
                    ? code
                    : StatusCodes.Status400BadRequest;

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(new
                {
                    Status = false,
                    Msg = ex.Message,
                });
            }
            catch (BusinessException ex) // lỗi nghiệp vụ
            {
                _logger.LogWarning(ex, "Business error occurred.");

                context.Response.StatusCode = ex.StatusCode;
                await context.Response.WriteAsJsonAsync(new
                {
                    Status = false,
                    Msg = ex.Message
                });
            }
            catch (Exception ex) // lỗi khác
            {
                _logger.LogError(ex, "Unexpected error occurred.");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    Status = false,
                    Msg = $"Đã xảy ra lỗi hệ thống, vui lòng thử lại sau."
                });
            }
        }
    }
}

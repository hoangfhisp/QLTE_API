using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Backend_QLTE.ChildService.Infrastructure.Data;
using Backend_QLTE.ChildService.Application.Interfaces.Services;
using Backend_QLTE.ChildService.Infrastructure.Data.HttpClients;
using Backend_QLTE.ChildService.Infrastructure.Data.HttpClients.Interfaces;
using Backend_QLTE.ChildService.Infrastructure.Repositories;
using Backend_QLTE.ChildService.Application.Interfaces.Repositories;
using Backend_QLTE.ChildService.Application.Interfaces.Orchestrators;
using Backend_QLTE.ChildService.Application.Orchestrators;
using Backend_QLTE.ChildService.Domain.Services.Interfaces;
using Backend_QLTE.ChildService.Domain.Services;
using Backend_QLTE.ChildService.Application.Services;
using Backend_QLTE.ChildService.Application.Interfaces.Mappers;
using Backend_QLTE.ChildService.Application.Mappers;
using Backend_QLTE.ChildService.Application.Factories;
using Backend_QLTE.ChildService.Application.Interfaces.Factories;
using Backend_QLTE.ChildService.Api.Middlewares;

namespace Backend_QLTE.ChildService
{
    public static class ChildServiceConfig
    {
        // Đăng ký DI cho toàn bộ tầng Application + Domain + Infrastructure
        public static IServiceCollection AddChildService(this IServiceCollection services, IConfiguration config)
        {
            /*-----------------------------------------APPLICATION--------------------------------------------*/
            // SERVICES
            services.AddScoped<IStudyPeriodService, StudyPeriodService>();

            // FACTORIES
            services.AddScoped<IStudyPeriodFactory, StudyPeriodFactory>();

            // MAPPERS
            services.AddScoped<IStudyPeriodResponseMapper, StudyPeriodResponseMapper>();

            // ORCHESTRATORS
            services.AddScoped<IStudyPeriodOrchestrator, StudyPeriodOrchestrator>();

            // VALIRATORS
            // Dịch vụ validation chung

            // Đăng ký tất cả validators tự động
            //services.Scan(scan => scan
            //    .FromAssemblies(typeof(RegisterUserValidator).Assembly) // Quét tất cả các lớp trong assembly chứa RegisterUserValidator 
            //    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))// Đăng ký tất cả các lớp triển khai IValidator<>
            //    .AsImplementedInterfaces()// Đăng ký chúng theo giao diện mà chúng triển khai
            //    .WithScopedLifetime()); // Sử dụng phạm vi Scoped cho các dịch vụ này

            /*-----------------------------------------DOMAIN--------------------------------------------*/
            // DOMAIN SERVICES
            services.AddScoped<IStudyPeriodDomainService, StudyPeriodDomainService>();

            //MAPPERS
            services.AddScoped<IPaginationMapper, PaginationMapper>();

            /*-----------------------------------------INFRASTRUCTURE--------------------------------------------*/
            //CONFIG

            // HTTP Client


            // Configure database contexts
            services.AddDbContext<ChildDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("ChildDb")));

            // REPOSITORIES
            services.AddScoped<IStudyPeriodRepository, StudyPeriodRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>(); // Unit of Work
            return services;
        }

        // Middleware pipeline riêng cho ChildService
        public static IApplicationBuilder UseChildService(this IApplicationBuilder app)
        {
            // Nếu sau này bạn có middleware riêng (exception handler, logger, v.v.)
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Backend_QLTE.ChildService.Infrastructure.Data;

namespace Backend_QLTE.ChildService
{
    public static class ChildServiceConfig
    {
        // Đăng ký DI cho toàn bộ tầng Application + Domain + Infrastructure
        public static IServiceCollection AddChildService(this IServiceCollection services, IConfiguration config)
        {
            /* -----------------------------------------
             * INFRASTRUCTURE LAYER
             * ----------------------------------------- */
            services.AddDbContext<ChildDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("ChildDb")));


            // HTTP Client gọi sang UserService (tái sử dụng pattern của PetService)
            // services.AddHttpClient<IUserServiceClient, UserServiceClient>("UserServiceClient", (sp, client) =>
            // {
            //     var cfg = sp.GetRequiredService<IConfiguration>();
            //     var baseUrl = cfg["ServiceUrls:UserService"];
            //     client.BaseAddress = new Uri(baseUrl);
            // });

            return services;
        }

        // Middleware pipeline riêng cho ChildService
        public static IApplicationBuilder UseChildService(this IApplicationBuilder app)
        {
            // Nếu sau này bạn có middleware riêng (exception handler, logger, v.v.)
            // app.UseMiddleware<ChildExceptionMiddleware>();
            return app;
        }
    }
}

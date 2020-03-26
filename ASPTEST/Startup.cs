using ASPTEST.Data;
using ASPTEST.Services;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ASPTEST
{
    public class Startup
    {
        // 构造函数注入配置信息，就可以使用appsettings.json在内的所有配置信息了
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        // 向服务容器注册所有的服务，已经注册的服务就可以通过依赖注入的方式在其他地方使用
        public void ConfigureServices(IServiceCollection services)
        {
            // 注册缓存服务，然后就可以使用缓存中间件了
            services.AddResponseCaching();

            // asp.net 3.0之后使用AddControllers来构建api的服务
            // 在2.x时代以及之前，使用AddMvc来构建，它不仅包含了各种Controllers，还提供了构建视图（View）、Tag之类的其他服务，在做web api项目时用不到
            services.AddControllers(setup => {
                // 这里可以配置一些返回项的约束配置，默认为false，如果启用，则表示不支持的类型是会返回406的
                setup.ReturnHttpNotAcceptable = true;
            }).AddXmlDataContractSerializerFormatters()
            .ConfigureApiBehaviorOptions(setup => {
                // 这里配置API的行为选项，下面配置了模型验证错误后返回的内容
                setup.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Type = "http://www.ffm.com",
                        Title = "验证错误",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Detail = "查看详细信息",
                        Instance = context.HttpContext.Request.Path
                    };

                    problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

                    return new UnprocessableEntityObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            // 注册AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // 注册数据库服务，数据库服务应该在每次HTTP请求周期创建一个实例，所以使用scoped
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            // 注册DbContext服务
            services.AddDbContext<RoutineDbContext>(option =>
            {
                option.UseSqlite("Data Source=routine.db");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // 配置HTTP响应的一个管道，每一句话都相当于一个中间件，HTTP的请求依次按顺序接受这些中间件的操作，然后返回
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // 开发模式，返回开发模式异常页面
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // 这里配置生产环境下的异常处理，并将需要返回给客户端的信息返回
                app.UseExceptionHandler(appBuilder =>
                {
                    // 这里面的内容，就是程序中出现未处理的异常，会在这里处理。
                    // 通常这里需要进行日志记录
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected Error!");
                    });
                });
            }

            app.UseStaticFiles();
            
            app.UseRouting();

            // 使用缓存中间件，先注册（在ConfigureServices中）
            // 需要注意的是，如果客户端发来的请求头包含Authorization属性，ResponseCaching不会起作用，这是官方文档说明的。可以使用其他缓存方式。
            app.UseResponseCaching();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // 依次注册每一个uri的端点
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});

                // 直接返回所有uri对应的控制器
                endpoints.MapControllers();
            });
        }
    }
}

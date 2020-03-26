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
        // ���캯��ע��������Ϣ���Ϳ���ʹ��appsettings.json���ڵ�����������Ϣ��
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        // ���������ע�����еķ����Ѿ�ע��ķ���Ϳ���ͨ������ע��ķ�ʽ�������ط�ʹ��
        public void ConfigureServices(IServiceCollection services)
        {
            // ע�Ỻ�����Ȼ��Ϳ���ʹ�û����м����
            services.AddResponseCaching();

            // asp.net 3.0֮��ʹ��AddControllers������api�ķ���
            // ��2.xʱ���Լ�֮ǰ��ʹ��AddMvc�������������������˸���Controllers�����ṩ�˹�����ͼ��View����Tag֮���������������web api��Ŀʱ�ò���
            services.AddControllers(setup => {
                // �����������һЩ�������Լ�����ã�Ĭ��Ϊfalse��������ã����ʾ��֧�ֵ������ǻ᷵��406��
                setup.ReturnHttpNotAcceptable = true;
            }).AddXmlDataContractSerializerFormatters()
            .ConfigureApiBehaviorOptions(setup => {
                // ��������API����Ϊѡ�����������ģ����֤����󷵻ص�����
                setup.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Type = "http://www.ffm.com",
                        Title = "��֤����",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Detail = "�鿴��ϸ��Ϣ",
                        Instance = context.HttpContext.Request.Path
                    };

                    problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

                    return new UnprocessableEntityObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            // ע��AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // ע�����ݿ�������ݿ����Ӧ����ÿ��HTTP�������ڴ���һ��ʵ��������ʹ��scoped
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            // ע��DbContext����
            services.AddDbContext<RoutineDbContext>(option =>
            {
                option.UseSqlite("Data Source=routine.db");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ����HTTP��Ӧ��һ���ܵ���ÿһ�仰���൱��һ���м����HTTP���������ΰ�˳�������Щ�м���Ĳ�����Ȼ�󷵻�
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ����ģʽ�����ؿ���ģʽ�쳣ҳ��
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // �����������������µ��쳣����������Ҫ���ظ��ͻ��˵���Ϣ����
                app.UseExceptionHandler(appBuilder =>
                {
                    // ����������ݣ����ǳ����г���δ������쳣���������ﴦ��
                    // ͨ��������Ҫ������־��¼
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected Error!");
                    });
                });
            }

            app.UseStaticFiles();
            
            app.UseRouting();

            // ʹ�û����м������ע�ᣨ��ConfigureServices�У�
            // ��Ҫע����ǣ�����ͻ��˷���������ͷ����Authorization���ԣ�ResponseCaching���������ã����ǹٷ��ĵ�˵���ġ�����ʹ���������淽ʽ��
            app.UseResponseCaching();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // ����ע��ÿһ��uri�Ķ˵�
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});

                // ֱ�ӷ�������uri��Ӧ�Ŀ�����
                endpoints.MapControllers();
            });
        }
    }
}

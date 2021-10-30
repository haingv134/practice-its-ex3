﻿using AutoMapper;
using DatabaseLayer.Context;
using DatabaseLayer.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServicesLayer.Implementation;
using ServicesLayer.Interface;
using ServicesLayer.Implementation.Datatable;
using ServicesLayer.Interface.Datatable;
using WebLayer.AutoMapper;
using DatabaseLayer.Entity.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using WebLayer.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebLayer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                // allow null desination
                mc.AllowNullCollections = true;
                mc.AddProfile(new CommonAutoMapper());
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            //.......................................................
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddDbContext<DatabaseContext>();
            services.AddScoped<UnitOfWork>();
            services.AddScoped<IClassServices, ClassServices>();
            services.AddScoped<ISubjectServices, SubjectServices>();
            services.AddScoped<IStudentServices, StudentServices>();
            services.AddScoped<IClassDtServices, ClassDtServices>();
            services.AddScoped<IStudentDtServices, StudentDtServices>();
            // Đăng ký dịch vụ gửi mail cho identity
            services.AddScoped<IEmailSender, SendMailServices>();
            // dịch vụ update hình ảnh cho user identity
            //.......................................................
            // dang ky dich vu cho Identity
            services.AddIdentity<AppUser, IdentityRole>()
                        .AddEntityFrameworkStores<DatabaseContext>()
                        .AddDefaultTokenProviders();               
            //services.AddDefaultIdentity<AppUser>().AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();
            //......................................................
            services.AddOptions();
            // đăng ký option cho mailservices, identity options  vào trong hệ thống
            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));
            // Truy cập IdentityOptions
            services.Configure<IdentityOptions>(options =>
            {
                // Thiết lập về Password
                options.Password.RequireDigit = false; // Không bắt phải có số
                options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
                options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
                options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
                options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  // Email là duy nhất

                // Cấu hình đăng nhập.
                options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
                options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
                options.SignIn.RequireConfirmedAccount = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication(); // use for login/logout

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                // endpoints.MapMethods("/IdentityUserAvatar", new string[] { "POST" }, async context =>
                // {
                //     var provider = app.ApplicationServices;

                //     var userManager = (UserManager<AppUser>) provider.GetService(typeof(UserManager<AppUser>));
                //     var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
                //     var logger = app.ApplicationServices.GetRequiredService<ILogger<UpdateAvatar>>();
                //     var update = new UpdateAvatar(userManager, httpContextAccessor, logger);
                //     await update.FormHander(context);
                //     string json = JsonConvert.SerializeObject(new {success = true, message = "Successfull"});
                //     await context.Response.WriteAsync(json);                
                // });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
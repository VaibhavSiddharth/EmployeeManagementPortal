using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeRegistrationApp.Models;
using EmployeeRegistrationApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeRegistrationApp
{
    public class Startup
    {
        private IConfiguration _config;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(_config.GetConnectionString("EmployeeDbConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 3;
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
            }).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation");
            

            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(5));

            services.Configure<CustomEmailConfirmationTokenProviderOptions>(
                                options => options.TokenLifespan = TimeSpan.FromDays(3));

            services.AddMvc(option => option.EnableEndpointRouting = false).AddXmlSerializerFormatters();

            services.ConfigureApplicationCookie(options =>
                                                          {
                                                              options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
                                                          }
                                                );

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role", "true"));
                //options.AddPolicy("EditRolePolicy", policy => policy.RequireRole("Admin")
                //                                                    .RequireClaim("Edit Role", "true")
                //                                                    .RequireRole("SuperAdmin"));
                //options.AddPolicy("EditRolePolicy", policy =>policy.RequireAssertion( context =>
                //                                                                      context.User.IsInRole("Admin") &&
                //                                                                      context.User.HasClaim( claim => claim.Type =="Edit Role" && claim.Value == "true") ||
                //                                                                      context.User.IsInRole("SuperAdmin")

                //                                                                    )
                options.AddPolicy("EditRolePolicy",
                                   policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement())
                                 );
                //options.InvokeHandlersAfterFailure = false;
                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));

            });

            services.AddAuthentication().AddGoogle(options =>
           {
               options.ClientId = "569684804447-to7bi3npdlm2titqcahmltcjc5jlcgpq.apps.googleusercontent.com";
               options.ClientSecret = "G6_JpyXdHLH4U5ZSaYHHPnMF";
           }).AddFacebook(options =>
           {
               options.AppId = "1143058896087007";
               options.AppSecret = "9b8a8ca5ed019be463821b6e04e2b95e";
           }
          ); 
           
            //services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                //DeveloperExceptionPageOptions developerExceptionPageOptions = new DeveloperExceptionPageOptions();
                //developerExceptionPageOptions.SourceCodeLineCount = 1;
                //app.UseDeveloperExceptionPage(developerExceptionPageOptions);
                app.UseDeveloperExceptionPage();

            }

            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseStatusCodePages();
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            //app.UseRouting();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World");
            //    });
            //});

            // app.Use(async (context, next) =>
            //{
            //    logger.LogInformation("MW1:Request received at Middleware 1");
            //    //await context.Response.WriteAsync("Hello from 1st Middleware \n");
            //    await next();
            //    logger.LogInformation("MW1:Response produced at Middleware 1");

            //});

            // app.Use(async (context, next) =>
            // {
            //     logger.LogInformation("MW2:Request received at Middleware 2");
            //     //await context.Response.WriteAsync("Hello from 1st Middleware \n");
            //     await next();
            //     logger.LogInformation("MW2:Response produced at Middleware 2");

            // });

            //DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            //defaultFilesOptions.DefaultFileNames.Clear();
            //defaultFilesOptions.DefaultFileNames.Add("foo.html");
            //app.UseDefaultFiles(defaultFilesOptions);
            //app.UseStaticFiles();

            //FileServerOptions fileServerOptions = new FileServerOptions();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("foo.html");
            //app.UseFileServer(fileServerOptions);

            //app.UseFileServer();
            app.UseStaticFiles();
            //app.UseMvcWithDefaultRoute();
            app.UseAuthentication();

            app.UseMvc(routes => routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}"));
            //app.UseMvc();

            //app.Run(async (context) => 
            //{
            //    //throw new Exception("This is custom exception");
            //    //await context.Response.WriteAsync("MW3:Request received at Middleware 3");
            //    //logger.LogInformation("MW3:Request received at Middleware 3");
            //    //await context.Response.WriteAsync("Current Environment is "+env.EnvironmentName);
            //    await context.Response.WriteAsync("Hello World");
            //});


        }
    }
}

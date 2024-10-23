using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IcddWebApp.Services;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using IcddWebApp.Areas.Identity;
using IcddWebApp.Areas.Identity.WebPWrecover.Services;
using IcddWebApp.Data;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IcddWebApp
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

            ///////////// API 
            services.AddControllers(options =>
            {
                options.AllowEmptyInputInBodyModelBinding = true;
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IContainerService, ContainerService>();
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<ILinksetService, LinksetService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IQueryService, QueryService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerGeneratorOptions = new SwaggerGeneratorOptions()
                {
                    Servers = new List<OpenApiServer>()
                    {
                        new OpenApiServer()
                        {
                            Url = "https://icdd.vm.rub.de/dev01/"
                        },
                        new OpenApiServer()
                        {
                            Url = "https://icdd.vm.rub.de/ui/"
                        },
                        new OpenApiServer()
                        {
                            Url = "https://icdd.vm.rub.de/amsfree/"
                        }
                    }
                };
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "OpenICDD API",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Email = "icdd-plattform@ruhr-uni-bochum.de",
                        Name = "icdd-plattform@ruhr-uni-bochum.de",
                        Url = new Uri("https://www.inf.bi.ruhr-uni-bochum.de/")
                    },
                    Description = "OpenICDD API",
                    License = new OpenApiLicense { },
                    TermsOfService = new Uri("https://www.inf.bi.ruhr-uni-bochum.de/")
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new String[] {}
                    }
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                //c.OrderActionsBy();
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                        ClockSkew = TimeSpan.FromSeconds(1)
                    };
                });
            //.AddCookie(options => {
            //    options.LoginPath = "/Account/Unauthorized/";
            //    options.AccessDeniedPath = "/Account/Forbidden/";
            //});

            services.AddCors(options =>
            {
                options.AddPolicy(name: "_allowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins("https://localhost", "http://localhost");
                    });
            });
            // services.AddResponseCaching();
            services.AddMvc();

            services.AddDbContext<DatabaseContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DatabaseContext")));

            services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<DatabaseContext>();

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddOData();

            AddFormatters(services);

            //WebApp
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest).AddMvcOptions(
            op => op.EnableEndpointRouting = false
            );
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddDirectoryBrowser();
            services.AddHttpContextAccessor();
            services.AddSession(options =>
            { options.IdleTimeout = TimeSpan.FromMinutes(30); });
            services.AddDistributedMemoryCache();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.Configure<FormOptions>(f =>
            {
                f.MultipartBodyLengthLimit = Int32.MaxValue;
                f.ValueLengthLimit = Int32.MaxValue;
                f.BufferBodyLengthLimit = Int64.MaxValue;
                f.MultipartBoundaryLengthLimit = Int32.MaxValue;
                f.MultipartHeadersLengthLimit = Int32.MaxValue;
                f.MemoryBufferThreshold = Int32.MaxValue;
                f.ValueCountLimit = Int32.MaxValue;

            });

            // requires
            // using Microsoft.AspNetCore.Identity.UI.Services;
            // using WebPWrecover.Services;
            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            // API
            app.UseSwagger(c => 
                c.RouteTemplate = "/swagger/{documentName}/swagger.json"
            );
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "v1");
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true)
                .AllowCredentials()
            );

            // app.UseResponseCaching();



            // WebApp

            var provider = new FileExtensionContentTypeProvider();
            //provider.Mappings[".icdd"] = "application/x-msdownload";
            provider.Mappings[".icdd"] = "application/zip";
            provider.Mappings[".ttl"] = "text/ttl";
            provider.Mappings[".wexbim"] = "application/octet-stream";
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads"));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "lib"));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "shapefiles"));
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider,
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads")),
                RequestPath = "/downloads"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider,
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "lib")),
                RequestPath = "/lib"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider,
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "shapefiles")),
                RequestPath = "/shapefiles"
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "shapefiles")),
                RequestPath = "/shapefiles"
            });
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Page}/{action=Index}/{id?}");
                routes.MapRoute(
                  name: "ContainerUpload",
                  template: "Project/{projId}/{controller=Container}/{action=Upload}");
                routes.MapRoute(
                    name: "Container",
                    template: "Project/{projId}/{controller=Container}/{id}/{containerVersion}/{action=Details}");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "api/{apiVersion}/projects/{projectId}/containerTypes/{containerType}/containers/{containerId}"
                );
                endpoints.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                endpoints.EnableDependencyInjection();
                endpoints.MapODataRoute("odata", "odata", GetEdmModel());

            });
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ContainerMetadata>("ContainerMetadatas");
            builder.EntitySet<ContentMetadata>("ContentMetadatas");
            builder.EntitySet<LinksetMetadata>("LinksetMetadatas");
            builder.EntitySet<Project>("Projects");
            builder.EntitySet<VersionApi>("Versions");
            builder.EntitySet<User>("Users");
            return builder.GetEdmModel();
        }

        private void AddFormatters(IServiceCollection services)
        {
            services.AddMvcCore(opt =>
            {
                foreach (var outputFormatter in opt.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in opt.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            });
        }
    }
}

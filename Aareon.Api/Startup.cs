using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aareon.Api.Middleware;
using Aareon.Business.DTO;
using Aareon.Business.Interfaces;
using Aareon.Business.Services;
using Aareon.Data;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;
using Aareon.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Aareon.Api
{
    public class Startup
    {
        private ILogger<Startup> _logger;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogger>(svc => svc.GetRequiredService<ILogger<Startup>>());
            services.AddDbContext<AareonContext>(dbConfig =>
            {
                const string path = "..\\Aareon.Data\\Ticketing.db";
                dbConfig.UseSqlite($"Data Source={path}");
                dbConfig.UseLazyLoadingProxies();
            });
            services.AddSingleton<ILogger>(svc => svc.GetRequiredService<ILogger<Startup>>());
            // services.AddApplicationInsightsTelemetry(); --optional, or Sentry, etc
            services.AddLogging(builder => builder.AddConsole());
            
            services.AddAutoMapper(config =>
            {
                config.AddMaps("Aareon.Api");
                config.AddMaps("Aareon.Business");
            });
            
            // Business
            services.AddScoped<IService<TicketDto>, TicketService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<INoteService, NoteService>();
            
            // Data
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepository<Ticket>, Repository<Ticket>>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            
            var corsHosts = Configuration.GetSection("Cors:AllowedOrigins").Get<List<string>>();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", p =>
                {
                    p.SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins(corsHosts.ToArray())
                        .AllowCredentials();
                });
            });
            
            // This prevents problems arising from cyclic parent child relationships 
            services.AddControllers()
                .AddNewtonsoftJson(
                    options => options.SerializerSettings.ReferenceLoopHandling = 
                        Newtonsoft.Json.ReferenceLoopHandling.Serialize);
            
            var authConfig = Configuration.GetSection("AuthSettings");
            var authSecret = Encoding.ASCII.GetBytes(authConfig.GetValue<string>("Secret"));
            services.Configure<AuthSettings>(authConfig);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var userSvc = context.HttpContext.RequestServices.GetRequiredService<IPersonService>();
                            var userId = context.Principal?.GetUserId();
                            if (userId != null)
                            {
                                var user = userSvc.GetById((int)userId).Result;
                                if (user != null)
                                    return Task.CompletedTask;
                            }

                            _logger.LogWarning($"Unauthorised login attempt from {userId}");
                            context.Fail("Unauthorized");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = _ => Task.CompletedTask,
                        OnChallenge = _ => Task.CompletedTask,
                        OnForbidden = context =>
                        {
                            var user = context.HttpContext.User.Identity?.Name;
                            var method = context.Request.Method;
                            var path = context.Request.Path;
                            var msg = $"User: {user} is forbidden from performing request: {method} on {path}";
                            _logger.LogWarning(msg);
                            return Task.CompletedTask;
                            
                        },
                        OnMessageReceived = _ => Task.CompletedTask
                    };
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(authSecret),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AareonTechnicalTest", Version = "v2" });
                
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
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
                        new string[] {}
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, IWebHostEnvironment env, AareonContext ctx, ILogger<Startup> logger)
        {
            _logger = logger;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AareonTechnicalTest v2"));
            }

            app.UseControllerExceptionHandler();
            app.UseAuditing();
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            
            ctx.Database.Migrate();
        }
    }
}
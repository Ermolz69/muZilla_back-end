using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using muZilla.Application.Services;
using muZilla.Infrastructure.Data;
using muZilla.Application.Interfaces;
using muZilla.Infrastructure.Repository;
using Microsoft.Extensions.Options;
using muZilla.Swagger;

namespace muZilla
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerFileUploadOperationFilter>();
            });
            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100MB, ����� ���������
            });

            builder.Services.AddDbContext<MuzillaDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                })
            );



            // Register custom services
            builder.Services.AddScoped<AccessLevelService>();
            builder.Services.AddScoped<ImageService>();
            builder.Services.AddScoped<ChatService>();
            builder.Services.AddScoped<FileStorageService>();
            builder.Services.AddScoped<FriendsCoupleService>();
            builder.Services.AddScoped<BlockedUserService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<SongService>();
            builder.Services.AddScoped<CollectionService>();
            builder.Services.AddScoped<SearchService>();
            builder.Services.AddScoped<BanService>();
            builder.Services.AddScoped<ReportService>();
            builder.Services.AddScoped<TechSupportService>();

            builder.Services.AddScoped<IGenericRepository, GenericRepository>();


            builder.Services.AddHostedService<BanCleanupService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var keyBytes = Encoding.UTF8.GetBytes(key);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };
            });

            builder.Services.AddAuthorization();

            // CORS policy configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200") // URL ������ Angular ����������
                          .AllowAnyHeader() // ��������� ����� ���������
                          .AllowAnyMethod() // ��������� ����� ������ (GET, POST � �.�.)
                          .AllowCredentials(); // ��������� �������� ����� ��� �������
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ������������� CORS
            app.UseCors("AllowAngular");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

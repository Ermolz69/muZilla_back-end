
using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.Services;
using System.Globalization;

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

            builder.Services.AddDbContext<MuzillaDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<AccessLevelService>();
            builder.Services.AddScoped<ImageService>();
            builder.Services.AddScoped<FileStorageService>();
            builder.Services.AddScoped<FriendsCoupleService>();
            builder.Services.AddScoped<BlockedUserService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<SongService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

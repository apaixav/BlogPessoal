
using blogpessoal.Configuration;
using blogpessoal.Data;
using blogpessoal.Model;
using blogpessoal.Security;
using blogpessoal.Security.Implements;
using blogpessoal.Service;
using blogpessoal.Service.Implements;
using blogpessoal.Validator;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace blogpessoal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            if (builder.Configuration["Enviroment:Start"] == "PROD")
            {
                builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("secrets.json");

                var connectionString = builder.Configuration
                .GetConnectionString("ProdConnection");

                builder.Services.AddDbContext<AppDbContext>(options =>
                     options.UseNpgsql(connectionString));

            }else
            {
                var connectionString = builder.Configuration
                    .GetConnectionString("DefaultConnection");

                builder.Services.AddDbContext<AppDbContext>(options =>
                     options.UseSqlServer(connectionString)
                );

            }

            builder.Services.AddTransient<IValidator<Postagem>, PostagemValidator>();
            builder.Services.AddTransient<IValidator<Tema>, TemaValidator>();
            builder.Services.AddTransient<IValidator<User>,UserValidator>();

            builder.Services.AddScoped<IPostagemService, PostagemService>();
            builder.Services.AddScoped<ITemaService, TemaService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(Settings.Secret);
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)

                };
            });


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Projeto Blog Pessoal",
                    Description = "Projeto Blog Pessoal ASP.NET Core 7.0",
                    Contact = new OpenApiContact
                    {
                        Name = "Lucas Ribeiro Paixão",
                        Email = "lucasrib01@gmail.com",
                        Url = new Uri("https://github.com/apaixav")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Github",
                        Url = new Uri("https://github.com/apaixav")
                    }
                });
                options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
                {
                    In= ParameterLocation.Header,
                    Description = "Digite um Token Jwt válido",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme= "Bearer"
                });
                options.OperationFilter<AuthResponsesOperationFilter>();
            });

            builder.Services.AddFluentValidationRulesToSwagger();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            
            var app = builder.Build();

            using(var scope = app.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            
            
                app.UseSwagger();

            if (app.Environment.IsProduction())
            {
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("(/swagger/v1/swagger.json/", "Blog Pessoal - v1");
                    options.RoutePrefix = string.Empty;
                });
            }
            

            app.UseCors("MyPolicy");

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
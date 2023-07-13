using Backend.DbContextBD;
using Backend.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json.Serialization;
using Backend.Repositories;
using Npgsql;
using Serilog;
using NpgsqlTypes;
using Serilog.Sinks.PostgreSQL;
using Serilog.Context;
using Serilog.Events;
using Backend.Dtos;
using Backend.Middleware;
using Jint.Native;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Server=db;Port=5432;User " +
                "Id=postgres;Password=topadmin2023;Database=TP-PROVISIONING;";

// Create the Logs table if it doesn't exist
using (var connection = new NpgsqlConnection(connectionString))
{
    connection.Open();

    using (var command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS Logs (Id SERIAL PRIMARY KEY,Date TIMESTAMP NOT NULL, " +
        "Action TEXT NOT NULL ,Message TEXT NOT NULL)", connection))
    {
        command.ExecuteNonQuery();
    }

    string tableName = "Logs";


    IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text)},
    {"action", new RenderedMessageColumnWriter(NpgsqlDbType.Text)},

   // {"action", new CustomColumnWriter(NpgsqlDbType.Text)},
    {"date", new TimestampColumnWriter(NpgsqlDbType.Timestamp)},
};

    string connectionstring = ("Server=db;Port=5432;User " +
                "Id=postgres;Password=topadmin2023;Database=TP-PROVISIONING;");

    Log.Logger = new LoggerConfiguration()
     .WriteTo.File("wwwroot/Log/log.txt")
     .WriteTo.PostgreSQL(
    connectionString: connectionString,
    tableName: tableName,
    columnWriters)
     //.WriteTo.PostgreSQL(connectionString, tableName, columnWriters)
     .CreateLogger();

}

builder.Host.UseSerilog();


/**************************************
       * 
       * Add services to the container.
       * 
       * ***************/
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
/**************************************
       * 
       * accept dateTime
       * 
       * ***************/

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
;
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();
/**************************************
       * 
       * Add JWT Bearer Token
       * 
       * ***************/
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer token\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

/**************************************
 * 
 * CROSS ORIGIN  Add Cors
 * 
 * ***************/

builder.Services.AddCors(options => options.AddPolicy(name: "NgOrigins",
    policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    }));

// Added configuration for PostgreSQL
/*r configuration = builder.Configuration;
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));*/



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NgOrigins");


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

//app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();
app.UseStaticFiles();


app.Run();
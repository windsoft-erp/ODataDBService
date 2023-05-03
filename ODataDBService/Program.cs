using DynamicODataToSQL;
using DynamicODataToSQL.Interfaces;
using ODataDBService.Controllers.Handlers.OData.Interfaces;
using ODataDBService.Controllers.Handlers.OData;
using ODataDBService.Services;
using ODataDBService.Services.Repositories;
using SqlKata.Compilers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using ODataDBService.Controllers.Handlers.OData.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OData-related services
builder.Services.AddScoped<IODataToSqlConverter, ODataToSqlConverter>();
builder.Services.AddScoped<IEdmModelBuilder, EdmModelBuilder>();

// SqlKata compiler
builder.Services.AddSingleton<Compiler>(new SqlServerCompiler { UseLegacyPagination=false });

// ODataV4: Service and repository
builder.Services.AddScoped<IODataV4Service, ODataV4Service>();
builder.Services.AddScoped<IODataV4Repository, ODataV4Repository>();

// ODataV4: Request handlers and factory
builder.Services.AddScoped<IODataRequestHandlerFactory, ODataRequestHandlerFactory>();
builder.Services.AddScoped<IQueryRequestHandler, QueryRequestHandler>();
builder.Services.AddScoped<IDeleteRequestHandler, DeleteRequestHandler>();
builder.Services.AddScoped<IInsertRequestHandler, InsertRequestHandler>();
builder.Services.AddScoped<IUpdateRequestHandler, UpdateRequestHandler>();
builder.Services.AddScoped<IInvalidateCacheRequestHandler, InvalidateCacheRequestHandler>();

// Swagger
builder.Services.Configure<SwaggerGenOptions>(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title="OData DB Service",
        Version="v1",
        Description="An OData V4/SQL command API for database operations",
        Contact=new OpenApiContact
        {
            Name="Windsoft Developer Team",
            Email="dev@windsoft.ro"
        },
        License=new OpenApiLicense
        {
            Name="Windsoft Software License",
            Url=new Uri("https://your-license-url.com")
        }
    });
    options.OperationFilter<ODataOperationFilter>();
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData DB Service");
    c.RoutePrefix="";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

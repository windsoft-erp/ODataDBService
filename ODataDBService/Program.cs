using DynamicODataToSQL;
using DynamicODataToSQL.Interfaces;
using SqlKata.Compilers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IODataToSqlConverter, ODataToSqlConverter>();
builder.Services.AddScoped<IEdmModelBuilder, EdmModelBuilder>();
builder.Services.AddSingleton<Compiler>(new SqlServerCompiler { UseLegacyPagination = false });

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
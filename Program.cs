using TaskList.Contexts;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TaskContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// AutoMapper Register
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
    
    // Validar todos os mapeamentos
    try
    {
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        Console.WriteLine("✅ Configuração do AutoMapper é válida!");
    }
    catch (AutoMapperConfigurationException ex)
    {
        Console.WriteLine($"❌ Erro na configuração do AutoMapper: {ex.Message}");
        // Log detalhado dos erros
        foreach (var error in ex.Errors)
        {
            Console.WriteLine($"  - {error.TypeMap.SourceType.Name} -> {error.TypeMap.DestinationType.Name}");
            foreach (var unmappedProperty in error.UnmappedPropertyNames)
            {
                Console.WriteLine($"    Propriedade não mapeada: {unmappedProperty}");
            }
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

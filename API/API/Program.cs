using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AcessoTotal",
        policyBuilder => policyBuilder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite("Data Source=app.db"));

var app = builder.Build();

app.MapGet("/", () => "Prova A1");

// ENDPOINTS DE CATEGORIA
// GET: http://localhost:5000/api/categoria/listar
app.MapGet("/api/categoria/listar", async ([FromServices] AppDataContext ctx) =>
{
    var categorias = await ctx.Categorias.ToListAsync();
    return categorias.Any() ? Results.Ok(categorias) : Results.NotFound("Nenhuma categoria encontrada");
});

// POST: http://localhost:5000/api/categoria/cadastrar
app.MapPost("/api/categoria/cadastrar", async ([FromServices] AppDataContext ctx, [FromBody] Categoria categoria) =>
{
    ctx.Categorias.Add(categoria);
    await ctx.SaveChangesAsync();
    return Results.Created($"/api/categoria/{categoria.CategoriaId}", categoria);
});

// ENDPOINTS DE TAREFA
// GET: http://localhost:5000/api/tarefa/listar
app.MapGet("/api/tarefa/listar", async ([FromServices] AppDataContext ctx) =>
{
    var tarefas = await ctx.Tarefas.ToListAsync();
    return tarefas.Any() ? Results.Ok(tarefas) : Results.NotFound("Nenhuma tarefa encontrada");
});

// POST: http://localhost:5000/api/tarefa/cadastrar
app.MapPost("/api/tarefa/cadastrar", async ([FromServices] AppDataContext ctx, [FromBody] Tarefa tarefa) =>
{
    var categoria = await ctx.Categorias.FindAsync(tarefa.CategoriaId);
    if (categoria == null)
    {
        return Results.NotFound("Categoria não encontrada");
    }
    tarefa.Categoria = categoria;
    ctx.Tarefas.Add(tarefa);
    await ctx.SaveChangesAsync();
    return Results.Created($"/api/tarefa/{tarefa.TarefaId}", tarefa);
});

// PATCH: http://localhost:5000/api/tarefa/alterar/{id}
app.MapPatch("/api/tarefa/alterar/{id:int}", async ([FromServices] AppDataContext ctx, int id, [FromBody] Tarefa updatedTarefa) =>
{
    var tarefa = await ctx.Tarefas.FindAsync(id);
    if (tarefa == null)
    {
        return Results.NotFound(new { message = $"Tarefa com ID {id} não encontrada" });
    }

    // Atualizar os campos da tarefa com os dados recebidos
    tarefa.Titulo = updatedTarefa.Titulo;
    tarefa.Descricao = updatedTarefa.Descricao;
    tarefa.Status = updatedTarefa.Status;
    tarefa.CategoriaId = updatedTarefa.CategoriaId;

    ctx.Tarefas.Update(tarefa);
    await ctx.SaveChangesAsync();

    return Results.Ok(tarefa);
});


// GET: http://localhost:5000/api/tarefa/naoconcluidas
app.MapGet("/api/tarefa/naoconcluidas", async ([FromServices] AppDataContext ctx) =>
{
    var tarefasNaoConcluidas = await ctx.Tarefas
        .Where(t => t.Status == "Não iniciada" || t.Status == "Em andamento")
        .ToListAsync();

    return Results.Ok(tarefasNaoConcluidas);
});

// GET: http://localhost:5000/api/tarefa/concluidas
app.MapGet("/api/tarefa/concluidas", async ([FromServices] AppDataContext ctx) =>
{
    var tarefasConcluidas = await ctx.Tarefas
        .Where(t => t.Status == "Concluído")
        .ToListAsync();

    return Results.Ok(tarefasConcluidas);
});

app.UseCors("AcessoTotal");
app.Run();

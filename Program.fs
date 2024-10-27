open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Oxpecker
open Microsoft.Extensions.Configuration
open Npgsql
open Services
open Markdig

open Serilog

Log.Logger <- LoggerConfiguration().WriteTo.Console().CreateLogger()

let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

builder.Services
  .AddSingleton<MarkdownPipeline>(fun _ ->
    MarkdownPipelineBuilder()
      .UseAdvancedExtensions()
      .UsePreciseSourceLocation()
      .Build())
  .AddScoped<MarkdownService>(fun services ->
    Markdown.getMarkdownService(services.GetService<MarkdownPipeline>()))
  .AddSingleton<NpgsqlDataSource>(fun services ->
    Database.getDataSource(services.GetService<IConfiguration>()))
  .AddSingleton<ConnectionFactory>(fun services ->
    Database.getConnectionFactory(services.GetService<NpgsqlDataSource>()))
  .AddScoped<PostsService>(fun services ->
    Posts.getPostService(
      services.GetService<ConnectionFactory>(),
      services.GetService<MarkdownService>()
    ))
  .AddScoped<AuthorsService>(fun services ->
    Authors.getAuthorService(services.GetService<ConnectionFactory>()))
|> ignore<IServiceCollection>

builder.Services.AddAntiforgery().AddRouting().AddOxpecker().AddSerilog()
|> ignore<IServiceCollection>

let app = builder.Build()

app
  .UseStaticFiles()
  .UseAntiforgery()
  .UseRouting()
  .UseOxpecker(
    [
      GET [
        route "/" Controllers.Public.index
        route "/about" Controllers.Public.about
        route "/posts" Controllers.Posts.newPost
        routef
          "/posts/{%s:regex(^.+_(\\d{{4}}-\\d{{2}}-\\d{{2}}-\\d+)$)}"
          Controllers.Public.postDetail
      ]
      CsrfPost [ route "/posts" Controllers.Posts.savePost ]
    ]
  )
|> ignore

app.Run()

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Oxpecker
open Microsoft.Extensions.Configuration
open Npgsql
open Services

let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

builder.Services
  .AddSingleton<NpgsqlDataSource>(fun services ->
    Database.getDataSource(services.GetService<IConfiguration>()))
  .AddSingleton<ConnectionFactory>(fun services ->
    Database.getConnectionFactory(services.GetService<NpgsqlDataSource>()))
  .AddScoped<PostsService>(fun services ->
    Posts.getPostService(services.GetService<ConnectionFactory>()))
  .AddScoped<AuthorsService>(fun services ->
    Authors.getAuthorService(services.GetService<ConnectionFactory>()))
|> ignore<IServiceCollection>

builder.Services.AddAntiforgery().AddRouting().AddOxpecker()
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
        routef "/posts/%s" Controllers.Public.postDetail
      ]
      CsrfPost [ route "/posts" Controllers.Posts.savePost ]
    ]
  )
|> ignore

app.Run()

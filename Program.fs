open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Oxpecker


let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

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
      ]
      CsrfPost [ route "/posts" Controllers.Posts.savePost ]
    ]
  )
|> ignore

app.Run()

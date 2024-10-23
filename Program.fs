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
        route "/" Controllers.Index.index
        route "/about" Controllers.About.index
        route "/posts/new" Controllers.NewPost.newPost
      ]
    ]
  )
|> ignore

app.Run()

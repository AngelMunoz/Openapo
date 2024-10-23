[<AutoOpen>]
module Extensions

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Antiforgery
open Oxpecker
open Hox
open Hox.Rendering

let sendHox(view: Core.Node) : EndpointHandler =
  fun (ctx: HttpContext) -> task {
    ctx.Response.ContentType <- "text/html; charset=utf-8"

    do! ctx.Response.StartAsync(ctx.RequestAborted)
    do! Render.toStream(view, ctx.Response.Body, ctx.RequestAborted)
    do! ctx.Response.CompleteAsync()
  }

let verifyAntiforgery: EndpointMiddleware =
  fun next ctx -> task {
    let antiforgery = ctx.GetService<IAntiforgery>()
    let! isValid = antiforgery.IsRequestValidAsync(ctx)

    if isValid then
      return! next ctx
    else
      return! setStatusCode 403 ctx
  }

type LayoutEnv = { isDevelopment: bool }

type HttpContext with
  member this.CsrfInput() =
    let tokens = this.GetService<IAntiforgery>().GetAndStoreTokens(this)

    h("input")
      .attr("type", "hidden")
      .attr("name", tokens.FormFieldName)
      .attr("value", tokens.RequestToken)


  member this.GetLayoutEnv() =
    let env = this.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>()

    {
      isDevelopment = env.EnvironmentName = "Development"
    }


// Define a middleware to verify the antiforgery token manually
let inline CsrfPost routes =
  applyBefore verifyAntiforgery (POST routes)

let inline CsrfPut routes =
  applyBefore verifyAntiforgery (PUT routes)

let inline CsrfPatch routes =
  applyBefore verifyAntiforgery (PATCH routes)

let inline CsrfDelete routes =
  applyBefore verifyAntiforgery (DELETE routes)

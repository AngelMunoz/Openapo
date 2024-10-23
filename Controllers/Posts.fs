module Controllers.Posts

open Oxpecker

open Views

let newPost: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()
    let csrfInput = ctx.CsrfInput()


    sendHox (Posts.index(csrfInput, env)) ctx


let savePost: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()
    let csrfInput = ctx.CsrfInput()

    (setStatusCode 201 >=> redirectTo "/" false) ctx

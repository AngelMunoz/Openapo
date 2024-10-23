module Controllers.NewPost

open Oxpecker

open Views

let newPost: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()
    let csrfInput = ctx.CsrfInput()


    sendHox (NewPost.view { env = env; csrfInput = csrfInput }) ctx

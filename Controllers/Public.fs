module Controllers.Public

open Oxpecker

open Views

let index: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()

    sendHox (Public.index(env)) ctx

let about: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()

    sendHox (Public.about(env)) ctx

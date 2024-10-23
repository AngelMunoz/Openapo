module Controllers.Index

open Oxpecker

open Views

let index: EndpointHandler =
  fun ctx ->
    let env = ctx.GetWebHostEnvironment()

    sendHox
      (Index.index {
        isDevelopment = env.EnvironmentName = "Development"
      })
      ctx

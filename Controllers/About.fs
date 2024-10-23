module Controllers.About

open Oxpecker

open Views

let index: EndpointHandler =
  fun ctx ->
    let env = ctx.GetWebHostEnvironment()

    sendHox
      (About.index {
        isDevelopment = env.EnvironmentName = "Development"
      })
      ctx

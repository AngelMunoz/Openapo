module Controllers.Public

open Oxpecker

open Views
open Models
open Services

let index: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()
    let posts = ctx.GetService<PostsService>()
    let getSummaries = fun () -> posts.findPostSummaries()

    sendHox (Public.index(getSummaries, env)) ctx

let postDetail(path: string) : EndpointHandler =
  fun ctx -> task {
    let env = ctx.GetLayoutEnv()
    let posts = ctx.GetService<PostsService>()

    let idx = path.LastIndexOf("_")

    let title = path.Substring(0, idx) |> System.Web.HttpUtility.UrlDecode
    let slug = path.Substring(idx + 1)

    let! post = posts.findPostByTitleAndSlug(title, slug)

    return!
      match post with
      | Some(post) ->
        jsonChunked
          {|
            post with
                status = post.status.AsString
          |}
          ctx
      | None -> (setStatusCode 404 >=> text "Not Found") ctx

  }

let about: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()

    sendHox (Public.about(env)) ctx

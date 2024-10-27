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
    let markdown = ctx.GetService<MarkdownService>()

    let idx = path.LastIndexOf("_")

    let title = path.Substring(0, idx) |> PermaPath.fromUrl
    let slug = path.Substring(idx + 1)

    let! post = posts.findPostByTitleAndSlug(title, slug)


    return!
      match post with
      | Some post ->


        let postParams: Layout.PostPageParams = {
          title = post.title
          authorName = post.author.name
          publishedAt =
            post.publishedAt
            |> Option.map(_.ToShortDateString())
            |> Option.defaultValue ""
          summary = post.content[0..55] |> markdown.toText
          content = post.content |> markdown.toHtml
        }

        sendHox (Public.post(postParams, env)) ctx
      | None ->
        let message =
          $"We were unable to find this blog post. Please check the URL and try again. '{title}_{slug}'"

        let responseContent = Layout.Layout.NotFound(title, message, env)
        (setStatusCode 404 >=> sendHox responseContent) ctx
  }

let about: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()

    sendHox (Public.about(env)) ctx

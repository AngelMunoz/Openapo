module Views.Layout

open Hox


type PostPageParams = {
  title: string
  authorName: string
  publishedAt: string
  content: string
  summary: string
}


[<AutoOpen>]
type Layout =

  static member Page
    (
      content: Core.Node,
      ?title: string,
      ?scripts: Core.Node,
      ?styles: Core.Node,
      ?env: LayoutEnv
    ) =
    let scripts = scripts |> Option.defaultValue empty
    let styles = styles |> Option.defaultValue empty

    let title =
      title
      |> Option.map(fun title ->
        if System.String.IsNullOrWhiteSpace title then
          "Openapo"
        else
          title)
      |> Option.defaultValue ""

    let env = env |> Option.defaultValue { isDevelopment = true }

    h(
      "html[lang=en-US]",
      h(
        "head",
        h "meta[charset=utf-8]",
        h "meta[name=viewport][content=width=device-width, initial-scale=1.0]",
        h("title", title),
        h("link[rel=stylesheet]")
          .attr(
            "href",
            if env.isDevelopment then
              "/libs/uikit-3.21.13/css/uikit.css"
            else
              "/libs/uikit-3.21.13/css/uikit.min.css"
          ),
        h("link[rel=stylesheet][href=/styles/app.css]"),
        styles
      ),
      h(
        "body",
        content,
        h("script")
          .attr(
            "src",
            if env.isDevelopment then
              "/libs/uikit-3.21.13/js/uikit.js"
            else
              "/libs/uikit-3.21.13/js/uikit.min.js"
          ),
        h("script")
          .attr(
            "src",
            if env.isDevelopment then
              "/libs/uikit-3.21.13/js/uikit-icons.js"
            else
              "/libs/uikit-3.21.13/js/uikit-icons.min.js"
          ),
        scripts
      )
    )

  static member inline Post
    (
      post: PostPageParams,
      ?sidenav: Core.Node,
      ?extraMeta: Core.Node,
      ?env: LayoutEnv
    ) =

    Page(
      h(
        "main.uk-container",
        h(
          "article.uk-article",
          h("h1.uk-article-title", post.title),
          h(
            "p.uk-article-meta",
            $"Written by {post.authorName} on {post.publishedAt}"
          ),
          fragment(
            [
              match extraMeta with
              | Some extraMeta -> h("section.extra-meta", extraMeta)
              | None -> empty
              match sidenav with
              | Some sidenav -> h("aside.sidenav", sidenav)
              | None -> empty
            ]
          ),
          h(
            "section.uk-card.uk-card-default
                    .uk-card-body.uk-card-hover",
            raw post.content
          )
        )
      ),
      scripts =
        fragment(
          h("script[src=/libs/highlight/js/highlight.min.js]"),
          h("script[src=/js/highlight.js][type=module]")
        ),
      styles = h("link[rel=stylesheet][href=/libs/highlight/css/nord.min.css]"),
      ?env = env
    )

  static member inline NotFound(title, message: string, ?env: LayoutEnv) =
    Page(
      h(
        "main.uk-container",
        h(
          "section.uk-section.uk-section-default",
          h("p.uk-text-large", message)
        )
      ),
      title = title,
      ?env = env
    )

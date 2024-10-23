module Views.Layout

open Hox


[<AutoOpen>]
type Layout =

  static member Page
    (
      content: Core.Node,
      ?scripts: Core.Node,
      ?styles: Core.Node,
      ?env: LayoutEnv
    ) =
    let scripts = scripts |> Option.defaultValue empty
    let styles = styles |> Option.defaultValue empty
    let env = env |> Option.defaultValue { isDevelopment = true }

    h(
      "html[lang=en-US]",
      h(
        "head",
        h "meta[charset=utf-8]",
        h "meta[name=viewport][content=width=device-width, initial-scale=1.0]",
        h("title", text "Hox"),
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

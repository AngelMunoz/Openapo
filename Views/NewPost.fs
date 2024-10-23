module Views.NewPost

open Microsoft.AspNetCore.Antiforgery
open Oxpecker
open Hox
open Layout

type NewPostParams = { env: LayoutEnv; csrfInput: Core.Node }

let view newPostParams =
  Page(
    h(
      "article.uk-padding",
      h(
        "form.uk-form-stacked",
        h(
          "section.uk-margin",
          h("label.uk-form-label", "Blog Title"),
          h(
            "div.uk-form-controls",
            h("input.uk-input[type=text][name=title][placeholder=Title]")
          )
        ),
        h(
          "section.uk-margin",
          h("label.uk-form-label", "Blog Entry Content"),
          h("ope-markdown-input[text=# Markdown][name=content]")
        ),
        newPostParams.csrfInput,
        h(
          "section.uk-margin",
          h(
            "div.uk-form-controls",
            h("button.uk-button.uk-button-secondary[type=submit]", "Save Post")
          )
        )

      )
    ),
    env = newPostParams.env,
    scripts = fragment(h("script[type=module][src=/js/markdown.js]"))
  )

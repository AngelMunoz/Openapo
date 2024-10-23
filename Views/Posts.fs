namespace Views

open Oxpecker
open Hox
open Layout

// if the file gets "too big" we can split it into multiple files and just add
// more extension members to tis type
type Posts = class end


type Posts with

  static member index(csrfInput: Core.Node, ?env: LayoutEnv) =

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
          csrfInput,
          h(
            "section.uk-margin",
            h(
              "div.uk-form-controls",
              h(
                "button.uk-button.uk-button-secondary[type=submit]",
                "Save Post"
              )
            )
          )

        )
      ),
      scripts = fragment(h("script[type=module][src=/js/markdown.js]")),
      ?env = env
    )

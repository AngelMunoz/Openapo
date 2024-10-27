namespace Views

open System.Threading.Tasks
open Models
open Oxpecker
open Hox
open Layout



module Posts =
  module Components =

    let getAuthorOptions(getAuthors: unit -> Task<Author list>) =
      fragment(
        task {
          let! authors = getAuthors()

          return seq {
            h("option[value=null]", text "Select an author")

            for author in authors do
              h($"option[value={author.id}", $"{author.name} - {author.email}")
          }
        }
      )

open Posts

// if the file gets "too big" we can split it into multiple files and just add
// more extension members to tis type
type Posts = class end

type Posts with

  static member index
    (
      getAuthors: unit -> Task<Author list>,
      csrfInput: Core.Node,
      ?env: LayoutEnv
    ) =

    Page(
      h(
        "article.uk-padding",
        h(
          "form.uk-form-stacked[method=post][action=/posts]",
          h(
            "section.uk-margin",
            h("label.uk-form-label", "Blog Title"),
            h(
              "div.uk-form-controls",
              h(
                "input.uk-input[type=text][name=title][placeholder=Title][required]"
              )
            )
          ),
          h(
            "section.uk-margin",
            h(
              "label",
              h("input.uk-checkbox[type=checkbox][name=status]"),
              text "Publish on save"
            )
          ),
          h(
            "section.uk-margin",
            h("label.uk-form-label", "Author"),
            h(
              "div.uk-form-controls",
              h(
                "select.uk-select[name=author]",
                Components.getAuthorOptions(getAuthors)
              )
            )
          ),
          h(
            "section.uk-margin",
            h("label.uk-form-label", "Blog Entry Content"),
            h("ope-markdown-input[name=content][text][required]")
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

namespace Views

open System.Threading.Tasks

open Hox
open Layout
open Models

module Public =

  module Components =

    let postSummaryItems getPosts =
      fragment(
        task {

          let! posts = getPosts()

          return seq {
            for post in posts do
              h(
                "li",
                h(
                  "header",
                  h($"a[href={post.permanentPath}]", h("h3", post.title))
                ),
                h(
                  "section",
                  h(
                    "div.uk-flex",
                    h("div", post.authorName),
                    h("div", post.publishedAt.ToShortDateString())
                  ),
                  h("p", post.summary)
                )
              )
          }
        }
      )

open Public

// if the file gets "too big" we can split it into multiple files and just add
// more extension members to this type
type Public = class end

type Public with

  static member index(postListSummary, ?env: LayoutEnv) =
    Page(
      h(
        "article.uk-padding",
        h(
          "header",
          h("h1", "Somewhat of a blog here's a few things"),
          h(
            "ul.uk-list.uk-list-large.uk-list-striped",
            Components.postSummaryItems postListSummary
          )
        )
      ),
      ?env = env
    )


  static member about(?env: LayoutEnv) =

    Page(
      h(
        "article.uk-padding",
        h(
          "header",
          h("h1", "Hola, esta es la pagina del pompo!"),
          h(
            "p.uk-text-large",
            text "Tambien llamado: Hector. Tiene un hermano llamado: ",
            h("a[href=/].uk-link-text", "Ompo")
          )
        )
      ),
      ?env = env
    )

module Views.About

open Hox
open Layout

let index aboutParams =
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
    env = aboutParams
  )

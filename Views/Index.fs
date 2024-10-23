module Views.Index

open Hox
open Layout

let index indexParams =
  Page(
    h(
      "article.uk-padding",
      h(
        "header",
        h("h1", "Hola! Bienvenido a la pagina del Ompo"),
        h(
          "p.uk-text-large",
          text "Tambien conocido como Victor. Tiene un hermano llamado: ",
          h("a[href=/about].uk-link-text", "Pompo")
        )
      )
    ),
    env = indexParams
  )

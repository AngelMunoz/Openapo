namespace Views

open Hox
open Layout

// if the file gets "too big" we can split it into multiple files and just add
// more extension members to this type
type Public = class end


type Public with

  static member index(?env: LayoutEnv) =
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

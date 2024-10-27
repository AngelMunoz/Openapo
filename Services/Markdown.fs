namespace Services


open Markdig

type MarkdownService =
  abstract toText: string -> string
  abstract toHtml: string -> string


module Markdown =

  let getMarkdownService(markdownConfig: MarkdownPipeline) : MarkdownService =
    { new MarkdownService with
        member _.toText text =
          Markdown.ToPlainText(text, markdownConfig)

        member _.toHtml text = Markdown.ToHtml(text, markdownConfig)
    }

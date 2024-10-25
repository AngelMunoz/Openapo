module Controllers.Posts

open System
open Models
open Oxpecker

open Services
open Views

module Helpers =
  let requiredStringBinder(value: string) =
    if String.IsNullOrWhiteSpace value then None else Some value

  let requiredGuidBinder(value: string) =
    match Guid.TryParse value with
    | true, guid -> Some guid
    | _ -> None

let newPost: EndpointHandler =
  fun ctx ->
    let env = ctx.GetLayoutEnv()
    let csrfInput = ctx.CsrfInput()
    let authors = ctx.GetService<AuthorsService>()
    let getAuthors = fun () -> authors.findAuthors()

    sendHox (Posts.index(getAuthors, csrfInput, env)) ctx


let savePost: EndpointHandler =
  fun ctx -> task {
    let posts = ctx.GetService<PostsService>()

    let title =
      ctx.TryGetFormValue("title")
      |> Option.bind Helpers.requiredStringBinder
      |> Option.defaultWith(fun _ -> failwith "Required Property ")

    let content =
      ctx.TryGetFormValue("content")
      |> Option.bind Helpers.requiredStringBinder
      |> Option.defaultWith(fun _ -> failwith "Required Property ")

    let status =
      ctx.TryGetFormValue("status")
      |> Option.map PostStatus.fromString
      |> Option.defaultValue Draft

    let authorId =
      ctx.TryGetFormValue("author") |> Option.bind Helpers.requiredGuidBinder

    let newPost: NewPostPayload = {
      title = title
      content = content
      status = status
      authorId = authorId
    }

    do! posts.savePost(newPost)

    return! (setStatusCode 201 >=> redirectTo "/" false) ctx
  }

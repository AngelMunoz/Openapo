namespace Models

open System

type PostStatus =
  | Draft
  | Published

type Author = {
  id: Guid
  name: string
  email: string
  bio: string
  socialNetworks: Map<string, string>
}

type Post = {
  id: Guid
  title: string
  content: string
  status: PostStatus
  author: Author
  slug: string option
  createdAt: DateTime
  updatedAt: DateTime
  publishedAt: DateTime option
}

type PostSummary = {
  id: Guid
  title: string
  summary: string
  authorName: string
  permanentPath: string
  publishedAt: DateTime
}

type NewPostPayload = {
  title: string
  content: string
  status: PostStatus
  authorId: Guid option
}

type NewAuthorPayload = {
  name: string
  email: string
  bio: string
  socialNetworks: Map<string, string>
}


[<AutoOpen>]
module Extensions =

  type PostStatus with
    member this.AsString =
      match this with
      | Draft -> "draft"
      | Published -> "published"

    static member fromString(str: string) =
      match str.ToLowerInvariant() with
      | "on"
      | "published" -> Published
      | _ -> Draft

namespace Services

open System
open System.Threading.Tasks

open Npgsql
open System.Data
open Donald

open Models
open NpgsqlTypes

type ConnectionFactory =
  abstract member CreateConnectionAsync: unit -> Task<IDbConnection>

module Database =
  open Microsoft.Extensions.Configuration

  let getDataSource(options: IConfiguration) =
    let connectionString = options.GetConnectionString("DatabaseConnection")
    NpgsqlDataSource.Create(connectionString)

  let getConnectionFactory(source: NpgsqlDataSource) =
    { new ConnectionFactory with
        member _.CreateConnectionAsync() : Task<IDbConnection> = task {
          let! connection = source.OpenConnectionAsync()
          return connection :> IDbConnection
        }
    }


type PostsService =
  abstract findPosts: unit -> Task<Post list>
  abstract savePost: NewPostPayload -> Task<unit>
  abstract findPostById: Guid -> Task<Post option>
  abstract updatePost: Post -> Task<unit>

type AuthorsService =
  abstract findAuthors: unit -> Task<Author list>
  abstract updateAuthor: Author -> Task<unit>
  abstract saveAuthor: NewAuthorPayload -> Task<unit>

module Mappings =

  let authorMapper(reader: IDataReader) =
    let id = reader.ReadGuidOption("author_id")

    match id with
    | None ->
        // for some reason this post has no author
        {
          id = Guid.Empty
          name = "Unknown"
          email = ""
          bio = ""
          socialNetworks = Map.empty
        }
    | Some id ->

      let name = reader.ReadString("author_name")
      let email = reader.ReadString("author_email")
      let bio = reader.ReadString("author_bio")

      let socialNetworks =
        reader.ReadStringOption("author_social_networks")
        |> Option.map(
          System.Text.Json.JsonSerializer.Deserialize<Map<string, string>>
        )
        |> Option.defaultValue Map.empty

      {
        id = id
        name = name
        email = email
        bio = bio
        socialNetworks = socialNetworks
      }

  let postMapper authorMapper (reader: IDataReader) =
    let id = reader.ReadGuid("id")
    let title = reader.ReadString("title")
    let content = reader.ReadString("content")

    let status =
      reader.ReadString("status")
      |> function
        | "draft" -> Draft
        | "published" -> Published
        | _ -> failwith "Invalid Post Status"

    let createdAt = reader.ReadDateTime("created_at")
    let updatedAt = reader.ReadDateTime("updated_at")
    let publishedAt = reader.ReadDateTimeOption("published_at")
    let slug = reader.ReadStringOption("slug")

    {
      id = id
      title = title
      content = content
      status = status
      slug = slug
      author = authorMapper reader
      createdAt = createdAt
      updatedAt = updatedAt
      publishedAt = publishedAt
    }

module Queries =
  [<Literal>]
  let selectPosts =
    """
    SELECT
        p.id as id, p.title as title, p.content as content, p.status as status, p.slug as slug,
        p.created_at as created_at , p.updated_at as updated_at, p.published_at as published_at,
        a.id as author_id, a.name as author_name, a.email as author_email, a.bio as author_bio,
        a.social_networks as author_social_networks
    FROM
        posts p
    LEFT JOIN
        authors a ON p.author = a.id"""

  [<Literal>]
  let insertPost =
    """
    INSERT INTO posts (title, content, status, slug, author_id, created_at, updated_at, published_at)
    VALUES (@title, @content, @status, @slug, @author, @created_at, @updated_at, @published_at)"""

  [<Literal>]
  let updatePost =
    """
    UPDATE posts
    SET title = @title, content = @content, status = @status, slug = @slug,
        author_id = @author, updated_at = @updated_at, published_at = @published_at
    WHERE id = @id"""

  [<Literal>]
  let createAuthor =
    """
    INSERT INTO authors (name, email, bio, social_networks)
    VALUES (@name, @email, @bio, @social_networks)"""

  [<Literal>]
  let selectAuthors =
    """
    SELECT
      id as author_id, name as author_name, email as author_email, bio as author_bio,
      social_networks as author_social_networks
    FROM
      authors"""

  [<Literal>]
  let updateAuthor =
    """
    UPDATE authors
    SET name = @name, email = @email, bio = @bio, social_networks = @social_networks
    WHERE id = @id"""


module Posts =
  let getPostService(connectionFactory: ConnectionFactory) =
    { new PostsService with
        member _.findPostById id = task {
          use! connection = connectionFactory.CreateConnectionAsync()

          let! postWithAuthor =
            connection
            |> Db.newCommand Queries.selectPosts
            |> Db.setParams [ "id", SqlType.Guid id ]
            |> Db.Async.querySingle(Mappings.postMapper Mappings.authorMapper)

          return postWithAuthor
        }

        member _.updatePost post = task {
          use! connection = connectionFactory.CreateConnectionAsync()

          let query =
            if post.slug.IsSome then
              Queries.updatePost.Replace("slug = @slug,", "")
            else
              Queries.updatePost

          do!
            connection
            |> Db.newCommand query
            |> Db.setParams [
              "id", SqlType.Guid post.id
              "title", SqlType.String post.title
              "content", SqlType.String post.content
              "status", SqlType.String post.status.AsString
              "author", SqlType.Guid post.author.id
              "updated_at", SqlType.DateTime DateTime.UtcNow
              match post.status with
              | Published ->
                let date = DateTime.UtcNow

                yield! [
                  "published_at", SqlType.DateTime date
                  if post.slug.IsNone then
                    let slug =
                      $"""{date.ToString("yyyy-MM-dd")}-{post.title.Length}"""

                    "slug", SqlType.String slug
                ]
              | Draft -> ()
            ]
            |> Db.Async.exec
        }

        member _.findPosts() = task {
          use! connection = connectionFactory.CreateConnectionAsync()

          let! postsWithAuthors =
            connection
            |> Db.newCommand Queries.selectPosts
            |> Db.Async.query(Mappings.postMapper Mappings.authorMapper)

          return postsWithAuthors
        }

        member _.savePost newPost = task {
          use! connection = connectionFactory.CreateConnectionAsync()

          let insertPost =
            match newPost.authorId with
            | Some _ -> Queries.insertPost
            | None ->
              Queries.insertPost
                .Replace("author_id,", "")
                .Replace("@author,", "")

          use cmd =
            new NpgsqlCommand(insertPost, connection :?> NpgsqlConnection)


          cmd.Parameters.AddWithValue("title", newPost.title)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue("content", newPost.content)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue(
            "status",
            NpgsqlDbType.Unknown,
            newPost.status.AsString
          )
          |> ignore<NpgsqlParameter>

          match newPost.authorId with
          | Some id ->
            cmd.Parameters.AddWithValue("author", NpgsqlDbType.Uuid, id)
            |> ignore<NpgsqlParameter>
          | None -> ()

          cmd.Parameters.AddWithValue(
            "created_at",
            NpgsqlDbType.TimestampTz,
            DateTime.UtcNow
          )
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue(
            "updated_at",
            NpgsqlDbType.TimestampTz,
            DateTime.UtcNow
          )
          |> ignore<NpgsqlParameter>

          match newPost.status with
          | Published ->
            let date = DateTime.UtcNow

            let slug =
              $"""{date.ToString("yyyy-MM-dd")}-{newPost.title.Length}"""

            cmd.Parameters.AddWithValue(
              "published_at",
              NpgsqlDbType.TimestampTz,
              date
            )
            |> ignore<NpgsqlParameter>

            cmd.Parameters.AddWithValue("slug", NpgsqlDbType.Text, slug)
            |> ignore<NpgsqlParameter>

          | Draft ->
            cmd.Parameters.AddWithValue(
              "published_at",
              NpgsqlDbType.TimestampTz,
              DBNull.Value
            )
            |> ignore<NpgsqlParameter>

            cmd.Parameters.AddWithValue("slug", NpgsqlDbType.Text, DBNull.Value)
            |> ignore<NpgsqlParameter>

          let! result = cmd.ExecuteNonQueryAsync()
          return if result = 0 then failwith "Failed to save post" else ()
        }

    }


module Authors =

  let getAuthorService(connectionFactory: ConnectionFactory) =
    { new AuthorsService with
        member this.findAuthors() = task {
          use! connection = connectionFactory.CreateConnectionAsync()

          let! authors =
            connection
            |> Db.newCommand Queries.selectAuthors
            |> Db.Async.query(Mappings.authorMapper)

          return authors
        }

        member this.saveAuthor(author) = task {
          let! connection = connectionFactory.CreateConnectionAsync()

          use cmd =
            new NpgsqlCommand(
              Queries.createAuthor,
              connection :?> NpgsqlConnection
            )

          cmd.Parameters.AddWithValue("name", author.name)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue("email", author.email)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue("bio", author.bio)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue(
            "social_networks",
            NpgsqlDbType.Jsonb,
            author.socialNetworks |> System.Text.Json.JsonSerializer.Serialize
          )
          |> ignore<NpgsqlParameter>

          let! result = cmd.ExecuteNonQueryAsync()
          return if result = 0 then failwith "Failed to save author" else ()
        }

        member this.updateAuthor(author) = task {

          let! connection = connectionFactory.CreateConnectionAsync()

          use cmd =
            new NpgsqlCommand(
              Queries.updateAuthor,
              connection :?> NpgsqlConnection
            )

          cmd.Parameters.AddWithValue("id", author.id)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue("name", author.name)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue("email", author.email)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue("bio", author.bio)
          |> ignore<NpgsqlParameter>

          cmd.Parameters.AddWithValue(
            "social_networks",
            NpgsqlDbType.Jsonb,
            author.socialNetworks |> System.Text.Json.JsonSerializer.Serialize
          )
          |> ignore<NpgsqlParameter>

          let! result = cmd.ExecuteNonQueryAsync()

          return
            if result = 0 then
              failwith "Failed to update author"
            else
              ()
        }
    }

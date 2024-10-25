# WIP



# Setup
```bash
dotnet tool restore
dotnet build -tl

# Make sure your DB is already available
dotnet run
```

# Database stuff

Set up your database, and in case you want to do it locally you can try out podman/docker:
```bash
# Use docker equivalent or podman like below
podman run --name posgrito -e POSTGRES_PASSWORD=posgres -e POSTGRES_USER=posgres -p 5432:5432 -d docker.io/library/postgres:alpine
```

Once the db server is up and running, create a database with the name "openapo", or whatever name you choose (just don't forget to update your connection string for both migrondi.json and appsettings.json. Once setup, plase run the migrations

```bash
dotnet migrondi up
```
And your database should be ready to go.

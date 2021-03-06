# Bookmark Manager

A simple bookmark manager written in C#

## Technologies

- `aspnet core 5`
- `sql server`
- `ef core 5`
- `rabbit mq`

## Architecture

![architecure](./docs/architecture.png)

## Running locally

```sh
# start dependencies
docker-compose up -d db queue

# see help
dotnet run -- -h

# run api
dotnet run -- api

# run consumer
dotnet run -- bookmark-inserted-consumer
```

## References

- https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-5.0&tabs=visual-studio
- https://medium.com/@sergiobarriel/how-to-automatically-validate-a-model-with-mvc-filter-and-fluent-validation-package-ae51098bcf5b
- https://www.rabbitmq.com/dotnet-api-guide.html#connection-and-channel-lifspan
- https://github.com/Tyrrrz/CliFx?utm_source=csharp&utm_medium=email&utm_campaign=digest#quick-start
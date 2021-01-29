# Bookmark Manager

A simple bookmark manager written in `aspnet core 5`

## Technologies

- `aspnet core 5`
- `sql server`
- `ef core 5`
- `rabbit mq`

## Running locally

```sh
# start dependencies
docker-compose up -d db queue

# run api
dotnet run -- api

# run consumer
dotnet run -- bookmark-inserted-consumer
```

## References

- https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-5.0&tabs=visual-studio
- https://medium.com/@sergiobarriel/how-to-automatically-validate-a-model-with-mvc-filter-and-fluent-validation-package-ae51098bcf5b
- https://www.rabbitmq.com/dotnet-api-guide.html#connection-and-channel-lifspan

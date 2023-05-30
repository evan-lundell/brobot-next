FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /build
COPY ./src/Brobot/Brobot.csproj ./src/Brobot/
COPY ./src/Brobot.Frontend/Brobot.Frontend.csproj ./src/Brobot.Frontend/
COPY ./src/Brobot.Shared/Brobot.Shared.csproj ./src/Brobot.Shared/
COPY brobot-next.sln .
RUN dotnet restore
COPY ./src ./src
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as app
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "Brobot.dll" ]
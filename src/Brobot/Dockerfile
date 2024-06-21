FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /build
COPY ./src/Brobot/Brobot.csproj ./src/Brobot/Brobot.csproj
COPY ./src/Brobot.Frontend/Brobot.Frontend.csproj ./src/Brobot.Frontend/Brobot.Frontend.csproj
COPY ./src/Brobot.Shared/Brobot.Shared.csproj ./src/Brobot.Shared/Brobot.Shared.csproj
RUN dotnet restore ./src/Brobot/Brobot.csproj
COPY ./src ./src
RUN dotnet publish ./src/Brobot/Brobot.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as app
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "Brobot.dll" ]

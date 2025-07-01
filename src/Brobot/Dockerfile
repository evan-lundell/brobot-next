FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build
COPY ./src/Brobot/Brobot.csproj ./src/Brobot/Brobot.csproj
COPY ./src/Brobot.Frontend/Brobot.Frontend.csproj ./src/Brobot.Frontend/Brobot.Frontend.csproj
COPY ./src/Brobot.Shared/Brobot.Shared.csproj ./src/Brobot.Shared/Brobot.Shared.csproj
RUN dotnet restore ./src/Brobot/Brobot.csproj
COPY ./src ./src
RUN dotnet publish ./src/Brobot/Brobot.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS app
WORKDIR /app
COPY --from=build /app .
RUN apt-get update && apt-get install -y libfontconfig1 libfreetype6
ENTRYPOINT [ "dotnet", "Brobot.dll" ]

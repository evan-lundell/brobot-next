FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /source
COPY ./Brobot/Brobot.csproj ./Brobot/
COPY ./Brobot.Frontend/Brobot.Frontend.csproj ./Brobot.Frontend/
COPY ./Brobot.Shared/Brobot.Shared.csproj ./Brobot.Shared/
COPY brobot-next.sln .
RUN dotnet restore
COPY ./Brobot/ ./Brobot/
COPY ./Brobot.Frontend/ ./Brobot.Frontend/
COPY ./Brobot.Shared/ ./Brobot.Shared/
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as app
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "Brobot.dll" ]
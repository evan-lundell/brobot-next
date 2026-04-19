FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=unknown
ARG NUGET_TOKEN
ENV APP_VERSION=$VERSION
WORKDIR /build
COPY ./src/Brobot/Brobot.csproj ./src/Brobot/Brobot.csproj
COPY ./src/Brobot.Frontend/Brobot.Frontend.csproj ./src/Brobot.Frontend/Brobot.Frontend.csproj
COPY ./src/Brobot.Shared/Brobot.Shared.csproj ./src/Brobot.Shared/Brobot.Shared.csproj
COPY nuget.config .
RUN dotnet nuget update source github \
    --username github \
    --password $NUGET_TOKEN \
    --store-password-in-clear-text \
    --configfile nuget.config
RUN dotnet restore ./src/Brobot/Brobot.csproj
COPY ./src ./src
RUN dotnet publish ./src/Brobot/Brobot.csproj -c Release -o /app /p:Version=$VERSION
RUN echo "$VERSION" > /app/version.txt

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS app
WORKDIR /app
COPY --from=build /app .
RUN apt-get update && apt-get install -y libfontconfig1 libfreetype6
ENTRYPOINT [ "dotnet", "Brobot.dll" ]
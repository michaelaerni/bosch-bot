FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore -r linux-musl-x64

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app -r linux-musl-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true

# Final stage/image
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine
WORKDIR /app
COPY --from=build /app ./

# This is temporary: Install fonts
RUN apk add --no-cache ttf-liberation

ENTRYPOINT ["./BoschBot"]

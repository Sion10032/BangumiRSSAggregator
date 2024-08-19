# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
WORKDIR /source/BangumiRSSAggregator.Server
COPY BangumiRSSAggregator.Server/BangumiRSSAggregator.Server.csproj .
RUN dotnet restore

# copy everything else and build app
COPY BangumiRSSAggregator.Server/. /source/BangumiRSSAggregator.Server/
WORKDIR /source/BangumiRSSAggregator.Server
RUN dotnet publish -c release -o /app --no-restore

FROM node:20 AS react-build
WORKDIR /app
COPY BangumiRSSAggregator.Client/package.json /app/package.json
COPY BangumiRSSAggregator.Client/package-lock.json /app/package-lock.json
# RUN npm install
RUN npm install --registry=https://registry.npmmirror.com

COPY BangumiRSSAggregator.Client/. /app/
RUN npm run build

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
COPY --from=react-build /app/dist ./wwwroot
ENTRYPOINT ["dotnet", "BangumiRSSAggregator.Server.dll"]

EXPOSE 8080/tcp
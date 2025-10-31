FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /source

# Copy project file and restore as distinct layers
COPY --link . .
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish -c Release -a $TARGETARCH --no-restore -o /app


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Issueneter.dll"]
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Brecho.Api/Brecho.Api.csproj Brecho.Api/
RUN dotnet restore Brecho.Api/Brecho.Api.csproj

COPY Brecho.Api/ Brecho.Api/
RUN dotnet publish Brecho.Api/Brecho.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ARG RENDER_GIT_COMMIT=local
ENV RENDER_GIT_COMMIT=${RENDER_GIT_COMMIT}
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Brecho.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY . .
RUN dotnet publish --configuration Release -o /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS final
EXPOSE 5337
ENV ASPNETCORE_URLS=http://*:5337
WORKDIR /app
COPY --from=publish /app .
CMD tail -f /dev/null
ENTRYPOINT ["dotnet", "basicapi.dll"]
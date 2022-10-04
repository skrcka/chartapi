FROM mcr.microsoft.com/dotnet/sdk:6.0 as prep
RUN apt-get update
RUN apt-get install -y dirmngr gnupg apt-transport-https ca-certificates software-properties-common
RUN apt-key adv --keyserver keyserver.ubuntu.com --recv-keys E298A3A825C0D65DFD57CBB651716619E084DAB9
RUN add-apt-repository 'deb https://cloud.r-project.org/bin/linux/ubuntu focal-cran40/'
RUN apt-get install -y r-base
RUN apt-get install -y build-essential
COPY ./scripts/Install.R Intall.R
RUN Rscript.exe Install.R

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY . .
RUN dotnet publish --configuration Release -o /app

FROM prep AS final
EXPOSE 5337
ENV ASPNETCORE_URLS=http://*:5337
WORKDIR /app
COPY --from=publish /app .
CMD tail -f /dev/null
ENTRYPOINT ["dotnet", "basicapi.dll"]
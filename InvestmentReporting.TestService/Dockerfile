FROM mcr.microsoft.com/dotnet/runtime-deps:5.0.3-alpine3.13-arm64v8

EXPOSE 8083

ADD bin/Debug/net5.0/linux-musl-arm64/publish publish

ENV ASPNETCORE_URLS=http://+:8083

WORKDIR ./publish
ENTRYPOINT ["./InvestmentReporting.TestService"]

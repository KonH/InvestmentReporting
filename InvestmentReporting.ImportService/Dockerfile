ARG DOTNET_IMAGE_SUFFIX
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0.3-alpine3.13-$DOTNET_IMAGE_SUFFIX

EXPOSE 8086

RUN echo "http://dl-cdn.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories
RUN apk --update add libgdiplus

ADD publish /publish

ENV ASPNETCORE_URLS=http://+:8086
ENV ASPNETCORE_ENVIRONMENT=Development

WORKDIR /publish
ENTRYPOINT ["./InvestmentReporting.ImportService"]

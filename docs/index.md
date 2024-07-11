---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
  name: "LibSql.Http.Client"
  text: "A .NET HTTP client for libSQL"
  # tagline: My great project tagline
  actions:
    - theme: brand
      text: Get started
      link: /guide/getting-started
    - theme: alt
      text: GitHub
      link: https://github.com/hermogenes/libsql-http-client-dotnet
    - theme: alt
      text: NuGet
      link: https://www.nuget.org/packages/LibSql.Http.Client

features:
  - title: NativeAOT compatible
    details: Native AOT apps have faster startup time and smaller memory footprints. These apps can run on machines that don't have the .NET runtime installed.
    icon: 🚀
  - title: Minimum memory usage
    details: The requests and response are handled using System.Text.Json low-level APIs, which are more efficient than standard JSON serialization and deserialization.
    icon: 📉
  - title: Nice developer experience
    details: Clean and simple API, allowing you to execute SQL statements with ease.
    icon: 😃
---

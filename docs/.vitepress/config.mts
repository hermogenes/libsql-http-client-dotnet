import { defineConfig } from "vitepress";

// https://vitepress.dev/reference/site-config
export default defineConfig({
  cleanUrls: true,
  title: "LibSql.Http.Client",
  description: "A .NET HTTP client for libSQL",
  head: [["link", { rel: "icon", href: "/favicon.ico" }]],
  themeConfig: {
    logo: "/logo.png",
    nav: [
      { text: "Home", link: "/" },
      { text: "Guide", link: "/guide/getting-started" },
    ],

    sidebar: [
      {
        text: "Introduction",
        collapsed: false,
        items: [
          { text: "Why?", link: "/guide/why" },
          { text: "Getting started", link: "/guide/getting-started" },
          {
            text: "Guidelines",
            link: "/guide/guidelines",
          },
          {
            text: "JSON Serializer Context",
            link: "/guide/concepts/json-serializer-context",
          },
          {
            text: "Transactions",
            link: "/guide/concepts/transactions",
          },
          {
            text: "Demo Console App",
            link: "/guide/demo-console-app",
          },
        ],
      },
      {
        text: "Querying",
        collapsed: false,
        link: "/guide/query",
        items: [
          {
            text: "Executing statements",
            link: "/guide/query/execute",
          },
          { text: "Using args", link: "/guide/query/args" },
          { text: "Scalar Values", link: "/guide/query/scalar-values" },
          { text: "Single Row", link: "/guide/query/single-row" },
          { text: "Multiple Rows", link: "/guide/query/multiple-rows" },
          { text: "Multiple Results", link: "/guide/query/multiple-results" },
        ],
      },
    ],

    socialLinks: [
      {
        icon: "github",
        link: "https://github.com/hermogenes/libsql-http-client-dotnet",
      },
      {
        icon: {
          svg: `<img class="nuget-logo" src="/nuget.svg" alt="Nuget logo icon" />`,
        },
        ariaLabel: "NuGet link",
        link: "https://www.nuget.org/packages/LibSql.Http.Client",
      },
    ],
  },
});

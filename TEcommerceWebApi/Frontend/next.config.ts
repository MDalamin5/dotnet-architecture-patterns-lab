import type { NextConfig } from "next";

const API_PROXY_TARGET = process.env.API_PROXY_TARGET ?? "http://localhost:5131";

const nextConfig: NextConfig = {
  cacheComponents: false,
  partialPrefetching: false,
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
  // The ASP.NET API has no CORS policy, so the browser talks to it same-origin through this proxy.
  async rewrites() {
    return [{ source: "/api/v2/:path*", destination: `${API_PROXY_TARGET}/api/v2/:path*` }];
  },
};

export default nextConfig;

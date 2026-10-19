export const env = {
  apiUrl: process.env.NEXT_PUBLIC_API_URL ?? "/api/v2",
  rootDomain: process.env.NEXT_PUBLIC_ROOT_DOMAIN ?? "byvstore.com",
  mediaUrl: process.env.NEXT_PUBLIC_MEDIA_URL ?? "http://localhost:9000",
} as const;

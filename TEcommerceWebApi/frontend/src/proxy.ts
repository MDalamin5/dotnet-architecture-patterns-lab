import { NextResponse, type NextRequest } from "next/server";

const ROOT_DOMAIN = process.env.NEXT_PUBLIC_ROOT_DOMAIN ?? "byvstore.com";
const PLATFORM_SUBDOMAINS = new Set(["app", "www"]);

/**
 * Multi-tenant host routing.
 *   nike.byvstore.com/...   -> internally /store/nike/...   (white-labeled storefront)
 *   app.byvstore.com/admin  -> unchanged                    (merchant portal)
 *   localhost:3000/store/<tenantId>, /admin                 -> unchanged (local development)
 * The tenant GUID is resolved client-side and sent to the API as X-Tenant-Id.
 */
export function proxy(request: NextRequest) {
  const host = (request.headers.get("host") ?? "").split(":")[0].toLowerCase();
  const { pathname } = request.nextUrl;

  if (host.endsWith(`.${ROOT_DOMAIN}`)) {
    const subdomain = host.slice(0, -(ROOT_DOMAIN.length + 1));
    const isPlatform = PLATFORM_SUBDOMAINS.has(subdomain);
    const alreadyRouted = pathname.startsWith("/store/") || pathname.startsWith("/admin");

    if (!isPlatform && !alreadyRouted) {
      const url = request.nextUrl.clone();
      url.pathname = `/store/${subdomain}${pathname === "/" ? "" : pathname}`;
      return NextResponse.rewrite(url);
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)"],
};

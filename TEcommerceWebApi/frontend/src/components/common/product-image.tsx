import { ImageOff } from "lucide-react";
import { resolveMediaUrl } from "@/lib/format";
import { cn } from "@/lib/utils";

interface ProductImageProps {
  src?: string | null;
  alt: string;
  className?: string;
}

/**
 * Plain <img> on purpose: MinIO serves from localhost/private hosts, which the next/image
 * optimiser refuses to fetch, and the files are already public static objects.
 */
export function ProductImage({ src, alt, className }: ProductImageProps) {
  const url = resolveMediaUrl(src);
  return (
    <div className={cn("relative overflow-hidden bg-muted", className)}>
      {url ? (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={url} alt={alt} loading="lazy" className="size-full object-cover" />
      ) : (
        <div className="flex size-full items-center justify-center text-muted-foreground" role="img" aria-label="No image">
          <ImageOff className="size-1/4 min-h-5 min-w-5" />
        </div>
      )}
    </div>
  );
}

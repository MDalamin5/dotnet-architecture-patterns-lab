"use client";

import { useEffect, useId, useMemo, useRef, useState } from "react";
import { ImagePlus, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

export const ACCEPTED_IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"] as const;
export const MAX_IMAGE_BYTES = 5 * 1024 * 1024;

/** Returns an error message, or null when the file is acceptable. */
export function validateImage(file: File): string | null {
  if (!(ACCEPTED_IMAGE_TYPES as readonly string[]).includes(file.type)) return "Only .jpg, .png and .webp images are allowed.";
  if (file.size > MAX_IMAGE_BYTES) return "Image must be 5 MB or smaller.";
  return null;
}

interface ImageDropzoneProps {
  file: File | null;
  onChange: (file: File | null) => void;
  disabled?: boolean;
}

export function ImageDropzone({ file, onChange, disabled }: ImageDropzoneProps) {
  const inputId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const [dragging, setDragging] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Instant local preview, released when the file changes or the dialog unmounts.
  const previewUrl = useMemo(() => (file ? URL.createObjectURL(file) : null), [file]);
  useEffect(() => () => void (previewUrl && URL.revokeObjectURL(previewUrl)), [previewUrl]);

  function accept(next: File | undefined) {
    if (!next) return;
    const problem = validateImage(next);
    setError(problem);
    if (!problem) onChange(next);
  }

  return (
    <div className="space-y-1.5">
      <div
        onDragOver={(e) => {
          e.preventDefault();
          if (!disabled) setDragging(true);
        }}
        onDragLeave={() => setDragging(false)}
        onDrop={(e) => {
          e.preventDefault();
          setDragging(false);
          if (!disabled) accept(e.dataTransfer.files[0]);
        }}
        className={cn(
          "relative flex min-h-40 items-center justify-center overflow-hidden rounded-lg border-2 border-dashed transition-colors",
          dragging ? "border-primary bg-primary/5" : "border-input",
          disabled && "opacity-50",
        )}
      >
        {previewUrl ? (
          <>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={previewUrl} alt="Selected product preview" className="max-h-56 w-full object-contain" />
            <Button
              type="button"
              variant="secondary"
              size="icon-sm"
              className="absolute top-2 right-2"
              aria-label="Remove image"
              disabled={disabled}
              onClick={() => {
                onChange(null);
                setError(null);
                if (inputRef.current) inputRef.current.value = "";
              }}
            >
              <X />
            </Button>
          </>
        ) : (
          <label
            htmlFor={inputId}
            className="flex w-full cursor-pointer flex-col items-center gap-2 p-6 text-center text-sm text-muted-foreground"
          >
            <ImagePlus className="size-8" />
            <span>
              <span className="font-medium text-primary">Click to upload</span> or drag and drop
            </span>
            <span className="text-xs">JPG, PNG or WEBP · max 5 MB</span>
          </label>
        )}
        <input
          ref={inputRef}
          id={inputId}
          type="file"
          accept={ACCEPTED_IMAGE_TYPES.join(",")}
          className="sr-only"
          disabled={disabled}
          onChange={(e) => accept(e.target.files?.[0])}
        />
      </div>
      {error && (
        <p role="alert" className="text-xs text-destructive">
          {error}
        </p>
      )}
    </div>
  );
}

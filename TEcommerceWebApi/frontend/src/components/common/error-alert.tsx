import { AlertCircle } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { getErrorMessages } from "@/lib/errors";

interface ErrorAlertProps {
  error: unknown;
  title?: string;
  className?: string;
}

export function ErrorAlert({ error, title = "Something went wrong", className }: ErrorAlertProps) {
  if (!error) return null;
  const messages = getErrorMessages(error);
  return (
    <Alert variant="destructive" className={className}>
      <AlertCircle />
      <AlertTitle>{title}</AlertTitle>
      <AlertDescription>
        {messages.length === 1 ? (
          messages[0]
        ) : (
          <ul className="list-disc pl-4">
            {messages.map((m) => (
              <li key={m}>{m}</li>
            ))}
          </ul>
        )}
      </AlertDescription>
    </Alert>
  );
}

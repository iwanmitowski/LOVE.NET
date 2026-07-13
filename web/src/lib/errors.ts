import { AxiosError } from "axios";

/** Flattens the API's assorted error shapes into displayable lines (ported behavior). */
export function errorMessages(error: unknown): string[] {
  if (error instanceof AxiosError && error.response?.data) {
    const data = error.response.data as Record<string, unknown>;

    if (Array.isArray(data)) return data.map(String);
    if (typeof data === "string") return [data];

    const lines: string[] = [];
    if (typeof data.Error === "string") lines.push(data.Error);
    if (Array.isArray(data.errors)) lines.push(...data.errors.map(String));
    else if (data.errors && typeof data.errors === "object") {
      for (const value of Object.values(data.errors as Record<string, unknown>)) {
        if (Array.isArray(value)) lines.push(...value.map(String));
        else lines.push(String(value));
      }
    }
    if (lines.length > 0) return lines.flatMap((l) => l.split("\n"));
  }

  if (error instanceof Error && error.message) return error.message.split("\n");
  return ["Something went wrong"];
}

export class CustomHttpHeaders {
  headers: Record<string, string[]> = {};

  get contentType(): string | null {
    return this.headers["content-type"]?.[0] ?? null;
  }
}

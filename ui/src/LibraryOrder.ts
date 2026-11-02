import type { Game } from "./types";

export function releaseYear(game: Game): number | null {
  const year = game.releaseYear;
  return typeof year === "number" && Number.isInteger(year) && year >= 1970 && year <= 2099 ? year : null;
}

export function sortLibrary(games: readonly Game[], preferredIds: readonly string[] = [], language = "en"): Game[] {
  const preferred = new Set(preferredIds);
  const titles = new Intl.Collator(language, { numeric: true, sensitivity: "base" });
  return [...games].sort((a, b) =>
    Number(preferred.has(b.id)) - Number(preferred.has(a.id)) ||
    (releaseYear(b) ?? 0) - (releaseYear(a) ?? 0) ||
    titles.compare(a.title, b.title) || titles.compare(a.platform, b.platform) ||
    titles.compare(a.id, b.id),
  );
}

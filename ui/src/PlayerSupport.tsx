import { Users } from "lucide-react";
import { t, message } from "./i18n";
import type { PlayerSupport } from "./types";

export const supportsLocalPlayers = (support?: PlayerSupport | null) => !!support && ["local-coop", "local-versus", "local-mixed", "alternating", "asymmetric"].includes(support.mode);

export function playerLabel(support?: PlayerSupport | null): string {
  if (!support) return t("Spielerzahl ungeprüft");
  const count = support.maxPlayers ?? "2+";
  switch (support.mode) {
    case "single": return t("1 Spieler");
    case "local-coop": return t("{0} Spieler · lokaler Koop", count);
    case "local-versus": return t("{0} Spieler · lokal gegeneinander", count);
    case "local-mixed": return t("{0} Spieler · lokal", count);
    case "alternating": return t("{0} Spieler · abwechselnd", count);
    case "asymmetric": return t("{0} Spieler · unterschiedliche Rollen", count);
    case "online": return t("Mehrspieler online");
    case "linked": return t("{0} Spieler · verknüpfte Automaten", count);
  }
}

export function PlayerInfo({ support, details = false }: {support?: PlayerSupport | null; details?: boolean}) {
  return <div className={details ? "player-support-details" : "player-support"}>
    <span className="player-support-label"><Users size={16} aria-hidden="true"/>{playerLabel(support)}</span>
    {details && <>
      {support?.note && <p>{message(support.note)}</p>}
      <p>{support ? t("Spielmodus laut Hersteller oder Store. Die Funktion mit deinen Guns wird separat geprüft.") : t("Für diese Spielversion fehlt noch eine verifizierte Quelle zur Spielerzahl.")}</p>
      {support?.sources.map((source, index) => <a key={source} href={source} target="_blank" rel="noreferrer" onClick={event => {
        if (window.chrome?.webview) { event.preventDefault(); window.chrome.webview.postMessage({type:"open-player-source",payload:{url:source}}); }
      }}>{t("Quelle {0}", index + 1)}</a>)}
    </>}
  </div>;
}

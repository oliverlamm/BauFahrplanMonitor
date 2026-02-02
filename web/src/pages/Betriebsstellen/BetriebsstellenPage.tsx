import "./BetriebsstellenPage.css";
import { useEffect, useMemo, useState } from "react";
import {
    MapContainer,
    TileLayer,
    Marker,
    Popup,
    useMap
} from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

import { useBetriebsstellen } from "../../hooks/useBetriebsstellen";
import { useTrassenfinderInfrastrukturen } from "../../hooks/useTrassenfinder";
import type { BetriebsstelleDetail } from "../../models/betriebsstelle";

/* ============================================================
 * Status-Badge Helper
 * ============================================================ */
function badgeFromInfraState(
    state: InfraUpdateState
): { label: string; variant: "neutral" | "running" | "ok" | "error" } {
    switch (state) {
        case "running":
            return { label: "Importiere", variant: "running" };
        case "done":
            return { label: "Fertig", variant: "ok" };
        case "error":
            return { label: "Fehler", variant: "error" };
        default:
            return { label: "Bereit", variant: "neutral" };
    }
}

/* ============================================================
 * Leaflet Icon Fix (Vite)
 * ============================================================ */
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
    iconRetinaUrl:
        "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
    iconUrl:
        "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
    shadowUrl:
        "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
});

/* ============================================================
 * Recenter Helper
 * ============================================================ */
function RecenterMap({ lat, lon }: { lat: number; lon: number }) {
    const map = useMap();

    useEffect(() => {
        map.setView([lat, lon], map.getZoom(), { animate: true });
    }, [lat, lon, map]);

    return null;
}

/* ============================================================
 * Datum Helper
 * ============================================================ */
function formatDateDE(value: string | null | undefined): string {
    if (!value) return "";

    const d = new Date(value);
    if (isNaN(d.getTime())) return "";

    return d.toLocaleDateString("de-DE", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric"
    });
}


/* ============================================================
 * Types
 * ============================================================ */
type InfraUpdateState = "idle" | "running" | "done" | "error";

/* ============================================================
 * Page
 * ============================================================ */
export default function BetriebsstellenPage() {

    /* ================= Trassenfinder ================= */
    const {
        items: infraList,
        loading: infraLoading,
        error: infraError
    } = useTrassenfinderInfrastrukturen();

    const [selectedInfraId, setSelectedInfraId] =
        useState<number | null>(null);

    /* ================= Infra-Update-State ================= */
    const [infraUpdateState, setInfraUpdateState] =
        useState<InfraUpdateState>("idle");

    const [infraProgress, setInfraProgress] = useState(0);
    const [infraMessage, setInfraMessage] = useState<string | null>(null);

    const infraBadge = useMemo(
        () => badgeFromInfraState(infraUpdateState),
        [infraUpdateState]
    );

    /* ================= Betriebsstellen ================= */
    const {
        list,
        detail,
        loadDetail,
        loading,
        error,
        saveDetail
    } = useBetriebsstellen();

    const [local, setLocal] =
        useState<BetriebsstelleDetail | null>(null);

    const [selectedGeoIndex, setSelectedGeoIndex] =
        useState<number | null>(null);

    const dirty = useMemo(() => {
        if (!detail || !local) return false;
        return JSON.stringify(detail) !== JSON.stringify(local);
    }, [detail, local]);

    const [saveState, setSaveState] =
        useState<"idle" | "saving" | "saved">("idle");

    /* ================= Effects ================= */
    useEffect(() => {
        if (detail) {
            setLocal(structuredClone(detail));
            setSelectedGeoIndex(detail.geo.length > 0 ? 0 : null);
            setSaveState("idle");
        } else {
            setLocal(null);
            setSelectedGeoIndex(null);
        }
    }, [detail]);

    useEffect(() => {
        if (!selectedInfraId) return;
        setInfraUpdateState("idle");
        setInfraProgress(0);
        setInfraMessage(null);
    }, [selectedInfraId]);

    /* ================= Actions ================= */
    async function startInfraUpdate() {
        if (!selectedInfraId) return;

        setInfraUpdateState("running");
        setInfraProgress(0);
        setInfraMessage("Starte Aktualisierung…");

        try {
            const res = await fetch(
                `/api/trassenfinder/infrastruktur/${selectedInfraId}/refresh`,
                { method: "POST" }
            );

            if (!res.ok) {
                throw new Error("Start fehlgeschlagen");
            }

            const data = await res.json();
            pollInfraJob(data.jobId);
        } catch (e) {
            setInfraUpdateState("error");
            setInfraMessage("Aktualisierung konnte nicht gestartet werden");
        }
    }

    function pollInfraJob(jobId: string) {
        const timer = setInterval(async () => {
            try {
                const res = await fetch(`/api/trassenfinder/jobs/${jobId}`);
                if (!res.ok) throw new Error("Polling fehlgeschlagen");

                const data = await res.json();

                setInfraProgress(data.progress ?? 0);
                setInfraMessage(data.message ?? null);

                if (data.state === "Done") {
                    clearInterval(timer);
                    setInfraUpdateState("done");
                }

                if (data.state === "Failed") {
                    clearInterval(timer);
                    setInfraUpdateState("error");
                    setInfraMessage(data.message ?? "Fehler beim Aktualisieren");
                }
            } catch (e) {
                clearInterval(timer);
                setInfraUpdateState("error");
                setInfraMessage("Kommunikationsfehler mit Backend");
            }
        }, 1000); // 1s Polling
    }
    
    /* ================= RENDER ================= */
    return (
        <div className="importer-page">
            <section className="importer-card bs-page">

                <header className="bs-header">
                    <h2>Betriebsstellenverwaltung</h2>
                    <div className="importer-subtitle">
                        Verwaltung und Bearbeitung von Betriebsstellen
                    </div>
                </header>

                {/* ================= Filter ================= */}
                <section className="bs-filter">
                    <div className="rl100-row">
                        <div className="rl100-select-wrapper">
                            <select
                                className="rl100-select"
                                disabled={loading}
                                onChange={e => {
                                    const id = Number(e.target.value);
                                    if (id > 0) void loadDetail(id);
                                }}
                            >
                                <option value="">
                                    {loading
                                        ? "Lade Betriebsstellen…"
                                        : "RL100 auswählen"}
                                </option>
                                {!loading && list.map(b => (
                                    <option key={b.id} value={b.id}>
                                        {b.name} [{b.rl100}]
                                    </option>
                                ))}
                            </select>
                            {loading && <span className="rl100-spinner" />}
                        </div>
                    </div>
                </section>

                {error && (
                    <div className="importer-error">
                        <i className="fa-solid fa-triangle-exclamation" />
                        &nbsp;{error}
                    </div>
                )}

                {/* ================= Zwei-Spalten-Grid ================= */}
                {local && (
                    <section className="bs-grid">

                        {/* Stammdaten */}
                        <div className="bs-card">
                            <table className="bs-form">
                                <tbody>
                                <tr><td>RL100</td><td><input value={local.rl100} disabled /></td></tr>
                                <tr><td>Name</td><td>
                                    <input
                                        className={local.name !== detail?.name ? "dirty" : ""}
                                        value={local.name}
                                        onChange={e =>
                                            setLocal({ ...local, name: e.target.value })
                                        }
                                    />
                                </td></tr>
                                </tbody>
                            </table>

                            <button
                                className="btn btn-primary full"
                                disabled={!dirty || saveState === "saving"}
                                onClick={async () => {
                                    if (!local) return;
                                    setSaveState("saving");
                                    await saveDetail(local);
                                    setSaveState("saved");
                                    setTimeout(() => setSaveState("idle"), 2000);
                                }}
                            >
                                Speichern
                            </button>
                        </div>

                        {/* Karte */}
                        <div className="bs-card">
                            <div className="bs-map">
                                {selectedGeoIndex !== null && (() => {
                                    const g = local.geo[selectedGeoIndex];
                                    return (
                                        <MapContainer
                                            center={[g.lat, g.lon]}
                                            zoom={14}
                                            style={{ height: "100%", width: "100%" }}
                                        >
                                            <TileLayer
                                                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                                            />
                                            <RecenterMap lat={g.lat} lon={g.lon} />
                                            <Marker position={[g.lat, g.lon]}>
                                                <Popup>VzG {g.vzGNr}</Popup>
                                            </Marker>
                                        </MapContainer>
                                    );
                                })()}
                            </div>
                        </div>
                    </section>
                )}

                {/* ================= Untere Aktions-Card ================= */}
                <section className="bs-bottom-card">
                    <div className="bs-bottom-row">
                        <div className="bs-bottom-left">
                            <label>Infrastruktur auswählen:</label>

                            <select
                                disabled={infraLoading}
                                value={selectedInfraId ?? ""}
                                onChange={e =>
                                    setSelectedInfraId(Number(e.target.value) || null)
                                }
                            >
                                <option value="">
                                    {infraLoading
                                        ? "Lade Infrastrukturen…"
                                        : "Infrastruktur auswählen"}
                                </option>
                                {infraList.map(i => (
                                    <option key={i.id} value={i.id}>
                                        {i.id} – {i.bezeichnung} (
                                        {formatDateDE(i.gueltigVon)} – {formatDateDE(i.gueltigBis)}
                                        )
                                    </option>
                                ))}
                            </select>
                            {infraError && (
                                <div className="importer-error" style={{ marginTop: 8 }}>
                                    <i className="fa-solid fa-triangle-exclamation" />
                                    &nbsp;{infraError}
                                </div>
                            )}

                            <button
                                className="btn btn-primary"
                                disabled={!selectedInfraId || infraUpdateState === "running"}
                                onClick={startInfraUpdate}
                            >
                                Importieren
                            </button>

                            <button className="btn btn-ghost">
                                Abbruch
                            </button>
                        </div>

                        <div className="bs-bottom-right">
                            <span className={`status-badge ${infraBadge.variant}`}>
                                {infraBadge.label}
                            </span>
                        </div>
                    </div>

                    <div className="bs-progress">
                        <div className="bs-progress-label">
                            {infraMessage ?? "Bereit"}
                        </div>
                        <div className="bs-progress-bar">
                            <div
                                className="bs-progress-fill"
                                style={{ width: `${infraProgress}%` }}
                            />
                        </div>
                        <div className="bs-progress-percent">
                            {infraProgress}%
                        </div>
                    </div>
                </section>

            </section>
        </div>
    );
}

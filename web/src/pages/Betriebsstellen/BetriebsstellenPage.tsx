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
 * Job / Badge Helper
 * ============================================================ */
function badgeFromJobState(
    state?: number
): { label: string; variant: "neutral" | "running" | "ok" | "error" } {
    switch (state) {
        case 1:
            return { label: "Importiere", variant: "running" };
        case 2:
            return { label: "Fertig", variant: "ok" };
        case 3:
            return { label: "Fehler", variant: "error" };
        default:
            return { label: "Bereit", variant: "neutral" };
    }
}

type TrassenfinderJobDto = {
    jobId: string;
    state: number;
    progress: number;
    message: string;
};

/* ============================================================
 * Leaflet Fix
 * ============================================================ */
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
    iconRetinaUrl:
        "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
    iconUrl:
        "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
    shadowUrl:
        "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png"
});

function RecenterMap({ lat, lon }: { lat: number; lon: number }) {
    const map = useMap();
    useEffect(() => {
        map.setView([lat, lon], map.getZoom(), { animate: true });
    }, [lat, lon, map]);
    return null;
}

/* ============================================================
 * Date Helper
 * ============================================================ */
function formatDateDE(value?: string) {
    if (!value) return "";
    return new Date(value).toLocaleDateString("de-DE");
}

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

    /* ================= Job State ================= */
    const [jobId, setJobId] = useState<string | null>(null);
    const [job, setJob] = useState<TrassenfinderJobDto | null>(null);

    const badge = useMemo(
        () => badgeFromJobState(job?.state),
        [job?.state]
    );

    /* ================= Job Polling ================= */
    useEffect(() => {
        if (!jobId) return;

        const timer = setInterval(async () => {
            const res = await fetch(`/api/trassenfinder/jobs/${jobId}`);
            if (!res.ok) return;

            const data = await res.json();
            setJob(data);

            if (data.state === 2 || data.state === 3) {
                clearInterval(timer);
            }
        }, 1000);

        return () => clearInterval(timer);
    }, [jobId]);

    async function startInfraUpdate() {
        if (!selectedInfraId) return;

        setJob(null);

        const res = await fetch(
            `/api/trassenfinder/infrastruktur/${selectedInfraId}/refresh`,
            { method: "POST" }
        );

        const data = await res.json();
        setJobId(data.jobId);
    }

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

    useEffect(() => {
        if (!detail) {
            setLocal(null);
            setSelectedGeoIndex(null);
            return;
        }

        setLocal(structuredClone(detail));
        setSelectedGeoIndex(detail.geo.length > 0 ? 0 : null);
    }, [detail]);

    /* ============================================================
     * RENDER
     * ============================================================ */
    return (
        <div className="importer-page">
            <section className="importer-card bs-page">

                {/* ================= Header ================= */}
                <header className="bs-header">
                    <h2>Betriebsstellenverwaltung</h2>
                    <div className="importer-subtitle">
                        Trassenfinder-Integration
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

                                {!loading &&
                                    list.map(b => (
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

                        {/* ================= Stammdaten ================= */}
                        <div className="bs-card">
                            <table className="bs-form">
                                <tbody>
                                <tr>
                                    <td>RL100</td>
                                    <td>
                                        <input value={local.rl100} disabled />
                                    </td>
                                </tr>

                                <tr>
                                    <td>Name</td>
                                    <td>
                                        <input
                                            className={
                                                local.name !== detail?.name
                                                    ? "dirty"
                                                    : ""
                                            }
                                            value={local.name}
                                            onChange={e =>
                                                setLocal({
                                                    ...local,
                                                    name: e.target.value
                                                })
                                            }
                                        />
                                    </td>
                                </tr>
                                </tbody>
                            </table>

                            <button
                                className="btn btn-primary full"
                                disabled={!dirty}
                                onClick={() => local && saveDetail(local)}
                            >
                                Speichern
                            </button>
                        </div>

                        {/* ================= Karte + Geo ================= */}
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
                                                <Popup>
                                                    VzG {g.vzGNr}
                                                </Popup>
                                            </Marker>
                                        </MapContainer>
                                    );
                                })()}
                            </div>

                            <table className="bs-geo">
                                <thead>
                                <tr>
                                    <th></th>
                                    <th>VzG</th>
                                    <th>Longitude</th>
                                    <th>Latitude</th>
                                    <th>km_l</th>
                                    <th>km_i</th>
                                </tr>
                                </thead>
                                <tbody>
                                {local.geo.map((g, i) => (
                                    <tr key={i}>
                                        <td>
                                            <input
                                                type="radio"
                                                name="geo"
                                                checked={selectedGeoIndex === i}
                                                onChange={() =>
                                                    setSelectedGeoIndex(i)
                                                }
                                            />
                                        </td>
                                        <td>{g.vzGNr}</td>
                                        <td>{g.lon.toFixed(6)}</td>
                                        <td>{g.lat.toFixed(6)}</td>
                                        <td>{g.kmL ?? "-"}</td>
                                        <td>{g.kmI ?? "-"}</td>
                                    </tr>
                                ))}
                                </tbody>
                            </table>

                            <button className="btn btn-primary full">
                                <i className="fa-solid fa-location-dot" />
                                Geodaten speichern
                            </button>
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
                                    setSelectedInfraId(
                                        Number(e.target.value) || null
                                    )
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
                                        {formatDateDE(i.gueltigVon)} –{" "}
                                        {formatDateDE(i.gueltigBis)})
                                    </option>
                                ))}
                            </select>

                            {infraError && (
                                <div
                                    className="importer-error"
                                    style={{ marginTop: 8 }}
                                >
                                    <i className="fa-solid fa-triangle-exclamation" />
                                    &nbsp;{infraError}
                                </div>
                            )}

                            <button
                                className="btn btn-primary"
                                disabled={
                                    !selectedInfraId || job?.state === 1
                                }
                                onClick={startInfraUpdate}
                            >
                                Importieren
                            </button>

                            <button className="btn btn-ghost">
                                Abbruch
                            </button>
                        </div>

                        <div className="bs-bottom-right">
                            <span
                                className={`status-badge ${badge.variant}`}
                            >
                                {badge.label}
                            </span>
                        </div>
                    </div>

                    <div className="bs-progress">
                        <div className="bs-progress-label">
                            {job?.message ?? "Bereit"}
                        </div>

                        <div className="bs-progress-bar">
                            <div
                                className="bs-progress-fill"
                                style={{
                                    width: `${job?.progress ?? 0}%`
                                }}
                            />
                        </div>

                        <div className="bs-progress-percent">
                            {job?.progress ?? 0}%
                        </div>
                    </div>
                </section>

            </section>
        </div>
    );
}

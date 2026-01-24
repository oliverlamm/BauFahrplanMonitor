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
import type { BetriebsstelleDetail } from "../../models/betriebsstelle";

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
 * Page
 * ============================================================ */
export default function BetriebsstellenPage() {
    const {
        list,
        detail,
        lookups,
        loadList,
        loadDetail,
        loading,
        error,
        saveDetail
    } = useBetriebsstellen();

    /* lokaler Edit-State */
    const [local, setLocal] = useState<BetriebsstelleDetail | null>(null);

    /* Dirty-Tracking */
    const dirty = useMemo(() => {
        if (!detail || !local) return false;
        return JSON.stringify(detail) !== JSON.stringify(local);
    }, [detail, local]);

    /* Save-Feedback */
    const [saveState, setSaveState] =
        useState<"idle" | "saving" | "saved">("idle");

    /* Geo-Auswahl */
    const [selectedGeoIndex, setSelectedGeoIndex] =
        useState<number | null>(null);

    /* Detail → local kopieren */
    useEffect(() => {
        if (detail) {
            setLocal(structuredClone(detail));
            setSelectedGeoIndex(
                detail.geo.length > 0 ? 0 : null
            );
            setSaveState("idle");
        } else {
            setLocal(null);
            setSelectedGeoIndex(null);
        }
    }, [detail]);

    /* ================= RENDER ================= */
    return (
        <div className="importer-page">
            <section className="importer-card bs-page">

                {/* ================= Header ================= */}
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
                                    if (id > 0) loadDetail(id);
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

                        <div className="rl100-filter">
                            <label>
                                <input
                                    type="radio"
                                    name="basis"
                                    defaultChecked
                                    onChange={() => loadList()}
                                />
                                alle
                            </label>
                            <label>
                                <input
                                    type="radio"
                                    name="basis"
                                    onChange={() => loadList("only")}
                                />
                                nur Basisdatensätze
                            </label>
                            <label>
                                <input
                                    type="radio"
                                    name="basis"
                                    onChange={() => loadList("without")}
                                />
                                ohne Basisdatensätze
                            </label>
                        </div>
                    </div>
                </section>

                {/* ================= Fehler ================= */}
                {error && (
                    <div className="importer-error">
                        <i className="fa-solid fa-triangle-exclamation" />
                        &nbsp;{error}
                    </div>
                )}

                {/* ================= Kein Datensatz ================= */}
                {!local && (
                    <div style={{ padding: "24px", color: "#6b7280" }}>
                        Bitte eine Betriebsstelle auswählen
                    </div>
                )}

                {/* ================= Inhalt ================= */}
                {local && (
                    <section className="bs-grid">

                        {/* ================= Stammdaten ================= */}
                        <div className="bs-card">
                            <table className="bs-form">
                                <tbody>
                                <tr>
                                    <td>RL100</td>
                                    <td>
                                        <input
                                            value={local.rl100}
                                            disabled
                                        />
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

                                <tr>
                                    <td>Zustand</td>
                                    <td>
                                        <select
                                            className={
                                                local.zustand !==
                                                detail?.zustand
                                                    ? "dirty"
                                                    : ""
                                            }
                                            value={local.zustand}
                                            onChange={e =>
                                                setLocal({
                                                    ...local,
                                                    zustand: e.target.value
                                                })
                                            }
                                        >
                                            {lookups.zustaende.map(z => (
                                                <option key={z} value={z}>
                                                    {z}
                                                </option>
                                            ))}
                                        </select>
                                    </td>
                                </tr>

                                <tr>
                                    <td>Typ</td>
                                    <td>
                                        <select
                                            className={
                                                local.typ !== detail?.typ
                                                    ? "dirty"
                                                    : ""
                                            }
                                            value={local.typ}
                                            onChange={e =>
                                                setLocal({
                                                    ...local,
                                                    typ: e.target.value
                                                })
                                            }
                                        >
                                            {lookups.typen.map(t => (
                                                <option key={t.id} value={t.name}>
                                                    {t.name}
                                                </option>
                                            ))}
                                        </select>
                                    </td>
                                </tr>

                                <tr>
                                    <td>Netzbezirk</td>
                                    <td>
                                        <select
                                            className={
                                                local.netzbezirk !==
                                                detail?.netzbezirk
                                                    ? "dirty"
                                                    : ""
                                            }
                                            value={local.netzbezirk}
                                            onChange={e =>
                                                setLocal({
                                                    ...local,
                                                    netzbezirk: e.target.value
                                                })
                                            }
                                        >
                                            {lookups.netzbezirke.map(n => (
                                                <option key={n.id} value={n.name}>
                                                    {n.name}
                                                </option>
                                            ))}
                                        </select>
                                    </td>
                                </tr>

                                <tr>
                                    <td>Region</td>
                                    <td>
                                        <select
                                            className={
                                                local.region !== detail?.region
                                                    ? "dirty"
                                                    : ""
                                            }
                                            value={local.region}
                                            onChange={e =>
                                                setLocal({
                                                    ...local,
                                                    region: e.target.value
                                                })
                                            }
                                        >
                                            {lookups.regionen.map(r => (
                                                <option key={r.id} value={r.name}>
                                                    {r.name}
                                                </option>
                                            ))}
                                        </select>
                                    </td>
                                </tr>

                                <tr>
                                    <td>Basisdatensatz</td>
                                    <td>
                                        <input
                                            type="checkbox"
                                            checked={local.istBasis}
                                            readOnly
                                        />
                                    </td>
                                </tr>
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
                                {saveState === "saving" && (
                                    <>
                                        <i className="fa-solid fa-spinner fa-spin" />
                                        Speichern…
                                    </>
                                )}

                                {saveState === "saved" && (
                                    <>
                                        <i className="fa-solid fa-check" />
                                        Gespeichert
                                    </>
                                )}

                                {saveState === "idle" && (
                                    <>
                                        <i className="fa-solid fa-floppy-disk" />
                                        Speichern
                                    </>
                                )}
                            </button>


                            {saveState === "saved" && (
                                <div className="save-ok">
                                    ✔ Änderungen gespeichert
                                </div>
                            )}
                        </div>

                        {/* ================= Karte + Geo ================= */}
                        <div className="bs-card">
                            <div className="bs-map">
                                {selectedGeoIndex !== null && (
                                    (() => {
                                        const g = local.geo[selectedGeoIndex];
                                        return (
                                            <MapContainer
                                                center={[g.lat, g.lon]}
                                                zoom={14}
                                                style={{
                                                    height: "100%",
                                                    width: "100%"
                                                }}
                                            >
                                                <TileLayer
                                                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                                                />
                                                <RecenterMap
                                                    lat={g.lat}
                                                    lon={g.lon}
                                                />
                                                <Marker
                                                    position={[g.lat, g.lon]}
                                                >
                                                    <Popup>
                                                        VzG {g.vzGNr}
                                                    </Popup>
                                                </Marker>
                                            </MapContainer>
                                        );
                                    })()
                                )}
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
                                                checked={
                                                    selectedGeoIndex === i
                                                }
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
            </section>
        </div>
    );
}

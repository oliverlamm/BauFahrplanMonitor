import { useState } from "react";
import "./ZugsichtPage.css";
import "../../styles/importer-threads.css";
import TimeWegDiagramm from "./TimeWegDiagramm";

import { loadZugTimeline } from "../../api/zugTimelineApi";
import type { ZugTimelineResult } from "../../models/ZugTimelineDto";

import { loadZugMassnahmen } from "../../api/zugMassnahmenApi";
import type { ZwlMassnahmeOverlayDto } from "../../models/ZwlMassnahmeOverlayDto";

export default function ZugsichtPage() {
    const [zugNr, setZugNr] = useState("525");
    const [date, setDate]   = useState("2026-02-07");

    const [loading, setLoading] = useState(false);
    const [error, setError]     = useState<string | null>(null);
    const [data, setData]       = useState<ZugTimelineResult | null>(null);

    const [overlays, setOverlays] =
        useState<ZwlMassnahmeOverlayDto[]>([]);


    async function onShow() {
        setLoading(true);
        setError(null);
        setData(null);
        setOverlays([]);

        try {
            const jahr = new Date(date).getFullYear();

            const [timelineResult, massnahmenResult] =
                await Promise.all([
                    loadZugTimeline(zugNr, date),
                    loadZugMassnahmen(jahr, zugNr, date)
                ]);

            setData(timelineResult);
            setOverlays(massnahmenResult);
        }
        catch (e: any) {
            setError(e?.message ?? "Zugsicht konnte nicht geladen werden");
        }
        finally {
            setLoading(false);
        }
    }

    return (
        <div className="page zugsicht-page">

            {/* =========================
             Zugsicht Konfiguration
             ========================= */}
            <section className="card">
                <h3>Zugsicht Konfiguration</h3>

                <div className="form-row">
                    <div className="form-field">
                        <label>Zugnummer:</label>
                        <input
                            type="text"
                            inputMode="numeric"
                            pattern="[0-9]*"
                            value={zugNr}
                            onChange={e => {
                                const v = e.target.value;
                                if (/^\d*$/.test(v)) setZugNr(v);
                            }}
                        />
                    </div>

                    <div className="form-field">
                        <label>Verkehrstag:</label>
                        <input
                            type="date"
                            value={date}
                            onChange={e => setDate(e.target.value)}
                        />
                    </div>

                    <div className="form-field form-field-button">
                        <button
                            className="btn btn-primary"
                            onClick={onShow}
                            disabled={loading || !zugNr}
                        >
                            {loading ? "Lade…" : "Anzeigen"}
                        </button>
                    </div>
                </div>

                {error && (
                    <div className="error-box">
                        {error}
                    </div>
                )}
            </section>

            {/* =========================
             Zeit-Wege-Diagramm
             ========================= */}
            <section className="card">
                <h3>Zeit-Wege-Diagramm</h3>

                {!data && !loading && (
                    <div className="diagram-placeholder">
                        Keine Daten geladen
                    </div>
                )}

                {loading && (
                    <div className="diagram-placeholder">
                        Daten werden geladen…
                    </div>
                )}

                {data && (
                    <TimeWegDiagramm
                        data={data}
                        overlays={overlays}
                    />
                )}

                <div className="diagram-hint">
                    Hinweis: Das Diagramm zeigt die Zeit auf der vertikalen Achse
                    (absteigend) und die Betriebsstellen auf der horizontalen Achse (DS100).
                </div>
            </section>

        </div>
    );
}

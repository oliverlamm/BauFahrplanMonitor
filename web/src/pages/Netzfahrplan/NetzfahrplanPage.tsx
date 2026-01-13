import {useEffect, useState} from "react";
import "./NetzfahrplanPage.css";
import "../../styles/importer-threads.css";

import {
    getNetzfahrplanStatus,
    scanNetzfahrplan,
    startNetzfahrplan,
    cancelNetzfahrplan
} from "../../api/netzfahrplanApi";

import type {NetzfahrplanStatus} from "../../models/nfpl-status.ts";
import {ImporterStatusBadge} from "../../components/importer/ImporterStatusBadge.tsx";
import {mapWorkerState} from "../../utils/importThreadStatusMapper.ts";

function uiStateFromJob(state: string): "neutral" | "running" | "ok" | "warning" | "error" {
    switch (state) {
        case "Running":
        case "Scanning":
            return "running";

        case "Finished":
            return "ok";

        case "Error":
        case "Cancelled":
            return "error";

        default:
            return "neutral"; // Idle
    }
}

function headerStatusText(state: string): string {
    switch (state) {
        case "Idle":
            return "Bereit";
        case "Scanning":
            return "Scan läuft";
        case "Running":
            return "Import läuft";
        case "Finished":
            return "Abgeschlossen";
        case "Cancelled":
            return "Abgebrochen";
        case "Error":
            return "Fehler";
        default:
            return state;
    }
}


function progressPercent(done: number, total: number): number {
    if (!total || total <= 0) return 0;
    return Math.min(100, Math.round((done / total) * 100));
}
export default function NetzfahrplanPage() {

    const [status, setStatus] = useState<NetzfahrplanStatus | null>(null);

    /* Polling */
    useEffect(() => {
        const load = async () => {
            try {
                setStatus(await getNetzfahrplanStatus());
            } catch {
                // optional: Fehlerstatus
            }
        };

        load();
        const t = setInterval(load, 2000);
        return () => clearInterval(t);
    }, []);

    if (!status) {
        return <div className="importer-page">Lade Status…</div>;
    }

    const percent = progressPercent(
        status.processedFiles,
        status.totalFiles
    );

    const canStart =
        status.state === "Idle" || status.state === "Finished";

    const canCancel =
        status.state === "Running" || status.state === "Scanning";

    return (
        <div className="importer-page">

            <section className={`importer-card ${uiStateFromJob(status.state)}`}>

                {/* Header */}
                <section className="importer-header">

                    <div className="importer-header-left">
                        <h2>Netzfahrplan</h2>
                        <div className="importer-subtitle">
                            Import des Jahresfahrplans
                        </div>
                    </div>

                    <ImporterStatusBadge status={status.state} text={headerStatusText(status.state)}/>

                </section>


                {/* Controls */}
                <section className="nfpl-controls">

                    <div className="nfpl-row">
                        <div className="nfpl-row-left">
                            <label>Verzeichnis</label>
                            <input
                                type="text"
                                value="DBNetz / Netzfahrplan"
                                readOnly
                            />
                        </div>

                        <div className="nfpl-row-right">
                            <button
                                className="btn"
                                onClick={scanNetzfahrplan}
                            >
                                <i className="fa-solid fa-magnifying-glass"/> Scannen
                            </button>
                        </div>
                    </div>

                    <div className="nfpl-row">
                        <div className="nfpl-row-left"/>

                        <div className="nfpl-row-right">
                            <button
                                className="btn btn-primary"
                                disabled={!canStart}
                                onClick={startNetzfahrplan}
                            >
                                <i className="fa-solid fa-play"/> Start
                            </button>

                            <button
                                className="btn btn-secondary"
                                disabled={!canCancel}
                                onClick={cancelNetzfahrplan}
                            >
                                <i className="fa-solid fa-xmark"/> Stopp
                            </button>
                        </div>
                    </div>

                </section>

                {/* Gesamtfortschritt */}
                <section className="progress-block">
                    <div className="progress-label">
                        <span>Gesamtfortschritt</span>
                        <span>{percent}%</span>
                    </div>
                    <div className="progress-bar">
                        <div
                            className="progress-value"
                            style={{width: `${percent}%`}}
                        />
                    </div>
                </section>

                {/* Threads */}
                <section className="netzfahrplan-threads">
                    <h3>Import-Threads ({status.workers.length})</h3>

                    <div className="thread-grid">
                        {status.workers.map(worker => {
                            const { cls, text } = mapWorkerState(worker.state);

                            return (
                                <div key={worker.workerId} className="thread-card">

                                    <div className="thread-header">
                                        <span>Import Thread {worker.workerId}</span>

                                        <ImporterStatusBadge
                                            status={cls}
                                            text={text}
                                        />
                                    </div>

                                    <div className="thread-file">
                                        Datei:&nbsp;
                                        {worker.currentFile
                                            ? worker.currentFile.split("/").pop()
                                            : "—"}
                                    </div>

                                    <div className="thread-status">
                                        Status: {worker.progressMessage}
                                    </div>

                                    <div className="thread-count">
                                        {worker.errors > 0 && (
                                            <> — ⚠ {worker.errors} Fehler</>
                                        )}
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </section>
            </section>
        </div>
    );
}

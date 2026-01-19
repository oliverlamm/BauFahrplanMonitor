import "./NetzfahrplanPage.css";
import "../../styles/importer-threads.css";

import {ImporterStatusBadge} from "../../components/importer/ImporterStatusBadge";
import {mapWorkerState} from "../../utils/importThreadStatusMapper";
import {useNetzfahrplan} from "../../hooks/useNetzfahrplan";
import type { ImportJobState } from "../../models/importer-status.ts";
import {mapImporterStatus} from "../../utils/importerStatusMapper.ts";

/* ------------------------------------------------------------
 * Status → Card-Style
 * ------------------------------------------------------------ */
function uiStateFromJob(
    state: string
): "neutral" | "running" | "ok" | "warning" | "error" {
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
            return "neutral";
    }
}

/* ------------------------------------------------------------
 * Progress %
 * ------------------------------------------------------------ */
function progressPercent(done: number, total: number): number {
    if (!total || total <= 0) return 0;
    return Math.min(100, Math.round((done / total) * 100));
}

/* ============================================================
 * PAGE
 * ============================================================ */
export default function NetzfahrplanPage() {
    const {
        status,
        loading,
        error,
        scan,
        startImport,
        cancelImport
    } = useNetzfahrplan();

    if (!status) {
        return <div className="importer-page">Lade Status…</div>;
    }

    const percent = (() => {
        switch (status.state) {
            case "Scanning":
                // Scan läuft → wir kennen die Gesamtzahl noch nicht
                return 0;

            case "Scanned":
                // Scan abgeschlossen
                return 100;

            case "Running":
            case "Finished":
            case "FinishedWithErrors":
                return progressPercent(
                    status.processedFiles,
                    status.totalFiles
                );

            default:
                return 0;
        }
    })();


    /* ------------------------------------------------------------
     * Button-Logik (fachlich korrekt)
     * ------------------------------------------------------------ */
    const canStart =
        (status.state === "Scanned" || status.state === "Finished") &&
        status.totalFiles > 0 &&
        !loading;

    const canCancel =
        status.state === "Running" ||
        status.state === "Scanning";

    const badge = mapImporterStatus(status.state as ImportJobState);

    return (
        <div className="importer-page">
            <section className={`importer-card ${uiStateFromJob(status.state)}`}>

                {/* ================= Header ================= */}
                <section className="importer-header">
                    <div className="importer-header-left">
                        <h2>Netzfahrplan</h2>
                        <div className="importer-subtitle">
                            Import des Jahresfahrplans
                        </div>
                    </div>

                    <ImporterStatusBadge
                        status={badge.cls}
                        text={badge.text}
                    />
                </section>

                {/* ================= Controls ================= */}
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

                        <button
                            className="btn"
                            onClick={scan}
                            disabled={loading}
                        >
                            <i className="fa-solid fa-magnifying-glass"/>
                            &nbsp;Scannen
                        </button>
                    </div>

                    <div className="nfpl-row">
                        <div className="nfpl-row-left"/>

                        <div className="nfpl-row-right">
                            <button
                                className="btn btn-primary"
                                disabled={!canStart}
                                onClick={startImport}
                            >
                                <i className="fa-solid fa-play"/>
                                &nbsp;Start
                            </button>

                            <button
                                className="btn btn-secondary"
                                disabled={!canCancel}
                                onClick={cancelImport}
                            >
                                <i className="fa-solid fa-xmark"/>
                                &nbsp;Stopp
                            </button>
                        </div>
                    </div>

                    {error && (
                        <div className="importer-error">
                            <i className="fa-solid fa-triangle-exclamation"/>
                            &nbsp;{error}
                        </div>
                    )}
                </section>

                {/* ================= Progress ================= */}
                <section className="progress-block">
                    <div className="progress-label">
                        <span>
                            {status.state === "Scanning" && "Scan-Fortschritt"}
                                                    {status.state === "Scanned" && "Scan abgeschlossen"}
                                                    {(status.state === "Running" ||
                                                            status.state === "Finished" ||
                                                            status.state === "FinishedWithErrors") &&
                                                        "Import-Fortschritt"}
                        </span>
                        <span>{percent}%</span>
                    </div>

                    <div className="progress-bar">
                        <div
                            className="progress-value"
                            style={{width: `${percent}%`}}
                        />
                    </div>

                    <div className="progress-subline">
                        {status.state === "Scanning" && (
                            <>Scan läuft…</>
                        )}

                        {status.state === "Scanned" && (
                            <>
                                {status.totalFiles.toLocaleString()} Dateien gefunden
                            </>
                        )}

                        {(status.state === "Running" ||
                            status.state === "Finished" ||
                            status.state === "FinishedWithErrors") && (
                            <>
                                {status.processedFiles.toLocaleString()}
                                {" / "}
                                {status.totalFiles.toLocaleString()}
                                {" Dateien verarbeitet"}

                                {status.queueCount > 0 && (
                                    <> — {status.queueCount} in Warteschlange</>
                                )}

                                {status.errors > 0 && (
                                    <> — ⚠ {status.errors} Fehler</>
                                )}
                            </>
                        )}
                    </div>
                </section>

                {/* ================= Threads ================= */}
                <section className="netzfahrplan-threads">
                    <h3>Import-Threads ({status.workers.length})</h3>

                    <div className="thread-grid">
                        {status.workers.map(worker => {
                            const {cls, text} = mapWorkerState(worker.state);

                            return (
                                <div
                                    key={worker.workerId}
                                    className="thread-card"
                                >
                                    <div className="thread-header">
                                        <span>
                                            Import Thread {worker.workerId}
                                        </span>

                                        <ImporterStatusBadge
                                            status={cls}
                                            text={text}
                                        />
                                    </div>

                                    <div className="thread-file">
                                        Datei:&nbsp;
                                        {worker.currentFile
                                            ? worker.currentFile
                                                .split("/")
                                                .pop()
                                            : "—"}
                                    </div>

                                    <div className="thread-status">
                                        Status:&nbsp;
                                        {worker.progressMessage
                                            ? worker.progressMessage
                                            : mapWorkerState(worker.state).text}
                                    </div>

                                    {worker.processedItems > 0 && (
                                        <div className="thread-overall-counter">
                                            Dateien: {worker.processedItems}
                                        </div>
                                    )}

                                    {worker.errors > 0 && (
                                        <div className="thread-count">
                                            ⚠ {worker.errors} Fehler
                                        </div>
                                    )}
                                </div>
                            );
                        })}
                    </div>
                </section>
            </section>
        </div>
    );
}

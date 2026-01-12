import {useState} from "react";

import "./ZvFExportPage.css";
import "../../styles/importer-threads.css";

import {useStatus} from "../../hooks/useStatus";
import {useZvFExport} from "../../hooks/useZvFExport";
import {mapImporterStatus} from "../../utils/importerStatusMapper";
import {ImporterStatusBadge} from "../../components/importer/ImporterStatusBadge";

import type {ZvFFileFilter} from "../../api/zvfExportApi";
import {mapWorkerState} from "../../utils/importThreadStatusMapper.ts";

export default function ZvFExportPage() {

    // ✅ HIER ist der Hook erlaubt
    const [filter, setFilter] = useState<ZvFFileFilter>(7); // All

    const {
        status,
        loading,
        error,
        startImport,
        cancelImport,
        scan
    } = useZvFExport();

    const {
        data: cfg,
        loading: cfgLoading,
        error: cfgError
    } = useStatus();

    if (cfgLoading) {
        return <div>Lade Systemstatus…</div>;
    }

    if (cfgError || !cfg) {
        return <div className="error">Fehler: {error}</div>;
    }

    // ---------------------------------
    // Status noch nicht geladen
    // ---------------------------------
    if (!status) {
        return (
            <div className="importer-page">
                <section className="importer-card">
                    <header className="importer-header">
                        <h2>ZvF / ÜB / Fplo</h2>
                        <ImporterStatusBadge
                            status="status-bereit"
                            text="Lade Status…"
                        />
                    </header>
                </section>
            </div>
        );
    }

    // ---------------------------------
    // Progress-Berechnung
    // ---------------------------------

    const showScanProgress =
        status.state === "Starting" ||
        status.state === "Scanning" ||
        status.state === "Scanned";

    const showImportProgress =
        status.state === "Running" ||
        status.state === "Stopping" ||
        status.state === "Finished" ||
        status.state === "FinishedWithErrors";

    const scanPercent =
        status.scanTotalFiles > 0
            ? Math.round(
                (status.scanProcessedFiles / status.scanTotalFiles) * 100
            )
            : 0;

    const importPercent =
        status.importTotalItems > 0
            ? Math.round(
                (status.importProcessedItems / status.importTotalItems) * 100
            )
            : 0;

    // ---------------------------------
    // Fachlicher Status
    // ---------------------------------
    const importerState = status.state;

    const {cls, text} = mapImporterStatus(importerState);
    
    const canStart =
        importerState === "Scanned" &&
        status.scanTotalFiles > 0 &&
        !loading;

    const canCancel =
        importerState === "Starting" ||
        importerState === "Scanning" ||
        importerState === "Running" ||
        importerState === "Stopping";

    return (
        <div className="importer-page">
            <section className="importer-card">

                {/* Header */}
                <header className="importer-header">
                    <div>
                        <h2>ZvF / ÜB / Fplo</h2>
                        <div className="importer-subtitle">
                            Import für Fahrplanprodukte
                        </div>
                    </div>

                    <ImporterStatusBadge status={cls} text={text}/>
                </header>

                {/* Controls */}
                <section className="zvf-controls">

                    {/* Verzeichnis */}
                    <div className="zvf-row">
                        <div className="zvf-row-left">
                            <label>Verzeichnis</label>
                            <input
                                type="text"
                                value={cfg.datei.importpfad}
                                readOnly
                            />
                        </div>

                        <div className="zvf-row-right">
                            <button
                                className="btn"
                                onClick={() => scan(filter)}
                                disabled={loading}
                            >
                                <i className="fa-solid fa-magnifying-glass"/> Scannen
                            </button>
                        </div>
                    </div>

                    {/* Filter + Actions */}
                    <div className="zvf-row zvf-row-inline">

                        {/* Filter links */}
                        <div className="zvf-row-left">
                            <div className="zvf-filter">
                                <span className="zvf-filter-label">Filter</span>

                                <div className="zvf-filter-options">
                                    <label>
                                        <input
                                            type="radio"
                                            name="zvf-filter"
                                            checked={filter === 7}
                                            onChange={() => setFilter(7)}
                                        />
                                        Alle
                                    </label>

                                    <label>
                                        <input
                                            type="radio"
                                            name="zvf-filter"
                                            checked={filter === 1}
                                            onChange={() => setFilter(1)}
                                        />
                                        ZvF
                                    </label>

                                    <label>
                                        <input
                                            type="radio"
                                            name="zvf-filter"
                                            checked={filter === 2}
                                            onChange={() => setFilter(2)}
                                        />
                                        ÜB
                                    </label>

                                    <label>
                                        <input
                                            type="radio"
                                            name="zvf-filter"
                                            checked={filter === 4}
                                            onChange={() => setFilter(4)}
                                        />
                                        Fplo
                                    </label>
                                </div>
                            </div>
                        </div>

                        {/* Actions rechts */}
                        <div className="zvf-row-right">
                            <button
                                className="btn"
                                disabled={!canStart || loading}
                                onClick={() => {
                                    if (!canStart) return;
                                    startImport();
                                }}
                            >
                                <i className="fa-solid fa-play"/> Start
                            </button>

                            <button
                                className="btn"
                                disabled={!canCancel}
                                onClick={cancelImport}
                            >
                                <i className="fa-solid fa-xmark"/> Stopp
                            </button>
                        </div>

                    </div>

                    {error && (
                        <div className="importer-error">
                            <i className="fa-solid fa-triangle-exclamation"/>
                            {error}
                        </div>
                    )}
                </section>

                {/* Gesamtfortschritt (noch statisch) */}
                <section className="progress-block">
                    <div className="progress-label">
                        <span>
                            {showScanProgress
                                ? "Scan-Fortschritt"
                                : "Import-Fortschritt"}
                        </span>

                        <span>
                            {showScanProgress
                                ? `${scanPercent}%`
                                : `${importPercent}%`}
                        </span>
                    </div>

                    <div className="progress-bar">
                        <div
                            className="progress-value"
                            style={{
                                width: showScanProgress
                                    ? `${scanPercent}%`
                                    : `${importPercent}%`
                            }}
                        />
                    </div>

                    <div className="progress-subline">
                        {showScanProgress && (
                            <>
                                {status.scanProcessedFiles.toLocaleString()}
                                {" / "}
                                {status.scanTotalFiles.toLocaleString()}
                                {" Dateien geprüft"}
                            </>
                        )}

                        {showImportProgress && (
                            <>
                                {status.importProcessedItems.toLocaleString()}
                                {" / "}
                                {status.importTotalItems.toLocaleString()}
                                {" Dateien importiert"}

                                {status.importErrorItems > 0 && (
                                    <> — ⚠ {status.importErrorItems} Fehler</>
                                )}
                            </>
                        )}
                    </div>
                </section>
                
                {/* Threads */}
                <section className="zvf-threads">
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

                {/* Statistik */}
                <section className="zvf-statistic">
                    <div className="thread-card statistic-card">

                        {/* Header */}
                        <div className="thread-header">
                            <span>Statistik (Scan)</span>
                        </div>

                        {/* Inhalt */}
                        <div className="stat-grid">

                            {/* ZvF */}
                            <div className="stat-column">
                                <div className="stat-title">ZvF</div>

                                <div className="stat-row">
                                    <span>neu</span>
                                    <span>{status.scanStat.zvF_New}</span>
                                </div>

                                <div className="stat-row muted">
                                    <span>importiert</span>
                                    <span>{status.scanStat.zvF_Imported}</span>
                                </div>
                            </div>

                            {/* ÜB */}
                            <div className="stat-column">
                                <div className="stat-title">ÜB</div>

                                <div className="stat-row">
                                    <span>neu</span>
                                    <span>{status.scanStat.ueB_New}</span>
                                </div>

                                <div className="stat-row muted">
                                    <span>importiert</span>
                                    <span>{status.scanStat.ueB_Imported}</span>
                                </div>
                            </div>

                            {/* Fplo */}
                            <div className="stat-column">
                                <div className="stat-title">Fplo</div>

                                <div className="stat-row">
                                    <span>neu</span>
                                    <span>{status.scanStat.fplo_New}</span>
                                </div>

                                <div className="stat-row muted">
                                    <span>importiert</span>
                                    <span>{status.scanStat.fplo_Imported}</span>
                                </div>
                            </div>

                        </div>
                    </div>
                </section>
                
            </section>
            
        </div>
    );
}

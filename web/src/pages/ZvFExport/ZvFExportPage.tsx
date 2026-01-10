import "./ZvFExportPage.css";
import "../../styles/importer-threads.css";

export default function ZvFExportPage() {
    return (
        <div className="importer-page">

            <section className="importer-card">

                {/* Header */}
                <header className="importer-header">
                    <div>
                        <h2>ZvF / ÜB / Fplo</h2>
                        <div className="importer-subtitle">Import für Fahrplanprodukte</div>
                    </div>

                </header>

                <section className="zvf-controls">

                    {/* Verzeichnis */}
                    <div className="zvf-row">
                        <div className="zvf-row-left">
                            <label>Verzeichnis</label>
                            <input
                                type="text"
                                value="c:/Users/mail/Documents/DBNetz/import"
                                readOnly
                            />
                        </div>

                        <div className="zvf-row-right">
                            <button className="btn">
                                <i className="fa-solid fa-magnifying-glass" /> Scannen
                            </button>
                        </div>
                    </div>

                    {/* Filter + Actions */}
                    <div className="zvf-row">
                        <div className="zvf-row-left zvf-filter">
                            <span>Filter</span>
                            <label><input type="radio" checked /> Alle</label>
                            <label><input type="radio" /> ZvF</label>
                            <label><input type="radio" /> ÜB</label>
                            <label><input type="radio" /> Fplo</label>
                        </div>

                        <div className="zvf-row-right">
                            <button className="btn btn-primary">
                                <i className="fa-solid fa-play" /> Start
                            </button>
                            <button className="btn btn-secondary">
                                <i className="fa-solid fa-xmark" /> Stopp
                            </button>
                        </div>
                    </div>

                </section>

                {/* Gesamtfortschritt */}
                <section className="progress-block">
                    <div className="progress-label">
                        <span>Gesamtfortschritt</span>
                        <span>0%</span>
                    </div>
                    <div className="progress-bar">
                        <div className="progress-value" style={{ width: "0%" }} />
                    </div>
                </section>

                {/* Threads */}
                <section className="zvf-threads">
                    <h3>Import-Threads (3)</h3>

                    <div className="thread-grid">

                        <div className="thread-card">
                            <div className="thread-header">
                                <span>Import Thread</span>
                                <span className="badge">Bereit</span>
                            </div>

                            <div className="thread-file">
                                Datei: zvf_data_2024.xml
                            </div>

                            <div className="thread-status">Warte auf Start</div>
                            <div className="thread-count">0 / 10.000 Datensätze</div>

                            <div className="progress-bar small">
                                <div className="progress-value" style={{ width: "0%" }} />
                            </div>
                        </div>

                        <div className="thread-card">
                            <div className="thread-header">
                                <span>Import Thread</span>
                                <span className="badge">Bereit</span>
                            </div>

                            <div className="thread-file">
                                Datei: ueb_data_2024.xml
                            </div>

                            <div className="thread-status">Warte auf Start</div>
                            <div className="thread-count">0 / 10.000 Datensätze</div>

                            <div className="progress-bar small">
                                <div className="progress-value" style={{ width: "0%" }} />
                            </div>
                        </div>

                        <div className="thread-card">
                            <div className="thread-header">
                                <span>Import Thread</span>
                                <span className="badge">Bereit</span>
                            </div>

                            <div className="thread-file">
                                Datei: fplo_data_2024.xml
                            </div>

                            <div className="thread-status">Warte auf Start</div>
                            <div className="thread-count">0 / 10.000 Datensätze</div>

                            <div className="progress-bar small">
                                <div className="progress-value" style={{ width: "0%" }} />
                            </div>
                        </div>

                    </div>
                </section>

            </section>
        </div>
    );
}

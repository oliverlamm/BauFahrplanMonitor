import "./BbpNeoPage.css";

export default function BbpNeoPage() {
    return (
        <div className="importer-page">

            <section className="importer-card">

                {/* Header */}
                <header className="importer-header">
                    <div>
                        <h2>BBPNeo</h2>
                        <div className="importer-subtitle">
                            Import für Maßnahmen aus BBPNeo
                        </div>
                    </div>
                </header>
                
                <section className="bbp-controls">

                    {/* Verzeichnis */}
                    <div className="bbp-row">
                        <div className="bbp-row-left">
                            <label>Datei</label>
                            <input
                                type="text"
                                value="c:/Users/mail/Documents/DBNetz/import/BBP.xml"
                                readOnly
                            />
                        </div>

                        <div className="bbp-row-right">
                            <button className="btn">
                                <i className="fa fa-folder-open" /> Öffnen
                            </button>
                        </div>
                    </div>

                    {/* Filter + Actions */}
                    <div className="bbp-row">
                        <div className="bbp-row-left">
                            
                        </div>

                        <div className="bbp-row-right">
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

                {/* Details */}
                <section className="bbp-details">

                    {/* Thread */}
                    <div className="bbpneo-thread">
                        <div className="thread-header">
                            <span>Import Thread</span>
                            <span className="badge status-ready">Bereit</span>
                        </div>

                        <div className="thread-status">Warte auf Start</div>
                        <div className="thread-count">0 / 50.000 Datensätze</div>
                        <div className="thread-meta">&nbsp;</div>
                        <div className="stat-row">
                            <span>Queue-Größe</span>
                            <span>0</span>
                        </div>
                        <div className="stat-row">
                            <span>Consumer aktiv</span>
                            <span>0</span>
                        </div>
                        

                    </div>

                    {/* Statistik */}
                    <div className="bbpneo-stats">
                        <h4>Statistik</h4>

                        <div className="stat-row">
                            <span>Anzahl Maßnahmen</span>
                            <span>0</span>
                        </div>
                        <div className="stat-row">
                            <span>Anzahl Regelungen</span>
                            <span>0</span>
                        </div>
                        <div className="stat-row">
                            <span>Anzahl BvE</span>
                            <span>0</span>
                        </div>
                        <div className="stat-row">
                            <span>Anzahl APS</span>
                            <span>0</span>
                        </div>
                        <div className="stat-row">
                            <span>Anzahl IAV</span>
                            <span>0</span>
                        </div>
                    </div>
                </section>

            </section>
        </div>
    );
}

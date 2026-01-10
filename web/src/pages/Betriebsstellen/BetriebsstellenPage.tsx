import "./BetriebsstellenPage.css";

export default function BetriebsstellenPage() {
    return (
        <div className="importer-page">
            <section className="importer-card bs-page">

                {/* Header */}
                <header className="bs-header">
                    <h2>Betriebsstellenverwaltung</h2>
                    <div className="importer-subtitle">
                        Verwaltung und Bearbeitung von Betriebsstellen
                    </div>
                </header>

                {/* Filter */}
                <section className="bs-filter">
                    <div className="rl100-row">
                        <select className="rl100-select">
                            <option>RL100 Code auswählen</option>
                        </select>

                        <div className="rl100-filter">
                            <label><input type="radio" /> alle</label>
                            <label><input type="radio" /> nur Basisdatensätze</label>
                            <label><input type="radio" /> ohne Basisdatensätze</label>
                        </div>
                    </div>

                </section>

                {/* Hauptlayout */}
                <section className="bs-grid">

                    {/* Stammdaten */}
                    <div className="bs-card">
                        <table className="bs-form">
                            <tbody>
                            <tr>
                                <td>RL100</td>
                                <td><input value="KA" disabled/></td>
                            </tr>
                            <tr>
                                <td>Name</td>
                                <td><input value="Karlsruhe Hbf"/></td>
                            </tr>
                            <tr>
                                <td>Zustand</td>
                                <td><input value="Aktiv"/></td>
                            </tr>
                            <tr>
                                <td>Typ</td>
                                <td><select>
                                    <option>Bahnhof</option>
                                </select></td>
                            </tr>
                            <tr>
                                <td>Netzbezirk</td>
                                <td><select>
                                    <option>Stuttgart</option>
                                </select></td>
                            </tr>
                            <tr>
                                <td>Region</td>
                                <td><select>
                                    <option>Süd</option>
                                </select></td>
                            </tr>
                            <tr>
                                <td>Basisdatensatz</td>
                                <td><input type="checkbox" checked/></td>
                            </tr>
                            </tbody>
                        </table>

                        <button className="btn btn-primary">
                            <i className="fa-solid fa-floppy-disk" />
                            Speichern
                        </button>
                    </div>

                    {/* Karte + Geodaten */}
                    <div className="bs-card">
                        <div className="bs-map">
                            {/* Leaflet Map */}
                        </div>

                        <table className="bs-geo">
                            <thead>
                            <tr>
                                <th>Aktiv</th>
                                <th>VzG</th>
                                <th>Longitude</th>
                                <th>Latitude</th>
                            </tr>
                            </thead>
                            <tbody>
                            {/* Rows */}
                            </tbody>
                        </table>

                        <button className="btn btn-primary">
                            <i className="fa-solid fa-location-dot" />
                            Geodaten speichern
                        </button>
                    </div>

                </section>
            </section>
        </div>
    );
}
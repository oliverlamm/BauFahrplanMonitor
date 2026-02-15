import { useMemo, useState } from "react";
import type { ZugTimelineResult } from "../../models/ZugTimelineDto";
import type { ZwlMassnahmeOverlayDto } from "../../models/ZwlMassnahmeOverlayDto";
import "./TimeWegDiagramm.css";

/* ============================================================
   Layout
   ============================================================ */

const PADDING_LEFT = 70;
const PADDING_RIGHT = 20;
const PADDING_TOP = 40;
const PADDING_BOTTOM = 30;

const PIXELS_PER_MINUTE = 2.5;
const X_STEP = 40;

/* ============================================================
   Tooltip Types
   ============================================================ */

type TooltipData =
    | {
    kind: "station";
    x: number;
    y: number;
    rl100: string;
    name: string;
    arrival?: string | null;
    departure?: string | null;
    dwellMin: number;
}
    | {
    kind: "massnahme";
    x: number;
    y: number;
    von: string;
    bis: string;
    beginn: string;
    ende: string;
    regelungen: string;
    vzg: string;
    durchgehend: boolean;
    color: string;
    zeitraum?: string;
};

export default function TimeWegDiagramm({
                                            data,
                                            overlays = []
                                        }: {
    data: ZugTimelineResult;
    overlays?: ZwlMassnahmeOverlayDto[];
}) {
    const points = useMemo(() => {
        const seen = new Set<string>();
        return data.timeline.filter(p => {
            if (seen.has(p.rl100)) return false;
            seen.add(p.rl100);
            return true;
        });
    }, [data.timeline]);
    const [tooltip, setTooltip] = useState<TooltipData | null>(null);

    if (!points.length)
        return <div className="diagram-placeholder">Keine Daten</div>;

    /* ------------------------------------------------------------
       RL100 → Index Mapping
       ------------------------------------------------------------ */

    const rl100IndexMap = useMemo(() => {
        const map = new Map<string, number>();
        points.forEach((p, i) => map.set(p.rl100, i));
        return map;
    }, [points]);

    /* ------------------------------------------------------------
       Zeitbereich
       ------------------------------------------------------------ */

    const minMinute = Math.min(
        ...points.map(p => Math.min(p.arrivalMinute, p.departureMinute))
    );

    const maxMinute = Math.max(
        ...points.map(p => Math.max(p.arrivalMinute, p.departureMinute))
    );

    function yFromMinute(min: number): number {
        return PADDING_TOP + (min - minMinute) * PIXELS_PER_MINUTE;
    }

    function xFromIndex(i: number): number {
        return PADDING_LEFT + i * X_STEP;
    }

    const svgHeight = yFromMinute(maxMinute) + PADDING_BOTTOM;
    const svgWidth =
        PADDING_LEFT + points.length * X_STEP + PADDING_RIGHT;

    /* ------------------------------------------------------------
       Zeit-Gitter
       ------------------------------------------------------------ */

    const gridMinutes: number[] = [];
    const start = Math.floor(minMinute / 30) * 30;
    const end = Math.ceil(maxMinute / 30) * 30;

    for (let m = start; m <= end; m += 30)
        gridMinutes.push(m);

    function formatMinute(min: number): string {
        const day = Math.floor(min / 1440);
        const rest = min % 1440;
        const h = Math.floor(rest / 60);
        const m = rest % 60;

        return `${h.toString().padStart(2, "0")}:${m
            .toString()
            .padStart(2, "0")}${day > 0 ? ` (+${day})` : ""}`;
    }

    /* ------------------------------------------------------------
       Farb-Logik
       ------------------------------------------------------------ */

    function colorForRegelung(regelung: string): string {
        const r = regelung.toLowerCase();

        if (r.includes("tsp") || r.includes("strsp"))
            return "rgba(220,0,0,0.45)";

        if (r.includes("esp") || r.includes("gwb"))
            return "rgba(255,200,0,0.45)";

        return "rgba(150,150,150,0.35)";
    }

    function hideTooltip() {
        setTooltip(null);
    }

    /* ------------------------------------------------------------
       Fahrlinie
       ------------------------------------------------------------ */

    const polylinePoints: string[] = [];
    const x0 = xFromIndex(0);

    polylinePoints.push(`${x0},${yFromMinute(points[0].arrivalMinute)}`);

    if (points[0].arrivalMinute !== points[0].departureMinute)
        polylinePoints.push(`${x0},${yFromMinute(points[0].departureMinute)}`);

    for (let i = 1; i < points.length; i++) {
        const p = points[i];
        const x = xFromIndex(i);

        polylinePoints.push(`${x},${yFromMinute(p.arrivalMinute)}`);

        if (p.arrivalMinute !== p.departureMinute)
            polylinePoints.push(`${x},${yFromMinute(p.departureMinute)}`);
    }

    /* ============================================================
       Render
       ============================================================ */

    return (
        <div className="diagram-scroll">
            <svg width={svgWidth} height={svgHeight}>

                {/* Zeit-Gitter */}
                {gridMinutes.map(m => (
                    <g key={m}>
                        <line
                            x1={PADDING_LEFT}
                            x2={svgWidth - PADDING_RIGHT}
                            y1={yFromMinute(m)}
                            y2={yFromMinute(m)}
                            stroke="#eee"
                        />
                        <text
                            x={PADDING_LEFT - 8}
                            y={yFromMinute(m) + 4}
                            textAnchor="end"
                            fontSize="11"
                            fill="#666"
                        >
                            {formatMinute(m)}
                        </text>
                    </g>
                ))}

                {/* Hover-Zonen (unten) */}
                {points.map((p, i) => {
                    const x = xFromIndex(i) - X_STEP / 2;

                    return (
                        <rect
                            key={`hover-${i}`}
                            x={x}
                            y={PADDING_TOP}
                            width={X_STEP}
                            height={svgHeight - PADDING_TOP - PADDING_BOTTOM}
                            fill="transparent"
                            onMouseEnter={(e) =>
                                setTooltip({
                                    kind: "station",
                                    x: e.clientX,
                                    y: e.clientY,
                                    rl100: p.rl100,
                                    name: p.name,
                                    arrival: p.arrival,
                                    departure: p.departure,
                                    dwellMin: Math.max(
                                        0,
                                        p.departureMinute - p.arrivalMinute
                                    )
                                })
                            }
                            onMouseLeave={hideTooltip}
                        />
                    );
                })}

                {/* Baumaßnahmen (über Hover-Zonen!) */}
                {overlays.map((o, idx) => {
                    const startIndex = rl100IndexMap.get(o.vonRl100);
                    const endIndex = rl100IndexMap.get(o.bisRl100);
                    if (startIndex === undefined || endIndex === undefined)
                        return null;

                    const x1 = xFromIndex(Math.min(startIndex, endIndex));
                    const x2 = xFromIndex(Math.max(startIndex, endIndex));

                    const selectedDate = new Date(data.date);
                    selectedDate.setHours(0, 0, 0, 0);

                    const begin = new Date(o.massnahmeBeginn);
                    const end = new Date(o.massnahmeEnde);
                    begin.setHours(0, 0, 0, 0);
                    end.setHours(0, 0, 0, 0);

                    if (selectedDate < begin || selectedDate > end)
                        return null;

                    const y1 = yFromMinute(minMinute);
                    const y2 = yFromMinute(maxMinute);
                    const color = colorForRegelung(o.regelungen);

                    return (
                        <rect
                            key={`overlay-${idx}`}
                            x={x1 - X_STEP / 2}
                            y={y1}
                            width={(x2 - x1) + X_STEP}
                            height={y2 - y1}
                            fill={color}
                            style={{ cursor: "pointer" }}
                            onMouseEnter={(e) =>
                                setTooltip({
                                    kind: "massnahme",
                                    x: e.clientX,
                                    y: e.clientY,
                                    von: o.vonRl100,
                                    bis: o.bisRl100,
                                    beginn: o.massnahmeBeginn,
                                    ende: o.massnahmeEnde,
                                    regelungen: o.regelungen,
                                    vzg: o.vzgListe,
                                    durchgehend: o.durchgehend,
                                    zeitraum: o.zeitraum,
                                    color
                                })
                            }
                            onMouseLeave={hideTooltip}
                        />
                    );
                })}

                {/* Fahrlinie */}
                <polyline
                    points={polylinePoints.join(" ")}
                    fill="none"
                    stroke="#1e88e5"
                    strokeWidth={2}
                />

                {/* RL100 Labels */}
                {points.map((p, i) => (
                    <text
                        key={`lbl-${i}`}
                        x={xFromIndex(i)}
                        y={svgHeight - 8}
                        textAnchor="middle"
                        fontSize="11"
                        fill="#444"
                    >
                        {p.rl100}
                    </text>
                ))}
            </svg>

            {/* Tooltip */}
            {tooltip && (
                <div
                    className="diagram-tooltip"
                    style={{
                        left: tooltip.x + 12,
                        top: tooltip.y + 12
                    }}
                >
                    {tooltip.kind === "station" && (
                        <>
                            <strong>{tooltip.rl100}</strong>
                            <div>{tooltip.name}</div>
                            {tooltip.arrival && <div>Ank: {tooltip.arrival}</div>}
                            {tooltip.departure && <div>Abf: {tooltip.departure}</div>}
                            <div>Aufenthalt: {tooltip.dwellMin} min</div>
                        </>
                    )}

                    {tooltip.kind === "massnahme" && (
                        <>
                            <div style={{ fontWeight: 600, marginBottom: 6 }}>
                                Baumaßnahme
                            </div>

                            <div style={{ fontWeight: 600 }}>
                                {tooltip.von} → {tooltip.bis}
                            </div>

                            {/* Zeitraum-Text aus DB */}
                            {tooltip.zeitraum && (
                                <div style={{
                                    fontSize: 12,
                                    marginTop: 4,
                                    marginBottom: 4,
                                }}>
                                    <strong>Zeitraum:</strong> {tooltip.zeitraum}
                                </div>
                            )}

                            {/* Regelung */}
                            <div>
                                <strong>Regelung:</strong> {tooltip.regelungen}
                            </div>

                            {tooltip.vzg && (
                                <div>
                                    <strong>VZG:</strong> {tooltip.vzg}
                                </div>
                            )}

                            <div>
                                <strong>Durchgehend:</strong>{" "}
                                {tooltip.durchgehend ? "Ja" : "Nein"}
                            </div>
                        </>
                    )}
                </div>
            )}
        </div>
    );
}

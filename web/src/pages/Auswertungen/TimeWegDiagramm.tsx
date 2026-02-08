import {useState} from "react";
import type {ZugTimelineResult} from "../../models/ZugTimelineDto";
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
const X_AXIS_TOP_Y = 12;

/* ============================================================
   Tooltip
   ============================================================ */

type TooltipData = {
    x: number;
    y: number;
    rl100: string;
    name: string;
    arrival?: string | null;
    departure?: string | null;
    dwellMin: number;
};

/* ============================================================
   Komponente
   ============================================================ */

export default function TimeWegDiagramm({
                                            data
                                        }: {
    data: ZugTimelineResult;
}) {
    const points = data.timeline;
    const [tooltip, setTooltip] = useState<TooltipData | null>(null);

    if (!points.length)
        return <div className="diagram-placeholder">Keine Daten</div>;

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

    const svgHeight =
        yFromMinute(maxMinute) + PADDING_BOTTOM;

    const svgWidth =
        PADDING_LEFT +
        points.length * X_STEP +
        PADDING_RIGHT;

    /* ------------------------------------------------------------
       Zeit-Gitter (volle & halbe Stunden)
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
   Fahrlinie (richtig: Fahrtsegmente = Dep(i) -> Arr(i+1))
   plus Aufenthalte = Arr -> Dep am gleichen X
   ------------------------------------------------------------ */

    const polylinePoints: string[] = [];

    const x0 = xFromIndex(0);

    // Startpunkt: Arrival (und ggf. Aufenthalt bis Departure)
    polylinePoints.push(`${x0},${yFromMinute(points[0].arrivalMinute)}`);
    if (points[0].arrivalMinute !== points[0].departureMinute) {
        polylinePoints.push(`${x0},${yFromMinute(points[0].departureMinute)}`);
    }

    // Danach für jede nächste Station:
    // 1) diagonal zur Ankunft (Arrival)
    // 2) falls Halt: vertikal zur Abfahrt (Departure)
    for (let i = 1; i < points.length; i++) {
        const p = points[i];
        const x = xFromIndex(i);

        // Fahrtsegment: von vorheriger Departure (letzter Polyline-Punkt)
        // zur Ankunft der nächsten Station
        polylinePoints.push(`${x},${yFromMinute(p.arrivalMinute)}`);

        // Aufenthalt am Bahnhof
        if (p.arrivalMinute !== p.departureMinute) {
            polylinePoints.push(`${x},${yFromMinute(p.departureMinute)}`);
        }
    }
    
    /* ------------------------------------------------------------
       Tooltip
       ------------------------------------------------------------ */

    function showTooltip(
        evt: React.MouseEvent,
        p: typeof points[number]
    ) {
        setTooltip({
            x: evt.clientX,
            y: evt.clientY,
            rl100: p.rl100,
            name: p.name,
            arrival: p.arrival,
            departure: p.departure,
            dwellMin: Math.max(0, p.departureMinute - p.arrivalMinute),
        });
    }

    function hideTooltip() {
        setTooltip(null);
    }

    /* ============================================================
       Render
       ============================================================ */

    return (
        <div className="diagram-scroll">
            <svg
                width={svgWidth}
                height={svgHeight}
                viewBox={`0 0 ${svgWidth} ${svgHeight}`}
            >

                {/* =========================
                   Zeit-Gitter
                   ========================= */}
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

                {/* =========================
                   Vertikale Gitterlinien
                   ========================= */}
                {points.map((_, i) => {
                    const x = xFromIndex(i);
                    return (
                        <line
                            key={`grid-x-${i}`}
                            x1={x}
                            x2={x}
                            y1={PADDING_TOP}
                            y2={svgHeight - PADDING_BOTTOM}
                            stroke="#f2f2f2"
                        />
                    );
                })}

                {/* =========================
                   Fahrlinie
                   ========================= */}
                <polyline
                    points={polylinePoints.join(" ")}
                    fill="none"
                    stroke="#1e88e5"
                    strokeWidth={2}
                />

                {/* =========================
                   Aufenthalte
                   ========================= */}
                {points.map((p, i) => {
                    if (p.arrivalMinute === p.departureMinute)
                        return null;

                    const x = xFromIndex(i);

                    return (
                        <line
                            key={`stop-${i}`}
                            x1={x}
                            x2={x}
                            y1={yFromMinute(p.arrivalMinute)}
                            y2={yFromMinute(p.departureMinute)}
                            stroke="#1e88e5"
                            strokeWidth={2}
                        />
                    );
                })}

                {/* =========================
                   Durchfahrten
                   ========================= */}
                {points.map((p, i) => {
                    if (p.arrivalMinute !== p.departureMinute)
                        return null;

                    const x = xFromIndex(i);

                    return (
                        <circle
                            key={`pt-${i}`}
                            cx={x}
                            cy={yFromMinute(p.arrivalMinute)}
                            r={3}
                            fill="#1e88e5"
                        />
                    );
                })}

                {/* =========================
                   Hover-Zonen
                   ========================= */}
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
                            onMouseEnter={e => showTooltip(e, p)}
                            onMouseLeave={hideTooltip}
                        />
                    );
                })}

                {/* =========================
                   DS100 Labels
                   ========================= */}
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

                {/* =========================
                   DS100 Labels (oben)
                   ========================= */}
                {points.map((p, i) => (
                    <text
                        key={`lbl-top-${i}`}
                        x={xFromIndex(i)}
                        y={X_AXIS_TOP_Y}
                        textAnchor="middle"
                        fontSize="11"
                        fill="#444"
                    >
                        {p.rl100}
                    </text>
                ))}

            </svg>

            {/* =========================
               Tooltip
               ========================= */}
            {tooltip && (
                <div
                    className="diagram-tooltip"
                    style={{
                        left: tooltip.x + 12,
                        top: tooltip.y + 12
                    }}
                >
                    <strong>{tooltip.rl100}</strong>
                    <div>{tooltip.name}</div>

                    {tooltip.arrival && (
                        <div>Ank: {tooltip.arrival}</div>
                    )}
                    {tooltip.departure && (
                        <div>Abf: {tooltip.departure}</div>
                    )}

                    {tooltip.arrival &&
                        tooltip.departure && (
                            <div>
                                Aufenthalt:{" "}
                                {Math.max(
                                    0,
                                    points.find(p => p.rl100 === tooltip.rl100)!.departureMinute -
                                    points.find(p => p.rl100 === tooltip.rl100)!.arrivalMinute
                                )}{" "}
                                min
                            </div>
                        )}
                </div>
            )}
        </div>
    );
}

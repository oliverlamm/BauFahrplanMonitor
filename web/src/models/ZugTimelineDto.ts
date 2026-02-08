export interface ZugTimelinePoint {
    seqNo: number;

    // =========================
    // Technische Zeit (MINUTEN)
    // =========================
    arrivalMinute: number;     // z.B. 1864  (= 31:04)
    departureMinute: number;   // z.B. 1874

    // =========================
    // Anzeigezeiten (optional)
    // =========================
    arrival?: string | null;    // "07:04:24"
    departure?: string | null;  // "07:14:24"

    // =========================
    // Betriebsstelle
    // =========================
    rl100: string;
    name: string;
    typ: string;    // aus Backend ("Bf", "Hp", "Abzw", …)
}

export interface ZugTimelineResult {
    zugNr: number;
    date: string; // YYYY-MM-DD
    timeline: ZugTimelinePoint[];
}

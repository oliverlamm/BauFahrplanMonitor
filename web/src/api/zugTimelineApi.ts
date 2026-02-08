import type { ZugTimelineResult } from "../models/ZugTimelineDto.ts";

export async function loadZugTimeline(
    zugNr: string,
    date: string
): Promise<ZugTimelineResult> {

    const res = await fetch(
        `/api/zug/${zugNr}/timeline?date=${date}`
    );

    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Zugsicht konnte nicht geladen werden");
    }

    return res.json();
}

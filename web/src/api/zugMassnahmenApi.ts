export async function loadZugMassnahmen(
    jahr: number,
    zugNr: string,
    date: string
) {
    const response = await fetch(
        `/api/zug/${jahr}/${zugNr}/massnahmen?date=${date}`
    );

    if (!response.ok)
        throw new Error("Massnahmen konnten nicht geladen werden");

    return await response.json();
}

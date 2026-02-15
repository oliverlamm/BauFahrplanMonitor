export type ZwlMassnahmeOverlayDto = {
    vonRl100: string;
    bisRl100: string;
    massnahmeBeginn: string; // ISO-String
    massnahmeEnde: string;   // ISO-String
    regelungen: string;
    vzgListe: string;
    durchgehend: boolean;
    zeitraum?: string;
};

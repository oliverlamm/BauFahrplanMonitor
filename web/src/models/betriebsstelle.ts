export interface BetriebsstellenListRow {
    id: number;
    rl100: string;
    name: string;
    istBasis: boolean;
}

export interface BetriebsstelleGeo {
    vzGNr: number;
    lon: number;
    lat: number;
    kmL?: number | null;
    kmI?: number | null;
}

export interface BetriebsstelleDetail {
    id: number;
    rl100: string;
    name: string;
    zustand: string;

    typId: number;
    typ: string;

    regionId: number;
    region: string;

    netzbezirkId: number;
    netzbezirk: string;

    istBasis: boolean;

    geo: BetriebsstelleGeo[];
}


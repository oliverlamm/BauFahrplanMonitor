export type StatusLevel = "Ok" | "Warning" | "Error";

export interface DatabaseInfo {
    host: string;
    port: number;
    database: string;
    user: string;
    efLogging: boolean;
    efSensitiveLogging: boolean;
    expectedSchemaVersion: number;
}

export interface DatabaseStatus {
    status: StatusLevel;
    currentSchemaVersion: number;
    expectedSchemaVersion: number;
    message: string;
}

export interface PathStatus {
    path: string;
    status: StatusLevel;
    message: string;
}

export interface SystemPaths {
    import: PathStatus;
    archive: PathStatus;
}

export interface AllgemeinConfig {
    importThreads: number;
    debugging: boolean;
    stopAfterException: boolean;
    name: string;
    version: string;
    machineName: string;
}

export interface SystemStatus {
    application: string;
    session: string;
    name: string;
    version: string;
    time: string;

    datenbank: DatabaseInfo;
    databaseStatus: DatabaseStatus;
    paths: SystemPaths;
    allgemein: AllgemeinConfig;

    datei: DateiConfig;
}

export interface DateiConfig {
    importpfad: string;
    archivpfad: string;
    archivieren: boolean;
    nachImportLoeschen: boolean;
}
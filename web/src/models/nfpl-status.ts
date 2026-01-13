export interface NetzfahrplanWorkerStatus {
    workerId: number;
    state: string;
    currentFile: string | null;
    startedAt: string | null;
    processedItems: number;
    errors: number;
    progressMessage: string | null;
}

export interface NetzfahrplanStatus {
    state: string;
    totalFiles: number;
    queueCount: number;
    processedFiles: number;
    errors: number;
    startedAt: string | null;
    activeWorkers: number;
    workers: NetzfahrplanWorkerStatus[];
}

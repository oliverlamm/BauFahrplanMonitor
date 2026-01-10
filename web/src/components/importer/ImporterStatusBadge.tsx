interface ImporterStatusBadgeProps {
    status: string;
    text: string;
}

export function ImporterStatusBadge({ status, text }: ImporterStatusBadgeProps) {
    return (
        <div className={`importer-badge ${status}`}>
            <span className="badge-text">{text}</span>
        </div>
    );
}

export enum FindingStatus {
  Open = 0,
  Assigned = 1,
  Mitigated = 2,
  Retesting = 3,
  Closed = 4,
  Reopened = 5
}
export interface Finding {
  id: string;
  assetId: string;
  title: string;
  description: string;
  source: string;
  externalId?: string;
  sourceSeverity: string;
  status: FindingStatus;
  createdAt: Date;
}

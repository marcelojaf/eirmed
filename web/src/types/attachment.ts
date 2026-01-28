export interface Attachment {
  id: string;
  fileName: string;
  fileUrl: string;
  contentType: string | null;
  fileSizeBytes: number;
  createdAt: string;
}

export interface UploadResponse {
  id: string;
  fileName: string;
  fileUrl: string;
  contentType: string | null;
  fileSizeBytes: number;
  createdAt: string;
}

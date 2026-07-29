export interface AiHealthStatus {
  isHealthy: boolean;
  message: string;
  checkedAt: string;
}


export interface OllamaModelInfo {
  name: string;
  size: string;
  format: string;
  family: string;
  parameterSize: string;
}


export interface OllamaServerDetails {
  version: string;
  isConnected: boolean;
  models: OllamaModelInfo[];
}

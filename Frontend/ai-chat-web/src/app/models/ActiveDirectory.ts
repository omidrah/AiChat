export interface ActiveDirectorySettings {
  enabled: boolean;
  domain: string;
  container: string;
  server: string;
  servers: string[];
  useSsl: boolean;
}

export enum ActiveDirectoryDiagnosticType {
  Ldap389 = 0,
  Kerberos88 = 1,
  Dns53 = 2,
  DomainControllerDiscovery = 3,
  LdapSrvLookup = 4
}

export interface ActiveDirectoryDiagnosticRequest {
  type: ActiveDirectoryDiagnosticType;
  server?: string | null;
  domain?: string | null;
}

export interface ActiveDirectoryDiagnosticResult {
  success: boolean;
  testName: string;
  command: string;
  output: string;
  error?: string | null;
  durationMs: number;
  executedAt: string;
}
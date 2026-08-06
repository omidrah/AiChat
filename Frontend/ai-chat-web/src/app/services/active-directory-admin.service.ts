import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActiveDirectoryDiagnosticRequest, ActiveDirectoryDiagnosticResult, ActiveDirectorySettings } from '../models/ActiveDirectory';
import { environment } from '../../environments/environment'


@Injectable({ providedIn: 'root' })
export class ActiveDirectoryAdminApi {
  baseUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getSettings(): Observable<ActiveDirectorySettings> {
    return this.http.get<ActiveDirectorySettings>(
      `${this.baseUrl}/active-directory/settings`
    );
  }

  updateSettings(model: ActiveDirectorySettings): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/active-directory/settings`,
      model
    );
  }

  runDiagnostic(
    model: ActiveDirectoryDiagnosticRequest
  ): Observable<ActiveDirectoryDiagnosticResult> {
    return this.http.post<ActiveDirectoryDiagnosticResult>(
      `${this.baseUrl}/active-directory/diagnostics`,
      model
    );
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface LoginResult {
  accessToken: string;
  refreshToken: string;
  userName: string;
  expiresAt: string;
}

export interface RefreshResult {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {

    baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }

    getMode() {
        return this.http.get<{ mode: string }>(`${this.baseUrl}/auth/mode`);
    }

    login(userName: string, password: string) {
        return this.http.post<LoginResult>(`${this.baseUrl}/auth/login`, { userName, password })
        .pipe(
            tap(res => this.storeTokens(res))
        );
    }

    refresh() {
        const refreshToken = this.getRefreshToken();

        return this.http.post<RefreshResult>(`${this.baseUrl}/auth/refresh`, { refreshToken })
        .pipe(
            tap(res => this.storeTokens(res))
        );
    }
 
    getRefreshToken() {
        return localStorage.getItem('refreshToken');
    }

    isLoggedIn() {
        return !!this.getToken() && !!this.getRefreshToken();
    }

    logout() {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('accessTokenExpiresAt');
    }

    private storeTokens(res: LoginResult | RefreshResult) {
        localStorage.setItem('accessToken', res.accessToken);
        localStorage.setItem('refreshToken', res.refreshToken);
        localStorage.setItem('accessTokenExpiresAt', res.expiresAt);
    }

    getMe() {
        return this.http.get(`${this.baseUrl}/auth/me`);
    }

    getToken() {
        return localStorage.getItem('accessToken');
    }
}
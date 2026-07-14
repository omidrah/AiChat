import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface LoginResult {
  access_token: string;
  refresh_token: string;
  userName: string;
  expiresAt: string;
}

export interface RefreshResult {
  access_token: string;
  refresh_token: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {

    baseUrl = environment.apiUrl;
    private mode = '';
    constructor(private http: HttpClient) { }

    setMode(mode:string)
    {
        const normalized = mode.toLowerCase();
        this.mode = normalized;
        localStorage.setItem('auth_mode', normalized);
    }

    getAuthMode()
    {
        return this.mode ||  localStorage.getItem('auth_mode') || '';
    }
    
    getMode() {
        return this.http.get<{ mode: string }>(`${this.baseUrl}/auth/mode`);
    }

    windowsLogin() {
        return this.http.get(`${this.baseUrl}/auth/me`, {
        withCredentials: true
        });
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
        .pipe(tap(res => this.storeTokens(res)));
    }
 
    getRefreshToken() {
        return localStorage.getItem('refresh_token');
    }

    isLoggedIn() {
        return !!this.getToken() && !!this.getRefreshToken();
    }

    logout() {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        localStorage.removeItem('accessTokenExpiresAt');
        localStorage.removeItem('auth_mode');
    }

    private storeTokens(res: LoginResult | RefreshResult) {
        localStorage.setItem('access_token', res.access_token);
        localStorage.setItem('refresh_token', res.refresh_token);
        localStorage.setItem('accessTokenExpiresAt', res.expiresAt);
    }

    getMe() {
        return this.http.get(`${this.baseUrl}/auth/me`);
    }

    getToken() {
        return localStorage.getItem('access_token');
    }

    getUserName() {

        const token = this.getToken();
        if (!token)  return '';

        const payload = JSON.parse(atob(token.split('.')[1]));

       return (
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        payload['name'] ||
        ''
        );
    }
}
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {

    baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }

    getMode() {
        return this.http.get<{ mode: string }>(`${this.baseUrl}/mode`);
    }

    login(userName: string, password: string) {
        return this.http.post<any>(`${this.baseUrl}/login`, { userName, password })
            .pipe(
                tap(res => {
                    localStorage.setItem('token', res.accessToken);
                })
            );
    }

    getMe() {
        return this.http.get(`${this.baseUrl}/me`);
    }

    getToken() {
        return localStorage.getItem('token');
    }

    logout() {
        localStorage.removeItem('token');
    }
}
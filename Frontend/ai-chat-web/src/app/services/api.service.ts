import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable } from 'rxjs'
import { Conversation } from '../models/conversation'
import { Message } from '../models/message'
import { environment } from '../../environments/environment'

@Injectable({ providedIn: 'root' })
export class ApiService {

  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  createConversation(): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/conversations`, {})
  }

  getConversations(): Observable<Conversation[]> {
    return this.http.get<Conversation[]>(`${this.baseUrl}/conversations`)
  }

  getConversation(conversationId: string) {
    return this.http.get<any>(
      `${this.baseUrl}/conversations/${conversationId}`
    );
  }

  getMessages(conversationId: string): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.baseUrl}/conversations/${conversationId}`)
  }

  sendMessage(conversationId: string, message: string) {
    return this.http.post(
      `${this.baseUrl}/conversations/${conversationId}/messages`,
      { message }
    );
  }

  deleteConversation(id: string) {

    return this.http.delete(
      `${this.baseUrl}/conversations/${id}`
    );

  }

  renameConversation(id: string, title: string) {

    return this.http.put(
      `${this.baseUrl}/conversations/${id}`,
      {
        title
      }
    );

  }

  // services/api.service.ts
  cancelMessage(conversationId: string) {
    return this.http.post(`${this.baseUrl}/conversations/${conversationId}/cancel`, {});
  }


  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/users`);
  }

  createUser(user: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/users`, user);
  }

  updateUser(id: string, user: any): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/users/${id}`, user);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${id}`);
  }

}

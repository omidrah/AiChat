import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable } from 'rxjs'
import { Conversation } from '../models/conversation'
import { Message } from '../models/message'

@Injectable({ providedIn: 'root' })
export class ApiService {

  baseUrl = "https://localhost:7117/api"

  constructor(private http: HttpClient) {}

  createConversation(): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/conversations`, {})
  }

  getConversations() :Observable<Conversation[]> {
    return this.http.get<Conversation[]>(`${this.baseUrl}/conversations`)
  }

  getConversation(conversationId: string) {
    return this.http.get<any>(
      `${this.baseUrl}/conversations/${conversationId}`
    );
  }
  
  getMessages(conversationId: string) :Observable<Message[]> {
    return this.http.get<Message[]>(`${this.baseUrl}/conversations/${conversationId}`)
  }

  sendMessage(conversationId: string, message: string) {
    return this.http.post(
      `${this.baseUrl}/conversations/${conversationId}/messages`,
      { message }
    );
  }
}

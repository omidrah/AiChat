import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable } from 'rxjs'

@Injectable({ providedIn: 'root' })
export class ApiService {

  baseUrl = "https://localhost:7117/api"

  constructor(private http: HttpClient) {}

  createConversation(): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/conversations`, {})
  }

  getConversations() {
    return this.http.get(`${this.baseUrl}/conversations`)
  }

  getMessages(conversationId: string) {
    return this.http.get(`${this.baseUrl}/conversations/${conversationId}`)
  }

  sendMessage(conversationId: string, message: string) {
    return this.http.post(
      `${this.baseUrl}/conversations/${conversationId}/messages`,
      { message }
    )
  }
}

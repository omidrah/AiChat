import * as signalR from '@microsoft/signalr'
import { Injectable } from '@angular/core'

@Injectable({ providedIn: 'root' })
export class SignalRService {

  private hub!: signalR.HubConnection

  start() {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl("https://localhost:7117/hubs/chat")
      .withAutomaticReconnect()
      .build()

    return this.hub.start()
  }

  joinConversation(id: string) {
    return this.hub.invoke("JoinConversation", id)
  }

  onReceiveToken(callback: (token:string)=>void) {
    this.hub.on("ReceiveToken", callback)
  }
}

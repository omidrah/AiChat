import * as signalR from '@microsoft/signalr'
import { Injectable } from '@angular/core'
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SignalRService {

  private hub?: signalR.HubConnection;

  async start(): Promise<void> {
    if (this.hub && this.hub.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR already connected');
      return;
    }

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl
          //  ,{withCredentials: true}
      )
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hub.onclose(error => {
        console.warn('SignalR closed', error);
      });
  
    this.hub.onreconnecting(error => {  
        console.warn('SignalR reconnecting', error);
      });
  
     this.hub.onreconnected(connectionId => {
        console.log('SignalR reconnected', connectionId);
      });
    
    await this.hub.start();

    console.log('SignalR connected:', this.hub.connectionId);
  }

   async joinConversation(conversationId: string) {
      if (!this.hub) {
        throw new Error('SignalR hub is not started');
      }

      await this.hub.invoke("JoinConversation", conversationId);
      
      console.log('Joined conversation group:', conversationId);

  }

  offReceiveToken() {
    this.hub?.off('ReceiveToken');
  }
 // on every token from backend recieved
  onReceiveToken(callback: (token:string)=>void) {
    if (!this.hub) {
      throw new Error('SignalR hub is not started');
    }
    console.log('Registering ReceiveToken handler');

    this.hub.off('ReceiveToken'); //previous connection remove

    this.hub.on('ReceiveToken', token => {
      console.log('SignalR ReceiveToken:', token);
      callback(token);
    });
  }
// when stream from backend finished
  onReceiveCompleted(callback: () => void) {
    this.hub?.on('ReceiveCompleted', callback);
  }

  offReceiveCompleted() {
    this.hub?.off('ReceiveCompleted');
  }


  async stop() {
    if (this.hub) {
      await this.hub.stop();
      this.hub = undefined;
    }
  }
  
}

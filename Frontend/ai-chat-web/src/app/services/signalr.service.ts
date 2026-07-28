import * as signalR from '@microsoft/signalr'
import { Injectable } from '@angular/core'
import { environment } from '../../environments/environment';
import { AuthService } from './AuthService';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SignalRService {

  constructor(private auth: AuthService) {}

  private hub?: signalR.HubConnection;
  private currentConversationId?: string;

  async start(): Promise<void> {
    if (this.hub)
    {
        if(this.hub.state === signalR.HubConnectionState.Connected ||
          this.hub.state === signalR.HubConnectionState.Connecting ||
          this.hub.state === signalR.HubConnectionState.Reconnecting) 
        {
          console.log('SignalR already connected');
          return;
        }
        if (this.hub.state === signalR.HubConnectionState.Disconnected) {
          await this.hub.start();
           if (this.currentConversationId)
            {
             await this.joinConversation(this.currentConversationId);
            }
          return;
        }
    }

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl,  {  
         accessTokenFactory: async () => {      //  برای پشتیبانی از رفرش توکن قبل از اتصال
              let token = this.auth.getToken();

              // بررسی منقضی شدن توکن قبل از پاس دادن به سیگنال‌آر
              if (token && this.isTokenExpired(token)) {
                try {
                  console.log("Token expired. Refreshing token for SignalR...");
                  // صدا زدن متد رفرش به صورت Promise
                  const refreshResult = await firstValueFrom(this.auth.refresh());
                  token = refreshResult.access_token;
                } 
                catch (e) {
                  console.error("Failed to refresh token during SignalR handshake", e);
                  this.auth.logout();
                  // در صورت ناموفق بودن رفرش، کاربر را مجبور به لاگین مجدد کنید
                  window.location.href = '/login'; 
                  return '';
                }
              }
              // console.log("SignalR Token:", token);
              return token ?? '';
          }
       } )
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hub.onclose(error => {
        console.warn('SignalR closed', error);
      });
  
    this.hub.onreconnecting(error => {  
        console.warn('SignalR reconnecting', error);
      });
  
    this.hub.onreconnected(async connectionId => {
        if (this.currentConversationId) {
            //console.log("Joining again...");
            await this.joinConversation(this.currentConversationId);
        }
    });
       
    await this.hub.start();
  }

  //check expiration time for token in front
   private isTokenExpired(token: string): boolean {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        if (!payload.exp) return false;
        
        const expiry = payload.exp * 1000; // تبدیل به میلی‌ثانیه
        // اگر کمتر از ۳۰ ثانیه به پایان اعتبار توکن مانده باشد، آن را منقضی در نظر می‌گیریم
        return (Date.now() + 30000) >= expiry;
      } catch {
        return true;
      }
    }

   async joinConversation(conversationId: string) {
      if (!this.hub) {
        throw new Error('SignalR hub is not started');
      }

      this.currentConversationId = conversationId;
      await this.hub.invoke("JoinConversation", conversationId);
    }

  offReceiveToken() {
    this.hub?.off('ReceiveToken');
  }

 // on every token from backend recieved
  onReceiveToken(callback: (token:string)=>void) {
    if (!this.hub) {
      throw new Error('SignalR hub is not started');
    }
    this.hub.off('ReceiveToken'); //previous connection remove
    this.hub.on('ReceiveToken', chunk => {
      //console.log('SignalR ReceiveToken:', token);
      callback(chunk);
    });
  }

  // when stream from backend finished
  onReceiveCompleted(callback: () => void) {
    this.hub?.off('ReceiveCompleted');    
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

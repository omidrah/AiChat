import { Component, ElementRef, NgZone, ViewChild, ChangeDetectorRef  } from '@angular/core';
import { ApiService } from '../services/api.service';
import { SignalRService } from '../services/signalr.service';
import {  Message } from '../models/message';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { marked } from 'marked';
import { markedHighlight } from 'marked-highlight';
import hljs from 'highlight.js';
import { ActivatedRoute } from '@angular/router';

// تنظیم markdown + highlight
marked.use(
  markedHighlight({
    highlight(code, lang) {
      if (lang && hljs.getLanguage(lang)) {
        return hljs.highlight(code, { language: lang }).value;
      }

      return hljs.highlightAuto(code).value;
    }
  })
);

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule],
  standalone:true,
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})

export class ChatComponent {

  @ViewChild('scrollContainer')
  private scrollContainer!: ElementRef;
  
  isThinking=false;

  messages: Message[] = []
  input = ""
  conversationId!: string

  isSending = false;
  isSignalRReady = false;
  constructor(
    private api: ApiService,
    private signalr: SignalRService,
    private zone: NgZone,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}


  async ngOnInit() {

    this.route.paramMap.subscribe(async params => {
      const id = params.get('id');
      if (!id) {
        console.error('Conversation id not found in route');
        return;
      }

      this.conversationId = id;

      console.log('Current conversation id:', this.conversationId);

      await this.signalr.start();

      this.signalr.onReceiveToken(token => {

        console.log('TOKEN FROM SIGNALR:', token);  
  
        this.zone.run(() => {  // update UI state
          let lastMessage = this.messages[this.messages.length - 1];
  
          if (!lastMessage || lastMessage.role.toLowerCase() !== 'assistant') {
            this.messages.push({
              role: 'assistant',
              content: token,
              createdAt: new Date()
            });
  
            this.scrollToBottom();
            return;
          }
  
          const index = this.messages.length - 1;
             this.messages = this.messages.map((m, i) => i === index  ? { ...m, content: m.content + token } : m
          );

          this.cdr.detectChanges(); // همین الان template را دوباره render کن
          this.scrollToBottom();
        });
      });

      await this.signalr.joinConversation(this.conversationId);
      this.isSignalRReady = true;

      console.log('Joined SignalR group:', this.conversationId);

      await this.loadMessages();
      this.cdr.detectChanges();
      this.scrollToBottom();

    });
  }

  async loadMessages() {
      const result: any = await firstValueFrom(
        this.api.getMessages(this.conversationId)
      );

      console.log('getMessages result:', result);

      const messagesfromApi = result.messages ?? [];

      this.messages = messagesfromApi.map((m: Message) => ({
        ...m,
        createdAt: m.createdAt ? new Date(m.createdAt) : new Date()
      }));

     this.scrollToBottom();
  }

  send() {

    const msg = this.input.trim();
    if (!msg) {return;}

    if (!this.isSignalRReady) {
      console.warn('SignalR is not ready yet');
      return;
    }
    this.messages.push({
      role: "user",
      content: msg,
      createdAt: new Date()
    });

    this.scrollToBottom();
    this.input = "";

    this.isThinking=true;

    this.api.sendMessage(this.conversationId, msg).subscribe({
      next: () => {
        this.isThinking=false;
        console.log('Message sent');
      },
      error: err => {
        console.error(err);
      }
    });
  }
  
  scrollToBottom(){
    setTimeout(()=>{
      this.scrollContainer.nativeElement.scrollTop =  this.scrollContainer.nativeElement.scrollHeight;
    });
  }

  render(text:string){
    return marked.parse(text);
  }

  ngOnDestroy() {
    this.signalr.offReceiveToken();
  }
}

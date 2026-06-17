import { Component, ElementRef, ViewChild } from '@angular/core';
import { ApiService } from '../services/api.service';
import { SignalRService } from '../services/signalr.service';
import { Message } from '../models/message';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { marked } from 'marked';
import { markedHighlight } from 'marked-highlight';
import hljs from 'highlight.js';

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

  messages: Message[] = []
  input = ""
  conversationId!: string

  constructor(
    private api: ApiService,
    private signalr: SignalRService
  ) {}

  scrollToBottom(){
    setTimeout(()=>{
      this.scrollContainer.nativeElement.scrollTop =  this.scrollContainer.nativeElement.scrollHeight;
    });
  }


  async ngOnInit() {

    this.conversationId = await firstValueFrom(
      this.api.createConversation()
    );
    
    await this.signalr.start()

    await this.signalr.joinConversation(this.conversationId)

    this.signalr.onReceiveToken(token => {

      let last = this.messages[this.messages.length - 1]

      if (!last || last.role !== "assistant") {
        last = { role:"assistant", content:"", createdAt:new Date() }
        this.messages.push(last)
      }

      last.content += token
    })
  }

  send() {

    const msg = this.input

    this.messages.push({
      role: "user",
      content: msg,
      createdAt: new Date()
    })
    this.scrollToBottom();

    this.api.sendMessage(this.conversationId, msg).subscribe()

    this.input = ""
  }

  render(text:string){
    return marked.parse(text);
  }
}

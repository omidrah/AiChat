import { Component } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { SignalRService } from '../../services/signalr.service';
import { Message } from '../../models/message';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class ChatComponent {

  messages: Message[] = []
  input = ""
  conversationId!: string

  constructor(
    private api: ApiService,
    private signalr: SignalRService
  ) {}

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

    this.api.sendMessage(this.conversationId, msg).subscribe()

    this.input = ""
  }
}

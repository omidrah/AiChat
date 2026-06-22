import { Component, ElementRef, NgZone, ViewChild, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '../services/api.service';
import { SignalRService } from '../services/signalr.service';
import { Message } from '../models/message';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { marked } from 'marked';
import { markedHighlight } from 'marked-highlight';
import hljs from 'highlight.js';
import { ActivatedRoute } from '@angular/router';

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
  standalone: true,
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class ChatComponent {

  @ViewChild('scrollContainer')
  private scrollContainer!: ElementRef<HTMLDivElement>;

  isThinking = false;
  isSending = false;
  isSignalRReady = false;

  shouldAutoScroll = true;

  messages: Message[] = [];
  input = '';
  conversationId!: string;

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

      await this.signalr.start();

      this.signalr.onReceiveToken(token => {
        this.zone.run(() => {
          let lastMessage = this.messages[this.messages.length - 1];

          if (!lastMessage || lastMessage.role.toLowerCase() !== 'assistant') {
            this.messages.push({
              role: 'assistant',
              content: token,
              createdAt: new Date()
            });

            this.cdr.detectChanges();
            this.scrollToBottomIfNeeded();
            return;
          }

          const index = this.messages.length - 1;

          this.messages = this.messages.map((m, i) =>
            i === index
              ? { ...m, content: m.content + token }
              : m
          );

          this.cdr.detectChanges();
          this.scrollToBottomIfNeeded();
        });
      });

      this.signalr.onReceiveCompleted(() => {
        this.zone.run(() => {
          this.isSending = false;
          this.isThinking = false;
          this.cdr.detectChanges();
        });
      });

      await this.signalr.joinConversation(this.conversationId);
      this.isSignalRReady = true;

      await this.loadMessages();

      this.cdr.detectChanges();
      this.forceScrollToBottom();
    });
  }

  async loadMessages() {
    const result: any = await firstValueFrom(
      this.api.getMessages(this.conversationId)
    );

    const messagesfromApi = result.messages ?? [];

    this.messages = messagesfromApi.map((m: Message) => ({
      ...m,
      createdAt: m.createdAt ? new Date(m.createdAt) : new Date()
    }));

    this.forceScrollToBottom();
  }

  send(textarea?: HTMLTextAreaElement) {
    const msg = this.input.trim();

    if (!msg) {
      return;
    }

    if (!this.isSignalRReady) {
      console.warn('SignalR is not ready yet');
      return;
    }

    this.messages.push({
      role: 'user',
      content: msg,
      createdAt: new Date()
    });

    this.input = '';
    this.isThinking = true;
    this.isSending = true;
    this.shouldAutoScroll = true;

    if (textarea) {
      textarea.style.height = 'auto';
    }

    this.forceScrollToBottom();

    this.api.sendMessage(this.conversationId, msg).subscribe({
      next: () => {
        this.isThinking = false;
        console.log('Message sent');
      },
      error: err => {
        this.isThinking = false;
        this.isSending = false;
        console.error(err);
      }
    });
  }

  onMessagesScroll(): void {
    const element = this.scrollContainer?.nativeElement;

    if (!element) {
      return;
    }

    const distanceFromBottom =
      element.scrollHeight - element.scrollTop - element.clientHeight;

    this.shouldAutoScroll = distanceFromBottom < 140;
  }

  scrollToBottomIfNeeded(): void {
    if (!this.shouldAutoScroll) {
      return;
    }

    this.forceScrollToBottom();
  }

  forceScrollToBottom(): void {
    setTimeout(() => {
      const element = this.scrollContainer?.nativeElement;

      if (!element) {
        return;
      }

      element.scrollTop = element.scrollHeight;
    }, 0);
  }

  enableAutoScrollAndGoBottom(): void {
    this.shouldAutoScroll = true;
    this.forceScrollToBottom();
  }

  render(text: string) {
    return marked.parse(text);
  }

  autoResizeTextarea(textarea: HTMLTextAreaElement): void {
    textarea.style.height = 'auto';
    textarea.style.height = `${Math.min(textarea.scrollHeight, 220)}px`;
  }

  handleEnter(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;

    if (keyboardEvent.key !== 'Enter') {
      return;
    }

    if (keyboardEvent.shiftKey) {
      return;
    }

    keyboardEvent.preventDefault();

    if (this.input?.trim() && !this.isSending) {
      this.send();
    }
  }

  ngOnDestroy() {
    this.signalr.offReceiveToken();
  this.signalr.offReceiveCompleted();
  }
}

import { Component, ElementRef, ViewChild, signal } from '@angular/core';
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

  isThinking = signal(false);
  isSending = signal(false);
  isSignalRReady = signal(false);
  input = '';
  conversationId!: string;
  shouldAutoScroll = true;
  messages = signal<Message[]>([]);

  constructor(
    private api: ApiService,
    private signalr: SignalRService,
    private route: ActivatedRoute) { }

  ngOnInit() {

    this.route.paramMap.subscribe(async params => {

      const id = params.get('id');
      if (!id) return;

      await this.openConversation(id);

      this.registerSignalRHandlers();

      this.forceScrollToBottom();
    });
  }

  // ویرایش متد ثبت هندلرهای سیگنال‌آر برای اتمام استریم
  private registerSignalRHandlers() {

    this.signalr.onReceiveToken(token => {
      this.isThinking.set(false);
      const current = this.messages();
      const lastMessage = current[current.length - 1];

      if (!lastMessage || lastMessage.role.toLowerCase() !== 'assistant') {

        this.messages.update(list => [
          ...list,
          { role: 'assistant', content: token, createdAt: new Date() }
        ]);

        this.scrollToBottomIfNeeded();

        return;
      }

      const index = current.length - 1;

      this.messages.update(
        list => list.map((m, i) => i === index ? { ...m, content: m.content + token } : m)
      );

      this.scrollToBottomIfNeeded();
    });

    this.signalr.onReceiveCompleted(() => {
      this.resetSendingState();
    });
  }

  private async openConversation(id: string) {
    this.conversationId = id;
    await this.signalr.start();
    await this.signalr.joinConversation(id);
    this.isSignalRReady.set(true);
    await this.loadMessages();
  }

  async loadMessages() {
    const result: any = await firstValueFrom(
      this.api.getMessages(this.conversationId)
    );

    const messagesfromApi = result.messages ?? [];

    this.messages.set(

      messagesfromApi.map((m: Message) => ({
        ...m,
        createdAt: m.createdAt ? new Date(m.createdAt) : new Date()
      })))

    this.forceScrollToBottom();
  }

  send(textarea?: HTMLTextAreaElement) {
    const msg = this.input.trim();

    if (!msg) { return; }

    if (!this.isSignalRReady()) {
      console.warn('SignalR is not ready yet');
      return;
    }

    this.messages.update(list => [...list, { role: 'user', content: msg, createdAt: new Date() }]);

    this.input = '';

    this.isThinking.set(true);
    this.isSending.set(true);
    this.shouldAutoScroll = true;

    if (textarea) {
      textarea.style.height = 'auto';
    }

    this.forceScrollToBottom();

    this.api.sendMessage(this.conversationId, msg).subscribe({
      next: () => {
        console.log('Message sent');
      },
      error: err => {
        this.resetSendingState();
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

    if (this.input?.trim() && !this.isSending()) {
      this.send();
    }
  }

  private resetSendingState() {
    this.isSending.set(false);
    this.isThinking.set(false);
  }

  cancel() {
    if (!this.conversationId || !this.isSending()) return;

    this.api.cancelMessage(this.conversationId).subscribe({
      next: () => {
        console.log('درخواست لغو با موفقیت به سرور ارسال شد.');
        this.resetSendingState();
      },
      error: (err) => {
        console.error('خطا در لغو درخواست:', err);
        // حتی در صورت خطای شبکه، رابط کاربری را آزاد می‌کنیم
        this.resetSendingState();
      }
    });
  }


  copyToClipboard(message: Message) {
    if (!message.content) return;

    // کپی کردن متن در کلیپ‌بورد با استفاده از API استاندارد مرورگر
    navigator.clipboard.writeText(message.content).then(() => {
      // ایجاد یک افکت بصری موقت برای دکمه کپی همان پیام
      message.copied = true;
      setTimeout(() => {
        message.copied = false;
      }, 2000); // بعد از ۲ ثانیه آیکون به حالت قبل برمی‌گردد
    }).catch(err => {
      console.error('Failed to copy text: ', err);
    });
  }

  ngOnDestroy() {
    this.signalr.offReceiveToken();
    this.signalr.offReceiveCompleted();
  }
}

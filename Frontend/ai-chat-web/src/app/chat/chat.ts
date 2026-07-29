import { Component, ElementRef, ViewChild, signal, OnInit, OnDestroy, inject } from '@angular/core';
import { ApiService } from '../services/api.service';
import { SignalRService } from '../services/signalr.service';
import { Message } from '../models/message';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, interval, startWith, Subscription, switchMap } from 'rxjs';
import { marked } from 'marked';
import { markedHighlight } from 'marked-highlight';
import hljs from 'highlight.js';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AiHealthStatus } from '../models/AiHealthStatus';

type ConnectionState = 'ONLINE' | 'OFFLINE' | 'RECONNECTING';


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

export class ChatComponent implements OnInit, OnDestroy {

  aiStatus: ConnectionState = 'RECONNECTING';
  private healthSub!: Subscription;

  models = signal<{ id: string; label: string }[]>([]);
  modelsLoading = signal(true);
  selectedModel = ''; // نگهداری مدل انتخابی کاربر

  @ViewChild('scrollContainer')
  private scrollContainer!: ElementRef<HTMLDivElement>;

  isThinking = signal(false);
  isSending = signal(false);
  isSignalRReady = signal(false);
  input = '';
  conversationId!: string;
  shouldAutoScroll = true;
  messages = signal<Message[]>([]);

  // تعریف متغیرها با استفاده از inject به جای Constructor

  private api = inject(ApiService);
  private snackBar = inject(MatSnackBar);
  private signalr = inject(SignalRService);
  private route = inject(ActivatedRoute);

  constructor() { }

  ngOnInit() {
    this.checkAiHealthCheckEvery15Seconds();
    this.loadModels();
    this.route.paramMap.subscribe(async params => {
      const id = params.get('id');
      if (!id) return;

      await this.openConversation(id);
      this.registerSignalRHandlers();
      this.forceScrollToBottom();
    });
  }

  checkAiHealthCheckEvery15Seconds() {
    // بررسی وضعیت به صورت دوره‌ای (هر 15 ثانیه)
    this.healthSub = interval(15000).pipe(
      startWith(0), // اولین بررسی بلافاصله در لود صفحه انجام شود
      switchMap(() => {
        if (this.aiStatus !== 'ONLINE') {
          this.aiStatus = 'RECONNECTING';
        }
        return this.api.getAiHealth();
      })
    ).subscribe({
      next: (status: AiHealthStatus) => {
        const previousStatus = this.aiStatus;
        this.aiStatus = status.isHealthy ? 'ONLINE' : 'OFFLINE';

        // اگر وضعیت از سالم به ناسالم تغییر کرد، به کاربر هشدار داده شود
        if (previousStatus === 'ONLINE' && this.aiStatus === 'OFFLINE') {
          this.snackBar.open('ارتباط با سرور هوش مصنوعی قطع شد.', 'بستن', {
            duration: 5000,
            panelClass: ['warning-snackbar']
          });
        }
      },
      error: () => {
        this.aiStatus = 'OFFLINE';
      }
    });

  }


  loadModels(): void {
    this.modelsLoading.set(true);
    this.api.getModels().subscribe({
      next: (res) => {
        // فرض می‌کنیم پاسخ بک‌اند لیستی از مدل‌ها با فیلد name است
        const list = res.map(x => ({ id: x.name, label: x.name }));
        this.models.set(list);

        if (list.length > 0) {
          this.selectedModel = list[0].id;
        }

        this.modelsLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading models:', err);
        this.models.set([]);
        this.modelsLoading.set(false);
      }
    });
  }

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
      }))
    );
    this.forceScrollToBottom();
  }

  send(textarea?: HTMLTextAreaElement) {

    if (this.aiStatus !== 'ONLINE' || this.isSending() || !this.input.trim()) {
      return;
    }

    const msg = this.input.trim();

    if (!this.isSignalRReady()) {
      console.warn('SignalR is not ready yet');
      return;
    }

    const userMessage: Message = {
      role: 'user',
      content: msg,
      createdAt: new Date(),
      model: this.selectedModel, // ارسال مدل انتخابی کنونی
    };

    this.messages.update(list => [...list, userMessage]);

    this.input = '';
    this.isThinking.set(true);
    this.isSending.set(true);
    this.shouldAutoScroll = true;

    if (textarea) {
      textarea.style.height = 'auto';
    }

    this.forceScrollToBottom();

    this.api.sendMessage(this.conversationId, msg, this.selectedModel).subscribe({
      next: () => {
        console.log('Message sent');
      },
      error: err => {
        this.resetSendingState();
        this.messages.update(list => [...list, {
          role: 'assistant',
          content: `⚠️ خطا: ${err.error?.detail || 'عدم پاسخگویی سرور'}`,
          createdAt: new Date()
        }]);
        console.error(err);
      }
    });
  }

  onMessagesScroll(): void {
    const element = this.scrollContainer?.nativeElement;
    if (!element) return;
    const distanceFromBottom = element.scrollHeight - element.scrollTop - element.clientHeight;
    this.shouldAutoScroll = distanceFromBottom < 140;
  }

  scrollToBottomIfNeeded(): void {
    if (!this.shouldAutoScroll) return;
    this.forceScrollToBottom();
  }

  forceScrollToBottom(): void {
    setTimeout(() => {
      const element = this.scrollContainer?.nativeElement;
      if (!element) return;
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
    if (keyboardEvent.key !== 'Enter') return;
    if (keyboardEvent.shiftKey) return;

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
        this.resetSendingState();
      }
    });
  }



  copyToClipboard(message: Message) {
    if (!message.content) return;

    const textToCopy = message.content;

    // بررسی اینکه آیا Clipboard API در دسترس است (فقط در HTTPS یا localhost)
    if (navigator.clipboard && window.isSecureContext) {
      navigator.clipboard.writeText(textToCopy).then(() => {
        this.handleCopySuccess(message);
      }).catch(err => {
        console.error('Clipboard API Error: ', err);
        this.fallbackCopyTextToClipboard(textToCopy, message);
      });
    } else {
      // استفاده از متد قدیمی برای HTTP و IP
      this.fallbackCopyTextToClipboard(textToCopy, message);
    }
  }

  private fallbackCopyTextToClipboard(text: string, message: Message) {
    const textArea = document.createElement("textarea");
    textArea.value = text;

    // خارج کردن از دید کاربر
    textArea.style.position = "fixed";
    textArea.style.left = "-999999px";
    textArea.style.top = "-999999px";
    document.body.appendChild(textArea);

    textArea.focus();
    textArea.select();

    try {
      const successful = document.execCommand('copy');
      if (successful) {
        this.handleCopySuccess(message);
      } else {
        console.error('Fallback: Copying text command was unsuccessful');
      }
    } catch (err) {
      console.error('Fallback: Oops, unable to copy', err);
    }

    document.body.removeChild(textArea);
  }

  private handleCopySuccess(message: Message) {
    message.copied = true;
    setTimeout(() => {
      message.copied = false;
    }, 2000);
  }


  ngOnDestroy() {
    this.signalr.offReceiveToken();
    this.signalr.offReceiveCompleted();
    if (this.healthSub) {
      this.healthSub.unsubscribe();
    }
  }
}

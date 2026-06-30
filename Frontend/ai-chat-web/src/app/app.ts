import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from './services/AuthService';
import { firstValueFrom } from 'rxjs';
import { ApiService } from './services/api.service';
import { ConversationList } from './conversations/conversation-list/conversation-list';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, ConversationList],
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
})

export class App {

  protected readonly title = signal('ai-chat-web');

  constructor(private auth: AuthService, private api: ApiService, private router: Router) { }

   dark = false;

  toggleTheme() {
    this.dark = !this.dark;

    if (this.dark)
      document.body.classList.add("dark");
    else
      document.body.classList.remove("dark");
  }

  get isLoginPage(): boolean {
    return this.router.url.startsWith('/login');
  }
  
  async ngOnInit() {

    const mode = await firstValueFrom(this.auth.getMode());

    if (mode.mode === 'Local' && !this.auth.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    try {

      const conversations = await firstValueFrom(this.api.getConversations());

      if (conversations.length > 0) {

        this.router.navigate([
          '/chat',
          conversations[0].id
        ]);

        return;
      }

      const id = await firstValueFrom(this.api.createConversation());

      this.router.navigate([
        '/chat',
        id
      ]);

    }
    catch {

      this.router.navigate(['/login']);

    }
  }

}

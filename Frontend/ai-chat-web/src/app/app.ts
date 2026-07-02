import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from './services/AuthService';
import { firstValueFrom } from 'rxjs';
import { ConversationList } from './conversations/conversation-list/conversation-list';
import { CommonModule } from '@angular/common';
import { ConversationStore } from './store/conversation.store';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, ConversationList],
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
})

export class App {

  protected readonly title = signal('ai-chat-web');
  
  sidebarCollapsed = false;
  dark = false;
  constructor(private auth: AuthService, private store: ConversationStore, private router: Router) { }

  toggleTheme() {
    this.dark = !this.dark;

    if (this.dark)
      document.body.classList.add("dark");
    else
      document.body.classList.remove("dark");
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
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

      await firstValueFrom(this.store.load());    
      const conversations = this.store.value;

      if (conversations.length > 0) {

        this.router.navigate([
          '/chat',
          conversations[0].id
        ]);

        return;
      }

      const id = await this.store.create();
      this.router.navigate(['/chat',id]);

    }
    catch {

      this.router.navigate(['/login']);

    }
  }

}

import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ConversationList } from './conversations/conversation-list/conversation-list';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/AuthService';

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
  constructor(private router: Router, private auth: AuthService) {}
  
  // تبدیل به Getter برای بررسی و بروزرسانی لحظه‌ای وضعیت ادمین پس از لاگین
  get isAdmin(): boolean {
    const name = this.auth.getUserName() || '';
    return name.toLowerCase() === 'administrator' || name.toLowerCase() === 'admin';
  }

  toggleTheme() {
    this.dark = !this.dark;

    if (this.dark)
      document.body.classList.add("dark");
    else
      document.body.classList.remove("dark");
  }
  
  usersRoute(){
    this.router.navigate(['/users']);
  }
  
  aiHealthRoute(){
    this.router.navigate(['/ollama']);
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }
  
  get isLoginPage(): boolean {
    return this.router.url.startsWith('/login');
  }
  
  ngOnInit() {
  }
}
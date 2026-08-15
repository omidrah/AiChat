import { Component, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ConversationList } from './conversations/conversation-list/conversation-list';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/AuthService';
import { ThemeService } from './services/theme.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, ConversationList],
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
})

export class App {

  protected readonly title = signal('ai-chat-web');

  private themeService = inject(ThemeService);
  readonly isDark = this.themeService.theme.asReadonly();

  sidebarCollapsed = false;
  constructor(private router: Router, private auth: AuthService) {}
  
  // تبدیل به Getter برای بررسی و بروزرسانی لحظه‌ای وضعیت ادمین پس از لاگین
  get isAdmin(): boolean {
    const name = this.auth.getUserName() || '';
    return name.toLowerCase() === 'administrator' || name.toLowerCase() === 'admin';
  }

  toggleTheme() {
    this.themeService.toggle();
  }
  
  usersRoute(){
    this.router.navigate(['/users']);
  }
  
  aiHealthRoute(){
    this.router.navigate(['/ollama']);
  }

  activeDirectoryRoute(){
    this.router.navigate(['/active-directory']);
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
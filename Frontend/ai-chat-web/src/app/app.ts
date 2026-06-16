import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConversationList } from './conversations/list/conversation-list/conversation-list';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,  ConversationList],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App {
  protected readonly title = signal('ai-chat-web');
}

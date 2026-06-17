import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConversationList } from './conversations/conversation-list/conversation-list';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,  ConversationList],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App {
  protected readonly title = signal('ai-chat-web');

  dark=false;

  toggleTheme(){
    this.dark=!this.dark;

    if(this.dark)
      document.body.classList.add("dark");
    else
      document.body.classList.remove("dark");
  }

}

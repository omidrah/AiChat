import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ConversationList } from './conversations/conversation-list/conversation-list';
import { AuthService } from './services/AuthService';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,  ConversationList],
  templateUrl: './app.html',
  styleUrl: './app.css',
  
})

export class App {
  
  protected readonly title = signal('ai-chat-web');

  constructor(private auth:AuthService, private router:Router){}

  
  dark=false;

  toggleTheme(){
    this.dark=!this.dark;

    if(this.dark)
      document.body.classList.add("dark");
    else
      document.body.classList.remove("dark");
  }
  ngOnInit(){

    this.auth.getMode().subscribe(x=>{

      if(x.mode === "Local"){
        this.router.navigate(['/login']);
      }
      else{
        this.router.navigate(['/chat']);
      }

    });
  }

}

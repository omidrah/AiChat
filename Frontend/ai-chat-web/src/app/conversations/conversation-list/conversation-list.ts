import { Component } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Conversation } from '../../models/conversation';
import { firstValueFrom } from 'rxjs';


@Component({
  selector: 'app-conversation-list',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './conversation-list.html',
  styleUrl: './conversation-list.css',
})
export class ConversationList {

  conversations: Conversation[] = [];

  constructor(private api:ApiService, private router:Router){  
  }

  ngOnInit() {
      this.load();
  }
  async create(){
    const id = await firstValueFrom(this.api.createConversation());
    this.router.navigate(['/chat',id]);
  }

  load(){
    this.api.getConversations().subscribe({
      next: x => {
        this.conversations = x;
      },
      error: err => {
        console.error('Failed to load conversations', err);
      }
    });
  }

  open(id:string){
    this.router.navigate(['/chat',id]);
  }

}

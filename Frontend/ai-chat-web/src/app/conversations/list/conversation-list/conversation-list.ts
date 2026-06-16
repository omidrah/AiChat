import { Component } from '@angular/core';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { ApiService } from '../../../services/api.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Conversation } from '../../../models/conversation';

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
    this.load();
  }

  async create(){
    const id = await firstValueFrom(this.api.createConversation());
    this.router.navigate(['/chat',id]);
  }

  load(){
    // this.api.getConversations().subscribe((x: Conversation[])=>{
    //   this.conversations = x;
    // });
  }

  open(id:string){
    this.router.navigate(['/chat',id]);
  }

}

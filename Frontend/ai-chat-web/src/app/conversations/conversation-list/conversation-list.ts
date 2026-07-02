import { Component } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Conversation } from '../../models/conversation';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../services/AuthService';
import { Input } from '@angular/core';
import { ConversationEventsService } from '../../services/conversation-events.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-conversation-list',
  imports: [CommonModule, FormsModule],
  standalone: true,
  templateUrl: './conversation-list.html',
  styleUrl: './conversation-list.css',
})
export class ConversationList {


  @Input()
  collapsed = false;
  
  editingId = '';
  editingTitle = '';
  confirmDeleteId = '';
  selectedId = '';
  conversations: Conversation[] = [];
  userName = '';

  constructor(private api:ApiService, private router:Router, private auth: AuthService,     private events: ConversationEventsService){ }

  ngOnInit() {

      this.userName = this.auth.getUserName();
      this.load();
      this.events.refresh$ .subscribe(() => this.load());
      
      this.router.events.subscribe(() => {
        const url = this.router.url;
        const parts = url.split('/');
        this.selectedId = parts[2] ?? '';
    });
  }

  async create(){
    const id = await firstValueFrom(this.api.createConversation());
    this.events.refresh();
    this.router.navigate(['/chat',id]);
  }

  showDelete(c:Conversation){
      this.confirmDeleteId=c.id;
  }
  delete(c:Conversation){

      this.api.deleteConversation(c.id).subscribe({

          next:()=>{

              this.confirmDeleteId='';

              const deletedId=c.id;

              this.conversations= this.conversations.filter(x=>x.id!==deletedId);
              this.events.refresh();

                if(this.selectedId===deletedId){

                    if(this.conversations.length){

                        this.router.navigate(['/chat', this.conversations[0].id]);

                    }else{

                        this.create();

                    }

                }

          }

      });
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

  rename(c: Conversation, event: MouseEvent) {

      event.stopPropagation();

      this.editingId = c.id;
      this.editingTitle = c.title;

      setTimeout(() => {
        const input = document.querySelector(
            '.rename-input'
        ) as HTMLInputElement;

        input?.focus();
        input?.select();
    });
  }

  saveRename(c: Conversation) {

      const title = this.editingTitle.trim();
      if (!title) {

        this.editingId = '';

        return;

      }

      this.api.renameConversation(c.id, title)
        .subscribe({
            next: () => {

                c.title = title;

                this.editingId = '';
                this.events.refresh();


            },
            error: err => {

              console.error(err);

            }
        });
   }

  cancelRename() {

      this.editingId = '';
  }

  open(id:string){
     this.selectedId = id;
    this.router.navigate(['/chat',id]);
  }
 
  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

}

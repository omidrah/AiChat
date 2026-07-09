import { Component, inject } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Conversation } from '../../models/conversation';
import { AuthService } from '../../services/AuthService';
import { Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConversationStore } from '../../store/conversation.store';
import { filter } from 'rxjs';

@Component({
  selector: 'app-conversation-list',
  imports: [CommonModule, FormsModule],
  standalone: true,
  templateUrl: './conversation-list.html',
  styleUrl: './conversation-list.css'
})
export class ConversationList {

  @Input()
  collapsed = false;
  editingId = '';
  editingTitle = '';
  confirmDeleteId = '';
  selectedId = '';
  userName = '';


  private router = inject(Router);
  private auth = inject(AuthService);
  private store = inject(ConversationStore);


  conversations = this.store.conversations;

  ngOnInit() {
    this.userName = this.auth.getUserName();
   
    this.router.events
    .pipe(
        filter(e => e instanceof NavigationEnd)
    )
    .subscribe(()=>{

        this.selectedId =
            this.router.url.split('/')[2] ?? '';

    });

  }

  async create() {
    const id = await this.store.create();
    this.router.navigate(['/chat', id]);
  }

  showDelete(c: Conversation) {
    this.confirmDeleteId = c.id;
  }
  
  async delete(c:Conversation){

      this.confirmDeleteId='';
      const nextId =
          await this.store.deleteAndNavigate(
              c.id,
              this.selectedId
          );

      if(nextId){

          this.router.navigate([
              '/chat',
              nextId
          ]);
      }
      else{

          this.create();
      }
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

  trackConversation(index: number, item: Conversation) {
    return item.id;
  }


  async saveRename(c: Conversation) {

    const title = this.editingTitle.trim();

    if (!title)
      return;
    await this.store.rename( c.id, title);

    this.editingId='';
    this.editingTitle='';

  }

  cancelRename() {

    this.editingId = '';
  }

  open(id: string) {
    this.selectedId = id;
    this.router.navigate(['/chat', id]);
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

}

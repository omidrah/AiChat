import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Conversation } from '../../models/conversation';
import { AuthService } from '../../services/AuthService';
import { Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConversationStore } from '../../store/conversation.store';

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

  constructor(private router: Router, private auth: AuthService, private store: ConversationStore) { }

  ngOnInit() {
    this.userName = this.auth.getUserName();
    this.store.conversations$
      .subscribe(x => {
        this.conversations = [...x];
      });

    this.updateSelectedConversation();
    this.router.events.subscribe(() => {
      this.updateSelectedConversation();
    });

  }

  async create() {
    const id = await this.store.create();
    this.router.navigate(['/chat', id]);
  }

  private updateSelectedConversation() {

    const parts = this.router.url.split('/');
    this.selectedId = parts[2] ?? '';
  }

  showDelete(c: Conversation) {
    this.confirmDeleteId = c.id;
  }

  delete(c: Conversation) {

    this.confirmDeleteId = '';

    this.store.delete(c.id)
      .subscribe({

        next: () => {

          const currentId = this.router.url.split('/')[2];

          if (currentId !== c.id)
            return;

          const list = this.store.value;

          if (list.length) {

            this.router.navigate([
              '/chat',
              list[0].id
            ]);

          }
          else {

            this.create();

          }

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

  trackConversation(index: number, item: Conversation) {
    return item.id;
  }


  saveRename(c: Conversation) {

    const title = this.editingTitle.trim();

    if (!title)
      return;

    this.store.rename(c.id, title)
      .subscribe({

        next: () => {

          this.editingId = '';

        }

      });

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

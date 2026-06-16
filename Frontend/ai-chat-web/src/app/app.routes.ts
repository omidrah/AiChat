import { Routes } from '@angular/router';
import { ChatComponent } from './chat/chat/chat';
import { ConversationList } from './conversations/list/conversation-list/conversation-list';

export const routes: Routes = [
    { path: '', component: ConversationList },
    { path: 'chat/:id', component: ChatComponent }
];

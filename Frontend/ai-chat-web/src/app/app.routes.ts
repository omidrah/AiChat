import { Routes } from '@angular/router';
import { ChatComponent } from './chat/chat';
import { ConversationList } from './conversations/conversation-list/conversation-list';

export const routes: Routes = [
    { path: '', component: ConversationList },
    { path: 'chat/:id', component: ChatComponent }
];

import { Routes } from '@angular/router';
import { ChatComponent } from './chat/chat';
import { LoginComponent } from './pages/login/login';
import { UserManagement } from './pages/user-management/user-management';
import { OllamaStatus } from './ollama-status/ollama-status';

export const routes: Routes = [
   { path: 'chat/:id', component: ChatComponent },
   {
      path: 'users',
      component: UserManagement
   },
   { path: 'ollama', component: OllamaStatus },
   { path: 'login', component: LoginComponent },
   { path: '', redirectTo: 'login', pathMatch: 'full' },
   { path: '**', redirectTo: 'login' }
];
